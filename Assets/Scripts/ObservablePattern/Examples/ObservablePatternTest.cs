using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ObservablePattern.Examples
{
    /// <summary>
    /// Example test class that demonstrates the observable pattern usage
    /// and shows the benefits over traditional callbacks
    /// </summary>
    public class ObservablePatternTest : MonoBehaviour
    {
        // Observable events for different game events
        public readonly ObservableEvent OnGameStart = new ObservableEvent();
        public readonly ObservableEvent OnGameOver = new ObservableEvent();
        public readonly ObservableEvent<int> OnScoreChanged = new ObservableEvent<int>();
        public readonly ObservableEvent<string> OnPlayerAction = new ObservableEvent<string>();
        public readonly ObservableEvent<bool> OnPauseStateChanged = new ObservableEvent<bool>();

        // Subscriptions to manage cleanup
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        // Test variables
        private int _score = 0;
        private bool _isPaused = false;

        void Start()
        {
            SetupObservableSubscriptions();
            
            // Trigger initial events
            OnGameStart.Trigger();
            OnScoreChanged.Trigger(0);
            OnPauseStateChanged.Trigger(false);
        }

        void OnDestroy()
        {
            // Clean up all subscriptions
            foreach (var subscription in _subscriptions)
            {
                if (subscription != null)
                    subscription.Dispose();
            }
            _subscriptions.Clear();

            // Dispose of all observable events
            if (OnGameStart != null)
                OnGameStart.Dispose();
            if (OnGameOver != null)
                OnGameOver.Dispose();
            if (OnScoreChanged != null)
                OnScoreChanged.Dispose();
            if (OnPlayerAction != null)
                OnPlayerAction.Dispose();
            if (OnPauseStateChanged != null)
                OnPauseStateChanged.Dispose();
        }

        private void SetupObservableSubscriptions()
        {
            // Subscribe to game start event
            _subscriptions.Add(OnGameStart.Subscribe(() =>
            {
                Debug.Log("Game started! Setting up initial state...");
                _score = 0;
                _isPaused = false;
            }));

            // Subscribe to game over event
            _subscriptions.Add(OnGameOver.Subscribe(() =>
            {
                Debug.Log("Game over! Final score: " + _score);
                
                // Use observable timing utilities for delayed actions
                ObservableTimingUtilities.Instance.WaitFor("gameOverDelay", 2.0f, () =>
                {
                    Debug.Log("Game over sequence completed!");
                });
            }));

            // Subscribe to score changes
            _subscriptions.Add(OnScoreChanged.Subscribe(newScore =>
            {
                _score = newScore;
                Debug.Log(string.Format("Score updated to: {0}", _score));
                
                // Check for win condition
                if (_score >= 100)
                {
                    Debug.Log("Win condition met!");
                    OnGameOver.Trigger();
                }
            }));

            // Subscribe to player actions
            _subscriptions.Add(OnPlayerAction.Subscribe(action =>
            {
                Debug.Log(string.Format("Player performed action: {0}", action));
                
                // React to specific actions
                switch (action.ToLower())
                {
                    case "attack":
                        OnScoreChanged.Trigger(_score + 10);
                        break;
                    case "defend":
                        OnScoreChanged.Trigger(_score + 5);
                        break;
                    case "special":
                        OnScoreChanged.Trigger(_score + 25);
                        break;
                }
            }));

            // Subscribe to pause state changes
            _subscriptions.Add(OnPauseStateChanged.Subscribe(isPaused =>
            {
                _isPaused = isPaused;
                Debug.Log(string.Format("Game paused: {0}", isPaused));
                
                if (isPaused)
                {
                    Debug.Log("Pausing game logic...");
                }
                else
                {
                    Debug.Log("Resuming game logic...");
                }
            }));

            // Demonstrate event composition - combine multiple events
            _subscriptions.Add(OnPlayerAction.Subscribe(action =>
            {
                if (action.ToLower() == "pause")
                {
                    OnPauseStateChanged.Trigger(!_isPaused);
                }
            }));
        }

        // Test methods to simulate game events
        [ContextMenu("Test Attack Action")]
        public void TestAttackAction()
        {
            OnPlayerAction.Trigger("Attack");
        }

        [ContextMenu("Test Defend Action")]
        public void TestDefendAction()
        {
            OnPlayerAction.Trigger("Defend");
        }

        [ContextMenu("Test Special Action")]
        public void TestSpecialAction()
        {
            OnPlayerAction.Trigger("Special");
        }

        [ContextMenu("Test Pause Toggle")]
        public void TestPauseToggle()
        {
            OnPlayerAction.Trigger("Pause");
        }

        [ContextMenu("Test Game Over")]
        public void TestGameOver()
        {
            OnGameOver.Trigger();
        }

        [ContextMenu("Test Score Reset")]
        public void TestScoreReset()
        {
            OnScoreChanged.Trigger(0);
        }

        // Demonstrate global event system usage
        [ContextMenu("Test Global Events")]
        public void TestGlobalEvents()
        {
            // Subscribe to global events
            var globalSubscription = EventSystem.Subscribe("testEvent", () =>
            {
                Debug.Log("Global test event received!");
            });

            // Publish global event
            EventSystem.Publish("testEvent");

            // Clean up
            globalSubscription.Dispose();
        }

        // Demonstrate timer usage
        [ContextMenu("Test Timers")]
        public void TestTimers()
        {
            Debug.Log("Starting timer test...");

            // Create multiple timers
            var timer1 = ObservableTimingUtilities.Instance.WaitFor("timer1", 1.0f, () =>
            {
                Debug.Log("Timer 1 completed!");
            });

            var timer2 = ObservableTimingUtilities.Instance.WaitFor("timer2", 2.0f, () =>
            {
                Debug.Log("Timer 2 completed!");
            });

            var timer3 = ObservableTimingUtilities.Instance.WaitFor("timer3", 3.0f, () =>
            {
                Debug.Log("Timer 3 completed!");
            });

            // Cancel timer2 after 1.5 seconds
            ObservableTimingUtilities.Instance.WaitFor("cancelTimer", 1.5f, () =>
            {
                Debug.Log("Cancelling timer 2...");
                timer2.Dispose();
            });

            // Clean up remaining timers after 4 seconds
            ObservableTimingUtilities.Instance.WaitFor("cleanup", 4.0f, () =>
            {
                timer1.Dispose();
                timer3.Dispose();
                Debug.Log("All timers cleaned up!");
            });
        }

        // Demonstrate error handling
        [ContextMenu("Test Error Handling")]
        public void TestErrorHandling()
        {
            var errorEvent = new ObservableEvent<string>();

            var subscription = errorEvent.Subscribe(
                data =>
                {
                    if (data == "error")
                    {
                        throw new Exception("Simulated error!");
                    }
                    Debug.Log(string.Format("Received data: {0}", data));
                },
                error =>
                {
                    Debug.LogError(string.Format("Error handled: {0}", error.Message));
                },
                () =>
                {
                    Debug.Log("Event completed successfully!");
                }
            );

            // Test normal operation
            errorEvent.Trigger("normal");

            // Test error condition
            errorEvent.Trigger("error");

            // Complete the event
            errorEvent.Complete();

            subscription.Dispose();
            errorEvent.Dispose();
        }

        // Demonstrate subscription management
        [ContextMenu("Test Subscription Management")]
        public void TestSubscriptionManagement()
        {
            Debug.Log(string.Format("Current subscription count: {0}", _subscriptions.Count));

            // Add a temporary subscription
            var tempSubscription = OnPlayerAction.Subscribe(action =>
            {
                Debug.Log(string.Format("Temporary subscription received: {0}", action));
            });

            Debug.Log(string.Format("Added temporary subscription. Total: {0}", _subscriptions.Count + 1));

            // Trigger an action
            OnPlayerAction.Trigger("TestAction");

            // Remove temporary subscription
            tempSubscription.Dispose();
            Debug.Log("Temporary subscription removed");
        }

        // Demonstrate typed events
        [ContextMenu("Test Typed Events")]
        public void TestTypedEvents()
        {
            var typedEvent = new ObservableEvent<int>();

            var subscription = typedEvent.Subscribe(value =>
            {
                Debug.Log(string.Format("Typed event received: {0} (type: {1})", value, value.GetType()));
            });

            // Trigger with different values
            typedEvent.Trigger(42);
            typedEvent.Trigger(100);
            typedEvent.Trigger(-5);

            subscription.Dispose();
            typedEvent.Dispose();
        }

        void Update()
        {
            // Simulate input for testing
            if (Input.GetKeyDown(KeyCode.A))
            {
                TestAttackAction();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                TestDefendAction();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                TestSpecialAction();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                TestPauseToggle();
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                TestGameOver();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                TestScoreReset();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                TestTimers();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                TestErrorHandling();
            }
        }
    }
} 