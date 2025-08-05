# Echo: High-Performance Event Bus for Unity

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Version: 1.0.0](https://img.shields.io/badge/Version-1.0.0-blue.svg)](https://github.com/Alaxxxx/Echo)
[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-green.svg)](https://unity3d.com/get-unity/download)

**Echo** is a static, high-performance, and type-safe event bus system designed for Unity. It leverages `struct`-based events to achieve **zero-allocation publishing**,
eliminating garbage collector spikes and ensuring smooth performance, even in high-frequency scenarios.

With a fluent filtering API, automatic subscription management, and seamless integration with GameObjects, Echo provides a powerful yet simple solution for creating
decoupled and maintainable code architecture in your projects.

## ✨ Key Features

- **🚀 Zero-Allocation Publishing**: Utilizes `structs` for events to prevent heap allocations and GC pressure.
- **🔒 Type-Safe by Design**: Generic, interface-constrained API prevents runtime errors by ensuring event correctness at compile time.
- **🎯 Advanced Filtering API**: A fluent, chainable interface (`Where(...).And(...).Or(...)`) to subscribe to events that meet complex conditions.
- **♻️ Automatic Subscription Management**: Scoped subscriptions (`IDisposable`) handle cleanup automatically, preventing common memory leaks.
- **📦 Event Batching & Aggregation**: Collect high-frequency events and publish them in batches to optimize performance.
- **🎮 Deep Unity Integration**: Helper extensions for GameObjects allow for easy source/target event tracking.

## 🚀 Installation

<details>
<summary><strong>1. Install via Git URL (Recommended)</strong></summary>
<br>
This method installs the package directly from the GitHub repository and allows you to easily update to the latest version.

1. In Unity, open the **Package Manager** (`Window > Package Management > Package Manager`).
2. Click the **+** button in the top-left corner and select **"Add package from git URL..."**.
3. Enter the following URL and click "Install":
   ```
   https://github.com/Alaxxxx/Echo.git
   ```

</details>

<details>
<summary><strong>2. Install via .unitypackage</strong></summary>
<br>
This method is great if you prefer a specific, stable version of the asset.

1. Go to the [**Releases**](https://github.com/Alaxxxx/Echo/releases) page.
2. Download the `.unitypackage` file from the latest release.
3. In your Unity project, go to **`Assets > Import Package > Custom Package...`** and select the downloaded file.

</details>

<details>
<summary><strong>3. Manual Installation (from .zip)</strong></summary>
<br>

1. Download this repository as a ZIP file by clicking **`Code > Download ZIP`** on the main repository page.
2. Unzip the downloaded file.
3. Drag and drop the main asset folder (the one containing all the scripts and resources) into the `Assets` folder of your Unity project.

</details>

**Requirements:**

- Unity 2021.3 or higher
- .NET Standard 2.1 or higher

## ⚡ Getting Started: The Basics

Using Echo involves three simple steps: defining, publishing, and listening to events.

### Understanding the Event System

Echo Event Bus is built around **struct-based events** that implement specific interfaces. This design choice ensures zero-allocation publishing and maximum performance.

#### Event Types

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

**Why Structs?**

- **Zero allocations**: No garbage collection pressure during event publishing
- **Memory efficiency**: Events are copied by value, no heap allocations
- **Performance**: Direct memory access with no indirection

**IEvent vs ITrackedEvent:**

- `IEvent`: Use for general game events (UI updates, state changes, notifications)
- `ITrackedEvent`: Use when you need to track relationships between entities (combat, interactions, AI communication)

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

#### 3. Event Markers - Zero-Data Events

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

## ♻️ Automatic Cleanup with Scoped Subscriptions

Forgetting to unsubscribe is a common source of bugs. Echo provides a safer, IDisposable-based pattern for subscriptions that handles cleanup automatically.

```csharp
using Echo.Core;
using System;
using UnityEngine;

public class UINotificationManager : MonoBehaviour
{
    // Store the subscription object
    private IDisposable _gameOverSubscription;

    void OnEnable()
    {
        // Subscribe and store the disposable object
        _gameOverSubscription = EventBus.SubscribeScoped<GameOverEvent>(ShowGameOverScreen);
    }

    void OnDisable()
    {
        // Dispose the subscription to unsubscribe automatically
        _gameOverSubscription?.Dispose();
    }

    private void ShowGameOverScreen(GameOverEvent evt)
    {
        // ... show UI ...
    }
}
```

## 🎯 Tracked Events: Source & Target

For events where you need to know "who did what to whom" (e.g., combat, interactions), use `ITrackedEvent`. This interface adds `SourceId` and `TargetId` properties to
your event.

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
    public DamageType Type;
}
```

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

## 🔥 Advanced Subscriptions: Fluent Filtering

Create highly specific subscriptions with the fluent `Where<T>()` API. Chain conditions with `And()` and `Or()` to build complex logic without cluttering your handler
methods.

```csharp
using Echo.Core;
using Echo.Core.Extensions; // Required for filter extensions
using UnityEngine;

public class SpecialEffectsManager : MonoBehaviour
{
    [SerializeField] private GameObject _localPlayer;

    void Start()
    {
        // Example 1: Play a sound for magic damage events with more than 50 damage
        EventBus.Where<DamageDealtEvent>()
            .WithValue(evt => evt.Type, DamageType.Magic) // Extension method
            .And(evt => evt.DamageAmount > 50)           // Custom lambda
            .Subscribe(PlayBigMagicHitSound);

        // Example 2: Show critical hit indicator if this player lands a critical hit
        EventBus.Where<CriticalHitEvent>()
            .FromSource(_localPlayer) // Filter by source GameObject
            .Subscribe(ShowCriticalHitIndicator);
            
        // Example 3: Subscribe with automatic cleanup
        using var sub = EventBus.Where<SpecialEvent>()
            .WithRange(evt => evt.Value, 100, 200)
            .SubscribeScoped(HandleSpecialEvent);
    }
    
    void PlayBigMagicHitSound(DamageDealtEvent evt)
    {
        // Play appropriate sound effect
    }
    
    void ShowCriticalHitIndicator(CriticalHitEvent evt)
    {
        // Show UI feedback
    }
    
    void HandleSpecialEvent(SpecialEvent evt)
    {
        // Handle the special event
    }
}
```

## 📦 Performance Tuning: Event Aggregation

For high-frequency events (like analytics or continuous damage), publishing every single event can be inefficient. The `EventAggregator` lets you collect events and
publish them as a single batch.

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

You can also use `.CollectAndFlush(flushThreshold)` to automatically publish when the buffer reaches a certain size:

```csharp
public void TrackHighFrequencyEvent()
{
    // Auto-flush when 50 events are collected
    new AnalyticsEvent().CollectAndFlush(50);
}
```

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
public class PlayerEvent : IEvent  // Don't use classes
{
    public string PlayerName;
}
```

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

## 🎯 Examples

### Example 1: Combat System

```csharp
// Define events
public struct AttackEvent : ITrackedEvent
{
    public int SourceId { get; set; }
    public int TargetId { get; set; }
    public float Damage;
    public WeaponType Weapon;
}

public struct HealthChangedEvent : IEvent
{
    public int EntityId;
    public float OldHealth;
    public float NewHealth;
}

// Combat manager
public class CombatManager : MonoBehaviour
{
    void Start()
    {
        // Subscribe to attack events
        EventBus.Subscribe<AttackEvent>(ProcessAttack);
        
        // Subscribe to critical hits only
        EventBus.Where<AttackEvent>()
            .WithRange(evt => evt.Damage, 100f, float.MaxValue)
            .Subscribe(OnCriticalHit);
    }
    
    void ProcessAttack(AttackEvent attack)
    {
        var target = FindEntityById(attack.TargetId);
        if (target != null)
        {
            var oldHealth = target.Health;
            target.Health -= attack.Damage;
            
            // Fire health changed event
            new HealthChangedEvent
            {
                EntityId = attack.TargetId,
                OldHealth = oldHealth,
                NewHealth = target.Health
            }.Fire();
        }
    }
}

// Weapon script
public class Weapon : MonoBehaviour
{
    public float damage = 25f;
    
    public void Attack(GameObject target)
    {
        new AttackEvent
        {
            Damage = damage,
            Weapon = WeaponType.Sword
        }.FireFromTo(gameObject, target);
    }
}
```

### Example 2: UI System

```csharp
// UI Events
public struct UIUpdateEvent : IEvent
{
    public UIElementType ElementType;
    public object Data;
}

public struct PlayerStatsChangedEvent : IEvent
{
    public int PlayerId;
    public PlayerStats Stats;
}

// UI Manager
public class UIManager : MonoBehaviour
{
    [SerializeField] private Text healthText;
    [SerializeField] private Text scoreText;
    
    void Start()
    {
        // Subscribe to player stats changes
        EventBus.Where<PlayerStatsChangedEvent>()
            .WithValue(evt => evt.PlayerId, GameManager.LocalPlayerId)
            .Subscribe(UpdatePlayerUI);
            
        // Subscribe to UI-specific updates
        EventBus.Where<UIUpdateEvent>()
            .WithValue(evt => evt.ElementType, UIElementType.Health)
            .Subscribe(UpdateHealthDisplay);
    }
    
    void UpdatePlayerUI(PlayerStatsChangedEvent evt)
    {
        healthText.text = $"Health: {evt.Stats.Health}";
        scoreText.text = $"Score: {evt.Stats.Score}";
    }
}
```

### Example 3: Analytics System

```csharp
// Analytics events
public struct PlayerActionEvent : IEvent
{
    public string ActionName;
    public Vector3 Position;
    public float Timestamp;
}

// Analytics collector
public class AnalyticsManager : MonoBehaviour
{
    private const int BATCH_SIZE = 50;
    
    void Start()
    {
        // Collect all player actions
        EventBus.Subscribe<PlayerActionEvent>(CollectAnalytics);
        
        // Auto-flush every minute
        InvokeRepeating(nameof(FlushAnalytics), 60f, 60f);
    }
    
    void CollectAnalytics(PlayerActionEvent evt)
    {
        // Collect with auto-flush
        evt.CollectAndFlush(BATCH_SIZE);
    }
    
    void FlushAnalytics()
    {
        // Manual flush for time-based batching
        EventAggregator<PlayerActionEvent>.Flush();
    }
}
```