using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Automatically tints a sprite based on the current Book of Five Rings mode
    /// Place this script on any GameObject with a SpriteRenderer to make it change color with the mode
    /// </summary>
    public class ModeBasedSpriteTinter : MonoBehaviour
    {
        [Header("Tinting Settings")]
        [SerializeField] private bool enableTinting = true;
        [SerializeField] private float tintIntensity = 0.9f; // How strong the tint effect is (0 = no tint, 1 = full color)
        [SerializeField] private bool preserveAlpha = true; // Whether to keep the original alpha value
        [SerializeField] private bool showDebugInfo = false;
        
        [Header("Mode Override")]
        [SerializeField] private bool useCustomMode = false; // If true, use customMode instead of following HeroSwipeController
        [SerializeField] private BookOfFiveRingsMode customMode = BookOfFiveRingsMode.Earth;
        
        [Header("Animation")]
        [SerializeField] private bool enableSmoothTransitions = true;
        [SerializeField] private float transitionSpeed = 2.0f; // How fast the color changes
        
        // Component references
        private SpriteRenderer _spriteRenderer;
        private HeroSwipeController _heroSwipeController;
        
        // Color tracking
        private Color _originalColor;
        private Color _targetColor;
        private Color _currentColor;
        private BookOfFiveRingsMode _lastMode;
        
        void Awake()
        {
            SetupComponents();
            SetupInitialColors();
        }
        
        void Start()
        {
            FindHeroSwipeController();
            ApplyModeTint();
        }
        
        void Update()
        {
            if (!enableTinting) return;
            
            // Check if mode has changed
            BookOfFiveRingsMode currentMode = GetCurrentMode();
            if (currentMode != _lastMode)
            {
                _lastMode = currentMode;
                ApplyModeTint();
            }
            
            // Smooth color transitions
            if (enableSmoothTransitions)
            {
                UpdateColorTransition();
            }
        }
        
        private void SetupComponents()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                // Debug.LogError(string.Format("ModeBasedSpriteTinter on {0}: No SpriteRenderer found!", gameObject.name));
                enabled = false;
                return;
            }
        }
        
        private void SetupInitialColors()
        {
            _originalColor = _spriteRenderer.color;
            _currentColor = _originalColor;
            _targetColor = _originalColor;
        }
        
        private void FindHeroSwipeController()
        {
            if (useCustomMode) return; // Don't need to find controller if using custom mode
            
            _heroSwipeController = FindObjectOfType<HeroSwipeController>();
            if (_heroSwipeController == null)
            {
                // Debug.LogWarning(string.Format("ModeBasedSpriteTinter on {0}: No HeroSwipeController found in scene! Using custom mode.", gameObject.name));
                useCustomMode = true;
            }
        }
        
        private BookOfFiveRingsMode GetCurrentMode()
        {
            if (useCustomMode)
            {
                return customMode;
            }
            
            if (_heroSwipeController != null)
            {
                return _heroSwipeController.GetCurrentMode();
            }
            
            return BookOfFiveRingsMode.Earth; // Default fallback
        }
        
        private void ApplyModeTint()
        {
            if (!enableTinting || _spriteRenderer == null) return;
            
            BookOfFiveRingsMode currentMode = GetCurrentMode();
            Color modeColor = GetModeColor(currentMode);
            
            // Calculate tinted color
            _targetColor = Color.Lerp(_originalColor, modeColor, tintIntensity);
            
            // Preserve alpha if requested
            if (preserveAlpha)
            {
                _targetColor.a = _originalColor.a;
            }
            
            // Apply immediately if smooth transitions are disabled
            if (!enableSmoothTransitions)
            {
                _currentColor = _targetColor;
                _spriteRenderer.color = _currentColor;
            }
            
            if (showDebugInfo)
            {
                // Debug.Log(string.Format("ModeBasedSpriteTinter on {0}: Applied {1} mode tint. Target color: {2}", gameObject.name, currentMode, _targetColor));
            }
        }
        
        private void UpdateColorTransition()
        {
            if (ColorUtility.ToHtmlStringRGB(_currentColor) != ColorUtility.ToHtmlStringRGB(_targetColor))
            {
                _currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * transitionSpeed);
                _spriteRenderer.color = _currentColor;
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
                    return new Color(0.1f, 0.05f, 0.15f); // Void almost-black (RGB: 26, 13, 38)
                default:
                    return Color.white;
            }
        }
        
        /// <summary>
        /// Manually set the tint intensity at runtime
        /// </summary>
        public void SetTintIntensity(float intensity)
        {
            tintIntensity = Mathf.Clamp01(intensity);
            ApplyModeTint();
        }
        
        /// <summary>
        /// Manually set a custom mode for this sprite
        /// </summary>
        public void SetCustomMode(BookOfFiveRingsMode mode)
        {
            customMode = mode;
            useCustomMode = true;
            ApplyModeTint();
        }
        
        /// <summary>
        /// Make this sprite follow the HeroSwipeController mode again
        /// </summary>
        public void FollowHeroMode()
        {
            useCustomMode = false;
            FindHeroSwipeController();
            ApplyModeTint();
        }
        
        /// <summary>
        /// Reset the sprite to its original color
        /// </summary>
        public void ResetToOriginalColor()
        {
            _currentColor = _originalColor;
            _targetColor = _originalColor;
            _spriteRenderer.color = _originalColor;
        }
        
        /// <summary>
        /// Force refresh the tint immediately
        /// </summary>
        public void ForceRefreshTint()
        {
            ApplyModeTint();
        }
        
        /// <summary>
        /// Get the current tint intensity
        /// </summary>
        public float GetTintIntensity()
        {
            return tintIntensity;
        }
        
        /// <summary>
        /// Get whether this sprite is using a custom mode
        /// </summary>
        public bool IsUsingCustomMode()
        {
            return useCustomMode;
        }
        
        /// <summary>
        /// Get the current mode being applied to this sprite
        /// </summary>
        public BookOfFiveRingsMode GetAppliedMode()
        {
            return GetCurrentMode();
        }
        
        // Context menu methods for testing
        [ContextMenu("Test All Modes")]
        public void TestAllModes()
        {
            // Debug.Log("=== Testing All Book of Five Rings Modes ===");
            
            BookOfFiveRingsMode[] allModes = { 
                BookOfFiveRingsMode.Earth, 
                BookOfFiveRingsMode.Water, 
                BookOfFiveRingsMode.Fire, 
                BookOfFiveRingsMode.Wind, 
                BookOfFiveRingsMode.Void 
            };
            
            foreach (BookOfFiveRingsMode mode in allModes)
            {
                customMode = mode;
                useCustomMode = true;
                ApplyModeTint();
                
                // Debug.Log(string.Format("Applied {0} mode - Color: {1}", mode, _targetColor));
                
                // Wait a moment to see the effect
                System.Threading.Thread.Sleep(500);
            }
            
            // Reset to follow hero mode
            FollowHeroMode();
        }
        
        [ContextMenu("Cycle Through Modes")]
        public void CycleThroughModes()
        {
            int currentIndex = (int)customMode;
            int nextIndex = (currentIndex + 1) % 5; // 5 modes total
            customMode = (BookOfFiveRingsMode)nextIndex;
            useCustomMode = true;
            ApplyModeTint();
            
            // Debug.Log(string.Format("Cycled to {0} mode", customMode));
        }
        
        [ContextMenu("Toggle Tinting")]
        public void ToggleTinting()
        {
            enableTinting = !enableTinting;
            
            if (!enableTinting)
            {
                ResetToOriginalColor();
            }
            else
            {
                ApplyModeTint();
            }
            
            // Debug.Log(string.Format("Tinting {0}", (enableTinting ? "enabled" : "disabled")));
        }
        
        [ContextMenu("Show Current Status")]
        public void ShowCurrentStatus()
        {
            // Debug.Log("=== ModeBasedSpriteTinter Status ===");
            // Debug.Log(string.Format("GameObject: {0}", gameObject.name));
            // Debug.Log(string.Format("Tinting Enabled: {0}", enableTinting));
            // Debug.Log(string.Format("Tint Intensity: {0}", tintIntensity));
            // Debug.Log(string.Format("Using Custom Mode: {0}", useCustomMode));
            // Debug.Log(string.Format("Current Mode: {0}", GetCurrentMode()));
            // Debug.Log(string.Format("Original Color: {0}", _originalColor));
            // Debug.Log(string.Format("Current Color: {0}", _currentColor));
            // Debug.Log(string.Format("Target Color: {0}", _targetColor));
            // Debug.Log(string.Format("Smooth Transitions: {0}", enableSmoothTransitions));
            // Debug.Log(string.Format("Transition Speed: {0}", transitionSpeed));
        }
    }
}
