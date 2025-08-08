# Observable Pattern Migration Guide

This guide explains how to refactor the existing callback-based code in the Samurai Swipe project to use the observable pattern.

## Overview

The observable pattern provides a more robust, testable, and maintainable way to handle events and callbacks. It replaces the current callback-based system with a reactive programming approach.

## Key Benefits

1. **Better Memory Management**: Automatic cleanup of subscriptions prevents memory leaks
2. **Type Safety**: Compile-time checking of event types and data
3. **Testability**: Easier to unit test event-driven code
4. **Composability**: Easy to combine and transform events
5. **Error Handling**: Built-in error handling and completion notifications
6. **Thread Safety**: Thread-safe event publishing and subscription

## Migration Steps

### 1. Replace Broadcaster with EventSystem

**Before (using Broadcaster):**
```csharp
// Subscribe to event
Broadcaster.EnableListener("heroHit", OnHeroHit);

// Publish event
Broadcaster.SendEvent("heroHit");

// Unsubscribe
Broadcaster.DisableListener("heroHit", OnHeroHit);
```

**After (using EventSystem):**
```csharp
// Subscribe to event
var subscription = EventSystem.Subscribe("heroHit", OnHeroHit);

// Publish event
EventSystem.Publish("heroHit");

// Unsubscribe (automatic cleanup)
subscription.Dispose();
```

### 2. Replace SimpleEvent with ObservableEvent

**Before (using SimpleEvent):**
```csharp
public SimpleEvent onHeroHit = new SimpleEvent();

// Subscribe
onHeroHit.Add(OnHeroHit);

// Trigger
onHeroHit.Run();

// Unsubscribe
onHeroHit.Remove(OnHeroHit);
```

**After (using ObservableEvent):**
```csharp
public readonly ObservableEvent OnHeroHit = new ObservableEvent();

// Subscribe
var subscription = OnHeroHit.Subscribe(OnHeroHit);

// Trigger
OnHeroHit.Trigger();

// Unsubscribe (automatic cleanup)
subscription.Dispose();
```

### 3. Replace TimingUtilities with ObservableTimingUtilities

**Before (using TimingUtilities):**
```csharp
TimingUtilities.Instance.WaitFor(() => {
    // callback code
}, 2.0f);
```

**After (using ObservableTimingUtilities):**
```csharp
// Option 1: Using subscription
var subscription = ObservableTimingUtilities.Instance.WaitFor(2.0f, () => {
    // callback code
});

// Option 2: Using observable
ObservableTimingUtilities.Instance.WaitForObservable(2.0f)
    .Subscribe(_ => {
        // callback code
    });

// Option 3: Using timer ID for cancellation
var timerSubscription = ObservableTimingUtilities.Instance.WaitFor("myTimer", 2.0f, () => {
    // callback code
});

// Cancel timer
timerSubscription.Dispose();
```

### 4. Replace Direct Callbacks with Observable Events

**Before (direct callbacks):**
```csharp
public void MoveBack(GameObject target, float to, float speed, Action action = null)
{
    // ... movement logic ...
    if (action != null) action();
}
```

**After (observable events):**
```csharp
public readonly ObservableEvent OnMovementComplete = new ObservableEvent();

public void MoveBack(GameObject target, float to, float speed)
{
    // ... movement logic ...
    OnMovementComplete.Trigger();
}

// Subscribe to movement completion
_subscriptions.Add(OnMovementComplete.Subscribe(() => {
    // Handle movement completion
}));
```

## Complete Example: Hero Class Refactor

### Before (Callback-based):
```csharp
public class Hero : MovingObject
{
    private void HeroBlockEndEventHandler()
    {
        MoveBack(CurrentTarget, .1f, 1f, () => HeroBlock(false));
        TimingUtilities.Instance.WaitFor(() => NpcHeroAnimator.Play("heroIdle"), .2f);
    }

    public void HeroHit(bool state, GameObject target = null)
    {
        // ... implementation ...
        if (state)
        {
            EnemyHit(1);
        }
    }
}
```

### After (Observable-based):
```csharp
public class HeroObservableExample : MovingObject
{
    // Observable events
    public readonly ObservableEvent OnHeroBlockEnd = new ObservableEvent();
    public readonly ObservableEvent<bool> OnHeroHit = new ObservableEvent<bool>();
    public readonly ObservableEvent<int> OnEnemyHit = new ObservableEvent<int>();

    // Subscriptions to manage cleanup
    private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

    private void SetupObservableSubscriptions()
    {
        // Subscribe to hero block end event
        _subscriptions.Add(OnHeroBlockEnd.Subscribe(() =>
        {
            ObservableTimingUtilities.Instance.WaitFor("heroBlockEnd", 0.2f, () =>
            {
                NpcHeroAnimator.Play("heroIdle");
            });
        }));

        // Subscribe to enemy hit events
        _subscriptions.Add(OnEnemyHit.Subscribe(damage =>
        {
            GetBloodEffect("Blood", "BloodEffect1");
            IsHit = true;
        }));
    }

    private void HeroBlockEndEventHandler()
    {
        // Trigger the observable event instead of direct callback
        OnHeroBlockEnd.Trigger();
    }

    public void HeroHit(bool state, GameObject target = null)
    {
        // ... implementation ...
        
        // Trigger the observable event
        OnHeroHit.Trigger(state);
        
        if (state)
        {
            OnEnemyHit.Trigger(1);
        }
    }

    private void OnDestroy()
    {
        // Clean up all subscriptions
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }
        _subscriptions.Clear();

        // Dispose of all observable events
        OnHeroBlockEnd?.Dispose();
        OnHeroHit?.Dispose();
        OnEnemyHit?.Dispose();
    }
}
```

## Best Practices

### 1. Always Dispose of Subscriptions
```csharp
private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

void OnDestroy()
{
    foreach (var subscription in _subscriptions)
    {
        subscription?.Dispose();
    }
    _subscriptions.Clear();
}
```

### 2. Use Typed Events When Possible
```csharp
// Instead of ObservableEvent with object casting
public readonly ObservableEvent<int> OnDamage = new ObservableEvent<int>();

// Subscribe with type safety
_subscriptions.Add(OnDamage.Subscribe(damage => {
    // damage is already an int
}));
```

### 3. Use Timer IDs for Cancellation
```csharp
// Create timer with ID
var timerSubscription = ObservableTimingUtilities.Instance.WaitFor("myTimer", 2.0f, callback);

// Cancel when needed
timerSubscription.Dispose();
```

### 4. Combine Events
```csharp
// Subscribe to multiple events
_subscriptions.Add(OnHeroHit.Subscribe(state => {
    if (state) OnEnemyHit.Trigger(1);
}));
```

### 5. Error Handling
```csharp
// Subscribe with error handling
_subscriptions.Add(OnHeroHit.Subscribe(
    onNext: state => { /* handle success */ },
    onError: error => { Debug.LogError($"Error: {error}"); },
    onCompleted: () => { /* handle completion */ }
));
```

## Migration Checklist

- [ ] Replace `Broadcaster` calls with `EventSystem`
- [ ] Replace `SimpleEvent` with `ObservableEvent`
- [ ] Replace `TimingUtilities` with `ObservableTimingUtilities`
- [ ] Add subscription management in `OnDestroy`
- [ ] Replace direct callbacks with observable events
- [ ] Add proper error handling
- [ ] Test all event flows
- [ ] Update documentation

## Performance Considerations

1. **Memory**: Observable pattern uses more memory but provides better cleanup
2. **CPU**: Minimal overhead for event publishing/subscription
3. **Threading**: Thread-safe operations with proper locking
4. **Garbage Collection**: Automatic cleanup reduces GC pressure

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

## Troubleshooting

### Common Issues

1. **Memory Leaks**: Always dispose of subscriptions
2. **Null Reference**: Check if events are disposed before subscribing
3. **Thread Safety**: Use proper locking in multi-threaded scenarios
4. **Event Order**: Events are processed in subscription order

### Debug Tips

1. Use `ObserverCount` to check subscription count
2. Use `IsDisposed` to check event state
3. Log event triggers for debugging
4. Use timer IDs for better tracking

This migration will significantly improve the codebase's maintainability, testability, and reliability while reducing memory leaks and callback-related bugs. 