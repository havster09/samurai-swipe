using System.Collections.Generic;
using Assets.Scripts.GoapHeroActions;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Book of Five Rings combat modes. Retained for ModeBasedSpriteTinter compatibility.
    /// </summary>
    public enum BookOfFiveRingsMode { Earth, Water, Fire, Wind, Void }

    /// <summary>
    /// Translates swipe input into queued attacks.
    /// Hold-to-retreat: hero walks backward toward the hold position, facing a locked enemy.
    /// GOAP calls DequeueAttack() when in attack range instead of picking randomly.
    /// </summary>
    public class HeroSwipeController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────────
        [Header("Mode")]
        [SerializeField] private BookOfFiveRingsMode currentMode = BookOfFiveRingsMode.Earth;

        [Header("Swipe Detector")]
        [SerializeField] private ImprovedSwipeDetector swipeDetector;

        [Header("Queue Settings")]
        [SerializeField] private int   maxQueueSize  = 3;
        [SerializeField] private float entryLifetime = 15f;

        [Header("Hold to Retreat")]
        [SerializeField] private float holdThreshold    = 0.15f;  // seconds before hold activates
        [SerializeField] private float walkSpeed        = 1.2f;   // world units/second (max walk speed)
        [SerializeField] private float walkSmoothTime   = 0.25f;  // seconds to reach full speed (SmoothDamp)
        [SerializeField] private float walkSlowRadius   = 0.5f;   // units from target where speed scales down
        [SerializeField] private float walkStopRange    = 0.15f;  // arrive threshold

        [Header("Double Tap to Turn")]
        [SerializeField] private float doubleTapWindow      = 0.3f;   // max seconds between taps
        [SerializeField] private float emptySpaceEnemyRadius = 1.5f;  // world units — tap must be this far from all enemies

        [Header("Debug Overlay")]
        [SerializeField] private bool showOverlay = true;

        // ── Static attack queue (read by Hero.GetAttackTypeAndDamage) ─────────────
        private static readonly Queue<QueuedAttack> _queue = new Queue<QueuedAttack>();

        private struct QueuedAttack
        {
            public string Attack;
            public float  ExpiresAt;
        }

        // ── Hold-to-retreat state ─────────────────────────────────────────────────
        private bool       _fingerDown;
        private float      _holdStartTime;
        private bool       _isHoldActive;
        private Vector3    _walkTarget;
        private bool       _walkTargetSet;
        private GameObject _lockedEnemy;   // locked at hold-start, held until release
        private float      _walkVelocity;  // SmoothDamp current velocity (world units/s)

        // ── Double-tap-to-turn state ──────────────────────────────────────────────
        private float _lastTapTime = -99f;

        /// <summary>True while player is holding to retreat. Suppresses GOAP Move().</summary>
        public static bool IsPlayerWalking { get; private set; }

        // ── Singleton / mode ──────────────────────────────────────────────────────
        private static HeroSwipeController _instance;
        public BookOfFiveRingsMode GetCurrentMode() { return currentMode; }

        // ── Public API used by Hero.cs ────────────────────────────────────────────

        public static string DequeueAttack()
        {
            while (_queue.Count > 0 && _queue.Peek().ExpiresAt <= Time.time)
                _queue.Dequeue();
            if (_queue.Count == 0) return null;
            return _queue.Dequeue().Attack;
        }

        public static bool HasPending
        {
            get
            {
                while (_queue.Count > 0 && _queue.Peek().ExpiresAt <= Time.time)
                    _queue.Dequeue();
                return _queue.Count > 0;
            }
        }

        public static void ClearQueue() { _queue.Clear(); }

        // ── Unity lifecycle ───────────────────────────────────────────────────────
        void Awake()
        {
            _instance = this;
            if (swipeDetector == null)
                swipeDetector = GetComponent<ImprovedSwipeDetector>();
            if (swipeDetector == null)
                swipeDetector = gameObject.AddComponent<ImprovedSwipeDetector>();
        }

        void Start()
        {
            swipeDetector.OnSwipeDetected += HandleSwipe;
        }

        void OnDestroy()
        {
            if (swipeDetector != null)
                swipeDetector.OnSwipeDetected -= HandleSwipe;
            if (_instance == this) _instance = null;
        }

        void Update()
        {
            while (_queue.Count > 0 && _queue.Peek().ExpiresAt <= Time.time)
                _queue.Dequeue();

            HandleHoldToRetreat();
        }

        // ── Hold-to-retreat input ─────────────────────────────────────────────────

        private void HandleHoldToRetreat()
        {
            if (Input.touchCount < 1)
            {
                if (_fingerDown) StopRetreat();
                return;
            }

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // ── Double-tap-to-turn check ──────────────────────────────────────
                float timeSinceLast = Time.time - _lastTapTime;
                if (timeSinceLast < doubleTapWindow && IsTapOnEmptySpace(touch.position))
                {
                    _lastTapTime = -99f; // consume the double tap
                    if (Hero.Instance != null)
                    {
                        Debug.Log("[Turn] double tap on empty space — turning");
                        Hero.Instance.TurnAround();
                    }
                    return;
                }
                _lastTapTime = Time.time;
                // ─────────────────────────────────────────────────────────────────

                _fingerDown    = true;
                _holdStartTime = Time.time;
                _isHoldActive  = false;
                // Lock nearest living enemy at the moment the finger comes down
                _lockedEnemy   = FindNearestEnemy();
                Debug.Log(string.Format("[Retreat] touch began  locked:{0}",
                    _lockedEnemy != null ? _lockedEnemy.name : "none"));
                return;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                StopRetreat();
                return;
            }

            if (!_fingerDown) return;

            float held = Time.time - _holdStartTime;
            if (!_isHoldActive)
            {
                if (held >= holdThreshold)
                {
                    _isHoldActive = true;
                    Debug.Log(string.Format("[Retreat] hold activated after {0:F2}s", held));
                }
                else return;
            }

            // Convert touch to world X
            float   depth = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 world = Camera.main.ScreenToWorldPoint(
                new Vector3(touch.position.x, touch.position.y, depth));
            _walkTarget    = new Vector3(world.x, 0f, 0f);
            _walkTargetSet = true;
            IsPlayerWalking = true;

            PerformRetreatStep();
        }

        // ── Retreat movement ──────────────────────────────────────────────────────

        private void PerformRetreatStep()
        {
            if (Hero.Instance == null || !_walkTargetSet) return;

            var hero = Hero.Instance;

            // Freeze position and bleed velocity during any blocking animation
            // (attack, hit, cross-sword, turn). When the animation ends the hero
            // resumes from rest rather than snapping back to full walk speed.
            if (hero.IsBlockingMovement())
            {
                _walkVelocity = 0f;
                hero.StopWalkAnimation();
                return;
            }

            float heroX   = hero.transform.position.x;
            float targetX = _walkTarget.x;
            float dist    = Mathf.Abs(heroX - targetX);

            // Clear GOAP run while player is driving
            hero.NpcHeroAnimator.SetBool("heroRun", false);

            if (dist < walkStopRange)
            {
                _walkVelocity = 0f;
                hero.StopWalkAnimation();
                return;
            }

            float dir = Mathf.Sign(targetX - heroX);

            // Scale target speed down when close to destination for a smooth stop
            float speedScale  = Mathf.Clamp01(dist / walkSlowRadius);
            float targetSpeed = dir * walkSpeed * speedScale;

            // Smooth acceleration / deceleration
            float smoothedSpeed = Mathf.SmoothDamp(
                _walkVelocity, targetSpeed, ref _walkVelocity, walkSmoothTime);

            float move = smoothedSpeed * Time.deltaTime;
            hero.transform.position = new Vector3(
                heroX + move, 0f, hero.transform.position.z);

            // Face locked enemy — keep lock even if they move closer/farther
            if (_lockedEnemy != null)
            {
                var enemy = _lockedEnemy.GetComponent<Enemy>();
                bool dead = enemy != null && enemy.IsDead;
                if (!dead)
                    hero.FaceTarget(_lockedEnemy);
            }

            // Walk forward if moving in facing direction, backward if moving against it
            hero.SetWalkAnimation(smoothedSpeed);
        }

        private void StopRetreat()
        {
            _fingerDown    = false;
            _isHoldActive  = false;
            _walkTargetSet = false;
            _lockedEnemy   = null;
            _walkVelocity  = 0f;

            if (IsPlayerWalking)
            {
                IsPlayerWalking = false;
                Debug.Log("[Retreat] stopped");

                if (Hero.Instance != null)
                    Hero.Instance.StopWalkAnimation();
            }
        }

        // ── Enemy utils ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the number of enemies currently alive and visible (not dead, not pending death).
        /// </summary>
        private int CountActiveEnemies()
        {
            int count = 0;
            foreach (var e in Object.FindObjectsOfType<Enemy>())
            {
                if (e == null) continue;
                if (e.IsDead) continue;
                count++;
            }
            return count;
        }

        private GameObject FindNearestEnemy()
        {
            var targets = GoapHeroAction.NpcTargetAttributes;
            if (targets == null || targets.Count == 0) return null;

            Vector3 heroPos = Hero.Instance.transform.position;
            GameObject best = null;
            float bestDist  = float.MaxValue;

            foreach (var npc in targets)
            {
                if (npc == null || npc.Health <= 0) continue;
                var enemy = npc.GetComponent<Enemy>();
                if (enemy != null && enemy.IsDead) continue;
                float d = Vector2.Distance(npc.transform.position, heroPos);
                if (d < bestDist) { bestDist = d; best = npc.gameObject; }
            }
            return best;
        }

        /// <summary>
        /// Returns true if the screen position is not within emptySpaceEnemyRadius
        /// world units of any live enemy. Used to gate the double-tap-to-turn.
        /// </summary>
        private bool IsTapOnEmptySpace(Vector2 screenPos)
        {
            if (Camera.main == null) return true;

            float depth  = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 world = Camera.main.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, depth));

            var targets = GoapHeroAction.NpcTargetAttributes;
            if (targets == null || targets.Count == 0) return true;

            foreach (var npc in targets)
            {
                if (npc == null) continue;
                var enemy = npc.GetComponent<Enemy>();
                if (enemy != null && enemy.IsDead) continue;
                float d = Vector2.Distance(new Vector2(world.x, world.y),
                                           new Vector2(npc.transform.position.x, npc.transform.position.y));
                if (d <= emptySpaceEnemyRadius) return false;
            }
            return true;
        }

        // ── Swipe → always queue an attack ────────────────────────────────────────

        private void HandleSwipe(ImprovedSwipeDetector.SwipeDirection direction, float velocity, float distance)
        {
            StopRetreat();

            var endSlope = swipeDetector.GetLastEndSlope();
            string attack = SwipeAttackMap.Resolve(direction, velocity, endSlope);

            // Dash attack is reserved for long strokes (> half screen width) with 2+ active enemies.
            // If conditions are not met, downgrade to a strong in-place slash.
            if (attack == "heroDashAttack")
            {
                bool isLongEnough  = distance >= Screen.width * 0.5f;
                bool enoughEnemies = CountActiveEnemies() >= 2;

                if (!isLongEnough || !enoughEnemies)
                {
                    Debug.Log(string.Format(
                        "[Swipe] Dash downgraded -> heroAttackThree  (dist:{0:F0} need:{1:F0}  enemies:{2})",
                        distance, Screen.width * 0.5f, CountActiveEnemies()));
                    attack = "heroAttackThree";
                }
                else
                {
                    // Dash qualifies: clear queue so it executes alone
                    _queue.Clear();
                    Debug.Log("[Swipe] Dash QUALIFIED — queue cleared");
                }
            }

            if (_queue.Count >= maxQueueSize)
                _queue.Dequeue();

            _queue.Enqueue(new QueuedAttack
            {
                Attack    = attack,
                ExpiresAt = Time.time + entryLifetime
            });

            Debug.Log(string.Format("[Swipe] {0} {1:F0}px/s dist:{2:F0}px -> {3}  (queue:{4})",
                direction, velocity, distance, attack, _queue.Count));
        }

        // ── On-screen overlay ─────────────────────────────────────────────────────
        void OnGUI()
        {
            if (!showOverlay) return;

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize  = 22;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;

            float x = 20f, y = 20f, w = 260f, h = 36f, pad = 4f;

            if (IsPlayerWalking)
            {
                GUIStyle rs = new GUIStyle(style);
                rs.fontSize = 18;
                rs.normal.textColor = new Color(1f, 0.5f, 0.2f, 1f);
                string lbl = _lockedEnemy != null
                    ? string.Format("  RETREATING  <- [{0}]", _lockedEnemy.name)
                    : "  RETREATING  <-";
                GUI.Label(new Rect(x, y, w, h), lbl, rs);
                y += h + pad;
            }

            GUIStyle hdr = new GUIStyle(style);
            hdr.fontSize = 16;
            hdr.normal.textColor = Color.yellow;
            GUI.Label(new Rect(x, y, w, 22f), "ATTACK QUEUE", hdr);
            y += 24f;

            var entries = _queue.ToArray();
            if (entries.Length == 0)
            {
                GUIStyle empty = new GUIStyle(style);
                empty.normal.textColor = new Color(1f, 1f, 1f, 0.4f);
                empty.fontSize = 18;
                GUI.Label(new Rect(x, y, w, h), "  — swipe to queue —", empty);
            }
            else
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    float remaining = entries[i].ExpiresAt - Time.time;
                    if (remaining <= 0f) continue;

                    float alpha = Mathf.Clamp01(remaining / entryLifetime);
                    Color col = i == 0
                        ? new Color(0.3f, 1f, 0.3f, alpha)
                        : new Color(1f,   1f, 0.3f, alpha);

                    style.normal.textColor = col;
                    GUI.Label(new Rect(x, y, w, h),
                        string.Format("  {0}  [{1:F1}s]", entries[i].Attack, remaining), style);
                    y += h + pad;
                }
            }

            if (!HasPending)
            {
                GUIStyle hint = new GUIStyle(hdr);
                hint.normal.textColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                hint.fontSize = 14;
                GUI.Label(new Rect(x, y + 8f, w, 22f), "  auto-attacking", hint);
            }
        }
    }
}
