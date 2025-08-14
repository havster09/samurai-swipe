using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Improved swipe detector with better accuracy and gesture recognition
    /// </summary>
    public class ImprovedSwipeDetector : MonoBehaviour
    {
        [Header("Swipe Detection Settings")]
        [SerializeField] private float minSwipeDistance = 50f;        // Minimum distance for a swipe
        [SerializeField] private float maxSwipeTime = 0.5f;           // Maximum time for a swipe
        [SerializeField] private float minSwipeVelocity = 100f;       // Minimum velocity for a swipe
        [SerializeField] private float maxSwipeVelocity = 2000f;      // Maximum velocity for a swipe
        [SerializeField] private float directionThreshold = 0.7f;     // How straight the swipe must be (0-1)
        [SerializeField] private float diagonalThreshold = 0.3f;      // Threshold for diagonal detection

        [Header("Advanced Settings")]
        [SerializeField] private bool enableDiagonalSwipes = true;    // Allow diagonal swipes
        [SerializeField] private bool enableCurveDetection = false;   // Detect curved swipes
        [SerializeField] private int minSwipePoints = 5;              // Minimum points for a valid swipe
        [SerializeField] private float noiseReduction = 2f;           // Reduce noise in touch data

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool drawSwipePath = true;

        // Touch tracking
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private float _startTime;
        private float _endTime;
        private List<Vector2> _touchPoints = new List<Vector2>();
        private bool _isTracking = false;

        // Swipe state
        private SwipeDirection _lastSwipeDirection = SwipeDirection.None;
        private float _lastSwipeVelocity = 0f;
        private float _lastSwipeDistance = 0f;

        // Events
        public System.Action<SwipeDirection, float, float> OnSwipeDetected; // Direction, Velocity, Distance
        public System.Action<Vector2[]> OnSwipePathRecorded; // Full swipe path

        public enum SwipeDirection
        {
            None,
            Up,
            Down,
            Left,
            Right,
            UpLeft,
            UpRight,
            DownLeft,
            DownRight
        }

        void Update()
        {
            HandleTouchInput();
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        StartSwipeTracking(touch);
                        break;
                    case TouchPhase.Moved:
                        UpdateSwipeTracking(touch);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        EndSwipeTracking(touch);
                        break;
                }
            }
        }

        private void StartSwipeTracking(Touch touch)
        {
            _startPosition = touch.position;
            _startTime = Time.time;
            _isTracking = true;
            _touchPoints.Clear();
            _touchPoints.Add(touch.position);

            if (showDebugInfo)
                Debug.Log(string.Format("Swipe started at: {0}", _startPosition));
        }

        private void UpdateSwipeTracking(Touch touch)
        {
            if (!_isTracking) return;

            // Add point with noise reduction
            Vector2 currentPos = touch.position;
            if (_touchPoints.Count == 0 || Vector2.Distance(currentPos, _touchPoints[_touchPoints.Count - 1]) > noiseReduction)
            {
                _touchPoints.Add(currentPos);
            }

            if (showDebugInfo && _touchPoints.Count % 10 == 0)
                Debug.Log(string.Format("Tracking points: {0}", _touchPoints.Count));
        }

        private void EndSwipeTracking(Touch touch)
        {
            if (!_isTracking) return;

            _endPosition = touch.position;
            _endTime = Time.time;
            _isTracking = false;

            // Add final point
            _touchPoints.Add(touch.position);

            // Validate and process swipe
            if (ValidateSwipe())
            {
                SwipeDirection direction = DetermineSwipeDirection();
                float velocity = CalculateSwipeVelocity();
                float distance = CalculateSwipeDistance();

                _lastSwipeDirection = direction;
                _lastSwipeVelocity = velocity;
                _lastSwipeDistance = distance;

                // Trigger events
                if (OnSwipeDetected != null)
                    OnSwipeDetected.Invoke(direction, velocity, distance);
                if (OnSwipePathRecorded != null)
                    OnSwipePathRecorded.Invoke(_touchPoints.ToArray());

                if (showDebugInfo)
                {
                    Debug.Log(string.Format("Swipe detected: {0}, Velocity: {1:F1}, Distance: {2:F1}", direction, velocity, distance));
                }

                // Handle specific swipe directions
                HandleSwipeDirection(direction);
            }
            else
            {
                if (showDebugInfo)
                    Debug.Log("Swipe validation failed");
            }

            // Clear tracking data
            _touchPoints.Clear();
        }

        private bool ValidateSwipe()
        {
            if (_touchPoints.Count < minSwipePoints)
            {
                if (showDebugInfo)
                    Debug.Log(string.Format("Too few points: {0} < {1}", _touchPoints.Count, minSwipePoints));
                return false;
            }

            float swipeTime = _endTime - _startTime;
            if (swipeTime > maxSwipeTime)
            {
                if (showDebugInfo)
                    Debug.Log(string.Format("Swipe too slow: {0:F2}s > {1:F2}s", swipeTime, maxSwipeTime));
                return false;
            }

            float distance = CalculateSwipeDistance();
            if (distance < minSwipeDistance)
            {
                if (showDebugInfo)
                    Debug.Log(string.Format("Swipe too short: {0:F1} < {1}", distance, minSwipeDistance));
                return false;
            }

            float velocity = CalculateSwipeVelocity();
            if (velocity < minSwipeVelocity)
            {
                if (showDebugInfo)
                    Debug.Log(string.Format("Swipe too slow: {0:F1} < {1}", velocity, minSwipeVelocity));
                return false;
            }

            if (velocity > maxSwipeVelocity)
            {
                if (showDebugInfo)
                    Debug.Log(string.Format("Swipe too fast: {0:F1} > {1}", velocity, maxSwipeVelocity));
                return false;
            }

            // Check for straightness if curve detection is disabled
            if (!enableCurveDetection && !IsSwipeStraight())
            {
                if (showDebugInfo)
                    Debug.Log("Swipe not straight enough");
                return false;
            }

            return true;
        }

        private bool IsSwipeStraight()
        {
            if (_touchPoints.Count < 3) return true;

            Vector2 startToEnd = _endPosition - _startPosition;
            float totalDistance = startToEnd.magnitude;
            if (totalDistance < minSwipeDistance) return true;

            float actualDistance = 0f;
            for (int i = 1; i < _touchPoints.Count; i++)
            {
                actualDistance += Vector2.Distance(_touchPoints[i], _touchPoints[i - 1]);
            }

            float straightness = totalDistance / actualDistance;
            return straightness >= directionThreshold;
        }

        private SwipeDirection DetermineSwipeDirection()
        {
            Vector2 direction = _endPosition - _startPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Normalize angle to 0-360
            if (angle < 0) angle += 360;

            // Determine primary direction
            if (angle >= 45 && angle < 135)
                return SwipeDirection.Up;
            else if (angle >= 135 && angle < 225)
                return SwipeDirection.Left;
            else if (angle >= 225 && angle < 315)
                return SwipeDirection.Down;
            else
                return SwipeDirection.Right;

            // Note: Diagonal detection can be added here if needed
        }

        private float CalculateSwipeVelocity()
        {
            float distance = CalculateSwipeDistance();
            float time = _endTime - _startTime;
            return distance / time;
        }

        private float CalculateSwipeDistance()
        {
            return Vector2.Distance(_startPosition, _endPosition);
        }

        private void HandleSwipeDirection(SwipeDirection direction)
        {
            switch (direction)
            {
                case SwipeDirection.Up:
                    OnSwipeUp();
                    break;
                case SwipeDirection.Down:
                    OnSwipeDown();
                    break;
                case SwipeDirection.Left:
                    OnSwipeLeft();
                    break;
                case SwipeDirection.Right:
                    OnSwipeRight();
                    break;
                case SwipeDirection.UpLeft:
                    OnSwipeUpLeft();
                    break;
                case SwipeDirection.UpRight:
                    OnSwipeUpRight();
                    break;
                case SwipeDirection.DownLeft:
                    OnSwipeDownLeft();
                    break;
                case SwipeDirection.DownRight:
                    OnSwipeDownRight();
                    break;
            }
        }

        // Swipe event handlers
        private void OnSwipeUp()
        {
            Debug.Log(string.Format("Swipe UP - Velocity: {0:F1}, Distance: {1:F1}", _lastSwipeVelocity, _lastSwipeDistance));
            // Add your swipe up logic here
        }

        private void OnSwipeDown()
        {
            Debug.Log(string.Format("Swipe DOWN - Velocity: {0:F1}, Distance: {1:F1}", _lastSwipeVelocity, _lastSwipeDistance));
            // Add your swipe down logic here
        }

        private void OnSwipeLeft()
        {
            Debug.Log(string.Format("Swipe LEFT - Velocity: {0:F1}, Distance: {1:F1}", _lastSwipeVelocity, _lastSwipeDistance));
            // Add your swipe left logic here
        }

        private void OnSwipeRight()
        {
            Debug.Log(string.Format("Swipe RIGHT - Velocity: {0:F1}, Distance: {1:F1}", _lastSwipeVelocity, _lastSwipeDistance));
            // Add your swipe right logic here
        }

        private void OnSwipeUpLeft()
        {
            Debug.Log(string.Format("Swipe UP-LEFT - Velocity: {0:F1}, Distance: {1:F1}", _lastSwipeVelocity, _lastSwipeDistance));
            // Add your diagonal swipe logic here
        }

        private void OnSwipeUpRight()
        {
            Debug.Log(string.Format("Swipe UP-RIGHT - Velocity: {0:F1}, Distance: {1:F1}", _lastSwipeVelocity, _lastSwipeDistance));
            // Add your diagonal swipe logic here
        }

        private void OnSwipeDownLeft()
        {
            Debug.Log(string.Format("Swipe DOWN-LEFT - Velocity: {0:F1}, Distance: {1:F1}", _lastSwipeVelocity, _lastSwipeDistance));
            // Add your diagonal swipe logic here
        }

        private void OnSwipeDownRight()
        {
            Debug.Log(string.Format("Swipe DOWN-RIGHT - Velocity: {0:F1}, Distance: {1:F1}", _lastSwipeVelocity, _lastSwipeDistance));
            // Add your diagonal swipe logic here
        }

        // Public methods for external access
        public SwipeDirection GetLastSwipeDirection() { return _lastSwipeDirection; }
        public float GetLastSwipeVelocity() { return _lastSwipeVelocity; }
        public float GetLastSwipeDistance() { return _lastSwipeDistance; }
        public bool IsTracking() { return _isTracking; }

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!drawSwipePath || _touchPoints.Count < 2) return;

            Gizmos.color = Color.red;
            for (int i = 1; i < _touchPoints.Count; i++)
            {
                Vector3 start = Camera.main.ScreenToWorldPoint(_touchPoints[i - 1]);
                Vector3 end = Camera.main.ScreenToWorldPoint(_touchPoints[i]);
                start.z = 0;
                end.z = 0;
                Gizmos.DrawLine(start, end);
            }
        }
    }
} 