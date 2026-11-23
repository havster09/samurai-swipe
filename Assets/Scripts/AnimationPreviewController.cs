using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    /// <summary>
    /// Standalone AnimPreview scene controller.
    /// Uses animator.Play(stateName, 0, normalizedTime) every frame so it works
    /// with animator STATE names directly — no clip-name mapping required.
    /// All booleans are cleared each frame so the FSM can never override the preview.
    /// </summary>
    public class AnimationPreviewController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float  clipDuration   = 2.0f;
        [SerializeField] private bool   autoAdvance    = true;
        [SerializeField] private bool   loopClip       = true;
        [SerializeField] private string mainSceneName  = "Main";

        // AnimationClip asset names (all genjuro* — confirmed from animator logs).
        private static readonly string[] ClipNames = new string[]
        {
            "genjuroIdle",
            "genjuroRun",
            "genjuroWalk",
            "genjuroWalkLoop",
            "genjuroWalkBack",
            "genjuroWalkBackLoop",
            "genjuroAttackOne",
            "genjuroAttackTwo",
            "genjuroAttackThree",
            "genjuroAttackFour",
            "genjuroAttackFive",
            "genjuroAttackSix",
            "genjuroAttackSeven",
            "genjuroDashAttack",
            "genjuroDashSlash",
            "genjuroDoubleSlashHigh",
            "genjuroDoubleMid",
            "genjuroDoubleSlashLow",
            "genjuroCrossSword",
            "genjuroWinCrossSword",
            "genjuroBlock",
            "genjuroHit",
            "genjuroBloodCover",
            "genjuroWipeBlood",
            "genjuroWipeBloodToIdle",
            "genjuroCleanWeapon",
            "genjuroTurnBack",
            "genjuroTurnBackToIdle",
            "genjuroTurnPause",
        };

        // runtime state
        private int     _index;
        private float   _sampleTime;
        private float   _autoTimer;
        private bool    _clipFound;
        private string  _currentName = "";
        private AnimationClip _currentClip;

        private System.Collections.Generic.Dictionary<string, AnimationClip> _clipMap =
            new System.Collections.Generic.Dictionary<string, AnimationClip>();

        private Animator   _animator;
        private GameObject _heroGO;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        void Start()
        {
            var hero = Hero.Instance;
            if (hero != null)
            {
                _animator = hero.NpcHeroAnimator;
                _heroGO   = hero.gameObject;
            }
            else
            {
                var anim = FindObjectOfType<Animator>();
                if (anim != null) { _animator = anim; _heroGO = anim.gameObject; }
            }

            if (_animator == null)
            {
                Debug.LogWarning("[AnimPreview] No Animator found.");
                return;
            }

            // Build clip name → AnimationClip lookup
            foreach (var clip in _animator.runtimeAnimatorController.animationClips)
                if (!_clipMap.ContainsKey(clip.name))
                    _clipMap[clip.name] = clip;

            // Disable animator so the FSM/GOAP can never override our sampling
            _animator.enabled = false;

            LoadClip(_index);
        }

        void Update()
        {
            if (_heroGO == null || _currentClip == null) return;

            // Sample the clip directly — completely bypasses the state machine
            _currentClip.SampleAnimation(_heroGO, _sampleTime);
            _sampleTime += Time.deltaTime;

            float clipLen = _currentClip.length;
            if (_sampleTime > clipLen)
            {
                if (loopClip) _sampleTime %= clipLen;
                else          _sampleTime  = clipLen;
            }

            if (autoAdvance)
            {
                _autoTimer += Time.deltaTime;
                float advanceAfter = Mathf.Max(clipLen * 2f, clipDuration);
                if (_autoTimer >= advanceAfter)
                    Advance(1);
            }
        }

        void OnDestroy()
        {
            if (_animator != null)
                _animator.enabled = true;
        }

        // ── GUI ───────────────────────────────────────────────────────────────

        void OnGUI()
        {
            float sw = Screen.width, sh = Screen.height;

            // Back button
            GUI.color = new Color(0.3f, 0.7f, 1f);
            if (GUI.Button(new Rect(20f, 20f, 140f, 50f), "\u25c4  BACK"))
            {
                if (_animator != null) _animator.speed = 1f;
                SceneManager.LoadScene(mainSceneName);
            }
            GUI.color = Color.white;

            // Panel background
            float pw = Mathf.Min(sw - 40f, 520f);
            float ph = 310f;
            float px = (sw - pw) * 0.5f;
            float py = sh - ph - 30f;
            GUI.Box(new Rect(px, py, pw, ph), "");

            // Counter
            GUIStyle ctr = new GUIStyle(GUI.skin.label);
            ctr.fontSize  = 14;
            ctr.alignment = TextAnchor.MiddleCenter;
            ctr.normal.textColor = new Color(1f, 0.9f, 0.3f);
            GUI.Label(new Rect(px, py + 8f, pw, 24f),
                string.Format("[ {0} / {1} ]", _index + 1, ClipNames.Length), ctr);

            // Clip name — strip "genjuro" prefix for cleaner display
            string displayName = _currentName.Replace("genjuro", "");
            if (displayName.Length > 0)
                displayName = char.ToUpper(displayName[0]) + displayName.Substring(1);

            GUIStyle nameS = new GUIStyle(GUI.skin.label);
            nameS.fontSize  = 28;
            nameS.fontStyle = FontStyle.Bold;
            nameS.alignment = TextAnchor.MiddleCenter;
            nameS.normal.textColor = _clipFound ? Color.white : new Color(1f, 0.4f, 0.4f);
            GUI.Label(new Rect(px, py + 36f, pw, 46f), displayName, nameS);

            if (!_clipFound)
            {
                GUIStyle err = new GUIStyle(GUI.skin.label);
                err.fontSize  = 13;
                err.alignment = TextAnchor.MiddleCenter;
                err.normal.textColor = new Color(1f, 0.4f, 0.4f);
                GUI.Label(new Rect(px, py + 82f, pw, 22f), "(clip not found)", err);
            }

            // Category badge
            GUIStyle cat = new GUIStyle(GUI.skin.label);
            cat.fontSize  = 18;
            cat.alignment = TextAnchor.MiddleCenter;
            cat.normal.textColor = new Color(0.6f, 0.9f, 1f);
            GUI.Label(new Rect(px, py + 86f, pw, 26f), GetCategory(_currentName), cat);

            // Swipe hint
            string hint = GetSwipeHint(_currentName);
            if (hint.Length > 0)
            {
                GUIStyle hs = new GUIStyle(GUI.skin.label);
                hs.fontSize  = 14;
                hs.alignment = TextAnchor.MiddleCenter;
                hs.normal.textColor = new Color(0.7f, 1f, 0.7f);
                GUI.Label(new Rect(px, py + 114f, pw, 22f), hint, hs);
            }

            // Clip duration info
            if (_currentClip != null)
            {
                GUIStyle dur = new GUIStyle(GUI.skin.label);
                dur.fontSize  = 13;
                dur.alignment = TextAnchor.MiddleCenter;
                dur.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                GUI.Label(new Rect(px, py + 136f, pw, 20f),
                    string.Format("{0:F2}s  {1} fps", _currentClip.length,
                        Mathf.RoundToInt(_currentClip.frameRate)), dur);
            }

            // Progress bar
            float bx  = px + 20f;
            float bby = py + 162f;
            float bw2 = pw - 40f;
            float bh2 = 10f;
            GUI.Box(new Rect(bx, bby, bw2, bh2), "");
            if (_currentClip != null && _currentClip.length > 0f)
            {
                float fill = (_sampleTime % _currentClip.length) / _currentClip.length;
                GUI.color = new Color(1f, 0.6f, 0.1f);
                GUI.Box(new Rect(bx, bby, bw2 * fill, bh2), "");
                GUI.color = Color.white;
            }

            // Nav buttons
            float nbw  = 130f, nbh = 54f;
            float navY = py + 186f;
            float cx   = px + pw * 0.5f;

            if (GUI.Button(new Rect(cx - nbw - 12f, navY, nbw, nbh), "\u25c4  PREV"))
                Advance(-1);
            if (GUI.Button(new Rect(cx + 12f, navY, nbw, nbh), "NEXT  \u25ba"))
                Advance(1);

            // Auto-advance toggle
            GUI.color = autoAdvance
                ? new Color(0.4f, 0.9f, 0.4f)
                : new Color(0.7f, 0.7f, 0.7f);
            if (GUI.Button(new Rect(cx - 70f, navY + nbh + 10f, 140f, 38f),
                autoAdvance ? "AUTO  ON" : "AUTO  OFF"))
            {
                autoAdvance = !autoAdvance;
                _autoTimer  = 0f;
            }
            GUI.color = Color.white;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void Advance(int dir)
        {
            _index = (_index + dir + ClipNames.Length) % ClipNames.Length;
            LoadClip(_index);
        }

        private void LoadClip(int idx)
        {
            _currentName = ClipNames[idx];
            _sampleTime  = 0f;
            _autoTimer   = 0f;

            AnimationClip clip;
            if (_clipMap.TryGetValue(_currentName, out clip))
            {
                _currentClip = clip;
                _clipFound   = true;
                Debug.Log(string.Format("[AnimPreview] {0}  ({1:F2}s)", _currentName, clip.length));
            }
            else
            {
                _currentClip = null;
                _clipFound   = false;
                Debug.LogWarning("[AnimPreview] Clip not found: " + _currentName);
            }
        }

        private string GetCategory(string s)
        {
            if (s.Contains("Walk") || s.Contains("Run"))              return "[ MOVEMENT ]";
            if (s.Contains("Attack") || s.Contains("Slash") ||
                s.Contains("Dash"))                                    return "[ ATTACK ]";
            if (s.Contains("Block") || s.Contains("Cross") ||
                s.Contains("Win"))                                     return "[ DEFENCE / CLASH ]";
            if (s.Contains("Hit") || s.Contains("Blood"))             return "[ HIT REACTION ]";
            if (s.Contains("Idle"))                                    return "[ IDLE ]";
            if (s.Contains("Wipe") || s.Contains("Clean") ||
                s.Contains("Turn") || s.Contains("Pause"))            return "[ FLAVOUR ]";
            return "";
        }

        private string GetSwipeHint(string s)
        {
            switch (s)
            {
                case "genjuroAttackOne":       return "swipe Down (light / medium)  ← exclusive";
                case "genjuroAttackTwo":       return "swipe Right (light)  |  Left (light)  |  UpLeft (light)  |  Down* (light)";
                case "genjuroAttackThree":     return "swipe Right (medium)  |  UpRight (light / medium)  ← momentum";
                case "genjuroAttackFour":      return "swipe Up (light / medium)  ← exclusive";
                case "genjuroAttackFive":      return "swipe Left (medium)  |  UpLeft (medium)  |  Down* (medium)";
                case "genjuroAttackSix":       return "swipe Up (heavy)  ← exclusive";
                case "genjuroAttackSeven":     return "swipe Down (heavy)  ← exclusive";
                case "genjuroDashAttack":      return "swipe Right (heavy)  ← momentum + physical dash";
                case "genjuroDoubleSlashHigh": return "swipe UpRight / UpLeft (heavy)";
                case "genjuroDoubleMid":       return "swipe Left (heavy)";
                case "genjuroDoubleSlashLow":  return "swipe DownRight / DownLeft (heavy)";
                default:                       return "";
            }
        }
    }
}
