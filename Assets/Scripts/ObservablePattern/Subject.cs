using System;
using System.Collections.Generic;

namespace Assets.Scripts.ObservablePattern
{
    /// <summary>
    /// A subject that can both observe and be observed, implementing the observable pattern
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    public class Subject<T> : ISubject<T>
    {
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();
        private readonly object _lock = new object();
        private bool _isCompleted;
        private Exception _error;

        /// <summary>
        /// Subscribe to this subject
        /// </summary>
        /// <param name="observer">The observer to subscribe</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            lock (_lock)
            {
                if (_isCompleted)
                {
                    if (_error != null)
                        observer.OnError(_error);
                    else
                        observer.OnCompleted();
                    return new EmptyDisposable();
                }

                _observers.Add(observer);
                return new Subscription(this, observer);
            }
        }

        /// <summary>
        /// Subscribe with action callbacks
        /// </summary>
        /// <param name="onNext">Action to execute when data is available</param>
        /// <param name="onError">Action to execute when an error occurs</param>
        /// <param name="onCompleted">Action to execute when the observable completes</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        public IDisposable Subscribe(Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            if (onNext == null)
                throw new ArgumentNullException("onNext");

            var observer = new ActionObserver<T>(onNext, onError, onCompleted);
            return Subscribe(observer);
        }

        /// <summary>
        /// Notify all observers of a new value
        /// </summary>
        /// <param name="value">The new value</param>
        public void OnNext(T value)
        {
            lock (_lock)
            {
                if (_isCompleted) return;

                foreach (var observer in _observers)
                {
                    try
                    {
                        observer.OnNext(value);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Notify all observers of an error
        /// </summary>
        /// <param name="error">The error that occurred</param>
        public void OnError(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            lock (_lock)
            {
                if (_isCompleted) return;

                _isCompleted = true;
                _error = error;

                foreach (var observer in _observers)
                {
                    try
                    {
                        observer.OnError(error);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception but don't re-throw to avoid infinite loops
                        UnityEngine.Debug.LogError(string.Format("Error in observer OnError: {0}", ex));
                    }
                }
            }
        }

        /// <summary>
        /// Notify all observers that the subject has completed
        /// </summary>
        public void OnCompleted()
        {
            lock (_lock)
            {
                if (_isCompleted) return;

                _isCompleted = true;

                foreach (var observer in _observers)
                {
                    try
                    {
                        observer.OnCompleted();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception but don't re-throw to avoid infinite loops
                        UnityEngine.Debug.LogError(string.Format("Error in observer OnCompleted: {0}", ex));
                    }
                }
            }
        }

        /// <summary>
        /// Remove an observer from the subject
        /// </summary>
        /// <param name="observer">The observer to remove</param>
        internal void Unsubscribe(IObserver<T> observer)
        {
            lock (_lock)
            {
                _observers.Remove(observer);
            }
        }

        /// <summary>
        /// Get the number of current subscribers
        /// </summary>
        public int ObserverCount
        {
            get
            {
                lock (_lock)
                {
                    return _observers.Count;
                }
            }
        }

        /// <summary>
        /// Check if the subject has completed
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                lock (_lock)
                {
                    return _isCompleted;
                }
            }
        }

        /// <summary>
        /// Subscription class for managing subscriptions
        /// </summary>
        private class Subscription : IDisposable
        {
            private readonly Subject<T> _subject;
            private readonly IObserver<T> _observer;
            private bool _disposed;

            public Subscription(Subject<T> subject, IObserver<T> observer)
            {
                _subject = subject;
                _observer = observer;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _subject.Unsubscribe(_observer);
                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// Empty disposable for completed subjects
        /// </summary>
        private class EmptyDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    /// <summary>
    /// Observer implementation that uses action callbacks
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    public class ActionObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        public ActionObserver(Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            if (onNext == null)
                throw new ArgumentNullException("onNext");
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }

        public void OnError(Exception error)
        {
            if (_onError != null)
                _onError(error);
        }

        public void OnCompleted()
        {
            if (_onCompleted != null)
                _onCompleted();
        }
    }
} 