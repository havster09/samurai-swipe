using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class ImprovedSwipeDetector : MonoBehaviour
    {
        [Header("Swipe Detection Settings")]
        [SerializeField] private float minSwipeDistance  = 50f;
        [SerializeField] private float maxSwipeTime      = 0.5f;
        [SerializeField] private float minSwipeVelocity  = 100f;
        [SerializeField] private float maxSwipeVelocity  = 2000f;
        [SerializeField] private float directionThreshold = 0.35f;
        [SerializeField] private float diagonalThreshold  = 0.3f;

        [Header("Advanced Settings")]
        [SerializeField] private bool  enableDiagonalSwipes = true;
        [SerializeField] private int   minSwipePoints       = 5;
        [SerializeField] private float noiseReduction       = 2f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private Vector2       _startPosition;
        private Vector2       _endPosition;
        private float         _startTime;
        private float         _endTime;
        private List<Vector2> _touchPoints = new List<Vector2>();
        private bool          _isTracking  = false;

        private SwipeDirection _lastSwipeDirection = SwipeDirection.None;
        private float          _lastSwipeVelocity  = 0f;
        private float          _lastSwipeDistance  = 0f;
        private EndSlope       _lastEndSlope       = EndSlope.Neutral;

        [Header("End Slope Settings")]
        [SerializeField] private float endSlopeAngle = 20f;
        [SerializeField] private float endSlopeTailFraction = 0.2f;

        // Direction, Velocity, Distance
        public System.Action<SwipeDirection, float, float> OnSwipeDetected;

        public enum SwipeDirection
        {
            None,
            Up, Down, Left, Right,
            UpLeft, UpRight, DownLeft, DownRight
        }

        public enum EndSlope { Up, Neutral, Down }

        void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:    StartSwipeTracking(touch);  break;
                    case TouchPhase.Moved:    UpdateSwipeTracking(touch); break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled: EndSwipeTracking(touch);    break;
                }
            }
        }

        private void StartSwipeTracking(Touch touch)
        {
            _startPosition = touch.position;
            _startTime     = Time.time;
            _isTracking    = true;
            _touchPoints.Clear();
            _touchPoints.Add(touch.position);
        }

        private void UpdateSwipeTracking(Touch touch)
        {
            if (!_isTracking) return;
            Vector2 pos = touch.position;
            if (_touchPoints.Count == 0 ||
                Vector2.Distance(pos, _touchPoints[_touchPoints.Count - 1]) > noiseReduction)
            {
                _touchPoints.Add(pos);
            }
        }

        private void EndSwipeTracking(Touch touch)
        {
            if (!_isTracking) return;
            _endPosition = touch.position;
            _endTime     = Time.time;
            _isTracking  = false;
            _touchPoints.Add(touch.position);

            if (ValidateSwipe())
            {
                SwipeDirection dir      = DetermineSwipeDirection();
                float          velocity = CalculateVelocity();
                float          distance = CalculateDistance();

                _lastSwipeDirection = dir;
                _lastSwipeVelocity  = velocity;
                _lastSwipeDistance  = distance;
                _lastEndSlope       = ComputeEndSlope();

                if (OnSwipeDetected != null)
                    OnSwipeDetected.Invoke(dir, velocity, distance);

                if (showDebugInfo)
                    Debug.Log(string.Format("[Swipe] {0}  {1:F0}px/s  {2:F0}px  slope:{3}",
                        dir, velocity, distance, _lastEndSlope));
            }
            else if (showDebugInfo)
            {
                Debug.Log("[Swipe] rejected");
            }

            _touchPoints.Clear();
        }

        private bool ValidateSwipe()
        {
            if (_touchPoints.Count < minSwipePoints) return false;

            float swipeTime = _endTime - _startTime;
            if (swipeTime > maxSwipeTime) return false;

            float distance = CalculateDistance();
            if (distance < minSwipeDistance) return false;

            float velocity = CalculateVelocity();
            if (velocity < minSwipeVelocity || velocity > maxSwipeVelocity) return false;

            return IsSwipeStraight();
        }

        private bool IsSwipeStraight()
        {
            if (_touchPoints.Count < 3) return true;
            Vector2 startToEnd = _endPosition - _startPosition;
            float totalDist    = startToEnd.magnitude;
            if (totalDist < minSwipeDistance) return true;

            float actualDist = 0f;
            for (int i = 1; i < _touchPoints.Count; i++)
                actualDist += Vector2.Distance(_touchPoints[i], _touchPoints[i - 1]);

            return (totalDist / actualDist) >= directionThreshold;
        }

        private SwipeDirection DetermineSwipeDirection()
        {
            Vector2 dir   = _endPosition - _startPosition;
            float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            if (!enableDiagonalSwipes)
            {
                if (angle >= 45  && angle < 135) return SwipeDirection.Up;
                if (angle >= 135 && angle < 225) return SwipeDirection.Left;
                if (angle >= 225 && angle < 315) return SwipeDirection.Down;
                return SwipeDirection.Right;
            }

            float diagBand = diagonalThreshold * 45f;
            float[] cardinals = { 0f, 90f, 180f, 270f, 360f };
            float[] diagonals = { 45f, 135f, 225f, 315f };

            float minCardDelta = 360f;
            foreach (float c in cardinals)
            {
                float d = Mathf.Abs(Mathf.DeltaAngle(angle, c));
                if (d < minCardDelta) minCardDelta = d;
            }

            float minDiagDelta    = 360f;
            int   nearestDiagIdx  = 0;
            for (int i = 0; i < diagonals.Length; i++)
            {
                float d = Mathf.Abs(Mathf.DeltaAngle(angle, diagonals[i]));
                if (d < minDiagDelta) { minDiagDelta = d; nearestDiagIdx = i; }
            }

            if (minDiagDelta < minCardDelta && minDiagDelta < diagBand)
            {
                switch (nearestDiagIdx)
                {
                    case 0: return SwipeDirection.UpRight;
                    case 1: return SwipeDirection.UpLeft;
                    case 2: return SwipeDirection.DownLeft;
                    case 3: return SwipeDirection.DownRight;
                }
            }

            if (angle >= 45  && angle < 135) return SwipeDirection.Up;
            if (angle >= 135 && angle < 225) return SwipeDirection.Left;
            if (angle >= 225 && angle < 315) return SwipeDirection.Down;
            return SwipeDirection.Right;
        }

        private float CalculateVelocity()
        {
            float t = _endTime - _startTime;
            return t > 0 ? CalculateDistance() / t : 0f;
        }

        private float CalculateDistance()
        {
            return Vector2.Distance(_startPosition, _endPosition);
        }

        /// <summary>
        /// Examines the final portion of the swipe trail to determine whether the
        /// gesture ended pointing upward, downward, or neutral (horizontal).
        /// </summary>
        private EndSlope ComputeEndSlope()
        {
            if (_touchPoints.Count < 2) return EndSlope.Neutral;

            int tailCount = Mathf.Max(2, Mathf.RoundToInt(_touchPoints.Count * endSlopeTailFraction));
            int startIdx  = Mathf.Max(0, _touchPoints.Count - tailCount);

            Vector2 seg   = _touchPoints[_touchPoints.Count - 1] - _touchPoints[startIdx];
            float   angle = Mathf.Atan2(seg.y, seg.x) * Mathf.Rad2Deg;

            if (angle >  endSlopeAngle) return EndSlope.Up;
            if (angle < -endSlopeAngle) return EndSlope.Down;
            return EndSlope.Neutral;
        }

        public SwipeDirection GetLastDirection()   { return _lastSwipeDirection; }
        public float          GetLastVelocity()    { return _lastSwipeVelocity;  }
        public float          GetLastDistance()    { return _lastSwipeDistance;  }
        public bool           IsTracking()         { return _isTracking;         }
        public Vector2        GetLastEndPosition() { return _endPosition;        }
        public EndSlope       GetLastEndSlope()    { return _lastEndSlope;       }
    }
}
