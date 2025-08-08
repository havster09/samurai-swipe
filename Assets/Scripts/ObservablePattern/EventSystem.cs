using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ObservablePattern
{
    /// <summary>
    /// Event system that uses the observable pattern to replace the Broadcaster system
    /// </summary>
    public static class EventSystem
    {
        private static readonly Dictionary<string, Subject<object>> _events = new Dictionary<string, Subject<object>>();
        private static readonly Dictionary<string, Subject<object>> _typedEvents = new Dictionary<string, Subject<object>>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Subscribe to an event by name
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <param name="callback">The callback to execute when the event is triggered</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        public static IDisposable Subscribe(string eventName, Action callback)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty", "eventName");

            if (callback == null)
                throw new ArgumentNullException("callback");

            lock (_lock)
            {
                if (!_events.ContainsKey(eventName))
                {
                    _events[eventName] = new Subject<object>();
                }

                return _events[eventName].Subscribe(_ => callback());
            }
        }

        /// <summary>
        /// Subscribe to a typed event by name
        /// </summary>
        /// <typeparam name="T">The type of data for the event</typeparam>
        /// <param name="eventName">The name of the event</param>
        /// <param name="callback">The callback to execute when the event is triggered</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        public static IDisposable Subscribe<T>(string eventName, Action<T> callback)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty", "eventName");

            if (callback == null)
                throw new ArgumentNullException("callback");

            lock (_lock)
            {
                if (!_typedEvents.ContainsKey(eventName))
                {
                    _typedEvents[eventName] = new Subject<object>();
                }

                return _typedEvents[eventName].Subscribe(obj => callback((T)obj));
            }
        }

        /// <summary>
        /// Publish an event by name
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        public static void Publish(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty", "eventName");

            lock (_lock)
            {
                if (_events.ContainsKey(eventName))
                {
                    _events[eventName].OnNext(null);
                }
                else
                {
                    Debug.LogWarning(string.Format("Event '{0}' has no subscribers", eventName));
                }
            }
        }

        /// <summary>
        /// Publish a typed event by name
        /// </summary>
        /// <typeparam name="T">The type of data for the event</typeparam>
        /// <param name="eventName">The name of the event</param>
        /// <param name="data">The data to publish</param>
        public static void Publish<T>(string eventName, T data)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty", "eventName");

            lock (_lock)
            {
                if (_typedEvents.ContainsKey(eventName))
                {
                    _typedEvents[eventName].OnNext(data);
                }
                else
                {
                    Debug.LogWarning(string.Format("Typed event '{0}' has no subscribers", eventName));
                }
            }
        }

        /// <summary>
        /// Check if an event has any subscribers
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <returns>True if the event has subscribers</returns>
        public static bool HasSubscribers(string eventName)
        {
            lock (_lock)
            {
                return _events.ContainsKey(eventName) && _events[eventName].ObserverCount > 0;
            }
        }

        /// <summary>
        /// Check if a typed event has any subscribers
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <returns>True if the event has subscribers</returns>
        public static bool HasTypedSubscribers(string eventName)
        {
            lock (_lock)
            {
                return _typedEvents.ContainsKey(eventName) && _typedEvents[eventName].ObserverCount > 0;
            }
        }

        /// <summary>
        /// Get the number of subscribers for an event
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <returns>The number of subscribers</returns>
        public static int GetSubscriberCount(string eventName)
        {
            lock (_lock)
            {
                return _events.ContainsKey(eventName) ? _events[eventName].ObserverCount : 0;
            }
        }

        /// <summary>
        /// Get the number of subscribers for a typed event
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <returns>The number of subscribers</returns>
        public static int GetTypedSubscriberCount(string eventName)
        {
            lock (_lock)
            {
                return _typedEvents.ContainsKey(eventName) ? _typedEvents[eventName].ObserverCount : 0;
            }
        }

        /// <summary>
        /// Clear all events and subscribers
        /// </summary>
        public static void ClearAll()
        {
            lock (_lock)
            {
                foreach (var subject in _events.Values)
                {
                    subject.OnCompleted();
                }
                _events.Clear();

                foreach (var subject in _typedEvents.Values)
                {
                    subject.OnCompleted();
                }
                _typedEvents.Clear();
            }
        }

        /// <summary>
        /// Remove a specific event
        /// </summary>
        /// <param name="eventName">The name of the event to remove</param>
        public static void RemoveEvent(string eventName)
        {
            lock (_lock)
            {
                if (_events.ContainsKey(eventName))
                {
                    _events[eventName].OnCompleted();
                    _events.Remove(eventName);
                }

                if (_typedEvents.ContainsKey(eventName))
                {
                    _typedEvents[eventName].OnCompleted();
                    _typedEvents.Remove(eventName);
                }
            }
        }
    }
} 