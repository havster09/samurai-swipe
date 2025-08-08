using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ObservablePattern
{
    /// <summary>
    /// Enhanced TimingUtilities that uses the observable pattern
    /// </summary>
    public class ObservableTimingUtilities : MonoBehaviour
    {
        public static ObservableTimingUtilities Instance;
        
        private readonly Dictionary<string, Subject<object>> _timers = new Dictionary<string, Subject<object>>();
        private readonly Dictionary<string, Coroutine> _activeCoroutines = new Dictionary<string, Coroutine>();
        private readonly object _lock = new object();

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
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Wait for a specified duration and then trigger a callback
        /// </summary>
        /// <param name="duration">The duration to wait</param>
        /// <param name="callback">The callback to execute after the wait</param>
        /// <returns>An observable that completes after the duration</returns>
        public IObservable<object> WaitFor(float duration, Action callback = null)
        {
            var subject = new Subject<object>();
            
            StartCoroutine(WaitForCoroutine(duration, () =>
            {
                if (callback != null)
                    callback();
                subject.OnNext(null);
                subject.OnCompleted();
            }));

            return subject;
        }

        /// <summary>
        /// Wait for a specified duration and return an observable
        /// </summary>
        /// <param name="duration">The duration to wait</param>
        /// <returns>An observable that completes after the duration</returns>
        public IObservable<object> WaitForObservable(float duration)
        {
            return WaitFor(duration);
        }

        /// <summary>
        /// Wait for a specified duration with a timer ID
        /// </summary>
        /// <param name="timerId">Unique identifier for the timer</param>
        /// <param name="duration">The duration to wait</param>
        /// <param name="callback">The callback to execute after the wait</param>
        /// <returns>Subscription that can be used to cancel the timer</returns>
        public IDisposable WaitFor(string timerId, float duration, Action callback = null)
        {
            if (string.IsNullOrEmpty(timerId))
                throw new ArgumentException("Timer ID cannot be null or empty", "timerId");

            lock (_lock)
            {
                // Cancel existing timer if it exists
                if (_activeCoroutines.ContainsKey(timerId))
                {
                    StopCoroutine(_activeCoroutines[timerId]);
                    _activeCoroutines.Remove(timerId);
                }

                if (_timers.ContainsKey(timerId))
                {
                    _timers[timerId].OnCompleted();
                    _timers.Remove(timerId);
                }

                var subject = new Subject<object>();
                _timers[timerId] = subject;

                var coroutine = StartCoroutine(WaitForCoroutine(duration, () =>
                {
                    if (callback != null)
                        callback();
                    subject.OnNext(null);
                    subject.OnCompleted();
                    
                    lock (_lock)
                    {
                        _activeCoroutines.Remove(timerId);
                        _timers.Remove(timerId);
                    }
                }));

                _activeCoroutines[timerId] = coroutine;

                return new TimerSubscription(this, timerId);
            }
        }

        /// <summary>
        /// Cancel a timer by ID
        /// </summary>
        /// <param name="timerId">The timer ID to cancel</param>
        public void CancelTimer(string timerId)
        {
            if (string.IsNullOrEmpty(timerId))
                return;

            lock (_lock)
            {
                if (_activeCoroutines.ContainsKey(timerId))
                {
                    StopCoroutine(_activeCoroutines[timerId]);
                    _activeCoroutines.Remove(timerId);
                }

                if (_timers.ContainsKey(timerId))
                {
                    _timers[timerId].OnCompleted();
                    _timers.Remove(timerId);
                }
            }
        }

        /// <summary>
        /// Check if a timer is active
        /// </summary>
        /// <param name="timerId">The timer ID to check</param>
        /// <returns>True if the timer is active</returns>
        public bool IsTimerActive(string timerId)
        {
            lock (_lock)
            {
                return _activeCoroutines.ContainsKey(timerId);
            }
        }

        /// <summary>
        /// Get the number of active timers
        /// </summary>
        /// <returns>The number of active timers</returns>
        public int GetActiveTimerCount()
        {
            lock (_lock)
            {
                return _activeCoroutines.Count;
            }
        }

        /// <summary>
        /// Cancel all active timers
        /// </summary>
        public void CancelAllTimers()
        {
            lock (_lock)
            {
                foreach (var coroutine in _activeCoroutines.Values)
                {
                    StopCoroutine(coroutine);
                }
                _activeCoroutines.Clear();

                foreach (var subject in _timers.Values)
                {
                    subject.OnCompleted();
                }
                _timers.Clear();
            }
        }

        /// <summary>
        /// Execute an action every frame for a specified duration
        /// </summary>
        /// <param name="duration">The duration to execute</param>
        /// <param name="action">The action to execute each frame</param>
        /// <returns>An observable that completes after the duration</returns>
        public IObservable<object> ExecuteEveryFrame(float duration, Action action)
        {
            var subject = new Subject<object>();
            
            StartCoroutine(ExecuteEveryFrameCoroutine(duration, action, () =>
            {
                subject.OnNext(null);
                subject.OnCompleted();
            }));

            return subject;
        }

        /// <summary>
        /// Execute an action at regular intervals
        /// </summary>
        /// <param name="interval">The interval between executions</param>
        /// <param name="action">The action to execute</param>
        /// <param name="maxExecutions">Maximum number of executions (0 for infinite)</param>
        /// <returns>An observable that completes after all executions</returns>
        public IObservable<object> ExecuteEvery(float interval, Action action, int maxExecutions = 0)
        {
            var subject = new Subject<object>();
            
            StartCoroutine(ExecuteEveryCoroutine(interval, action, maxExecutions, () =>
            {
                subject.OnNext(null);
                subject.OnCompleted();
            }));

            return subject;
        }

        private IEnumerator WaitForCoroutine(float duration, Action callback)
        {
            yield return new WaitForSeconds(duration);
            if (callback != null)
                callback();
        }

        private IEnumerator ExecuteEveryFrameCoroutine(float duration, Action action, Action onComplete)
        {
            float startTime = Time.time;
            
            while (Time.time < startTime + duration)
            {
                if (action != null)
                    action();
                yield return null;
            }
            
            if (onComplete != null)
                onComplete();
        }

        private IEnumerator ExecuteEveryCoroutine(float interval, Action action, int maxExecutions, Action onComplete)
        {
            int executions = 0;
            
            while (maxExecutions == 0 || executions < maxExecutions)
            {
                if (action != null)
                    action();
                executions++;
                yield return new WaitForSeconds(interval);
            }
            
            if (onComplete != null)
                onComplete();
        }

        /// <summary>
        /// Subscription class for managing timer subscriptions
        /// </summary>
        private class TimerSubscription : IDisposable
        {
            private readonly ObservableTimingUtilities _timingUtilities;
            private readonly string _timerId;
            private bool _disposed;

            public TimerSubscription(ObservableTimingUtilities timingUtilities, string timerId)
            {
                _timingUtilities = timingUtilities;
                _timerId = timerId;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _timingUtilities.CancelTimer(_timerId);
                    _disposed = true;
                }
            }
        }
    }
} 