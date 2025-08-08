# Observable Pattern Implementation

This directory contains a comprehensive observable pattern implementation designed to replace the callback-based system in the Samurai Swipe project.

## Overview

The observable pattern provides a reactive programming approach that offers better memory management, type safety, and testability compared to traditional callback-based systems.

## Core Components

### 1. IObservable<T> and IObserver<T>
- **IObservable<T>**: Interface for objects that can be observed
- **IObserver<T>**: Interface for objects that observe observables
- **ISubject<T>**: Interface for objects that can both observe and be observed

### 2. Subject<T>
A concrete implementation of ISubject<T> that:
- Manages multiple observers
- Provides thread-safe operations
- Handles error propagation
- Supports completion notifications
- Automatically manages subscriptions

### 3. EventSystem
A static event system that replaces the Broadcaster:
- Global event publishing and subscription
- Type-safe event handling
- Automatic cleanup
- Thread-safe operations

### 4. ObservableEvent
Enhanced event system that replaces SimpleEvent:
- Type-safe event triggers
- Automatic subscription management
- Built-in error handling
- Completion notifications

### 5. ObservableTimingUtilities
Enhanced timing utilities that replace TimingUtilities:
- Observable-based timing operations
- Timer cancellation support
- Multiple timer management
- Thread-safe operations

## Quick Start

### Basic Usage

```csharp
// Create an observable event
public readonly ObservableEvent OnHeroHit = new ObservableEvent();

// Subscribe to the event
var subscription = OnHeroHit.Subscribe(() => {
    Debug.Log("Hero was hit!");
});

// Trigger the event
OnHeroHit.Trigger();

// Clean up (important!)
subscription.Dispose();
```

### Typed Events

```csharp
// Create a typed observable event
public readonly ObservableEvent<int> OnDamage = new ObservableEvent<int>();

// Subscribe with type safety
var subscription = OnDamage.Subscribe(damage => {
    Debug.Log($"Hero took {damage} damage!");
});

// Trigger with data
OnDamage.Trigger(25);
```

### Global Events

```csharp
// Subscribe to global event
var subscription = EventSystem.Subscribe("gameOver", () => {
    Debug.Log("Game Over!");
});

// Publish global event
EventSystem.Publish("gameOver");

// Clean up
subscription.Dispose();
```

### Timing Operations

```csharp
// Wait for duration
var timerSubscription = ObservableTimingUtilities.Instance.WaitFor("myTimer", 2.0f, () => {
    Debug.Log("2 seconds passed!");
});

// Cancel timer
timerSubscription.Dispose();

// Execute every frame for duration
ObservableTimingUtilities.Instance.ExecuteEveryFrame(5.0f, () => {
    Debug.Log("Frame executed!");
}).Subscribe(_ => {
    Debug.Log("5 seconds of frame execution completed!");
});
```

## Advanced Features

### Error Handling

```csharp
var subscription = OnHeroHit.Subscribe(
    onNext: state => { /* handle success */ },
    onError: error => { Debug.LogError($"Error: {error}"); },
    onCompleted: () => { /* handle completion */ }
);
```

### Multiple Subscriptions

```csharp
private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

void Start()
{
    _subscriptions.Add(OnHeroHit.Subscribe(HandleHeroHit));
    _subscriptions.Add(OnEnemyHit.Subscribe(HandleEnemyHit));
}

void OnDestroy()
{
    foreach (var subscription in _subscriptions)
    {
        subscription?.Dispose();
    }
    _subscriptions.Clear();
}
```

### Event Composition

```csharp
// Subscribe to multiple events
_subscriptions.Add(OnHeroHit.Subscribe(state => {
    if (state) OnEnemyHit.Trigger(1);
}));
```

## Migration from Callbacks

### Replace Broadcaster

**Before:**
```csharp
Broadcaster.EnableListener("heroHit", OnHeroHit);
Broadcaster.SendEvent("heroHit");
Broadcaster.DisableListener("heroHit", OnHeroHit);
```

**After:**
```csharp
var subscription = EventSystem.Subscribe("heroHit", OnHeroHit);
EventSystem.Publish("heroHit");
subscription.Dispose();
```

### Replace SimpleEvent

**Before:**
```csharp
public SimpleEvent onHeroHit = new SimpleEvent();
onHeroHit.Add(OnHeroHit);
onHeroHit.Run();
onHeroHit.Remove(OnHeroHit);
```

**After:**
```csharp
public readonly ObservableEvent OnHeroHit = new ObservableEvent();
var subscription = OnHeroHit.Subscribe(OnHeroHit);
OnHeroHit.Trigger();
subscription.Dispose();
```

### Replace TimingUtilities

**Before:**
```csharp
TimingUtilities.Instance.WaitFor(() => {
    // callback code
}, 2.0f);
```

**After:**
```csharp
ObservableTimingUtilities.Instance.WaitFor("myTimer", 2.0f, () => {
    // callback code
});
```

## Best Practices

1. **Always Dispose**: Clean up subscriptions to prevent memory leaks
2. **Use Typed Events**: Leverage type safety when possible
3. **Manage Subscriptions**: Keep track of subscriptions for cleanup
4. **Error Handling**: Implement proper error handling for robust code
5. **Thread Safety**: Use thread-safe operations in multi-threaded scenarios

## Performance Considerations

- **Memory**: Slightly higher memory usage but better cleanup
- **CPU**: Minimal overhead for event operations
- **Threading**: Thread-safe with proper locking
- **GC**: Reduced garbage collection pressure due to automatic cleanup

## Testing

The observable pattern makes testing much easier:

```csharp
[Test]
public void TestHeroHitEvent()
{
    var hero = new HeroObservableExample();
    bool eventTriggered = false;
    
    var subscription = hero.OnHeroHit.Subscribe(state => {
        eventTriggered = true;
    });
    
    hero.HeroHit(true);
    
    Assert.IsTrue(eventTriggered);
    subscription.Dispose();
}
```

## File Structure

```
ObservablePattern/
├── IObservable.cs              # Core interfaces
├── Subject.cs                  # Subject implementation
├── EventSystem.cs              # Global event system
├── ObservableEvent.cs          # Enhanced event system
├── ObservableTimingUtilities.cs # Enhanced timing utilities
├── Examples/
│   └── HeroObservableExample.cs # Example refactored Hero class
├── MigrationGuide.md           # Detailed migration guide
└── README.md                   # This file
```

## Dependencies

- Unity 2021.3 or later
- .NET 4.x or later
- No external dependencies required

## License

This implementation is part of the Samurai Swipe project and follows the same licensing terms.

## Contributing

When contributing to the observable pattern implementation:

1. Follow the existing code style
2. Add proper documentation
3. Include unit tests for new features
4. Update the migration guide if needed
5. Ensure thread safety for new components

## Support

For questions or issues with the observable pattern implementation:

1. Check the MigrationGuide.md for common solutions
2. Review the Examples directory for usage patterns
3. Ensure proper subscription cleanup
4. Verify thread safety in multi-threaded scenarios 