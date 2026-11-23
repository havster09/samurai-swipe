using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    /// <summary>
    /// Renders a single "ANIM PREVIEW" button in the main scene.
    /// Tapping it loads the standalone AnimPreview scene.
    /// Run Tools → Create Anim Preview Scene once in the Unity Editor to set up that scene.
    /// </summary>
    public class HeroAnimationPreview : MonoBehaviour
    {
        [Header("Preview Settings")]
        [SerializeField] private bool  activeOnStart  = false;
        [SerializeField] private float clipDuration   = 1.8f;
        [SerializeField] private bool  autoAdvance    = true;

        // Full list of known hero animator clips — edit freely
        private static readonly string[] Clips = new string[]
        {
            "heroIdle",
            "heroRun",
            "heroWalkLoop",
            "heroWalkBackLoop",
            "heroAttackOne",
            "heroAttackTwo",
            "heroAttackThree",
            "heroAttackFour",
            "heroAttackFive",
            "heroAttackSix",
            "heroAttackSeven",
            "heroDashAttack",
            "heroDoubleSlashHigh",
            "heroDoubleSlashMid",
            "heroDoubleSlashLow",
            "heroCrossSword",
            "heroBlock",
            "heroHit",
            "heroWipeBlood",
            "heroCleanWeapon",
            "heroWalkBackReset",
        };

        private bool    _active;
        private int     _index;
        private float   _timer;
        private string  _currentClip = "";
        private bool    _errorOnClip;

        // ── Swipe input is blocked while preview is active ────────────────────────

        void Awake()
        {
            _active = activeOnStart;
        }

        void Update()
        {
            if (!_active) return;

            _timer += Time.deltaTime;
            if (autoAdvance && _timer >= clipDuration)
                Advance(1);
        }

        // ── GUI ───────────────────────────────────────────────────────────────────

        void OnGUI()
        {
            // Button in top-right — loads the AnimPreview scene
            float bw = 180f, bh = 50f;
            float bx = Screen.width - bw - 20f;
            float by = 20f;

            GUI.color = new Color(0.2f, 0.8f, 0.4f);
            if (GUI.Button(new Rect(bx, by, bw, bh), "ANIM PREVIEW"))
                SceneManager.LoadScene("AnimPreview");
            GUI.color = Color.white;

            if (!_active) return;

            // ── Panel background ──────────────────────────────────────────────────
            float pw = 420f, ph = 260f;
            float px = (Screen.width  - pw) * 0.5f;
            float py = Screen.height  - ph - 30f;

            GUI.Box(new Rect(px, py, pw, ph), "");

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize  = 14;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
            GUI.Label(new Rect(px, py + 8f, pw, 22f),
                string.Format("ANIMATION PREVIEW  [{0} / {1}]", _index + 1, Clips.Length),
                titleStyle);

            // ── Clip name ─────────────────────────────────────────────────────────
            GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
            nameStyle.fontSize  = 28;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = _errorOnClip ? Color.red : Color.white;
            GUI.Label(new Rect(px, py + 36f, pw, 44f), _currentClip, nameStyle);

            // ── Category tag ──────────────────────────────────────────────────────
            GUIStyle tagStyle = new GUIStyle(GUI.skin.label);
            tagStyle.fontSize  = 16;
            tagStyle.alignment = TextAnchor.MiddleCenter;
            tagStyle.normal.textColor = new Color(0.6f, 0.9f, 1f);
            GUI.Label(new Rect(px, py + 82f, pw, 24f), GetCategory(_currentClip), tagStyle);

            // ── Swipe hint (from SwipeAttackMap) ──────────────────────────────────
            string hint = GetSwipeHint(_currentClip);
            if (hint.Length > 0)
            {
                GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
                hintStyle.fontSize  = 14;
                hintStyle.alignment = TextAnchor.MiddleCenter;
                hintStyle.normal.textColor = new Color(0.7f, 1f, 0.7f);
                GUI.Label(new Rect(px, py + 108f, pw, 22f), hint, hintStyle);
            }

            // ── Progress bar ──────────────────────────────────────────────────────
            float barX = px + 20f, barY = py + 140f, barW = pw - 40f, barH = 10f;
            GUI.Box(new Rect(barX, barY, barW, barH), "");
            float fill = autoAdvance ? Mathf.Clamp01(_timer / clipDuration) : 1f;
            GUI.color = new Color(1f, 0.6f, 0.1f);
            GUI.Box(new Rect(barX, barY, barW * fill, barH), "");
            GUI.color = Color.white;

            // ── Nav buttons ───────────────────────────────────────────────────────
            float nbw = 120f, nbh = 54f, gap = 20f;
            float navY = py + 164f;
            float navCx = px + pw * 0.5f;

            if (GUI.Button(new Rect(navCx - nbw - gap * 0.5f, navY, nbw, nbh), "◀  PREV"))
                Advance(-1);

            if (GUI.Button(new Rect(navCx + gap * 0.5f, navY, nbw, nbh), "NEXT  ▶"))
                Advance(1);

            // ── Auto-advance toggle ───────────────────────────────────────────────
            float atx = px + pw * 0.5f - 70f;
            GUI.color = autoAdvance ? new Color(0.4f, 0.9f, 0.4f) : new Color(0.7f, 0.7f, 0.7f);
            if (GUI.Button(new Rect(atx, navY + nbh + 10f, 140f, 36f),
                autoAdvance ? "AUTO  ON" : "AUTO  OFF"))
            {
                autoAdvance = !autoAdvance;
                _timer      = 0f;
            }
            GUI.color = Color.white;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private void Advance(int dir)
        {
            _index = (_index + dir + Clips.Length) % Clips.Length;
            _timer = 0f;
            PlayCurrent();
        }

        private void PlayCurrent()
        {
            _currentClip = Clips[_index];
            _errorOnClip = false;

            var hero = Hero.Instance;
            if (hero == null || hero.NpcHeroAnimator == null) return;

            // Clear any lingering bools/triggers that could block the clip
            hero.NpcHeroAnimator.SetBool("heroRun",          false);
            hero.NpcHeroAnimator.SetBool("heroBlock",        false);
            hero.NpcHeroAnimator.SetBool("heroHit",          false);
            hero.NpcHeroAnimator.SetBool("heroCrossSword",   false);
            hero.NpcHeroAnimator.SetBool("heroBloodCover",   false);
            hero.NpcHeroAnimator.SetBool("heroTurnPause",    false);
            hero.NpcHeroAnimator.SetBool("heroWalkLoop",     false);
            hero.NpcHeroAnimator.SetBool("heroWalkBackLoop", false);
            hero.NpcHeroAnimator.SetBool("heroWalkBackReset",false);

            // Try Play() — Unity will log a warning internally if the clip name is wrong
            hero.NpcHeroAnimator.Play(_currentClip);
            Debug.Log(string.Format("[Preview] playing: {0}", _currentClip));
        }

        private void RestoreIdle()
        {
            _currentClip = "";
            var hero = Hero.Instance;
            if (hero == null || hero.NpcHeroAnimator == null) return;
            hero.NpcHeroAnimator.Play("heroIdle");
        }

        // Categorise the clip for the label
        private string GetCategory(string clip)
        {
            if (clip.Contains("Walk") || clip.Contains("Run"))   return "[ MOVEMENT ]";
            if (clip.Contains("Attack") || clip.Contains("Slash") ||
                clip.Contains("Dash"))                            return "[ ATTACK ]";
            if (clip.Contains("Block") || clip.Contains("Cross")) return "[ DEFENCE ]";
            if (clip.Contains("Hit"))                             return "[ HIT REACTION ]";
            if (clip.Contains("Idle"))                            return "[ IDLE ]";
            if (clip.Contains("Wipe") || clip.Contains("Clean")) return "[ IDLE FLAVOUR ]";
            return "";
        }

        // Show which swipe direction triggers this clip
        private string GetSwipeHint(string clip)
        {
            switch (clip)
            {
                case "heroAttackOne":         return "swipe Down (light/medium)";
                case "heroAttackTwo":         return "swipe Right (light)  |  swipe UpLeft (light)";
                case "heroAttackThree":       return "swipe Right (medium)";
                case "heroAttackFour":        return "swipe Up  |  swipe UpRight/UpLeft (medium)";
                case "heroAttackFive":        return "swipe Left (medium)  |  swipe Down/DownLeft (medium)";
                case "heroAttackSix":         return "swipe Left (light)  |  swipe UpRight (light)";
                case "heroAttackSeven":       return "swipe Up (heavy)";
                case "heroDashAttack":        return "swipe Right (heavy)";
                case "heroDoubleSlashHigh":   return "swipe UpRight/UpLeft (heavy)";
                case "heroDoubleSlashMid":    return "swipe Left (heavy)";
                case "heroDoubleSlashLow":    return "swipe Down/DownRight/DownLeft (heavy)";
                default:                      return "";
            }
        }
    }
}
