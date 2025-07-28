# Echo EventBus System

A static, high-performance, and type-safe event bus system designed for Unity. It uses `structs` for events to avoid memory allocations and garbage collector spikes.

<br>

## Core Concepts

- **Events are `structs`**: To ensure zero heap allocation, all events must be value types (`struct`).
- **`IEvent` Interface**: Every event `struct` must implement the `IEvent` interface. This is a marker interface that ensures type safety.
- **Lifecycle**: Subscription management is manual. It is **critical** to unsubscribe from events to prevent memory leaks.

<br>

## How to Use

Using the bus involves three simple steps: define, publish, and listen.

### 1. Defining an Event

Create a `struct` that implements `IEvent`. This structure will hold your event's data.

**Example**: An event for a player's death, containing the player's ID and position.

```csharp
using Echo.Interface;
using UnityEngine;

public struct PlayerDiedEvent : IEvent
{
    public int PlayerId;
    public Vector3 Position;
}
```

### 2. Publishing an Event

Use `EventBus.Publish()` anywhere in your code to dispatch an event.
**Example:** A `PlayerHealth` script that publishes the event when the player's health reaches zero.

```csharp
using Echo;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int playerId = 1;

    public void TakeFatalDamage()
    {
        // ... death logic ...

        // Create and publish the event
        EventBus.Publish(new PlayerDiedEvent
        {
            PlayerId = this.playerId,
            Position = transform.position
        });
    }
}
```

### 3. Subscribing to an Event

To listen for an event, use `EventBus.Subscribe()` with a method that matches the event's signature.

The safest and most common pattern in Unity is to subscribe in `OnEnable()` and **always unsubscribe** in `OnDisable()`.

Example: A `GameManager` script listening for the `PlayerDiedEvent`.

```csharp
using Echo;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void OnEnable()
    {
        // Subscribe to the event and specify the handler method
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnDisable()
    {
        // VERY IMPORTANT: Unsubscribe to prevent memory leaks
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnPlayerDied(PlayerDiedEvent eventData)
    {
        Debug.Log($"Player {eventData.PlayerId} died at position {eventData.Position}!");
        // ... logic to handle the player's death (e.g., respawn, update UI) ...
    }
}
```

## Best Practices
- **Always Unsubscribe:** Forgetting to unsubscribe is the most common cause of memory leaks with static event systems. Always use the OnEnable/OnDisable pattern.
- **Keep Events Lean:** Event structs should only contain the necessary data. Avoid putting logic inside them.
