using System;
using OpalStudio.Echo.Core.Data;
using OpalStudio.Echo.Core.Diagnostics;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core
{
      /// <summary>
      /// The Echo event bus. Provides a static, type-safe, zero-allocation
      /// publish/subscribe API built on top of <c>struct</c> events.
      /// </summary>
      public static class EventBus
      {
#region Publish

            /// <summary>
            /// Publishes an event of the specified type to all subscribers.
            /// </summary>
            /// <typeparam name="T">The type of the event to be published. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            /// <param name="eventData">The event data instance to be published to subscribers.</param>
            public static void Publish<T>(T eventData) where T : struct, IEvent
            {
                  EventChannel<T>.Invoke(eventData);
            }

#endregion

#region Subscribe

            /// <summary>
            /// Subscribes to events of the specified type by adding the given action as a handler.
            /// </summary>
            /// <typeparam name="T">The type of the event to subscribe to. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            /// <param name="action">The action to execute when an event of type <typeparamref name="T"/> is published.</param>
            public static void Subscribe<T>(Action<T> action) where T : struct, IEvent
            {
                  EventChannel<T>.OnEvent += action;
                  EchoInstrumentation.NotifySubscribe(typeof(T), action, SubscriptionKind.Plain);
            }

            /// <summary>
            /// Subscribes to an event of the specified type with a scoped subscription.
            /// The returned <see cref="ScopedSubscription{T}"/> implements
            /// <see cref="IDisposable"/> and, when disposed, automatically
            /// unsubscribes the handler.
            /// </summary>
            /// <typeparam name="T">The type of the event to subscribe to. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            /// <param name="action">The action to invoke when the event of type <typeparamref name="T"/> is raised.</param>
            /// <returns>A <see cref="ScopedSubscription{T}"/> instance that manages the scoped subscription.</returns>
            public static ScopedSubscription<T> SubscribeScoped<T>(Action<T> action) where T : struct, IEvent
            {
                  EventChannel<T>.OnEvent += action;
                  EchoInstrumentation.NotifySubscribe(typeof(T), action, SubscriptionKind.Scoped);

                  return new ScopedSubscription<T>(action);
            }

#endregion

#region Unsubscribe

            /// <summary>
            /// Unsubscribes an action from receiving notifications for events of the specified type.
            /// Calling this with an action that is not currently subscribed is a no-op.
            /// </summary>
            /// <typeparam name="T">The type of the event. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            /// <param name="action">The action to be unsubscribed from the event notifications.</param>
            public static void Unsubscribe<T>(Action<T> action) where T : struct, IEvent
            {
                  EventChannel<T>.OnEvent -= action;
                  EchoInstrumentation.NotifyUnsubscribe(typeof(T), action);
            }

            /// <summary>
            /// Unsubscribes all actions from the specified event type, across
            /// every layer (Core handlers and Unity-filtered handlers alike).
            /// </summary>
            /// <typeparam name="T">The type of the event to unsubscribe from. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            public static void UnsubscribeAll<T>() where T : struct, IEvent
            {
                  _ = EventChannel<T>.HasSubscribers;
                  EventChannelRegistry.ClearType(typeof(T));
            }

            /// <summary>
            /// Unsubscribes all actions from every event channel known to the bus.
            /// Intended for tests, end-of-session cleanup, or full state resets.
            /// </summary>
            public static void UnsubscribeAll()
            {
                  EventChannelRegistry.ClearAll();
            }

#endregion

#region Query

            /// <summary>
            /// Returns <c>true</c> if at least one handler is currently subscribed
            /// to events of type <typeparamref name="T"/>.
            /// </summary>
            public static bool HasSubscribers<T>() where T : struct, IEvent
            {
                  return EventChannel<T>.HasSubscribers;
            }

#endregion
      }
}