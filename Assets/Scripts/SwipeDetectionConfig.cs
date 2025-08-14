using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Configuration and comparison tool for swipe detection accuracy
    /// </summary>
    public class SwipeDetectionConfig : MonoBehaviour
    {
        [Header("Detection Method")]
        [SerializeField] private bool useImprovedDetector = true;
        [SerializeField] private bool showComparison = false;

        [Header("Improved Detector Settings")]
        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private float maxSwipeTime = 0.5f;
        [SerializeField] private float minSwipeVelocity = 100f;
        [SerializeField] private float maxSwipeVelocity = 2000f;
        [SerializeField] private float directionThreshold = 0.7f;
        [SerializeField] private int minSwipePoints = 5;
        [SerializeField] private float noiseReduction = 2f;

        [Header("Original Detector Settings")]
        [SerializeField] private float originalSwipeThreshold = 20f;

        [Header("Debug & Testing")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showDetectionStats = true;
        [SerializeField] private bool logSwipeDetails = true;

        // References
        private ImprovedSwipeDetector _improvedDetector;
        private SlashRenderer _originalDetector;
        private EnhancedSlashRenderer _enhancedRenderer;

        // Statistics
        private int _improvedDetections = 0;
        private int _originalDetections = 0;
        private int _missedDetections = 0;
        private float _averageVelocity = 0f;
        private float _averageDistance = 0f;

        void Start()
        {
            SetupDetectors();
            SetupEventListeners();
        }

        private void SetupDetectors()
        {
            // Find or create improved detector
            _improvedDetector = FindObjectOfType<ImprovedSwipeDetector>();
            if (_improvedDetector == null)
            {
                GameObject detectorObject = new GameObject("ImprovedSwipeDetector");
                _improvedDetector = detectorObject.AddComponent<ImprovedSwipeDetector>();
            }

            // Find original detector
            _originalDetector = FindObjectOfType<SlashRenderer>();

            // Find enhanced renderer
            _enhancedRenderer = FindObjectOfType<EnhancedSlashRenderer>();

            // Apply settings to improved detector
            if (_improvedDetector != null)
            {
                ApplySettingsToImprovedDetector();
            }
        }

        private void ApplySettingsToImprovedDetector()
        {
            // Use reflection to set private fields (since they're serialized)
            var type = typeof(ImprovedSwipeDetector);
            
            var minDistanceField = type.GetField("minSwipeDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minDistanceField != null) minDistanceField.SetValue(_improvedDetector, minSwipeDistance);

            var maxTimeField = type.GetField("maxSwipeTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (maxTimeField != null) maxTimeField.SetValue(_improvedDetector, maxSwipeTime);

            var minVelocityField = type.GetField("minSwipeVelocity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minVelocityField != null) minVelocityField.SetValue(_improvedDetector, minSwipeVelocity);

            var maxVelocityField = type.GetField("maxSwipeVelocity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (maxVelocityField != null) maxVelocityField.SetValue(_improvedDetector, maxSwipeVelocity);

            var directionThresholdField = type.GetField("directionThreshold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (directionThresholdField != null) directionThresholdField.SetValue(_improvedDetector, directionThreshold);

            var minPointsField = type.GetField("minSwipePoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minPointsField != null) minPointsField.SetValue(_improvedDetector, minSwipePoints);

            var noiseReductionField = type.GetField("noiseReduction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (noiseReductionField != null) noiseReductionField.SetValue(_improvedDetector, noiseReduction);
        }

        private void SetupEventListeners()
        {
            if (_improvedDetector != null)
            {
                _improvedDetector.OnSwipeDetected += OnImprovedSwipeDetected;
            }

            if (_enhancedRenderer != null)
            {
                _enhancedRenderer.OnSwipeDetected.Subscribe(OnEnhancedSwipeDetected);
                _enhancedRenderer.OnSwipeVelocityChanged.Subscribe(OnSwipeVelocityChanged);
                _enhancedRenderer.OnSwipeDistanceChanged.Subscribe(OnSwipeDistanceChanged);
            }
        }

        private void OnImprovedSwipeDetected(ImprovedSwipeDetector.SwipeDirection direction, float velocity, float distance)
        {
            _improvedDetections++;
            _averageVelocity = (_averageVelocity * (_improvedDetections - 1) + velocity) / _improvedDetections;
            _averageDistance = (_averageDistance * (_improvedDetections - 1) + distance) / _improvedDetections;

            if (logSwipeDetails)
            {
                Debug.Log(string.Format("[Improved] Swipe: {0}, Velocity: {1:F1}, Distance: {2:F1}", direction, velocity, distance));
            }

            if (showDetectionStats)
            {
                LogDetectionStats();
            }
        }

        private void OnEnhancedSwipeDetected(ImprovedSwipeDetector.SwipeDirection direction)
        {
            if (logSwipeDetails)
            {
                Debug.Log(string.Format("[Enhanced] Swipe detected: {0}", direction));
            }
        }

        private void OnSwipeVelocityChanged(float velocity)
        {
            if (logSwipeDetails)
            {
                Debug.Log(string.Format("[Enhanced] Velocity: {0:F1}", velocity));
            }
        }

        private void OnSwipeDistanceChanged(float distance)
        {
            if (logSwipeDetails)
            {
                Debug.Log(string.Format("[Enhanced] Distance: {0:F1}", distance));
            }
        }

        private void LogDetectionStats()
        {
            Debug.Log(string.Format("[Stats] Improved: {0}, Original: {1}, Missed: {2}", _improvedDetections, _originalDetections, _missedDetections));
            Debug.Log(string.Format("[Stats] Avg Velocity: {0:F1}, Avg Distance: {1:F1}", _averageVelocity, _averageDistance));
        }

        // Public methods for runtime configuration
        public void SetMinSwipeDistance(float distance)
        {
            minSwipeDistance = distance;
            ApplySettingsToImprovedDetector();
        }

        public void SetMaxSwipeTime(float time)
        {
            maxSwipeTime = time;
            ApplySettingsToImprovedDetector();
        }

        public void SetMinSwipeVelocity(float velocity)
        {
            minSwipeVelocity = velocity;
            ApplySettingsToImprovedDetector();
        }

        public void SetMaxSwipeVelocity(float velocity)
        {
            maxSwipeVelocity = velocity;
            ApplySettingsToImprovedDetector();
        }

        public void SetDirectionThreshold(float threshold)
        {
            directionThreshold = threshold;
            ApplySettingsToImprovedDetector();
        }

        public void SetMinSwipePoints(int points)
        {
            minSwipePoints = points;
            ApplySettingsToImprovedDetector();
        }

        public void SetNoiseReduction(float reduction)
        {
            noiseReduction = reduction;
            ApplySettingsToImprovedDetector();
        }

        public void ResetStatistics()
        {
            _improvedDetections = 0;
            _originalDetections = 0;
            _missedDetections = 0;
            _averageVelocity = 0f;
            _averageDistance = 0f;
            Debug.Log("Swipe detection statistics reset");
        }

        public void ToggleDetectionMethod()
        {
            useImprovedDetector = !useImprovedDetector;
            Debug.Log(string.Format("Switched to {0} detection method", useImprovedDetector ? "Improved" : "Original"));
        }

        // Context menu methods for easy testing
        [ContextMenu("Test Swipe Settings")]
        public void TestSwipeSettings()
        {
            Debug.Log("=== Swipe Detection Settings ===");
            Debug.Log(string.Format("Min Distance: {0}", minSwipeDistance));
            Debug.Log(string.Format("Max Time: {0}", maxSwipeTime));
            Debug.Log(string.Format("Min Velocity: {0}", minSwipeVelocity));
            Debug.Log(string.Format("Max Velocity: {0}", maxSwipeVelocity));
            Debug.Log(string.Format("Direction Threshold: {0}", directionThreshold));
            Debug.Log(string.Format("Min Points: {0}", minSwipePoints));
            Debug.Log(string.Format("Noise Reduction: {0}", noiseReduction));
        }

        [ContextMenu("Show Detection Stats")]
        public void ShowDetectionStats()
        {
            LogDetectionStats();
        }

        [ContextMenu("Reset Stats")]
        public void ResetStats()
        {
            ResetStatistics();
        }

        void OnGUI()
        {
            if (!showDetectionStats) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Swipe Detection Stats", GUI.skin.box);
            GUILayout.Label(string.Format("Improved Detections: {0}", _improvedDetections));
            GUILayout.Label(string.Format("Original Detections: {0}", _originalDetections));
            GUILayout.Label(string.Format("Missed Detections: {0}", _missedDetections));
            GUILayout.Label(string.Format("Avg Velocity: {0:F1}", _averageVelocity));
            GUILayout.Label(string.Format("Avg Distance: {0:F1}", _averageDistance));
            GUILayout.EndArea();
        }
    }
} 