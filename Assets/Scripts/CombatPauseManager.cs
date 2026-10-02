using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Global combat-pause window. Call TriggerPause(duration) from any attack that
    /// warrants a dramatic freeze (e.g. dash attack).
    ///
    /// During a pause:
    ///   - Enemy.Attack() is gated and returns immediately (no counter-attack).
    ///   - Enemy movement / repositioning is NOT affected.
    ///   - Any number of systems can subscribe to OnPauseBegin / OnPauseEnd to chain
    ///     additional effects (screen flash, slow-mo, camera shake, XP pop, etc.).
    /// </summary>
    public class CombatPauseManager : MonoBehaviour
    {
        public static CombatPauseManager Instance;

        /// <summary>True while a pause window is active.</summary>
        public bool IsPaused { get; private set; }

        /// <summary>Fired when a pause begins. Argument is the requested duration in seconds.</summary>
        public event System.Action<float> OnPauseBegin;

        /// <summary>Fired when the pause window expires.</summary>
        public event System.Action OnPauseEnd;

        private Coroutine _activeRoutine;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Begin a pause window of <paramref name="duration"/> seconds.
        /// If a pause is already active it is extended to the new duration.
        /// </summary>
        public void TriggerPause(float duration)
        {
            if (_activeRoutine != null)
                StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(PauseRoutine(duration));
        }

        private IEnumerator PauseRoutine(float duration)
        {
            IsPaused = true;
            Debug.Log(string.Format("[Pause] BEGIN  duration:{0:F2}s", duration));

            if (OnPauseBegin != null) OnPauseBegin(duration);

            yield return new WaitForSeconds(duration);

            IsPaused = false;
            _activeRoutine = null;
            Debug.Log("[Pause] END");

            if (OnPauseEnd != null) OnPauseEnd();
        }
    }
}
