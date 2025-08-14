using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using Assets.Scripts.ObservablePattern;
using UnityEngine;


namespace Assets.Scripts
{
    /// <summary>
    /// Book of Five Rings combat modes based on Miyamoto Musashi's teachings
    /// </summary>
    public enum BookOfFiveRingsMode
    {
        Earth,      // 地 - Chi: Foundation, basic techniques, single enemy focus
        Water,      // 水 - Sui: Adaptability, flowing movements, multiple enemies
        Fire,       // 火 - Ka: Aggressive, fast attacks, chain hits
        Wind,       // 風 - Fu: Speed and precision, critical hits
        Void        // 空 - Ku: Mastery, ultimate technique, unlimited potential
    }

    /// <summary>
    /// Enhanced hero swipe controller with action cancellation when swipes miss enemies
    /// </summary>
    public class HeroSwipeController : MonoBehaviour
    {
        [Header("Swipe Detection")]
        [SerializeField] private ImprovedSwipeDetector swipeDetector;
        [SerializeField] private EnhancedSlashRenderer enhancedSlashRenderer;
        [SerializeField] private SlashRenderer originalSlashRenderer;

        [Header("Action Settings")]
        [SerializeField] private float actionCancelDelay = 0.3f;
        [SerializeField] private bool enableActionCancellation = true;
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private BookOfFiveRingsMode currentMode = BookOfFiveRingsMode.Earth; // Default to Earth mode
        // [SerializeField] private bool powerupMode = false; // Enable multiple enemy hits per swipe

        [Header("Combat Detection")]
        [SerializeField] private float enemyDetectionRadius = 1.2f; // Reduced from 2f for more precise detection
        [SerializeField] private LayerMask enemyLayerMask = -1;
        [SerializeField] private float enemyHitThreshold = 0.8f; // New: precise hit threshold for enemy center

        [Header("Combat Stats")]
        [SerializeField] private int heroMaxHealth = 100;
        [SerializeField] private int heroCurrentHealth = 100;
        [SerializeField] private int heroBaseAttackPower = 1;
        [SerializeField] private float criticalHitChance = 0.15f; // 15% chance
        [SerializeField] private float criticalHitMultiplier = 2.0f;
        [SerializeField] private bool showCombatStats = true;

        [Header("Background Tinting")]
        [SerializeField] private bool enableBackgroundTinting = true;
        [SerializeField] private float backgroundTintIntensity = 0.3f; // How strong the tint effect is
        [SerializeField] private float backgroundTintTransitionSpeed = 2.0f; // How fast the color changes
        [SerializeField] private Color defaultBackgroundColor = Color.black; // Default background color

        [Header("Void Mode Effects")]
        [SerializeField] private bool enableVoidModeEffects = true;
        [SerializeField] private float voidModeEnemySlowFactor = 0.3f; // Enemies move at 30% speed in Void mode
        [SerializeField] private float voidModeEffectRadius = 10f; // Radius of the slow effect
        [SerializeField] private bool showVoidModeEffects = true;

        // Hero references
        private Hero _hero;
        private Animator _heroAnimator;

        // Camera reference for background tinting
        private Camera _mainCamera;
        private Color _targetBackgroundColor;
        private Color _currentBackgroundColor;

        // Swipe state tracking
        private bool _isSwipeActive = false;
        private bool _hasHitEnemy = false;
        private Coroutine _actionCancelCoroutine;
        private List<GameObject> _enemiesHitThisSwipe = new List<GameObject>();

        // Combat tracking
        private Dictionary<GameObject, EnemyHealth> _enemyHealthMap = new Dictionary<GameObject, EnemyHealth>();
        private int _totalDamageDealt = 0;
        private int _criticalHits = 0;
        private int _totalHits = 0;

        // Void mode effects tracking
        private Dictionary<GameObject, EnemyMovementData> _enemyMovementMap = new Dictionary<GameObject, EnemyMovementData>();
        private bool _voidModeActive = false;

        // Observable events
        public readonly ObservableEvent<bool> OnSwipeHitEnemy = new ObservableEvent<bool>();
        public readonly ObservableEvent OnActionCancelled = new ObservableEvent();
        public readonly ObservableEvent<Vector2[]> OnSwipePathRecorded = new ObservableEvent<Vector2[]>();

        // Subscriptions
        private List<IDisposable> _subscriptions = new List<IDisposable>();

        void Awake()
        {
            SetupReferences();
            SetupSwipeDetection();
            SetupObservableSubscriptions();
            
            // Apply initial mode visual effects
            ApplyModeVisualEffects();
        }

        void Update()
        {
            // Update background tinting for smooth transitions
            UpdateBackgroundTint();
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

            // Dispose observable events
            if (OnSwipeHitEnemy != null)
                OnSwipeHitEnemy.Dispose();
            if (OnActionCancelled != null)
                OnActionCancelled.Dispose();
            if (OnSwipePathRecorded != null)
                OnSwipePathRecorded.Dispose();
        }

        private void SetupReferences()
        {
            _hero = GetComponent<Hero>();
            if (_hero == null)
            {
                _hero = FindObjectOfType<Hero>();
            }

            if (_hero != null)
            {
                _heroAnimator = _hero.NpcHeroAnimator;
            }

            // Get main camera for background tinting
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                _mainCamera = FindObjectOfType<Camera>();
            }

            if (_mainCamera != null)
            {
                _currentBackgroundColor = _mainCamera.backgroundColor;
                _targetBackgroundColor = _currentBackgroundColor;
                
                if (showDebugInfo)
                {
                    Debug.Log("Camera reference found for background tinting");
                }
            }
            else
            {
                Debug.LogWarning("No camera found for background tinting!");
            }
        }

        private void SetupSwipeDetection()
        {
            // Find or create swipe detector
            if (swipeDetector == null)
            {
                swipeDetector = FindObjectOfType<ImprovedSwipeDetector>();
                if (swipeDetector == null)
                {
                    GameObject detectorObject = new GameObject("HeroSwipeDetector");
                    swipeDetector = detectorObject.AddComponent<ImprovedSwipeDetector>();
                }
            }

            // Find slash renderers
            if (enhancedSlashRenderer == null)
            {
                enhancedSlashRenderer = FindObjectOfType<EnhancedSlashRenderer>();
            }

            if (originalSlashRenderer == null)
            {
                originalSlashRenderer = FindObjectOfType<SlashRenderer>();
            }
        }

        private void SetupObservableSubscriptions()
        {
            if (swipeDetector != null)
            {
                swipeDetector.OnSwipeDetected += OnSwipeDetected;
                // Remove the incorrect subscription - OnSwipePathRecorded doesn't exist on swipeDetector
            }

            if (enhancedSlashRenderer != null)
            {
                _subscriptions.Add(enhancedSlashRenderer.OnSwipeDetected.Subscribe(OnEnhancedSwipeDetected));
                _subscriptions.Add(enhancedSlashRenderer.OnSwipePathRecorded.Subscribe(OnEnhancedSwipePathRecorded));
            }
        }

        private void OnSwipeDetected(ImprovedSwipeDetector.SwipeDirection direction, float velocity, float distance)
        {
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Hero swipe detected: {0}, Velocity: {1:F1}, Distance: {2:F1}", direction, velocity, distance));
            }

            // Start swipe tracking
            StartSwipeTracking();
            
            // Trigger hero attack based on swipe direction
            TriggerHeroAttack(direction, velocity, distance);
        }

        private void HandleSwipePathRecorded(Vector2[] path)
        {
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Hero swipe path recorded with {0} points", path.Length));
            }

            // Check for enemies along the swipe path
            CheckEnemiesAlongSwipePath(path);
            
            // Trigger observable event
            OnSwipePathRecorded.Trigger(path);
        }

        private void OnEnhancedSwipeDetected(ImprovedSwipeDetector.SwipeDirection direction)
        {
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Enhanced hero swipe: {0}", direction));
            }
        }

        private void OnEnhancedSwipePathRecorded(Vector2[] path)
        {
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Enhanced hero swipe path with {0} points", path.Length));
            }
        }

        private void StartSwipeTracking()
        {
            _isSwipeActive = true;
            _hasHitEnemy = false;
            _enemiesHitThisSwipe.Clear();

            // Start action cancellation timer
            if (enableActionCancellation)
            {
                if (_actionCancelCoroutine != null)
                {
                    StopCoroutine(_actionCancelCoroutine);
                }
                _actionCancelCoroutine = StartCoroutine(ActionCancelTimer());
            }
        }

        private void TriggerHeroAttack(ImprovedSwipeDetector.SwipeDirection direction, float velocity, float distance)
        {
            if (_hero == null) return;

            // Determine attack type based on swipe direction and velocity
            string attackType = DetermineAttackType(direction, velocity, distance);
            
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Triggering hero attack: {0}", attackType));
            }

            // Trigger the attack
            _hero.Attack(attackType);
        }

        private string DetermineAttackType(ImprovedSwipeDetector.SwipeDirection direction, float velocity, float distance)
        {
            // High velocity swipes get special attacks
            if (velocity > 1500f)
            {
                switch (direction)
                {
                    case ImprovedSwipeDetector.SwipeDirection.Up:
                        return "heroAttackFour"; // High slash
                    case ImprovedSwipeDetector.SwipeDirection.Down:
                        return "heroAttackOne"; // Down slash
                    case ImprovedSwipeDetector.SwipeDirection.Left:
                    case ImprovedSwipeDetector.SwipeDirection.Right:
                        return "heroAttackThree"; // Dash slash
                    default:
                        return "heroAttackOne"; // Default
                }
            }
            else
            {
                // Standard attacks based on direction
                switch (direction)
                {
                    case ImprovedSwipeDetector.SwipeDirection.Up:
                        return "heroAttackFour"; // High slash
                    case ImprovedSwipeDetector.SwipeDirection.Down:
                        return "heroAttackOne"; // Down slash
                    case ImprovedSwipeDetector.SwipeDirection.Left:
                    case ImprovedSwipeDetector.SwipeDirection.Right:
                        return "heroAttackTwo"; // Side slash
                    default:
                        return "heroAttackOne"; // Default
                }
            }
        }

        private void CheckEnemiesAlongSwipePath(Vector2[] path)
        {
            if (path == null || path.Length < 2) return;

            // Convert screen coordinates to world coordinates
            List<Vector3> worldPath = new List<Vector3>();
            foreach (Vector2 screenPoint in path)
            {
                Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 0));
                worldPath.Add(worldPoint);
            }

            // Calculate swipe properties for damage
            float swipeDistance = CalculateSwipeDistance(worldPath);
            float swipeVelocity = CalculateSwipeVelocity(worldPath);

            // Check for enemies along the path
            var enemiesInRange = Physics2D.OverlapCircleAll(transform.position, enemyDetectionRadius, enemyLayerMask);
            
            foreach (var enemyCollider in enemiesInRange)
            {
                if (enemyCollider.CompareTag("Enemy") || enemyCollider.CompareTag("NPC"))
                {
                    // Check if enemy is close to the swipe path
                    if (IsEnemyNearSwipePath(enemyCollider.transform.position, worldPath))
                    {
                        if (!_enemiesHitThisSwipe.Contains(enemyCollider.gameObject))
                        {
                            _enemiesHitThisSwipe.Add(enemyCollider.gameObject);
                            _hasHitEnemy = true;
                            
                            // Calculate and apply damage
                            int damage = CalculateDamage(swipeVelocity, swipeDistance);
                            bool isCritical = IsCriticalHit();
                            ApplyDamageToEnemy(enemyCollider.gameObject, damage, isCritical);
                            
                            if (showDebugInfo)
                            {
                                Debug.Log(string.Format("Enemy hit by swipe: {0}", enemyCollider.name));
                            }

                            // Apply Book of Five Rings mode logic
                            if (ShouldStopAfterThisHit())
                            {
                                if (showDebugInfo)
                                {
                                    Debug.Log(string.Format("{0} mode: Stopping after enemy hit", currentMode));
                                }
                                break; // Exit the loop based on mode rules
                            }
                        }
                    }
                }
            }

            // Trigger event
            OnSwipeHitEnemy.Trigger(_hasHitEnemy);
        }

        /// <summary>
        /// Calculate the total distance of the swipe path
        /// </summary>
        private float CalculateSwipeDistance(List<Vector3> worldPath)
        {
            float totalDistance = 0f;
            for (int i = 1; i < worldPath.Count; i++)
            {
                totalDistance += Vector3.Distance(worldPath[i - 1], worldPath[i]);
            }
            return totalDistance;
        }

        /// <summary>
        /// Calculate the average velocity of the swipe
        /// </summary>
        private float CalculateSwipeVelocity(List<Vector3> worldPath)
        {
            if (worldPath.Count < 2) return 0f;
            
            float totalDistance = CalculateSwipeDistance(worldPath);
            float timeSpan = 0.1f; // Assume swipe takes 0.1 seconds
            
            return totalDistance / timeSpan;
        }

        private bool IsEnemyNearSwipePath(Vector3 enemyPosition, List<Vector3> swipePath)
        {
            if (swipePath.Count < 2) return false;

            // Use the configurable threshold for more precise hit detection
            float minDistance = enemyHitThreshold;

            for (int i = 1; i < swipePath.Count; i++)
            {
                Vector3 lineStart = swipePath[i - 1];
                Vector3 lineEnd = swipePath[i];
                
                float distance = DistanceToLine(enemyPosition, lineStart, lineEnd);
                if (distance < minDistance)
                {
                    return true;
                }
            }

            return false;
        }

        private float DistanceToLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineVector = lineEnd - lineStart;
            Vector3 pointVector = point - lineStart;
            
            float lineLength = lineVector.magnitude;
            if (lineLength == 0) return Vector3.Distance(point, lineStart);
            
            Vector3 normalizedLineVector = lineVector / lineLength;
            float projection = Vector3.Dot(pointVector, normalizedLineVector);
            
            if (projection < 0)
            {
                return Vector3.Distance(point, lineStart);
            }
            else if (projection > lineLength)
            {
                return Vector3.Distance(point, lineEnd);
            }
            else
            {
                Vector3 closestPoint = lineStart + normalizedLineVector * projection;
                return Vector3.Distance(point, closestPoint);
            }
        }

        private IEnumerator ActionCancelTimer()
        {
            yield return new WaitForSeconds(actionCancelDelay);
            
            // If no enemies were hit, cancel the action
            if (!_hasHitEnemy && _isSwipeActive)
            {
                CancelHeroAction();
            }
            
            _isSwipeActive = false;
        }

        private void CancelHeroAction()
        {
            if (_hero == null) return;

            if (showDebugInfo)
            {
                Debug.Log("No enemies hit - cancelling hero action!");
            }

            // Stop current attack animation
            if (_heroAnimator != null)
            {
                _heroAnimator.SetTrigger("heroCancel");
                _heroAnimator.SetBool("heroAttack", false);
            }

            // Reset hero state
            if (_hero != null)
            {
                _hero.IsAttacking = false;
            }

            // Remove slash effects
            if (enhancedSlashRenderer != null)
            {
                enhancedSlashRenderer.ClearSlash();
            }
            else if (originalSlashRenderer != null)
            {
                originalSlashRenderer.RemoveSlash();
            }

            // Trigger observable event
            OnActionCancelled.Trigger();
        }

        // Public methods for external access
        public bool IsSwipeActive() { return _isSwipeActive; }
        public bool HasHitEnemy() { return _hasHitEnemy; }
        public List<GameObject> GetEnemiesHitThisSwipe() { return new List<GameObject>(_enemiesHitThisSwipe); }

        // Method to manually cancel current action
        public void CancelCurrentAction()
        {
            CancelHeroAction();
        }

        // Method to check if hero can perform actions
        public bool CanPerformAction()
        {
            if (_hero == null) return false;
            return !_hero.IsAttacking && !_isSwipeActive;
        }

        // Public method to adjust hit threshold at runtime
        public void SetEnemyHitThreshold(float threshold)
        {
            enemyHitThreshold = Mathf.Clamp(threshold, 0.1f, 2.0f);
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Enemy hit threshold set to: {0:F2}", enemyHitThreshold));
            }
        }

        // Public method to get current hit threshold
        public float GetEnemyHitThreshold()
        {
            return enemyHitThreshold;
        }

        // Public method to adjust detection radius at runtime
        public void SetEnemyDetectionRadius(float radius)
        {
            enemyDetectionRadius = Mathf.Clamp(radius, 0.5f, 5.0f);
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Enemy detection radius set to: {0:F2}", enemyDetectionRadius));
            }
        }

        // Powerup mode control methods
        // public void EnablePowerupMode() // This method is removed as powerupMode is removed
        // {
        //     powerupMode = true;
        //     if (showDebugInfo)
        //     {
        //         Debug.Log("Powerup mode enabled - can hit multiple enemies per swipe!");
        //     }
        // }

        // public void DisablePowerupMode() // This method is removed as powerupMode is removed
        // {
        //     powerupMode = false;
        //     if (showDebugInfo)
        //     {
        //         Debug.Log("Powerup mode disabled - can only hit one enemy per swipe");
        //     }
        // }

        // public void TogglePowerupMode() // This method is removed as powerupMode is removed
        // {
        //     powerupMode = !powerupMode;
        //     if (showDebugInfo)
        //     {
        //         Debug.Log(string.Format("Powerup mode {0}", powerupMode ? "enabled" : "disabled"));
        //     }
        // }

        // public bool IsPowerupModeActive() // This method is removed as powerupMode is removed
        // {
        //     return powerupMode;
        // }

        // Get information about current swipe hits
        public int GetEnemiesHitCount()
        {
            return _enemiesHitThisSwipe.Count;
        }

        /// <summary>
        /// Determines if the hero should stop hitting enemies based on the current Book of Five Rings mode
        /// </summary>
        private bool ShouldStopAfterThisHit()
        {
            switch (currentMode)
            {
                case BookOfFiveRingsMode.Earth:
                    // 地 - Chi: Foundation, basic techniques, single enemy focus
                    // Stop after hitting 1 enemy (basic technique)
                    return GetEnemiesHitCount() >= 1;
                
                case BookOfFiveRingsMode.Water:
                    // 水 - Sui: Adaptability, flowing movements, multiple enemies
                    // Can hit up to 3 enemies (flowing technique)
                    return GetEnemiesHitCount() >= 3;
                
                case BookOfFiveRingsMode.Fire:
                    // 火 - Ka: Aggressive, fast attacks, chain hits
                    // Can hit up to 5 enemies (aggressive technique)
                    return GetEnemiesHitCount() >= 5;
                
                case BookOfFiveRingsMode.Wind:
                    // 風 - Fu: Speed and precision, critical hits
                    // Can hit up to 7 enemies (precision technique)
                    return GetEnemiesHitCount() >= 7;
                
                case BookOfFiveRingsMode.Void:
                    // 空 - Ku: Mastery, ultimate technique, unlimited potential
                    // Can hit unlimited enemies (master technique)
                    return false;
                
                default:
                    return GetEnemiesHitCount() >= 1; // Default to Earth mode behavior
            }
        }

        // public bool CanHitMultipleEnemies() // This method is removed as powerupMode is removed
        // {
        //     return powerupMode;
        // }

        // Book of Five Rings mode control methods
        public void SetMode(BookOfFiveRingsMode mode)
        {
            // Remove void mode effects if switching away from void mode
            if (currentMode == BookOfFiveRingsMode.Void && mode != BookOfFiveRingsMode.Void)
            {
                RemoveVoidModeEffects();
            }

            currentMode = mode;
            
            // Automatically tint the hero sprite for the new mode
            TintHeroSpriteForMode();

            // Automatically apply background tint for the new mode
            ApplyBackgroundTint();

            // Apply void mode effects if switching to void mode
            if (mode == BookOfFiveRingsMode.Void)
            {
                ApplyVoidModeEffects();
            }
            
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Switched to {0} mode - {1}", mode, GetModeDescription(mode)));
            }
        }

        public BookOfFiveRingsMode GetCurrentMode()
        {
            return currentMode;
        }

        public string GetModeDescription(BookOfFiveRingsMode mode)
        {
            switch (mode)
            {
                case BookOfFiveRingsMode.Earth:
                    return "Foundation techniques - Defensive, lower damage, single enemy focus";
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

        public int GetMaxEnemiesForCurrentMode()
        {
            switch (currentMode)
            {
                case BookOfFiveRingsMode.Earth: return 1;
                case BookOfFiveRingsMode.Water: return 3;
                case BookOfFiveRingsMode.Fire: return 5;
                case BookOfFiveRingsMode.Wind: return 7;
                case BookOfFiveRingsMode.Void: return int.MaxValue;
                default: return 1;
            }
        }

        public void CycleToNextMode()
        {
            int currentIndex = (int)currentMode;
            int nextIndex = (currentIndex + 1) % 5; // 5 modes total
            SetMode((BookOfFiveRingsMode)nextIndex);
        }

        public void CycleToPreviousMode()
        {
            int currentIndex = (int)currentMode;
            int prevIndex = (currentIndex - 1 + 5) % 5; // 5 modes total, handle negative wrap
            SetMode((BookOfFiveRingsMode)prevIndex);
        }

        // Combat stats and health methods
        public int GetHeroCurrentHealth() { return heroCurrentHealth; }
        public int GetHeroMaxHealth() { return heroMaxHealth; }
        public float GetHeroHealthPercentage() { return (float)heroCurrentHealth / heroMaxHealth; }
        public int GetHeroBaseAttackPower() { return heroBaseAttackPower; }
        public int GetTotalDamageDealt() { return _totalDamageDealt; }
        public int GetCriticalHits() { return _criticalHits; }
        public int GetTotalHits() { return _totalHits; }
        public float GetCriticalHitRate() { return _totalHits > 0 ? (float)_criticalHits / _totalHits : 0f; }

        /// <summary>
        /// Get enemy health information
        /// </summary>
        public EnemyHealth GetEnemyHealth(GameObject enemy)
        {
            if (_enemyHealthMap.ContainsKey(enemy))
                return _enemyHealthMap[enemy];
            return null;
        }

        /// <summary>
        /// Get all tracked enemies
        /// </summary>
        public List<GameObject> GetTrackedEnemies()
        {
            return new List<GameObject>(_enemyHealthMap.Keys);
        }

        /// <summary>
        /// Get living enemies
        /// </summary>
        public List<GameObject> GetLivingEnemies()
        {
            List<GameObject> livingEnemies = new List<GameObject>();
            foreach (var kvp in _enemyHealthMap)
            {
                if (!kvp.Value.isDead)
                    livingEnemies.Add(kvp.Key);
            }
            return livingEnemies;
        }

        /// <summary>
        /// Get dead enemies
        /// </summary>
        public List<GameObject> GetDeadEnemies()
        {
            List<GameObject> deadEnemies = new List<GameObject>();
            foreach (var kvp in _enemyHealthMap)
            {
                if (kvp.Value.isDead)
                    deadEnemies.Add(kvp.Key);
            }
            return deadEnemies;
        }

        /// <summary>
        /// Heal the hero
        /// </summary>
        public void HealHero(int healAmount)
        {
            heroCurrentHealth = Mathf.Min(heroCurrentHealth + healAmount, heroMaxHealth);
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Hero healed for {0}! Health: {1}/{2}", healAmount, heroCurrentHealth, heroMaxHealth));
            }
        }

        /// <summary>
        /// Take damage (for when hero gets hit)
        /// </summary>
        public void TakeDamage(int damage)
        {
            heroCurrentHealth = Mathf.Max(heroCurrentHealth - damage, 0);
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Hero took {0} damage! Health: {1}/{2}", damage, heroCurrentHealth, heroMaxHealth));
            }
        }

        /// <summary>
        /// Reset all combat stats
        /// </summary>
        public void ResetCombatStats()
        {
            _totalDamageDealt = 0;
            _criticalHits = 0;
            _totalHits = 0;
            _enemyHealthMap.Clear();
            
            if (showDebugInfo)
            {
                Debug.Log("Combat stats reset");
            }
        }

        // Context menu methods for testing
        [ContextMenu("Show Combat Stats")]
        public void ShowCombatStats()
        {
            Debug.Log("=== Hero Combat Stats ===");
            Debug.Log(string.Format("Health: {0}/{1} ({2:F1}%)", heroCurrentHealth, heroMaxHealth, GetHeroHealthPercentage()));
            Debug.Log(string.Format("Base Attack Power: {0}", heroBaseAttackPower));
            Debug.Log(string.Format("Total Damage Dealt: {0}", _totalDamageDealt));
            Debug.Log(string.Format("Total Hits: {0}", _totalHits));
            Debug.Log(string.Format("Critical Hits: {0}", _criticalHits));
            Debug.Log(string.Format("Critical Hit Rate: {0:F1}%", GetCriticalHitRate() * 100f));
            Debug.Log(string.Format("Current Mode: {0}", currentMode));
            Debug.Log(string.Format("Mode Damage Multiplier: {0:F1}x", GetModeDamageMultiplier()));
        }

        [ContextMenu("Show Enemy Status")]
        public void ShowEnemyStatus()
        {
            Debug.Log("=== Enemy Status ===");
            Debug.Log(string.Format("Tracked Enemies: {0}", _enemyHealthMap.Count));
            Debug.Log(string.Format("Living Enemies: {0}", GetLivingEnemies().Count));
            Debug.Log(string.Format("Dead Enemies: {0}", GetDeadEnemies().Count));
            
            foreach (var kvp in _enemyHealthMap)
            {
                if (kvp.Key == null) continue;
                EnemyHealth health = kvp.Value;
                string status = health.isDead ? "DEAD" : "ALIVE";
                Debug.Log(string.Format("{0}: {1}/{2} HP ({3}) - {4} hits", 
                    kvp.Key.name, health.currentHealth, health.maxHealth, status, health.hitCount));
            }
        }

        [ContextMenu("Test Damage System")]
        public void TestDamageSystem()
        {
            Debug.Log("=== Testing Damage System ===");
            Debug.Log(string.Format("Base Attack Power: {0}", heroBaseAttackPower));
            Debug.Log(string.Format("Earth Mode Damage: {0} (40% less - Defensive)", CalculateDamage(1000f, 200f)));
            Debug.Log(string.Format("Water Mode Damage: {0} (20% more)", CalculateDamage(1000f, 200f)));
            Debug.Log(string.Format("Fire Mode Damage: {0} (50% more)", CalculateDamage(1000f, 200f)));
            Debug.Log(string.Format("Wind Mode Damage: {0} (80% more)", CalculateDamage(1000f, 200f)));
            Debug.Log(string.Format("Void Mode Damage: {0} (150% more)", CalculateDamage(1000f, 200f)));
            
            Debug.Log(string.Format("High Velocity Damage: {0}", CalculateDamage(2500f, 200f)));
            Debug.Log(string.Format("Long Distance Damage: {0}", CalculateDamage(1000f, 600f)));
        }

        [ContextMenu("Heal Hero")]
        public void HealHeroContext()
        {
            HealHero(25);
        }

        [ContextMenu("Take Damage")]
        public void TakeDamageContext()
        {
            TakeDamage(15);
        }

        [ContextMenu("Test Background Tinting")]
        public void TestBackgroundTinting()
        {
            Debug.Log("=== Testing Background Tinting ===");
            Debug.Log(string.Format("Current Mode: {0}", currentMode));
            Debug.Log(string.Format("Target Background Color: {0}", _targetBackgroundColor));
            Debug.Log(string.Format("Current Background Color: {0}", _currentBackgroundColor));
            Debug.Log(string.Format("Camera Found: {0}", _mainCamera != null));
            
            if (_mainCamera != null)
            {
                Debug.Log(string.Format("Camera Background Color: {0}", _mainCamera.backgroundColor));
            }
        }

        [ContextMenu("Force Background Tint")]
        public void ForceBackgroundTint()
        {
            ApplyBackgroundTint();
        }

        [ContextMenu("Reset Background Tint")]
        public void ResetBackgroundTintContext()
        {
            ResetBackgroundTint();
        }

        [ContextMenu("Test Void Mode Effects")]
        public void TestVoidModeEffects()
        {
            Debug.Log("=== Testing Void Mode Effects ===");
            Debug.Log(string.Format("Void Mode Effects Enabled: {0}", enableVoidModeEffects));
            Debug.Log(string.Format("Current Mode: {0}", currentMode));
            Debug.Log(string.Format("Void Mode Active: {0}", _voidModeActive));
            Debug.Log(string.Format("Enemy Slow Factor: {0:F1}%", voidModeEnemySlowFactor * 100f));
            Debug.Log(string.Format("Effect Radius: {0:F1}", voidModeEffectRadius));
            Debug.Log(string.Format("Tracked Enemies: {0}", _enemyMovementMap.Count));
            
            int slowedEnemies = 0;
            foreach (var kvp in _enemyMovementMap)
            {
                if (kvp.Value.isSlowed) slowedEnemies++;
            }
            Debug.Log(string.Format("Slowed Enemies: {0}", slowedEnemies));
        }

        [ContextMenu("Force Apply Void Effects")]
        public void ForceApplyVoidEffects()
        {
            if (currentMode == BookOfFiveRingsMode.Void)
            {
                ApplyVoidModeEffects();
            }
            else
            {
                Debug.LogWarning("Must be in Void mode to apply void effects!");
            }
        }

        [ContextMenu("Force Remove Void Effects")]
        public void ForceRemoveVoidEffects()
        {
            RemoveVoidModeEffects();
        }

        [ContextMenu("Show Enemy Movement Status")]
        public void ShowEnemyMovementStatus()
        {
            Debug.Log("=== Enemy Movement Status ===");
            Debug.Log(string.Format("Total Tracked Enemies: {0}", _enemyMovementMap.Count));
            
            foreach (var kvp in _enemyMovementMap)
            {
                if (kvp.Key == null) continue;
                EnemyMovementData data = kvp.Value;
                string status = data.isSlowed ? "SLOWED" : "NORMAL";
                string componentType = data.movementComponent != null ? data.movementComponent.GetType().Name : "NONE";
                Debug.Log(string.Format("{0}: {1} - {2} (Original Speed: {3:F1})", 
                    kvp.Key.name, status, componentType, data.originalSpeed));
            }
        }

        [ContextMenu("Test Earth Mode Combat")]
        public void TestEarthModeCombat()
        {
            Debug.Log("=== Earth Mode Combat Test ===");
            Debug.Log(string.Format("Current Mode: {0}", currentMode));
            
            if (currentMode == BookOfFiveRingsMode.Earth)
            {
                int baseDamage = Mathf.RoundToInt(heroBaseAttackPower * 0.6f);
                Debug.Log(string.Format("Earth Mode Base Damage: {0} (vs normal {1})", baseDamage, heroBaseAttackPower));
                
                // Calculate hits needed for different enemy types
                int standardEnemyHits = Mathf.CeilToInt(60f / baseDamage);
                int eliteEnemyHits = Mathf.CeilToInt(100f / baseDamage);
                int bossEnemyHits = Mathf.CeilToInt(200f / baseDamage);
                
                Debug.Log(string.Format("Standard Enemy (60 HP): {0} hits to kill", standardEnemyHits));
                Debug.Log(string.Format("Elite Enemy (100 HP): {0} hits to kill", eliteEnemyHits));
                Debug.Log(string.Format("Boss Enemy (200 HP): {0} hits to kill", bossEnemyHits));
                Debug.Log("Earth mode emphasizes defensive play and endurance!");
            }
            else
            {
                Debug.Log("Switch to Earth mode to test defensive combat!");
            }
        }

        /// <summary>
        /// Get the current mode's damage multiplier
        /// </summary>
        private float GetModeDamageMultiplier()
        {
            switch (currentMode)
            {
                case BookOfFiveRingsMode.Earth: return 0.6f;
                case BookOfFiveRingsMode.Water: return 1.2f;
                case BookOfFiveRingsMode.Fire: return 1.5f;
                case BookOfFiveRingsMode.Wind: return 1.8f;
                case BookOfFiveRingsMode.Void: return 2.5f;
                default: return 1.0f;
            }
        }

        /// <summary>
        /// Apply void mode effects to enemies (slow movement)
        /// </summary>
        public void ApplyVoidModeEffects()
        {
            if (!enableVoidModeEffects || currentMode != BookOfFiveRingsMode.Void) return;

            _voidModeActive = true;
            
            if (showDebugInfo)
            {
                Debug.Log("Void mode effects activated - enemies will move slower!");
            }

            // Find and slow all enemies in range
            SlowEnemiesInRange();
        }

        /// <summary>
        /// Remove void mode effects from enemies (restore normal speed)
        /// </summary>
        public void RemoveVoidModeEffects()
        {
            if (!enableVoidModeEffects) return;

            _voidModeActive = false;
            
            if (showDebugInfo)
            {
                Debug.Log("Void mode effects deactivated - enemies restored to normal speed!");
            }

            // Restore all enemies to normal speed
            RestoreEnemyMovement();
        }

        /// <summary>
        /// Slow enemies within the void mode effect radius
        /// </summary>
        private void SlowEnemiesInRange()
        {
            // Find all enemies in the effect radius
            Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, voidModeEffectRadius, enemyLayerMask);
            
            foreach (Collider2D enemyCollider in enemiesInRange)
            {
                if (enemyCollider.CompareTag("Enemy") || enemyCollider.CompareTag("NPC"))
                {
                    SlowEnemy(enemyCollider.gameObject);
                }
            }
        }

        /// <summary>
        /// Slow a specific enemy's movement
        /// </summary>
        private void SlowEnemy(GameObject enemy)
        {
            if (enemy == null) return;

            // Check if we're already tracking this enemy
            if (!_enemyMovementMap.ContainsKey(enemy))
            {
                // Try to find movement components and initialize tracking
                InitializeEnemyMovementTracking(enemy);
            }

            if (_enemyMovementMap.ContainsKey(enemy))
            {
                EnemyMovementData movementData = _enemyMovementMap[enemy];
                
                if (!movementData.isSlowed)
                {
                    ApplySlowEffect(movementData);
                }
            }
        }

        /// <summary>
        /// Initialize tracking for an enemy's movement components
        /// </summary>
        private void InitializeEnemyMovementTracking(GameObject enemy)
        {
            // Try to find common movement components
            Component movementComponent = null;
            float originalSpeed = 1f;

            // Check for NavMeshAgent (3D navigation)
            var navAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                movementComponent = navAgent;
                originalSpeed = navAgent.speed;
            }
            // Check for Rigidbody2D (2D physics movement)
            else
            {
                var rigidbody2D = enemy.GetComponent<Rigidbody2D>();
                if (rigidbody2D != null)
                {
                    movementComponent = rigidbody2D;
                    originalSpeed = rigidbody2D.velocity.magnitude;
                }
            }

            if (movementComponent != null)
            {
                _enemyMovementMap[enemy] = new EnemyMovementData(enemy, movementComponent, originalSpeed);
                
                if (showDebugInfo)
                {
                    Debug.Log(string.Format("Initialized movement tracking for {0} with speed {1}", enemy.name, originalSpeed));
                }
            }
        }

        /// <summary>
        /// Apply slow effect to an enemy
        /// </summary>
        private void ApplySlowEffect(EnemyMovementData movementData)
        {
            if (movementData.movementComponent == null) return;

            movementData.isSlowed = true;

            // Apply slow effect based on component type
            if (movementData.movementComponent is UnityEngine.AI.NavMeshAgent)
            {
                UnityEngine.AI.NavMeshAgent navAgent = movementData.movementComponent as UnityEngine.AI.NavMeshAgent;
                navAgent.speed = movementData.originalSpeed * voidModeEnemySlowFactor;
            }
            else if (movementData.movementComponent is Rigidbody2D)
            {
                Rigidbody2D rigidbody2D = movementData.movementComponent as Rigidbody2D;
                movementData.originalVelocity = rigidbody2D.velocity;
                rigidbody2D.velocity *= voidModeEnemySlowFactor;
            }

            if (showDebugInfo)
            {
                Debug.Log(string.Format("Slowed enemy {0} to {1:F1}% of original speed", 
                    movementData.enemyObject.name, voidModeEnemySlowFactor * 100f));
            }
        }

        /// <summary>
        /// Restore all enemies to normal movement speed
        /// </summary>
        private void RestoreEnemyMovement()
        {
            foreach (var kvp in _enemyMovementMap)
            {
                if (kvp.Value.isSlowed)
                {
                    RestoreEnemySpeed(kvp.Value);
                }
            }
        }

        /// <summary>
        /// Restore a specific enemy's movement speed
        /// </summary>
        private void RestoreEnemySpeed(EnemyMovementData movementData)
        {
            if (movementData.movementComponent == null) return;

            movementData.isSlowed = false;

            // Restore speed based on component type
            if (movementData.movementComponent is UnityEngine.AI.NavMeshAgent)
            {
                UnityEngine.AI.NavMeshAgent navAgent = movementData.movementComponent as UnityEngine.AI.NavMeshAgent;
                navAgent.speed = movementData.originalSpeed;
            }
            else if (movementData.movementComponent is Rigidbody2D)
            {
                Rigidbody2D rigidbody2D = movementData.movementComponent as Rigidbody2D;
                rigidbody2D.velocity = movementData.originalVelocity;
            }

            if (showDebugInfo)
            {
                Debug.Log(string.Format("Restored enemy {0} to normal speed", movementData.enemyObject.name));
            }
        }

        /// <summary>
        /// Get the background tint color for the current mode
        /// </summary>
        private Color GetModeBackgroundColor(BookOfFiveRingsMode mode)
        {
            Color modeColor = GetModeColor(mode);
            
            // Blend the mode color with the default background color
            Color tintedColor = Color.Lerp(defaultBackgroundColor, modeColor, backgroundTintIntensity);
            
            // Ensure the color isn't too bright (keep it as a background)
            float brightness = (tintedColor.r + tintedColor.g + tintedColor.b) / 3f;
            if (brightness > 0.4f)
            {
                tintedColor = Color.Lerp(tintedColor, defaultBackgroundColor, 0.3f);
            }
            
            return tintedColor;
        }

        /// <summary>
        /// Apply background tint for the current mode
        /// </summary>
        public void ApplyBackgroundTint()
        {
            if (!enableBackgroundTinting || _mainCamera == null) return;

            _targetBackgroundColor = GetModeBackgroundColor(currentMode);
            
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Background tint set to {0} mode color: {1}", currentMode, _targetBackgroundColor));
            }
        }

        /// <summary>
        /// Reset background to default color
        /// </summary>
        public void ResetBackgroundTint()
        {
            if (!enableBackgroundTinting || _mainCamera == null) return;

            _targetBackgroundColor = defaultBackgroundColor;
            
            if (showDebugInfo)
            {
                Debug.Log("Background tint reset to default color");
            }
        }

        /// <summary>
        /// Smoothly transition background color
        /// </summary>
        private void UpdateBackgroundTint()
        {
            if (!enableBackgroundTinting || _mainCamera == null) return;

            // Smoothly interpolate to target color
            _currentBackgroundColor = Color.Lerp(_currentBackgroundColor, _targetBackgroundColor, 
                Time.deltaTime * backgroundTintTransitionSpeed);
            
            // Apply the color to the camera background
            _mainCamera.backgroundColor = _currentBackgroundColor;
        }

        // Debug visualization for hit detection
        private void OnDrawGizmos()
        {
            if (!showDebugInfo) return;

            // Draw enemy detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyDetectionRadius);

            // Draw precise hit threshold area (smaller, more precise)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyHitThreshold);

            // Draw current swipe path if active
            if (_isSwipeActive && _enemiesHitThisSwipe.Count > 0)
            {
                // Show current mode status with different colors
                Gizmos.color = GetModeColor(currentMode);
                
                foreach (var enemy in _enemiesHitThisSwipe)
                {
                    if (enemy != null)
                    {
                        Gizmos.DrawWireSphere(enemy.transform.position, 0.3f);
                    }
                }
            }

            // Draw mode indicator
            Gizmos.color = GetModeColor(currentMode);
            Gizmos.DrawWireSphere(transform.position, enemyDetectionRadius + 0.2f);

            // Draw enemy health bars if combat stats are enabled
            if (showCombatStats)
            {
                DrawEnemyHealthBars();
            }

            // Draw void mode effects if enabled
            if (showVoidModeEffects && currentMode == BookOfFiveRingsMode.Void)
            {
                DrawVoidModeEffects();
            }
        }

        /// <summary>
        /// Draw void mode effect visualizations
        /// </summary>
        private void DrawVoidModeEffects()
        {
            // Draw void mode effect radius
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f); // Semi-transparent magenta
            Gizmos.DrawWireSphere(transform.position, voidModeEffectRadius);

            // Draw slowed enemies
            foreach (var kvp in _enemyMovementMap)
            {
                if (kvp.Value.isSlowed && kvp.Key != null)
                {
                    // Draw slow effect indicator
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(kvp.Key.transform.position, 0.5f);
                    
                    // Draw connection line to hero
                    Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
                    Gizmos.DrawLine(transform.position, kvp.Key.transform.position);
                }
            }
        }

        /// <summary>
        /// Draw health bars above tracked enemies
        /// </summary>
        private void DrawEnemyHealthBars()
        {
            foreach (var kvp in _enemyHealthMap)
            {
                if (kvp.Key == null) continue;

                Vector3 enemyPosition = kvp.Key.transform.position;
                EnemyHealth health = kvp.Value;

                // Draw health bar background
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(enemyPosition + Vector3.up * 1.5f, new Vector3(1f, 0.1f, 0.1f));

                // Draw health bar fill
                float healthPercentage = health.GetHealthPercentage();
                Gizmos.color = Color.green;
                Gizmos.DrawCube(enemyPosition + Vector3.up * 1.5f, new Vector3(healthPercentage, 0.08f, 0.08f));

                // Draw hit count indicator
                if (health.hitCount > 0)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(enemyPosition + Vector3.up * 1.8f, 0.2f);
                }
            }
        }

        /// <summary>
        /// Get the color associated with each Book of Five Rings mode
        /// </summary>
        private Color GetModeColor(BookOfFiveRingsMode mode)
        {
            switch (mode)
            {
                case BookOfFiveRingsMode.Earth:
                    return new Color(0.6f, 0.4f, 0.2f); // Earth brown (RGB: 153, 102, 51)
                case BookOfFiveRingsMode.Water:
                    return Color.blue; // Water blue
                case BookOfFiveRingsMode.Fire:
                    return Color.red; // Fire red
                case BookOfFiveRingsMode.Wind:
                    return Color.cyan; // Wind cyan
                case BookOfFiveRingsMode.Void:
                    return Color.magenta; // Void magenta
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Tint the hero sprite based on the current Book of Five Rings mode
        /// </summary>
        public void TintHeroSpriteForMode()
        {
            if (_hero == null) return;

            SpriteRenderer heroSprite = _hero.GetComponent<SpriteRenderer>();
            if (heroSprite == null) return;

            Color modeColor = GetModeColor(currentMode);
            Color tintedColor = new Color(
                modeColor.r * 0.3f + 0.7f, // Blend with white for subtle tint
                modeColor.g * 0.3f + 0.7f,
                modeColor.b * 0.3f + 0.7f,
                1.0f
            );

            heroSprite.color = tintedColor;

            if (showDebugInfo)
            {
                Debug.Log(string.Format("Hero sprite tinted for {0} mode with color {1}", currentMode, tintedColor));
            }
        }

        /// <summary>
        /// Reset hero sprite to original color
        /// </summary>
        public void ResetHeroSpriteColor()
        {
            if (_hero == null) return;

            SpriteRenderer heroSprite = _hero.GetComponent<SpriteRenderer>();
            if (heroSprite == null) return;

            heroSprite.color = Color.white;

            if (showDebugInfo)
            {
                Debug.Log("Hero sprite color reset to original");
            }
        }

        /// <summary>
        /// Get the tint color for the current mode (useful for UI elements)
        /// </summary>
        public Color GetCurrentModeTintColor()
        {
            Color modeColor = GetModeColor(currentMode);
            return new Color(
                modeColor.r * 0.3f + 0.7f,
                modeColor.g * 0.3f + 0.7f,
                modeColor.b * 0.3f + 0.7f,
                1.0f
            );
        }

        /// <summary>
        /// Get the full mode color (useful for effects and highlights)
        /// </summary>
        public Color GetCurrentModeFullColor()
        {
            return GetModeColor(currentMode);
        }

        /// <summary>
        /// Enemy health data structure
        /// </summary>
        [System.Serializable]
        public class EnemyHealth
        {
            public GameObject enemyObject;
            public int maxHealth;
            public int currentHealth;
            public bool isDead;
            public int hitCount;
            public float lastHitTime;

            public EnemyHealth(GameObject enemy, int health)
            {
                enemyObject = enemy;
                maxHealth = health;
                currentHealth = health;
                isDead = false;
                hitCount = 0;
                lastHitTime = 0f;
            }

            public float GetHealthPercentage()
            {
                return (float)currentHealth / maxHealth;
            }
        }

        /// <summary>
        /// Enemy movement data structure for void mode effects
        /// </summary>
        [System.Serializable]
        public class EnemyMovementData
        {
            public GameObject enemyObject;
            public Component movementComponent; // Could be NavMeshAgent, Rigidbody2D, etc.
            public float originalSpeed;
            public bool isSlowed;
            public Vector3 originalVelocity;

            public EnemyMovementData(GameObject enemy, Component component, float speed)
            {
                enemyObject = enemy;
                movementComponent = component;
                originalSpeed = speed;
                isSlowed = false;
                originalVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Calculate damage based on current mode and swipe properties
        /// </summary>
        private int CalculateDamage(float swipeVelocity, float swipeDistance)
        {
            int baseDamage = heroBaseAttackPower;
            
            // Mode-based damage multipliers
            switch (currentMode)
            {
                case BookOfFiveRingsMode.Earth:
                    baseDamage = Mathf.RoundToInt(heroBaseAttackPower * 0.6f); // 40% less damage - defensive mode
                    break;
                case BookOfFiveRingsMode.Water:
                    baseDamage = Mathf.RoundToInt(heroBaseAttackPower * 1.2f); // 20% more damage
                    break;
                case BookOfFiveRingsMode.Fire:
                    baseDamage = Mathf.RoundToInt(heroBaseAttackPower * 1.5f); // 50% more damage
                    break;
                case BookOfFiveRingsMode.Wind:
                    baseDamage = Mathf.RoundToInt(heroBaseAttackPower * 1.8f); // 80% more damage
                    break;
                case BookOfFiveRingsMode.Void:
                    baseDamage = Mathf.RoundToInt(heroBaseAttackPower * 2.5f); // 150% more damage
                    break;
            }

            // Velocity-based damage bonus
            float velocityBonus = Mathf.Clamp01(swipeVelocity / 2000f); // Max bonus at 2000 velocity
            baseDamage += Mathf.RoundToInt(baseDamage * velocityBonus * 0.5f);

            // Distance-based damage bonus
            float distanceBonus = Mathf.Clamp01(swipeDistance / 500f); // Max bonus at 500 distance
            baseDamage += Mathf.RoundToInt(baseDamage * distanceBonus * 0.3f);

            return baseDamage;
        }

        /// <summary>
        /// Check if attack is a critical hit
        /// </summary>
        private bool IsCriticalHit()
        {
            return UnityEngine.Random.Range(0f, 1f) < criticalHitChance;
        }

        /// <summary>
        /// Apply damage to an enemy
        /// </summary>
        private void ApplyDamageToEnemy(GameObject enemy, int damage, bool isCritical)
        {
            if (!_enemyHealthMap.ContainsKey(enemy))
            {
                // Initialize enemy health if not tracked yet
                int enemyMaxHealth = GetEnemyMaxHealth(enemy);
                _enemyHealthMap[enemy] = new EnemyHealth(enemy, enemyMaxHealth);
            }

            EnemyHealth enemyHealth = _enemyHealthMap[enemy];
            
            if (enemyHealth.isDead) return;

            // Apply damage
            int finalDamage = isCritical ? Mathf.RoundToInt(damage * criticalHitMultiplier) : damage;
            enemyHealth.currentHealth -= finalDamage;
            enemyHealth.hitCount++;
            enemyHealth.lastHitTime = Time.time;

            // Update combat stats
            _totalDamageDealt += finalDamage;
            _totalHits++;
            if (isCritical) _criticalHits++;

            // Check if enemy is dead
            if (enemyHealth.currentHealth <= 0)
            {
                enemyHealth.isDead = true;
                enemyHealth.currentHealth = 0;
                
                if (showDebugInfo)
                {
                    Debug.Log(string.Format("Enemy {0} defeated! Took {1} hits to kill", enemy.name, enemyHealth.hitCount));
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    string criticalText = isCritical ? " CRITICAL!" : "";
                    Debug.Log(string.Format("Hit {0} for {1} damage! Health: {2}/{3}{4}", 
                        enemy.name, finalDamage, enemyHealth.currentHealth, enemyHealth.maxHealth, criticalText));
                }
            }
        }

        /// <summary>
        /// Get the maximum health for an enemy (can be overridden for different enemy types)
        /// </summary>
        private int GetEnemyMaxHealth(GameObject enemy)
        {
            // Default enemy health - can be customized per enemy type
            if (enemy.CompareTag("Boss"))
                return 200;
            else if (enemy.CompareTag("Elite"))
                return 100;
            else
                return 60; // Standard enemy
        }

        /// <summary>
        /// Apply mode-specific visual effects to the hero
        /// </summary>
        public void ApplyModeVisualEffects()
        {
            if (_hero == null) return;

            // Tint the sprite
            TintHeroSpriteForMode();

            // Tint the swipe trail
            TintSwipeTrailForMode();

            // Apply background tint
            ApplyBackgroundTint();

            // Add mode-specific particle effects or other visual enhancements
            switch (currentMode)
            {
                case BookOfFiveRingsMode.Earth:
                    // Earth mode: Subtle brown glow
                    break;
                case BookOfFiveRingsMode.Water:
                    // Water mode: Flowing blue effect
                    break;
                case BookOfFiveRingsMode.Fire:
                    // Fire mode: Fiery red effect
                    break;
                case BookOfFiveRingsMode.Wind:
                    // Wind mode: Swift cyan effect
                    break;
                case BookOfFiveRingsMode.Void:
                    // Void mode: Mystical magenta effect
                    break;
            }
        }

        /// <summary>
        /// Tint the swipe trail to match the current Book of Five Rings mode
        /// </summary>
        public void TintSwipeTrailForMode()
        {
            Color modeColor = GetModeColor(currentMode);
            
            if (showDebugInfo)
            {
                Debug.Log(string.Format("Tinting trails for {0} mode with color {1}", currentMode, modeColor));
            }
            
            // Tint enhanced slash renderer if available
            if (enhancedSlashRenderer != null)
            {
                TintAllTrailsInObject(enhancedSlashRenderer.gameObject, modeColor, "Enhanced Slash");
            }

            // Tint original slash renderer if available
            if (originalSlashRenderer != null)
            {
                TintAllTrailsInObject(originalSlashRenderer.gameObject, modeColor, "Original Slash");
            }

            // Tint swipe detector trail if available
            if (swipeDetector != null)
            {
                TintAllTrailsInObject(swipeDetector.gameObject, modeColor, "Swipe Detector");
            }

            // Also search for any slash-related objects in the scene
            SearchAndTintSlashTrails(modeColor);
        }

        /// <summary>
        /// Tint all trail components in a specific GameObject and its children
        /// </summary>
        private void TintAllTrailsInObject(GameObject obj, Color modeColor, string objectName)
        {
            if (obj == null) return;

            // Tint TrailRenderer components
            TrailRenderer[] trails = obj.GetComponentsInChildren<TrailRenderer>();
            foreach (TrailRenderer trail in trails)
            {
                trail.startColor = modeColor;
                trail.endColor = new Color(modeColor.r, modeColor.g, modeColor.b, 0.3f);
                
                if (showDebugInfo)
                {
                    Debug.Log(string.Format("Tinted TrailRenderer on {0}: {1}", objectName, trail.name));
                }
            }

            // Tint LineRenderer components
            LineRenderer[] lines = obj.GetComponentsInChildren<LineRenderer>();
            foreach (LineRenderer line in lines)
            {
                line.startColor = modeColor;
                line.endColor = new Color(modeColor.r, modeColor.g, modeColor.b, 0.5f);
                
                if (showDebugInfo)
                {
                    Debug.Log(string.Format("Tinted LineRenderer on {0}: {1}", objectName, line.name));
                }
            }

            // Tint ParticleSystem components (for slash effects)
            ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                var main = particle.main;
                main.startColor = modeColor;
                
                if (showDebugInfo)
                {
                    Debug.Log(string.Format("Tinted ParticleSystem on {0}: {1}", objectName, particle.name));
                }
            }
        }

        /// <summary>
        /// Search the scene for slash-related objects and tint their trails
        /// </summary>
        private void SearchAndTintSlashTrails(Color modeColor)
        {
            // Search for objects with "slash", "trail", or "swipe" in their names
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("slash") || 
                    obj.name.ToLower().Contains("trail") || 
                    obj.name.ToLower().Contains("swipe") ||
                    obj.name.ToLower().Contains("effect"))
                {
                    // Check if it has trail components
                    TrailRenderer trail = obj.GetComponent<TrailRenderer>();
                    LineRenderer line = obj.GetComponent<LineRenderer>();
                    ParticleSystem particle = obj.GetComponent<ParticleSystem>();
                    
                    if (trail != null || line != null || particle != null)
                    {
                        TintAllTrailsInObject(obj, modeColor, "Found Slash Object");
                    }
                }
            }
        }

        /// <summary>
        /// Reset swipe trail to original colors
        /// </summary>
        public void ResetSwipeTrailColors()
        {
            Color originalColor = Color.white;
            
            // Reset enhanced slash renderer
            if (enhancedSlashRenderer != null)
            {
                TrailRenderer trail = enhancedSlashRenderer.GetComponent<TrailRenderer>();
                if (trail != null)
                {
                    trail.startColor = originalColor;
                    trail.endColor = new Color(1f, 1f, 1f, 0.3f);
                }
                
                LineRenderer line = enhancedSlashRenderer.GetComponent<LineRenderer>();
                if (line != null)
                {
                    line.startColor = originalColor;
                    line.endColor = new Color(1f, 1f, 1f, 0.5f);
                }
            }

            // Reset original slash renderer
            if (originalSlashRenderer != null)
            {
                TrailRenderer trail = originalSlashRenderer.GetComponent<TrailRenderer>();
                if (trail != null)
                {
                    trail.startColor = originalColor;
                    trail.endColor = new Color(1f, 1f, 1f, 0.3f);
                }
                
                LineRenderer line = originalSlashRenderer.GetComponent<LineRenderer>();
                if (line != null)
                {
                    line.startColor = originalColor;
                    line.endColor = new Color(1f, 1f, 1f, 0.5f);
                }
            }

            // Reset swipe detector
            if (swipeDetector != null)
            {
                TrailRenderer trail = swipeDetector.GetComponent<TrailRenderer>();
                if (trail != null)
                {
                    trail.startColor = originalColor;
                    trail.endColor = new Color(1f, 1f, 1f, 0.2f);
                }
                
                LineRenderer line = swipeDetector.GetComponent<LineRenderer>();
                if (line != null)
                {
                    line.startColor = originalColor;
                    line.endColor = new Color(1f, 1f, 1f, 0.4f);
                }
            }

            if (showDebugInfo)
            {
                Debug.Log("Swipe trail colors reset to original");
            }
        }

        /// <summary>
        /// Reset all visual effects to original state
        /// </summary>
        public void ResetAllVisualEffects()
        {
            ResetHeroSpriteColor();
            ResetSwipeTrailColors();
            
            if (showDebugInfo)
            {
                Debug.Log("All visual effects reset to original");
            }
        }

        /// <summary>
        /// Refresh trail colors during gameplay (useful for dynamic effects)
        /// </summary>
        public void RefreshTrailColors()
        {
            TintSwipeTrailForMode();
        }

        /// <summary>
        /// Force refresh all trail colors and log detailed information
        /// </summary>
        public void ForceRefreshTrailColors()
        {
            if (showDebugInfo)
            {
                Debug.Log("=== Force Refreshing Trail Colors ===");
                Debug.Log(string.Format("Current Mode: {0}", currentMode));
                Debug.Log(string.Format("Mode Color: {0}", GetModeColor(currentMode)));
            }

            // Force refresh the tinting
            TintSwipeTrailForMode();

            // Also try to find any red trails that might be overriding our colors
            FindRedTrails();
        }

        /// <summary>
        /// Debug method to find any red trails that might be overriding our colors
        /// </summary>
        private void FindRedTrails()
        {
            if (!showDebugInfo) return;

            TrailRenderer[] allTrails = FindObjectsOfType<TrailRenderer>();
            LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
            
            Debug.Log(string.Format("Found {0} TrailRenderers and {1} LineRenderers in scene", allTrails.Length, allLines.Length));
            
            foreach (TrailRenderer trail in allTrails)
            {
                if (trail.startColor.r > 0.8f && trail.startColor.g < 0.3f && trail.startColor.b < 0.3f)
                {
                    Debug.LogWarning(string.Format("Found RED TrailRenderer: {0} on {1}", trail.name, trail.gameObject.name));
                }
            }
            
            foreach (LineRenderer line in allLines)
            {
                if (line.startColor.r > 0.8f && line.startColor.g < 0.3f && line.startColor.b < 0.3f)
                {
                    Debug.LogWarning(string.Format("Found RED LineRenderer: {0} on {1}", line.name, line.gameObject.name));
                }
            }
        }
    }
} 