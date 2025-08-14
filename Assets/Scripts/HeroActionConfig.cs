using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.ObservablePattern;

namespace Assets.Scripts
{
    /// <summary>
    /// Configuration and monitoring for hero action cancellation system
    /// </summary>
    public class HeroActionConfig : MonoBehaviour
    {
        [Header("Action Cancellation Settings")]
        [SerializeField] private bool enableActionCancellation = true;
        [SerializeField] private float actionCancelDelay = 0.3f;
        [SerializeField] private float enemyDetectionRadius = 1.2f; // Reduced for more precise detection
        [SerializeField] private float enemyHitDistance = 0.8f; // Reduced for more precise hit detection
        [SerializeField] private BookOfFiveRingsMode currentMode = BookOfFiveRingsMode.Earth; // Current Book of Five Rings mode

        [Header("Performance Monitoring")]
        [SerializeField] private bool showPerformanceStats = true;
        [SerializeField] private bool logActionEvents = true;
        [SerializeField] private bool showDebugVisuals = true;

        [Header("References")]
        [SerializeField] private HeroSwipeController heroSwipeController;

        // Statistics
        private int _totalSwipes = 0;
        private int _successfulHits = 0;
        private int _cancelledActions = 0;
        private float _averageSwipeVelocity = 0f;
        private float _averageSwipeDistance = 0f;

        // Subscriptions
        private List<IDisposable> _subscriptions = new List<IDisposable>();

        void Start()
        {
            SetupHeroSwipeController();
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
        }

        private void SetupHeroSwipeController()
        {
            if (heroSwipeController == null)
            {
                heroSwipeController = FindObjectOfType<HeroSwipeController>();
            }

            if (heroSwipeController == null)
            {
                Debug.LogWarning("HeroSwipeController not found! Creating one...");
                Hero hero = FindObjectOfType<Hero>();
                if (hero != null)
                {
                    GameObject heroObject = hero.gameObject;
                    if (heroObject != null)
                    {
                        heroSwipeController = heroObject.AddComponent<HeroSwipeController>();
                    }
                }
            }
        }

        private void SetupObservableSubscriptions()
        {
            if (heroSwipeController != null)
            {
                _subscriptions.Add(heroSwipeController.OnSwipeHitEnemy.Subscribe(OnSwipeHitEnemy));
                _subscriptions.Add(heroSwipeController.OnActionCancelled.Subscribe(OnActionCancelled));
                _subscriptions.Add(heroSwipeController.OnSwipePathRecorded.Subscribe(OnSwipePathRecorded));
            }
        }

        private void OnSwipeHitEnemy(bool hitEnemy)
        {
            _totalSwipes++;
            
            if (hitEnemy)
            {
                _successfulHits++;
                if (logActionEvents)
                {
                    Debug.Log(string.Format("[HeroAction] Swipe hit enemy! Success rate: {0:F1}%", 
                        (_successfulHits * 100f) / _totalSwipes));
                }
            }
            else
            {
                if (logActionEvents)
                {
                    Debug.Log("[HeroAction] Swipe missed - action will be cancelled if no enemies hit");
                }
            }
        }

        private void OnActionCancelled()
        {
            _cancelledActions++;
            if (logActionEvents)
            {
                Debug.Log(string.Format("[HeroAction] Action cancelled! Cancellation rate: {0:F1}%", 
                    (_cancelledActions * 100f) / _totalSwipes));
            }
        }

        private void OnSwipePathRecorded(Vector2[] path)
        {
            if (logActionEvents)
            {
                Debug.Log(string.Format("[HeroAction] Swipe path recorded with {0} points", path.Length));
            }
        }

        // Public methods for runtime configuration
        public void SetActionCancelDelay(float delay)
        {
            actionCancelDelay = delay;
            if (heroSwipeController != null)
            {
                // Update the controller's delay through reflection or public property
                var delayField = typeof(HeroSwipeController).GetField("actionCancelDelay", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (delayField != null)
                {
                    delayField.SetValue(heroSwipeController, delay);
                }
            }
        }

        public void SetEnemyDetectionRadius(float radius)
        {
            enemyDetectionRadius = radius;
            if (heroSwipeController != null)
            {
                heroSwipeController.SetEnemyDetectionRadius(radius);
            }
        }

        public void SetEnemyHitThreshold(float threshold)
        {
            enemyHitDistance = threshold;
            if (heroSwipeController != null)
            {
                heroSwipeController.SetEnemyHitThreshold(threshold);
            }
        }

        // Book of Five Rings mode control methods
        public void SetMode(BookOfFiveRingsMode mode)
        {
            if (heroSwipeController != null)
            {
                heroSwipeController.SetMode(mode);
            }
            if (logActionEvents)
            {
                Debug.Log(string.Format("[HeroAction] Switched to {0} mode - {1}", mode, GetModeDescription(mode)));
            }
        }

        public void EnableVoidMode()
        {
            if (heroSwipeController != null)
            {
                heroSwipeController.SetMode(BookOfFiveRingsMode.Void);
            }
            if (logActionEvents)
            {
                Debug.Log("[HeroAction] Void mode enabled - unlimited enemy hits!");
            }
        }

        public void DisableVoidMode()
        {
            if (heroSwipeController != null)
            {
                heroSwipeController.SetMode(BookOfFiveRingsMode.Earth);
            }
            if (logActionEvents)
            {
                Debug.Log("[HeroAction] Void mode disabled - back to Earth mode (single enemy)");
            }
        }

        public void ToggleVoidMode()
        {
            if (heroSwipeController != null)
            {
                BookOfFiveRingsMode currentMode = heroSwipeController.GetCurrentMode();
                if (currentMode == BookOfFiveRingsMode.Void)
                {
                    heroSwipeController.SetMode(BookOfFiveRingsMode.Earth);
                }
                else
                {
                    heroSwipeController.SetMode(BookOfFiveRingsMode.Void);
                }
            }
            if (logActionEvents)
            {
                Debug.Log("[HeroAction] Toggled Void mode");
            }
        }

        public void CycleToNextMode()
        {
            if (heroSwipeController != null)
            {
                heroSwipeController.CycleToNextMode();
            }
        }

        public void CycleToPreviousMode()
        {
            if (heroSwipeController != null)
            {
                heroSwipeController.CycleToPreviousMode();
            }
        }

        public string GetModeDescription(BookOfFiveRingsMode mode)
        {
            switch (mode)
            {
                case BookOfFiveRingsMode.Earth:
                    return "Foundation techniques - Single enemy focus";
                case BookOfFiveRingsMode.Water:
                    return "Flowing movements - Up to 3 enemies";
                case BookOfFiveRingsMode.Fire:
                    return "Aggressive attacks - Up to 5 enemies";
                case BookOfFiveRingsMode.Wind:
                    return "Speed and precision - Up to 7 enemies";
                case BookOfFiveRingsMode.Void:
                    return "Mastery - Unlimited enemies";
                default:
                    return "Unknown mode";
            }
        }

        public void ToggleActionCancellation()
        {
            enableActionCancellation = !enableActionCancellation;
            if (heroSwipeController != null)
            {
                var enableField = typeof(HeroSwipeController).GetField("enableActionCancellation", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (enableField != null)
                {
                    enableField.SetValue(heroSwipeController, enableActionCancellation);
                }
            }
            
            Debug.Log(string.Format("[HeroAction] Action cancellation {0}", 
                enableActionCancellation ? "enabled" : "disabled"));
        }

        public void ResetStatistics()
        {
            _totalSwipes = 0;
            _successfulHits = 0;
            _cancelledActions = 0;
            _averageSwipeVelocity = 0f;
            _averageSwipeDistance = 0f;
            Debug.Log("[HeroAction] Statistics reset");
        }

        // Context menu methods for easy testing
        [ContextMenu("Test Action Cancellation")]
        public void TestActionCancellation()
        {
            Debug.Log("=== Hero Action Configuration ===");
            Debug.Log(string.Format("Action Cancellation: {0}", enableActionCancellation ? "Enabled" : "Disabled"));
            Debug.Log(string.Format("Cancel Delay: {0:F2}s", actionCancelDelay));
            Debug.Log(string.Format("Enemy Detection Radius: {0:F1}", enemyDetectionRadius));
            Debug.Log(string.Format("Enemy Hit Distance: {0:F1}", enemyHitDistance));
            Debug.Log(string.Format("Current Mode: {0}", currentMode));
            
            if (heroSwipeController != null)
            {
                Debug.Log(string.Format("Controller Hit Threshold: {0:F1}", heroSwipeController.GetEnemyHitThreshold()));
                Debug.Log(string.Format("Controller Mode: {0}", heroSwipeController.GetCurrentMode()));
                Debug.Log(string.Format("Max Enemies for Mode: {0}", heroSwipeController.GetMaxEnemiesForCurrentMode()));
            }
        }

        [ContextMenu("Test Book of Five Rings Modes")]
        public void TestBookOfFiveRingsModes()
        {
            Debug.Log("=== Book of Five Rings Mode Test ===");
            Debug.Log("Earth Mode: Single enemy focus");
            Debug.Log("Water Mode: Up to 3 enemies");
            Debug.Log("Fire Mode: Up to 5 enemies");
            Debug.Log("Wind Mode: Up to 7 enemies");
            Debug.Log("Void Mode: Unlimited enemies");
            
            if (heroSwipeController != null)
            {
                Debug.Log(string.Format("Current Mode: {0}", heroSwipeController.GetCurrentMode()));
                Debug.Log(string.Format("Mode Description: {0}", heroSwipeController.GetModeDescription(heroSwipeController.GetCurrentMode())));
            }
        }

        [ContextMenu("Cycle to Next Mode")]
        public void CycleToNextModeContext()
        {
            CycleToNextMode();
        }

        [ContextMenu("Cycle to Previous Mode")]
        public void CycleToPreviousModeContext()
        {
            CycleToPreviousMode();
        }

        [ContextMenu("Enable Void Mode")]
        public void EnableVoidModeContext()
        {
            EnableVoidMode();
        }

        [ContextMenu("Disable Void Mode")]
        public void DisableVoidModeContext()
        {
            DisableVoidMode();
        }

        [ContextMenu("Force Refresh Trail Colors")]
        public void ForceRefreshTrailColorsContext()
        {
            if (heroSwipeController != null)
            {
                heroSwipeController.ForceRefreshTrailColors();
            }
            else
            {
                Debug.LogWarning("[HeroAction] HeroSwipeController not found!");
            }
        }

        [ContextMenu("Test Trail Tinting")]
        public void TestTrailTinting()
        {
            if (heroSwipeController != null)
            {
                Debug.Log("=== Testing Trail Tinting ===");
                Debug.Log(string.Format("Current Mode: {0}", heroSwipeController.GetCurrentMode()));
                Debug.Log(string.Format("Mode Color: {0}", heroSwipeController.GetCurrentModeFullColor()));
                
                // Force refresh trail colors
                heroSwipeController.ForceRefreshTrailColors();
            }
            else
            {
                Debug.LogWarning("[HeroAction] HeroSwipeController not found!");
            }
        }

        [ContextMenu("Show Performance Stats")]
        public void ShowPerformanceStats()
        {
            if (_totalSwipes > 0)
            {
                float successRate = (_successfulHits * 100f) / _totalSwipes;
                float cancellationRate = (_cancelledActions * 100f) / _totalSwipes;
                
                Debug.Log("=== Hero Action Performance ===");
                Debug.Log(string.Format("Total Swipes: {0}", _totalSwipes));
                Debug.Log(string.Format("Successful Hits: {0} ({1:F1}%)", _successfulHits, successRate));
                Debug.Log(string.Format("Cancelled Actions: {0} ({1:F1}%)", _cancelledActions, cancellationRate));
                Debug.Log(string.Format("Average Velocity: {0:F1}", _averageSwipeVelocity));
                Debug.Log(string.Format("Average Distance: {0:F1}", _averageSwipeDistance));
            }
            else
            {
                Debug.Log("[HeroAction] No swipes recorded yet");
            }
        }

        [ContextMenu("Reset Stats")]
        public void ResetStats()
        {
            ResetStatistics();
        }

        [ContextMenu("Toggle Hit Precision")]
        public void ToggleHitPrecision()
        {
            // Toggle between precise (0.8f) and loose (1.5f) hit detection
            float newThreshold = (enemyHitDistance < 1.0f) ? 1.5f : 0.8f;
            SetEnemyHitThreshold(newThreshold);
            Debug.Log(string.Format("[HeroAction] Hit threshold toggled to: {0:F1}", newThreshold));
        }

        void OnGUI()
        {
            // Safety check to prevent GUI assertion failures
            if (!showPerformanceStats || Event.current == null) return;

            try
            {
                GUILayout.BeginArea(new Rect(10, 250, 300, 150));
                GUILayout.Label("Hero Action Stats", GUI.skin.box);
                GUILayout.Label(string.Format("Total Swipes: {0}", _totalSwipes));
                GUILayout.Label(string.Format("Successful Hits: {0}", _successfulHits));
                GUILayout.Label(string.Format("Cancelled Actions: {0}", _cancelledActions));
                
                if (_totalSwipes > 0)
                {
                    float successRate = (_successfulHits * 100f) / _totalSwipes;
                    float cancellationRate = (_cancelledActions * 100f) / _totalSwipes;
                    GUILayout.Label(string.Format("Success Rate: {0:F1}%", successRate));
                    GUILayout.Label(string.Format("Cancellation Rate: {0:F1}%", cancellationRate));
                }
                GUILayout.EndArea();
            }
            catch (System.Exception e)
            {
                // Log any GUI errors but don't crash
                Debug.LogWarning("[HeroActionConfig] GUI Error: " + e.Message);
            }
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!showDebugVisuals) return;

            // Draw enemy detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyDetectionRadius);

            // Draw enemy hit distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyHitDistance);
        }
    }
} 