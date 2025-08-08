using System;
using UnityEngine;

namespace Assets.Scripts.ObservablePattern
{
    /// <summary>
    /// Enhanced SimpleEvent that uses the observable pattern
    /// </summary>
    public class ObservableEvent
    {
        private readonly Subject<object> _subject = new Subject<object>();
        private bool _isDisposed;

        /// <summary>
        /// Subscribe to this event
        /// </summary>
        /// <param name="callback">The callback to execute when the event is triggered</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        public IDisposable Subscribe(Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            if (_isDisposed)
            {
                Debug.LogWarning("Attempting to subscribe to a disposed ObservableEvent");
                return new EmptyDisposable();
            }

            return _subject.Subscribe(_ => callback());
        }

        /// <summary>
        /// Subscribe with full observer pattern support
        /// </summary>
        /// <param name="onNext">Action to execute when data is available</param>
        /// <param name="onError">Action to execute when an error occurs</param>
        /// <param name="onCompleted">Action to execute when the observable completes</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        public IDisposable Subscribe(Action onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            if (onNext == null)
                throw new ArgumentNullException("onNext");

            if (_isDisposed)
            {
                Debug.LogWarning("Attempting to subscribe to a disposed ObservableEvent");
                return new EmptyDisposable();
            }

            return _subject.Subscribe(_ => onNext(), onError, onCompleted);
        }

        /// <summary>
        /// Subscribe with data
        /// </summary>
        /// <typeparam name="T">The type of data</typeparam>
        /// <param name="callback">The callback to execute when the event is triggered</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        public IDisposable Subscribe<T>(Action<T> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            if (_isDisposed)
            {
                Debug.LogWarning("Attempting to subscribe to a disposed ObservableEvent");
                return new EmptyDisposable();
            }

            return _subject.Subscribe(obj => callback((T)obj));
        }

        /// <summary>
        /// Trigger the event
        /// </summary>
        public void Trigger()
        {
            if (!_isDisposed)
            {
                _subject.OnNext(null);
            }
        }

        /// <summary>
        /// Trigger the event with data
        /// </summary>
        /// <typeparam name="T">The type of data</typeparam>
        /// <param name="data">The data to pass to subscribers</param>
        public void Trigger<T>(T data)
        {
            if (!_isDisposed)
            {
                _subject.OnNext(data);
            }
        }

        /// <summary>
        /// Complete the event and notify all subscribers
        /// </summary>
        public void Complete()
        {
            if (!_isDisposed)
            {
                _subject.OnCompleted();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Get the number of current subscribers
        /// </summary>
        public int SubscriberCount 
        { 
            get { return _subject.ObserverCount; } 
        }

        /// <summary>
        /// Check if the event has been disposed
        /// </summary>
        public bool IsDisposed 
        { 
            get { return _isDisposed; } 
        }

        /// <summary>
        /// Dispose of the event
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _subject.OnCompleted();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Empty disposable for disposed events
        /// </summary>
        private class EmptyDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    /// <summary>
    /// Typed observable event for specific data types
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    public class ObservableEvent<T>
    {
        private readonly Subject<T> _subject = new Subject<T>();
        private bool _isDisposed;

        /// <summary>
        /// Subscribe to this event
        /// </summary>
        /// <param name="callback">The callback to execute when the event is triggered</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        public IDisposable Subscribe(Action<T> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            if (_isDisposed)
            {
                Debug.LogWarning("Attempting to subscribe to a disposed ObservableEvent");
                return new EmptyDisposable();
            }

            return _subject.Subscribe(callback);
        }

        /// <summary>
        /// Subscribe with full observer pattern support
        /// </summary>
        /// <param name="onNext">Action to execute when data is available</param>
        /// <param name="onError">Action to execute when an error occurs</param>
        /// <param name="onCompleted">Action to execute when the observable completes</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        public IDisposable Subscribe(Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            if (onNext == null)
                throw new ArgumentNullException("onNext");

            if (_isDisposed)
            {
                Debug.LogWarning("Attempting to subscribe to a disposed ObservableEvent");
                return new EmptyDisposable();
            }

            return _subject.Subscribe(onNext, onError, onCompleted);
        }

        /// <summary>
        /// Trigger the event with data
        /// </summary>
        /// <param name="data">The data to pass to subscribers</param>
        public void Trigger(T data)
        {
            if (!_isDisposed)
            {
                _subject.OnNext(data);
            }
        }

        /// <summary>
        /// Complete the event and notify all subscribers
        /// </summary>
        public void Complete()
        {
            if (!_isDisposed)
            {
                _subject.OnCompleted();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Get the number of current subscribers
        /// </summary>
        public int SubscriberCount 
        { 
            get { return _subject.ObserverCount; } 
        }

        /// <summary>
        /// Check if the event has been disposed
        /// </summary>
        public bool IsDisposed 
        { 
            get { return _isDisposed; } 
        }

        /// <summary>
        /// Dispose of the event
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _subject.OnCompleted();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Empty disposable for disposed events
        /// </summary>
        private class EmptyDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
} 