using System;

namespace Assets.Scripts.ObservablePattern
{
    /// <summary>
    /// Interface for observable objects that can notify subscribers of events
    /// </summary>
    /// <typeparam name="T">The type of data to be observed</typeparam>
    public interface IObservable<T>
    {
        /// <summary>
        /// Subscribe to this observable
        /// </summary>
        /// <param name="observer">The observer to subscribe</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        IDisposable Subscribe(IObserver<T> observer);
        
        /// <summary>
        /// Subscribe with an action callback
        /// </summary>
        /// <param name="onNext">Action to execute when data is available</param>
        /// <param name="onError">Action to execute when an error occurs</param>
        /// <param name="onCompleted">Action to execute when the observable completes</param>
        /// <returns>Subscription that can be used to unsubscribe</returns>
        IDisposable Subscribe(Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null);
    }

    /// <summary>
    /// Interface for observers that can receive notifications from observables
    /// </summary>
    /// <typeparam name="T">The type of data being observed</typeparam>
    public interface IObserver<T>
    {
        /// <summary>
        /// Called when new data is available
        /// </summary>
        /// <param name="value">The new value</param>
        void OnNext(T value);
        
        /// <summary>
        /// Called when an error occurs
        /// </summary>
        /// <param name="error">The error that occurred</param>
        void OnError(Exception error);
        
        /// <summary>
        /// Called when the observable completes
        /// </summary>
        void OnCompleted();
    }

    /// <summary>
    /// Interface for subjects that can both observe and be observed
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    public interface ISubject<T> : IObservable<T>, IObserver<T>
    {
    }
} 