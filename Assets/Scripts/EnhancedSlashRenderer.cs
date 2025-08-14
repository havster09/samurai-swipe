using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GoapHeroActions;
using Assets.Scripts.ObservablePattern;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Enhanced slash renderer with improved swipe detection and observable pattern integration
    /// </summary>
    public class EnhancedSlashRenderer : MonoBehaviour
    {
        [Header("Slash Rendering")]
        [SerializeField] private Material slashMaterial;
        [SerializeField] private Color slashColor = Color.red;
        [SerializeField] private float slashWidth = 0.02f;
        [SerializeField] private int slashZPosition = 3;

        [Header("Swipe Detection")]
        [SerializeField] private ImprovedSwipeDetector swipeDetector;
        [SerializeField] private bool autoCreateSwipeDetector = true;

        [Header("Slash Effects")]
        [SerializeField] private bool enableSlashTrail = true;
        [SerializeField] private float trailFadeTime = 0.5f;
        [SerializeField] private int maxTrailPoints = 50;

        // Observable events
        public readonly ObservableEvent<ImprovedSwipeDetector.SwipeDirection> OnSwipeDetected = new ObservableEvent<ImprovedSwipeDetector.SwipeDirection>();
        public readonly ObservableEvent<Vector2[]> OnSwipePathRecorded = new ObservableEvent<Vector2[]>();
        public readonly ObservableEvent<float> OnSwipeVelocityChanged = new ObservableEvent<float>();
        public readonly ObservableEvent<float> OnSwipeDistanceChanged = new ObservableEvent<float>();

        // Slash rendering
        private GameObject _slashGameObject;
        private LineRenderer _lineRenderer;
        private List<Vector3> _slashPoints = new List<Vector3>();
        private bool _isDrawing = false;

        // Trail effect
        private List<GameObject> _trailObjects = new List<GameObject>();
        private Queue<float> _trailTimers = new Queue<float>();

        // Subscriptions
        private List<IDisposable> _subscriptions = new List<IDisposable>();

        void Awake()
        {
            SetupSwipeDetector();
            SetupSlashRenderer();
            SetupObservableSubscriptions();
        }

        void OnDestroy()
        {
            // Clean up subscriptions
            foreach (var subscription in _subscriptions)
            {
                if (subscription != null)
                    subscription.Dispose();
            }
            _subscriptions.Clear();

            // Dispose observable events - C# 4.0 compatible null checks
            if (OnSwipeDetected != null)
                OnSwipeDetected.Dispose();
            if (OnSwipePathRecorded != null)
                OnSwipePathRecorded.Dispose();
            if (OnSwipeVelocityChanged != null)
                OnSwipeVelocityChanged.Dispose();
            if (OnSwipeDistanceChanged != null)
                OnSwipeDistanceChanged.Dispose();
        }

        void Update()
        {
            UpdateTrailEffects();
        }

        private void SetupSwipeDetector()
        {
            if (swipeDetector == null && autoCreateSwipeDetector)
            {
                swipeDetector = gameObject.AddComponent<ImprovedSwipeDetector>();
            }

            if (swipeDetector != null)
            {
                // Subscribe to swipe detector events
                swipeDetector.OnSwipeDetected += HandleSwipeDetected;
                swipeDetector.OnSwipePathRecorded += HandleSwipePathRecorded;
            }
        }

        private void SetupSlashRenderer()
        {
            _slashGameObject = new GameObject("EnhancedSlash");
            _slashGameObject.transform.SetParent(transform);
            
            _lineRenderer = _slashGameObject.AddComponent<LineRenderer>();
            _lineRenderer.material = slashMaterial;
            _lineRenderer.startColor = slashColor;
            _lineRenderer.endColor = slashColor;
            _lineRenderer.startWidth = slashWidth;
            _lineRenderer.endWidth = 0f;
            _lineRenderer.positionCount = 0;
            _lineRenderer.sortingLayerName = "Slash";
            _lineRenderer.useWorldSpace = true;
        }

        private void SetupObservableSubscriptions()
        {
            // Subscribe to swipe events
            _subscriptions.Add(OnSwipeDetected.Subscribe(direction =>
            {
                HandleSwipeDirection(direction);
            }));

            _subscriptions.Add(OnSwipePathRecorded.Subscribe(path =>
            {
                CreateSlashFromPath(path);
            }));
        }

        private void HandleSwipeDetected(ImprovedSwipeDetector.SwipeDirection direction, float velocity, float distance)
        {
            // Trigger observable events
            OnSwipeDetected.Trigger(direction);
            OnSwipeVelocityChanged.Trigger(velocity);
            OnSwipeDistanceChanged.Trigger(distance);

            Debug.Log(string.Format("Enhanced swipe detected: {0}, Velocity: {1:F1}, Distance: {2:F1}", direction, velocity, distance));
        }

        private void HandleSwipePathRecorded(Vector2[] path)
        {
            OnSwipePathRecorded.Trigger(path);
        }

        private void HandleSwipeDirection(ImprovedSwipeDetector.SwipeDirection direction)
        {
            switch (direction)
            {
                case ImprovedSwipeDetector.SwipeDirection.Up:
                    OnSwipeUp();
                    break;
                case ImprovedSwipeDetector.SwipeDirection.Down:
                    OnSwipeDown();
                    break;
                case ImprovedSwipeDetector.SwipeDirection.Left:
                    OnSwipeLeft();
                    break;
                case ImprovedSwipeDetector.SwipeDirection.Right:
                    OnSwipeRight();
                    break;
                case ImprovedSwipeDetector.SwipeDirection.UpLeft:
                    OnSwipeUpLeft();
                    break;
                case ImprovedSwipeDetector.SwipeDirection.UpRight:
                    OnSwipeUpRight();
                    break;
                case ImprovedSwipeDetector.SwipeDirection.DownLeft:
                    OnSwipeDownLeft();
                    break;
                case ImprovedSwipeDetector.SwipeDirection.DownRight:
                    OnSwipeDownRight();
                    break;
            }
        }

        private void CreateSlashFromPath(Vector2[] path)
        {
            if (path == null || path.Length < 2) return;

            // Convert screen coordinates to world coordinates
            _slashPoints.Clear();
            foreach (Vector2 screenPoint in path)
            {
                Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, slashZPosition));
                _slashPoints.Add(worldPoint);
            }

            // Create the slash line
            _lineRenderer.positionCount = _slashPoints.Count;
            _lineRenderer.SetPositions(_slashPoints.ToArray());

            // Create trail effect if enabled
            if (enableSlashTrail)
            {
                CreateTrailEffect();
            }

            // Create slash collider
            CreateSlashCollider();

            // Clear the slash after a delay
            StartCoroutine(ClearSlashAfterDelay(0.1f));
        }

        private void CreateTrailEffect()
        {
            if (_slashPoints.Count < 2) return;

            GameObject trailObject = new GameObject("SlashTrail");
            trailObject.transform.SetParent(transform);

            LineRenderer trailRenderer = trailObject.AddComponent<LineRenderer>();
            trailRenderer.material = slashMaterial;
            trailRenderer.startColor = slashColor;
            trailRenderer.endColor = new Color(slashColor.r, slashColor.g, slashColor.b, 0f);
            trailRenderer.startWidth = slashWidth * 0.5f;
            trailRenderer.endWidth = 0f;
            trailRenderer.positionCount = _slashPoints.Count;
            trailRenderer.SetPositions(_slashPoints.ToArray());
            trailRenderer.sortingLayerName = "Slash";
            trailRenderer.sortingOrder = -1;

            _trailObjects.Add(trailObject);
            _trailTimers.Enqueue(Time.time + trailFadeTime);

            // Limit trail objects
            if (_trailObjects.Count > maxTrailPoints)
            {
                GameObject oldestTrail = _trailObjects[0];
                _trailObjects.RemoveAt(0);
                _trailTimers.Dequeue();
                Destroy(oldestTrail);
            }
        }

        private void UpdateTrailEffects()
        {
            for (int i = _trailObjects.Count - 1; i >= 0; i--)
            {
                if (Time.time > _trailTimers.ElementAt(i))
                {
                    GameObject trailObject = _trailObjects[i];
                    _trailObjects.RemoveAt(i);
                    _trailTimers.Dequeue();
                    Destroy(trailObject);
                }
            }
        }

        private void CreateSlashCollider()
        {
            if (_slashPoints.Count < 2) return;

            // Remove existing colliders
            RemoveSlashColliders();

            // Create new collider
            GameObject colliderObject = new GameObject("EnhancedSlashCollider");
            colliderObject.transform.SetParent(transform);
            BoxCollider2D slashCollider = colliderObject.AddComponent<BoxCollider2D>();
            slashCollider.tag = "SlashCollider";

            // Calculate collider properties
            Vector3 startPoint = _slashPoints[0];
            Vector3 endPoint = _slashPoints[_slashPoints.Count - 1];
            Vector3 midPoint = (startPoint + endPoint) / 2f;

            float lineLength = Vector3.Distance(startPoint, endPoint);
            slashCollider.size = new Vector2(lineLength, slashWidth * 2f);
            slashCollider.transform.position = midPoint;

            // Calculate rotation
            float angle = Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x) * Mathf.Rad2Deg;
            slashCollider.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void RemoveSlashColliders()
        {
            GameObject[] existingColliders = GameObject.FindGameObjectsWithTag("SlashCollider");
            foreach (GameObject collider in existingColliders)
            {
                Destroy(collider);
            }
        }

        private System.Collections.IEnumerator ClearSlashAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ClearSlash();
        }

        public void ClearSlash()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
            }
            _slashPoints.Clear();
        }

        // Swipe event handlers
        private void OnSwipeUp()
        {
            Debug.Log("Enhanced Swipe UP detected");
            // Add your swipe up logic here
        }

        private void OnSwipeDown()
        {
            Debug.Log("Enhanced Swipe DOWN detected");
            // Add your swipe down logic here
        }

        private void OnSwipeLeft()
        {
            Debug.Log("Enhanced Swipe LEFT detected");
            // Add your swipe left logic here
        }

        private void OnSwipeRight()
        {
            Debug.Log("Enhanced Swipe RIGHT detected");
            // Add your swipe right logic here
        }

        private void OnSwipeUpLeft()
        {
            Debug.Log("Enhanced Swipe UP-LEFT detected");
            // Add your diagonal swipe logic here
        }

        private void OnSwipeUpRight()
        {
            Debug.Log("Enhanced Swipe UP-RIGHT detected");
            // Add your diagonal swipe logic here
        }

        private void OnSwipeDownLeft()
        {
            Debug.Log("Enhanced Swipe DOWN-LEFT detected");
            // Add your diagonal swipe logic here
        }

        private void OnSwipeDownRight()
        {
            Debug.Log("Enhanced Swipe DOWN-RIGHT detected");
            // Add your diagonal swipe logic here
        }

        // Public methods
        public ImprovedSwipeDetector GetSwipeDetector() { return swipeDetector; }
        public bool IsDrawing() { return _isDrawing; }
        public List<Vector3> GetSlashPoints() { return new List<Vector3>(_slashPoints); }
    }
} 