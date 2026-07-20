using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Book of Five Rings combat modes. Retained for ModeBasedSpriteTinter compatibility.
    /// </summary>
    public enum BookOfFiveRingsMode { Earth, Water, Fire, Wind, Void }

    /// <summary>
    /// Translates swipe input into queued attacks.
    /// GOAP calls DequeueAttack() when in range instead of picking randomly.
    /// No GOAP files are modified.
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
        [SerializeField] private float entryLifetime = 3f;   // seconds before a queued move expires
        [SerializeField] private float stallTimeout  = 4f;   // seconds before safety fallback fires

        [Header("Debug Overlay")]
        [SerializeField] private bool showOverlay = true;

        // ── Static queue (read by Hero.GetAttackTypeAndDamage) ───────────────────
        private static readonly Queue<QueuedAttack> _queue = new Queue<QueuedAttack>();
        private static float _stallTimer = 0f;

        private struct QueuedAttack
        {
            public string Attack;
            public float  ExpiresAt;  // Time.time value
        }

        // ── Public API used by Hero.cs ────────────────────────────────────────────

        /// <summary>
        /// Returns the next queued attack name and removes it.
        /// Returns null if the queue is empty or all entries have expired.
        /// </summary>
        public static string DequeueAttack()
        {
            // Expire stale entries
            while (_queue.Count > 0 && _queue.Peek().ExpiresAt <= Time.time)
                _queue.Dequeue();

            if (_queue.Count == 0) return null;

            _stallTimer = 0f; // reset stall clock on successful dequeue
            return _queue.Dequeue().Attack;
        }

        /// <summary>True when the queue has at least one valid (non-expired) entry.</summary>
        public static bool HasPending
        {
            get
            {
                while (_queue.Count > 0 && _queue.Peek().ExpiresAt <= Time.time)
                    _queue.Dequeue();
                return _queue.Count > 0;
            }
        }

        /// <summary>
        /// Seconds the hero has been idle next to an enemy with no queued attack.
        /// Hero.cs uses this to fire a safety fallback after stallTimeout.
        /// </summary>
        public static float StallSeconds    => _stallTimer;
        public static float StallTimeoutCfg => _instance != null ? _instance.stallTimeout : 4f;

        // ── Singleton ref for config access ──────────────────────────────────────
        private static HeroSwipeController _instance;

        // ── Mode (ModeBasedSpriteTinter) ──────────────────────────────────────────
        public BookOfFiveRingsMode GetCurrentMode() { return currentMode; }

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
            // Only tick stall timer when no pending attack in queue
            if (!HasPending)
                _stallTimer += Time.deltaTime;
            else
                _stallTimer = 0f;
        }

        // ── Swipe handler ─────────────────────────────────────────────────────────
        private void HandleSwipe(ImprovedSwipeDetector.SwipeDirection direction, float velocity, float distance)
        {
            string attack = SwipeAttackMap.Resolve(direction, velocity);

            // Drop oldest if full
            if (_queue.Count >= maxQueueSize)
                _queue.Dequeue();

            _queue.Enqueue(new QueuedAttack
            {
                Attack    = attack,
                ExpiresAt = Time.time + entryLifetime
            });

            _stallTimer = 0f; // player swiped — reset stall clock

            Debug.Log(string.Format("[Swipe] {0} {1:F0}px/s -> {2}  (queue:{3})",
                direction, velocity, attack, _queue.Count));
        }

        // ── Clear queue on enemy death ────────────────────────────────────────────
        public static void ClearQueue() { _queue.Clear(); _stallTimer = 0f; }

        // ── On-screen queue visualizer ────────────────────────────────────────────
        void OnGUI()
        {
            if (!showOverlay) return;

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize  = 22;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;

            float x = 20f, y = 20f, w = 220f, h = 36f, pad = 4f;

            // Header
            GUIStyle hdr = new GUIStyle(style);
            hdr.fontSize = 16;
            hdr.normal.textColor = Color.yellow;
            GUI.Label(new Rect(x, y, w, 22f), "ATTACK QUEUE", hdr);
            y += 24f;

            // Snapshot to avoid modifying queue during iteration
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

                    // Fade colour as entry ages
                    float alpha = Mathf.Clamp01(remaining / entryLifetime);
                    Color col = i == 0
                        ? new Color(0.3f, 1f, 0.3f, alpha)   // next = green
                        : new Color(1f,   1f, 0.3f, alpha);   // rest = yellow

                    style.normal.textColor = col;
                    string label = string.Format("  {0}  [{1:F1}s]", entries[i].Attack, remaining);
                    GUI.Label(new Rect(x, y, w, h), label, style);
                    y += h + pad;
                }
            }

            // Stall indicator
            if (!HasPending && _stallTimer > 1f)
            {
                GUIStyle stall = new GUIStyle(style);
                stall.normal.textColor = new Color(1f, 0.4f, 0.4f, 1f);
                stall.fontSize = 16;
                float countdown = stallTimeout - _stallTimer;
                if (countdown > 0f)
                    GUI.Label(new Rect(x, y + 8f, w, 22f),
                        string.Format("  fallback in {0:F1}s", countdown), stall);
            }
        }
    }
}
