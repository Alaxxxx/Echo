using System;
using OpalStudio.Echo.Core.Data;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core
{
      /// <summary>
      /// A static class that handles the publish-subscribe pattern for event handling.
      /// Provides methods to publish events, batch publish events, and manage subscriptions to events.
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
                  Events<T>.Invoke(eventData);
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
                  Events<T>.OnEvent += action;
            }

            /// <summary>
            /// Subscribes to an event of the specified type with a scoped subscription.
            /// This ensures the subscription is automatically disposed when the
            /// corresponding <see cref="ScopedSubscription{T}"/> object is disposed of.
            /// </summary>
            /// <typeparam name="T">The type of the event to subscribe to. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            /// <param name="action">The action to invoke when the event of type <typeparamref name="T"/> is raised.</param>
            /// <returns>A <see cref="ScopedSubscription{T}"/> instance that manages the scoped subscription.</returns>
            public static ScopedSubscription<T> SubscribeScoped<T>(Action<T> action) where T : struct, IEvent
            {
                  Events<T>.OnEvent += action;

                  return new ScopedSubscription<T>(action);
            }

            /// <summary>
            /// Subscribes to events of the specified type with an additional filtering condition.
            /// The provided action will only be invoked if the event satisfies the filter predicate.
            /// </summary>
            /// <typeparam name="T">The type of the event to be subscribed to. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            /// <param name="action">The action to perform when an event of type <typeparamref name="T"/> is received and meets the filter condition.</param>
            /// <param name="filter">The predicate to determine whether the action should be executed for a given event.
            /// The event will only trigger the action if this predicate returns true.</param>
            public static void SubscribeFiltered<T>(Action<T> action, Func<T, bool> filter) where T : struct, IEvent
            {
                  Events<T>.AddFilteredHandler(action, filter);
            }

            /// <summary>
            /// Subscribes to events of the specified type with a scoped subscription, using a filter to determine which events to handle.
            /// </summary>
            /// <typeparam name="T">The type of the event to subscribe to. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            /// <param name="action">The action to execute when an event of type <typeparamref name="T"/> is received and matches the filter.</param>
            /// <param name="filter">The filter function used to determine whether an event should be handled.</param>
            /// <returns>A <see cref="FilteredSubscription{T}"/> object representing the scoped subscription.</returns>
            public static FilteredSubscription<T> SubscribeFilteredScoped<T>(Action<T> action, Func<T, bool> filter) where T : struct, IEvent
            {
                  int id = Events<T>.AddFilteredHandler(action, filter);

                  return new FilteredSubscription<T>(id);
            }

            /// <summary>
            /// Subscribes to events of the specified type from a specific source.
            /// Only events matching the given source identifier will trigger the action.
            /// </summary>
            /// <typeparam name="T">The type of the event to subscribe to. Must be a struct implementing <see cref="ITrackedEvent"/>.</typeparam>
            /// <param name="action">The action to invoke when an event matching the source is received.</param>
            /// <param name="sourceId">The identifier of the source to filter events from.</param>
            public static void SubscribeFromSource<T>(Action<T> action, int sourceId) where T : struct, ITrackedEvent
            {
                  Events<T>.AddFilteredHandler(action, evt => evt.SourceId == sourceId);
            }

            /// <summary>
            /// Subscribes to events of the specified type that are associated with the provided target identifier.
            /// Triggers the specified action for all matching events when they are published.
            /// </summary>
            /// <typeparam name="T">The type of the event to subscribe to. Must be a struct implementing <see cref="ITrackedEvent"/>.</typeparam>
            /// <param name="action">The action to invoke when an event matching the target identifier is published.</param>
            /// <param name="targetId">The identifier of the target for which events should be handled.</param>
            public static void SubscribeToTarget<T>(Action<T> action, int targetId) where T : struct, ITrackedEvent
            {
                  Events<T>.AddFilteredHandler(action, evt => evt.TargetId == targetId);
            }

            /// <summary>
            /// Subscribes to events of the specified type that match the given source and target identifiers.
            /// </summary>
            /// <typeparam name="T">The type of the event to subscribe to. Must be a struct implementing <see cref="ITrackedEvent"/>.</typeparam>
            /// <param name="action">The callback to invoke when an event matching the source and target identifiers is received.</param>
            /// <param name="sourceId">The identifier of the event source to filter subscriptions.</param>
            /// <param name="targetId">The identifier of the event target to filter subscriptions.</param>
            public static void SubscribeFromTo<T>(Action<T> action, int sourceId, int targetId) where T : struct, ITrackedEvent
            {
                  Events<T>.AddFilteredHandler(action, evt => evt.SourceId == sourceId && evt.TargetId == targetId);
            }

#endregion

#region Unsubscribe

            /// <summary>
            /// Unsubscribes an action from receiving notifications for events of the specified type.
            /// </summary>
            /// <typeparam name="T">The type of the event. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            /// <param name="action">The action to be unsubscribed from the event notifications.</param>
            public static void Unsubscribe<T>(Action<T> action) where T : struct, IEvent
            {
                  Events<T>.OnEvent -= action;
            }

            /// <summary>
            /// Unsubscribes all actions and filters from the current event type.
            /// </summary>
            /// <typeparam name="T">The type of the event to unsubscribe from. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            public static void UnsubscribeAll<T>() where T : struct, IEvent
            {
                  Events<T>.Clear();
            }

#endregion

            /// <summary>
            /// Creates a filter for events of the specified type, enabling fluent filtering and subscription configuration.
            /// </summary>
            /// <typeparam name="T">The type of the event for which the filter is being created. Must be a struct implementing <see cref="IEvent"/>.</typeparam>
            /// <returns>An instance of <see cref="EventFilterBuilder{T}"/> to configure advanced filtering and subscription behaviors.</returns>
            public static EventFilterBuilder<T> Where<T>() where T : struct, IEvent
            {
                  return new EventFilterBuilder<T>();
            }
      }
}