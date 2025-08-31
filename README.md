# Echo: High-Performance Event Bus for Unity

<p align="center">
  <a href="https://github.com/Alaxxxx/Echo/stargazers"><img src="https://img.shields.io/github/stars/Alaxxxx/Echo?style=flat-square&logo=github&color=FFC107" alt="GitHub Stars"></a>
  &nbsp;
  <a href="https://github.com/Alaxxxx?tab=followers"><img src="https://img.shields.io/github/followers/Alaxxxx?style=flat-square&logo=github&label=Followers&color=282c34" alt="GitHub Followers"></a>
  &nbsp;
  <a href="https://github.com/Alaxxxx/Echo/commits/main"><img src="https://img.shields.io/github/last-commit/Alaxxxx/Echo?style=flat-square&logo=github&color=blueviolet" alt="Last Commit"></a>
</p>
<p align="center">
  <a href="https://github.com/Alaxxxx/Echo/releases"><img src="https://img.shields.io/github/v/release/Alaxxxx/Echo?style=flat-square" alt="Release"></a>
  &nbsp;
  <a href="https://unity.com/"><img src="https://img.shields.io/badge/Unity-2021.3+-2296F3.svg?style=flat-square&logo=unity" alt="Unity Version"></a>
  &nbsp;
  <a href="https://github.com/Alaxxxx/Echo/blob/main/LICENSE"><img src="https://img.shields.io/github/license/Alaxxxx/Echo?style=flat-square" alt="License"></a>
</p>

**Echo** is a static, high-performance, and type-safe event bus system designed for Unity. It leverages `struct`-based events to achieve **zero-allocation publishing**, eliminating garbage collector spikes and ensuring smooth performance, even in high-frequency scenarios.

With a fluent filtering API, automatic subscription management, and seamless integration with GameObjects, Echo provides a powerful yet simple solution for creating decoupled and maintainable code architecture in your projects.

<br>

## ✨ Features

- **🚀 Zero-Allocation Publishing**: Utilizes `structs` for events to prevent heap allocations and GC pressure.
- **🔒 Type-Safe by Design**: Generic, interface-constrained API prevents runtime errors by ensuring event correctness at compile time.
- **🎯 Advanced Filtering API**: A fluent, chainable interface (`Where(...).And(...).Or(...)`) to subscribe to events that meet complex conditions.
- **♻️ Automatic Subscription Management**: Scoped subscriptions (`IDisposable`) handle cleanup automatically, preventing common memory leaks.
- **📦 Event Batching & Aggregation**: Collect high-frequency events and publish them in batches to optimize performance.
- **🎮 Unity Integration**: Helper extensions for GameObjects allow for easy source/target event tracking.

<br>

## 🚀 Getting Started

### Installation

<details>
<summary><strong>1. Install via Git URL (Recommended)</strong></summary>
<br>
This method installs the package directly from the GitHub repository and allows you to easily update to the latest version.

1.  In Unity, open the **Package Manager** (`Window > Package Management > Package Manager`).
2.  Click the **+** button in the top-left corner and select **"Add package from git URL..."**.
3.  Enter the following URL and click "Install":
    ```
    https://github.com/Alaxxxx/Echo.git
    ```
</details>

<details>
<summary><strong>2. Install via .unitypackage</strong></summary>
<br>
This method is great if you prefer a specific, stable version of the asset.

1.  Go to the [**Releases**](https://github.com/Alaxxxx/Echo/releases) page.
2.  Download the `.unitypackage` file from the latest release.
3.  In your Unity project, go to **`Assets > Import Package > Custom Package...`** and select the downloaded file.
</details>

<details>
<summary><strong>3. Manual Installation (from .zip)</strong></summary>
<br>

1.  Download this repository as a ZIP file by clicking **`Code > Download ZIP`** on the main repository page.
2.  Unzip the downloaded file.
3.  Drag and drop the main asset folder (the one containing all the scripts and resources) into the `Assets` folder of your Unity project.
</details>

**Requirements:**
- Unity 2021.3 or higher
- .NET Standard 2.1 or higher

<br>

## The Basics

Using Echo involves three simple steps: defining, publishing, and listening to events.

### Understanding the Event System

Echo's event bus is built around **`struct`**-based events to ensure **zero-allocation** publishing and maximum performance. 
> [!NOTE]
> It is essential to understand that the system is **synchronous by default**: an event is published and fully handled within the same frame, providing predictable code flow. For asynchronous needs, such as delays, specific extension methods are available.

<br>

### Event Types

**IEvent - Basic Events**
```csharp
using Echo.Interface;
using UnityEngine;

// An event carrying data about a player's death
public struct PlayerDiedEvent : IEvent
{
    public int PlayerId;
    public Vector3 Position;
}
```

<br>

**ITrackedEvent - Source/Target Events**
```csharp
using Echo.Interface;

public struct DamageEvent : ITrackedEvent
{
    public int SourceId { get; set; }  // Required by interface
    public int TargetId { get; set; }  // Required by interface
    public float Damage;
    public DamageType Type;
}
```

<br>

**Why Structs?**
- **Zero allocations**: No garbage collection pressure during event publishing
- **Memory efficiency**: Events are copied by value, no heap allocations
- **Performance**: Direct memory access with no indirection

**IEvent vs ITrackedEvent:**
- `IEvent`: Use for general game events (UI updates, state changes, notifications)
- `ITrackedEvent`: Use when you need to track relationships between entities (combat, interactions, AI communication)

<br>

### 1. Publishing Events

```csharp
using Echo.Core.Extensions;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int playerId = 1;

    public void Die()
    {
        // Method 1: Using extensions (recommended)
        new PlayerDiedEvent
        {
            PlayerId = this.playerId,
            Position = transform.position
        }.Fire(); // The Fire() extension publishes the event
        
        // Method 2: Direct publishing
        EventBus.Publish(new PlayerDiedEvent 
        { 
            PlayerId = this.playerId, 
            Position = transform.position 
        });
    }
}
```

### 2. Subscribing to Events

The standard pattern in Unity is to subscribe in `OnEnable()` and always unsubscribe in `OnDisable()` to prevent memory leaks.

```csharp
using Echo.Core;
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
        // VERY IMPORTANT: Unsubscribe to prevent memory leaks and errors
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnPlayerDied(PlayerDiedEvent eventData)
    {
        Debug.Log($"Player {eventData.PlayerId} died at {eventData.Position}!");
        // ... logic to handle the player's death (e.g., respawn, update UI) ...
    }
}
```

### 3. Event Markers - Zero-Data Events

Event markers are structs with no data, perfect for simple notifications:

```csharp
public struct GameStartedEvent : IEvent { }
public struct LevelCompletedEvent : IEvent { }
public struct PauseRequestedEvent : IEvent { }

// Usage
new GameStartedEvent().Fire();

// Subscribe
EventBus.Subscribe<GameStartedEvent>(OnGameStarted);

void OnGameStarted(GameStartedEvent evt)
{
    // evt parameter exists but contains no data
    InitializeGame();
}
```

**Why use Event Markers?**
- **Decoupling**: Systems don't need direct references to each other
- **Flexibility**: Easy to add new listeners without modifying existing code
- **Debugging**: Clear event flow in your game's architecture
- **Zero cost**: No memory overhead, just a type signature

<br>

## ♻️ Automatic Cleanup with Scoped Subscriptions

Forgetting to unsubscribe from an event in `OnDisable` is one of the most common sources of memory leaks and bugs in Unity. To make this process more robust, Echo offers a safer pattern based on the `IDisposable` interface.

The key insight is that this makes unsubscribing simpler and harder to get wrong. Instead of needing to manually call `Unsubscribe` with the exact same method reference, you just hold onto the subscription object and call its `Dispose()` method.

<br>

### 1. Use Case 1: Subscriptions Tied to a MonoBehaviour's Lifecycle

This is the most frequent scenario: a component needs to listen for an event as long as it's active (`OnEnable`) and must stop listening when it's disabled `OnDisable`).

The advantage here is that you no longer need to worry about which handler method you passed to `Subscribe`. Just call `.Dispose()` on the subscription object, and it cleans itself up.

```csharp
using Echo.Core;
using System;
using UnityEngine;

public class UINotificationManager : MonoBehaviour
{
    // Store the subscription object, which is "Disposable"
    private IDisposable _gameOverSubscription;

    void OnEnable()
    {
        // Subscribe to the event and keep the returned IDisposable object.
        _gameOverSubscription = EventBus.SubscribeScoped<GameOverEvent>(ShowGameOverScreen);
    }

    void OnDisable()
    {
        // By calling Dispose(), the object handles   
        // its own unsubscription from the EventBus.
        _gameOverSubscription?.Dispose();
    }

    private void ShowGameOverScreen(GameOverEvent evt)
    {
        // ... logic to show the Game Over screen ...
    }
}
```

<br>

### 2. Use Case 2: Temporary Subscriptions with `using` (Truly Automatic Cleanup)

There are times when you only need to listen for an event within a specific scope, like a single method or a coroutine. This is where the `IDisposable` pattern becomes incredibly powerful with C#'s `using` statement, which provides fully guaranteed and automatic cleanup.

As soon as the code execution leaves the **using** block—whether normally, through a `return`, or via an exception—the `Dispose()` method is called automatically.

Imagine a tutorial that waits for the player to perform a "jump" action, but only for a few seconds.

```csharp
using Echo.Core;
using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    // A coroutine that waits for a specific action
    public void PromptForJump()
    {
        StartCoroutine(WaitForJumpAction());
    }

    private System.Collections.IEnumerator WaitForJumpAction()
    {
        Debug.Log("Tutorial: Please jump now!");

        // We subscribe to 'PlayerJumpedEvent' only within this 'using' block.
        using var jumpSubscription = EventBus.SubscribeScoped<PlayerJumpedEvent>(OnPlayerJumped);

        // Wait for 5 seconds. The subscription is active during this time.
        yield return new WaitForSeconds(5f);

        // At the end of this yield, the method continues and the 'using' block ends.
        // 'jumpSubscription.Dispose()' is now called automatically,
        // which cleans up the subscription. If the player hasn't jumped in 5 seconds,
        // we stop listening.
    }

    private void OnPlayerJumped(PlayerJumpedEvent evt)
    {
        Debug.Log("Great! You jumped. Tutorial step complete.");
        // We can now stop the coroutine since the goal was achieved.
        StopCoroutine(nameof(WaitForJumpAction));
    }
}

// A simple event marker for this action
public struct PlayerJumpedEvent : IEvent { }
```

<br>

## 🎯 Tracked Events: Source & Target

For events where you need to know "who did what to whom" (e.g., combat, interactions), use `ITrackedEvent`. This interface adds `SourceId` and `TargetId` properties to your event.
It extends `IEvent` by adding two properties: `SourceId` and `TargetId`.

### 1. Define a Tracked Event
```csharp
using Echo.Interface;

public struct DamageDealtEvent : ITrackedEvent
{
    // Required by ITrackedEvent
    public int SourceId { get; set; }
    public int TargetId { get; set; }

    // Custom data
    public float DamageAmount;
}
```

<br>

### 2. Publish with Source and Target
Use the special extension methods to automatically populate the IDs from GameObjects.

```csharp
using Echo.Core.Extensions;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 25f;

    public void Attack(GameObject target)
    {
        new DamageDealtEvent { DamageAmount = damage }
            .FireFromTo(gameObject, target); // Sets SourceId and TargetId from GameObjects
    }
}
```

### 3. Subscribe with GameObject Helpers
You can easily subscribe to events that are sent from or to a specific GameObject.

```csharp
using Echo.Core.Extensions;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    void OnEnable()
    {
        // Subscribe to any damage event where this GameObject is the target
        gameObject.SubscribeToThis<DamageDealtEvent>(OnDamageReceived);
    }

    void OnDisable()
    {
        // Remember to unsubscribe to prevent memory leaks
        // Note: GameObject extensions don't have automatic cleanup
        EventBus.Unsubscribe<DamageDealtEvent>(OnDamageReceived);
    }

    private void OnDamageReceived(DamageDealtEvent evt)
    {
        Debug.Log($"Took {evt.DamageAmount} damage from entity {evt.SourceId}");
        // ... apply damage ...
    }
}
```

<br>

### How IDs Work: `GameObject.GetInstanceID()`

By default, Echo's helper extensions use Unity's built-in `GameObject.GetInstanceID()` method to populate the `SourceId` and `TargetId`.

`GetInstanceID()` returns a **unique integer** for every object that inherits from `UnityEngine.Object` (like GameObjects, Components, and Materials). This ID is guaranteed to be unique for the entire session your application is running, making it a fast and convenient way to reference specific object instances without passing direct object references.

<br>

> [!WARNING]
> A common source of errors is confusing the ID of a `GameObject` with the ID of one of its `Component`s. When your script inherits from `MonoBehaviour`, `this.GetInstanceID()` returns the unique ID of the **script component instance**, while `this.gameObject.GetInstanceID()` returns the unique ID of the **GameObject** it is attached to. These two IDs will **not** be the same. Be sure to use the correct one for your logic (Echo's helpers typically expect the GameObject's ID).

<br>

### Managing Complexity: A Note on ID Management

While using `GetInstanceID()` is efficient, it has a limitation: the ID is an arbitrary integer. A log stating that "Entity 1738 dealt damage to Entity 9254" is not very descriptive for debugging.

For more complex projects, you will likely want to implement your own system to map these instance IDs to more meaningful entities. Echo intentionally leaves this implementation to you, as every project's needs are different.

A common pattern is to create a central `EntityManager` or `Registry`:

1.  When an important entity (like a player, enemy, or interactive object) is created (`Awake` or `OnEnable`), it registers itself with the manager.
2.  The manager stores it in a `Dictionary<int, IGameEntity>`, using its `GetInstanceID()` as the key.
3.  When you receive a tracked event, you can pass the `SourceId` or `TargetId` to your manager to retrieve the actual `GameObject` or a custom entity class.

<br>

## 🔥 Advanced Subscriptions: Fluent Filtering

Create highly specific subscriptions with the fluent `Where<T>()` API. Chain conditions with `And()` and `Or()` to build complex logic without cluttering your handler methods.

```csharp
using Echo.Core;
using Echo.Core.Extensions; // Required for filter extensions
using UnityEngine;

public class SpecialEffectsManager : MonoBehaviour
{
    [SerializeField] private GameObject _player;

    void Start()
    {
        // Example 1: A complex chain combining AND/OR logic.
        // Listen for events where (the value is 100 AND the flag is true) OR (the value is over 200).
        EventBus.Where<GenericEventA>()
            .And(evt => evt.SomeValue == 100 && evt.SomeFlag == true)
            .Or(evt => evt.SomeValue > 200)
            .Subscribe(HandleComplexCondition);

        // Example 2: Filtering by source/target and specific values.
        // Listen for events sent FROM the player TO the enemy, where a specific tag matches.
        EventBus.Where<GenericTrackedEventB>()
            .FromSource(_player)
            .ToTarget(_enemy)
            .WithValue(evt => evt.Tag, "Interaction")
            .Subscribe(HandlePlayerToEnemyInteraction);

        // Example 3: Using a range and multiple source/target checks.
        // Listen for events where the source is the player OR another object,
        // the target is NOT the player, and a float value is within a specific range.
        EventBus.Where<GenericTrackedEventB>()
            .And(evt => evt.SourceId == _player.GetInstanceID() || evt.SourceId == _someOtherObject.GetInstanceID())
            .And(evt => evt.TargetId != _player.GetInstanceID())
            .WithRange(evt => evt.Amount, 10.5f, 50.0f)
            .Subscribe(HandleRangedEventFromMultipleSources);

        // Example 4: A temporary subscription with the 'using' block for automatic cleanup.
        // This listener is active only for the duration of this method. It filters events
        // that are between two specific objects or have a specific ID.
        using var tempSubscription = EventBus.Where<GenericTrackedEventB>()
            .Between(_player, _someOtherObject) // Helper for Source AND Target
            .Or(evt => evt.TargetId == _entityId)
            .SubscribeScoped(HandleTemporaryEvent);
        
        Debug.Log("Listeners configured. The temporary listener will now be disposed.");
    }

    ...
}
```

<br>

## 📦 Performance Tuning: Event Aggregation

For high-frequency events (like analytics or continuous damage), publishing every single event can be inefficient in some cases. The `EventAggregator` lets you collect events and publish them as a single batch.

### Collecting and Flushing Events

Use the `.Collect()` extension to add an event to a temporary buffer. Then, call `EventAggregator<T>.Flush()` to publish all collected events at once.

```csharp
using Echo.Core.Data;
using Echo.Core.Extensions;

public class AnalyticsManager : MonoBehaviour
{
    public void TrackPlayerAction(Vector3 position, string action)
    {
        // This event is not published immediately. It is collected.
        new PlayerActionEvent { Position = position, ActionName = action }.Collect();
    }

    // Call this periodically, or when the scene changes
    public void SendAnalyticsBatch()
    {
        // Publishes all collected PlayerActionEvent instances in one go
        EventAggregator<PlayerActionEvent>.Flush();
    }
}
```

<br>

You can also use `.CollectAndFlush(flushThreshold)` to automatically publish when the buffer reaches a certain size:

```csharp
public void TrackHighFrequencyEvent()
{
    // Auto-flush when 50 events are collected
    new AnalyticsEvent().CollectAndFlush(50);
}
```

<br>

> [!NOTE]
> The **EventAggregator** is a performance tool for **specific scenarios** and should not be treated as a default optimization. Firing an event directly with `.Fire()` is already extremely fast due to the zero-allocation nature of the system.
>
> The aggregator introduces its own small overhead by buffering events. This cost is only justified in very high-frequency situations (e.g., hundreds of events per frame from analytics, particle collisions, etc.). In these specific cases, the cost of making many individual calls to the event publishing system can become greater than the cost of buffering.
>
> **As a rule of thumb:** use direct publishing (`.Fire()`). Only consider using the aggregator if you have profiled your application and identified a clear bottleneck caused by an exceptionally high volume of events.

<br>

## 📖 More Features & API Highlights

### Event Extensions

A rich set of extension methods makes publishing expressive and powerful:

```csharp
// Fire only if a condition is met
new GameOverEvent().FireIf(currentHealth <= 0);

// Schedule an event for the next frame (requires a MonoBehaviour to start the coroutine)
StartCoroutine(new UiRefreshEvent().FireNextFrame());

// Fire after a 2-second delay
StartCoroutine(new BombExplodedEvent().FireDelayed(2.0f));

// Transform an event into another type before publishing
playerEvent.FireAs(evt => new UiUpdateEvent { PlayerId = evt.Id });

// Publish an entire array or list of events at once
DamageEvent[] damageBatch = GetDamageEvents();
damageBatch.FireBatch();
```

<br>

### Memory Management

The `EventAggregator` provides tools to manage memory for event buffers:

```csharp
// Pre-allocate buffer space if you know many events are coming
EventAggregator<MyEvent>.Reserve(1000);

// Free up unused memory after a batch is flushed
EventAggregator<MyEvent>.TrimExcess();

// Check the state of the aggregator
int pending = EventAggregator<MyEvent>.PendingCount;
int capacity = EventAggregator<MyEvent>.Capacity;
```

<br>

## 💡 Best Practices

### Event Design

```csharp
// ✅ Good: Struct with clear purpose
public struct PlayerLevelUpEvent : IEvent
{
    public int PlayerId;
    public int NewLevel;
    public int OldLevel;
}

// ❌ Avoid: Reference types
public struct PlayerEvent : IEvent
{
    public string PlayerName;
}
```

<br>

> [!NOTE]
> The **memory layout** and **size** of your event `struct` are critical for performance. The layout refers to how a struct's fields are arranged in memory by the C# compiler. By default, the compiler may add "padding" bytes between fields to ensure they align with the CPU's natural word size (e.g., aligning a 4-byte `int` on a 4-byte boundary). This can make a struct larger than the sum of its parts.
>
> **Why does this matter?** Because events are `struct`s, they are copied by value every time they are published. A larger struct means more data is copied to the stack, which consumes more CPU cycles. In high-frequency scenarios, this can become a noticeable overhead.
>
> **Recommendations:**
> * **Keep structs small and focused.** An event should carry only the essential data required by its listeners.
> * **Aim for a size under 64 bytes.** This is a common CPU cache line size. Keeping your struct within this limit can improve memory access patterns. You can check a struct's size with `sizeof(MyEventStruct)`.
> * **Order fields wisely.** For advanced optimization, you can use the `[StructLayout(LayoutKind.Explicit)]` attribute to control the exact memory layout and eliminate padding, but this is often unnecessary. A simpler trick is to declare fields from largest to smallest (e.g., `long`, `int`, `short`, `bool`) to help the compiler minimize padding naturally.

<br>

### Subscription Management

```csharp
public class GameSystem : MonoBehaviour
{
    private IDisposable _subscription;
    
    void OnEnable()
    {
        // ✅ Good: Use scoped subscriptions when possible
        _subscription = EventBus.SubscribeScoped<GameEvent>(HandleGameEvent);
    }
    
    void OnDisable()
    {
        // ✅ Good: Always clean up
        _subscription?.Dispose();
    }
}
```

<br>

### Performance Optimization

```csharp
// ✅ Good: Batch operations when possible
var events = new DamageEvent[100];
// ... populate events ...
events.FireBatch();

// ✅ Good: Use aggregation for high-frequency events
highFrequencyEvent.CollectAndFlush(50);

// ✅ Good: Reserve capacity for known workloads
EventAggregator<DamageEvent>.Reserve(1000);
```

<br>

## 🤝 Contributing & Supporting

This project is open-source under the **MIT License**, and any form of contribution is welcome and greatly appreciated!

If **Echo** helps you build a cleaner, more performant architecture in your projects, the best way to show your support is by **giving it a star ⭐️ on GitHub!** It helps a lot with visibility and motivates me to continue its development.

Here are other ways you can get involved:

* **💡 Share Ideas & Report Bugs:** Have a great idea for a new feature or found a potential performance issue? [Open an issue](https://github.com/Alaxxxx/Echo/issues) to share the details.
* **🔌 Contribute Code:** Feel free to fork the repository and submit a pull request for bug fixes or new features.
* **🗣️ Spread the Word:** Know other developers passionate about clean code and performance? Let them know about Echo!

Every contribution is incredibly valuable. Thank you for your support!
