using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Listens for swipe input and immediately fires the matching attack animation on the hero.
    /// GOAP continues to handle movement, targeting, and hit outcomes unchanged.
    /// </summary>
    public class HeroSwipeController : MonoBehaviour
    {
        [Header("Swipe Detector")]
        [SerializeField] private ImprovedSwipeDetector swipeDetector;

        [Header("Debug Overlay")]
        [SerializeField] private bool showOverlay = true;

        private Hero _hero;

        private string _overlaySwipe  = "";
        private string _overlayAttack = "";
        private float  _overlayTimer  = 0f;
        private const float OverlayDuration = 2.5f;

        void Awake()
        {
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
        }

        private void HandleSwipe(ImprovedSwipeDetector.SwipeDirection direction, float velocity, float distance)
        {
            if (_hero == null)
                _hero = FindObjectOfType<Hero>();

            if (_hero == null) return;

            string attack = MapAttack(direction, velocity);

            Debug.Log(string.Format("[Swipe] {0}  {1:F0}px/s  -> {2}", direction, velocity, attack));

            _hero.Attack(attack);

            _overlaySwipe  = string.Format("{0}  {1:F0}px/s", direction, velocity);
            _overlayAttack = attack;
            _overlayTimer  = OverlayDuration;
        }

        private static string MapAttack(ImprovedSwipeDetector.SwipeDirection dir, float velocity)
        {
            bool heavy  = velocity > 1500f;
            bool medium = velocity > 600f;

            switch (dir)
            {
                case ImprovedSwipeDetector.SwipeDirection.Up:
                    return heavy  ? "heroAttackSeven"
                         : medium ? "heroAttackFour"
                                  : "heroAttackFour";

                case ImprovedSwipeDetector.SwipeDirection.Down:
                    return heavy  ? "heroDoubleSlashLow"
                         : medium ? "heroAttackOne"
                                  : "heroAttackOne";

                case ImprovedSwipeDetector.SwipeDirection.Left:
                case ImprovedSwipeDetector.SwipeDirection.Right:
                    return heavy  ? "heroDashAttack"
                         : medium ? "heroAttackThree"
                                  : "heroAttackTwo";

                case ImprovedSwipeDetector.SwipeDirection.UpRight:
                case ImprovedSwipeDetector.SwipeDirection.UpLeft:
                    return heavy  ? "heroDoubleSlashHigh"
                         : medium ? "heroAttackFour"
                                  : "heroAttackSix";

                case ImprovedSwipeDetector.SwipeDirection.DownRight:
                case ImprovedSwipeDetector.SwipeDirection.DownLeft:
                    return heavy  ? "heroDoubleSlashLow"
                         : medium ? "heroAttackFive"
                                  : "heroAttackOne";

                default:
                    return "heroAttackOne";
            }
        }

        void Update()
        {
            if (_overlayTimer > 0f)
                _overlayTimer -= Time.deltaTime;
        }

        void OnGUI()
        {
            if (!showOverlay || _overlayTimer <= 0f) return;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize  = 28;
            style.fontStyle = FontStyle.Bold;

            style.normal.textColor = Color.black;
            GUI.Label(new Rect(21, 21, 500, 45), _overlaySwipe, style);
            GUI.Label(new Rect(21, 66, 500, 45), _overlayAttack, style);

            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(20, 20, 500, 45), _overlaySwipe, style);
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(20, 65, 500, 45), _overlayAttack, style);
        }
    }
}
