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
        [SerializeField] private float entryLifetime = 15f;  // seconds before a queued move expires

        [Header("Debug Overlay")]
        [SerializeField] private bool showOverlay = true;

        // ── Static queue (read by Hero.GetAttackTypeAndDamage) ───────────────────
        private static readonly Queue<QueuedAttack> _queue = new Queue<QueuedAttack>();

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
            // Expire stale entries first
            while (_queue.Count > 0 && _queue.Peek().ExpiresAt <= Time.time)
                _queue.Dequeue();

            if (_queue.Count == 0) return null;
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
            // Expire stale queue entries each frame so the overlay stays accurate
            while (_queue.Count > 0 && _queue.Peek().ExpiresAt <= Time.time)
                _queue.Dequeue();
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

            Debug.Log(string.Format("[Swipe] {0} {1:F0}px/s -> {2}  (queue:{3})",
                direction, velocity, attack, _queue.Count));
        }

        // ── Clear queue on enemy death ────────────────────────────────────────────
        public static void ClearQueue() { _queue.Clear(); }

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

            // Empty state hint
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
