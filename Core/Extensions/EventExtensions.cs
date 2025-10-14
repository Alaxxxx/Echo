using System;
using System.Collections;
using OpalStudio.Echo.Interface;
using UnityEngine;

namespace OpalStudio.Echo.Core.Extensions
{
      public static class EventExtensions
      {
            /// <summary>
            /// Publishes the specified event to the event bus.
            /// </summary>
            /// <typeparam name="T">The type of the event being published, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be published.</param>
            public static void Fire<T>(this T eventData) where T : struct, IEvent
            {
                  EventBus.Publish(eventData);
            }

            /// <summary>
            /// Executes the specified event after the given delay period and publishes it to the event bus.
            /// </summary>
            /// <typeparam name="T">The type of the event being published, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be published after the delay.</param>
            /// <param name="delay">The amount of time, in seconds, to wait before publishing the event.</param>
            /// <returns>A Unity coroutine enumerator that handles the delay execution of the event.</returns>
            /// <remarks>Allocates a new enumerator for the coroutine.</remarks>
            public static IEnumerator FireDelayed<T>(this T eventData, float delay) where T : struct, IEvent
            {
                  yield return new WaitForSeconds(delay);
                  EventBus.Publish(eventData);
            }

            /// <summary>
            /// Schedules the specified event to be published to the event bus on the next frame.
            /// </summary>
            /// <typeparam name="T">The type of the event being scheduled, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be scheduled for publishing.</param>
            /// <returns>An enumerator that waits for the next frame before publishing the event.</returns>
            public static IEnumerator FireNextFrame<T>(this T eventData) where T : struct, IEvent
            {
                  yield return null;
                  EventBus.Publish(eventData);
            }

            /// <summary>
            /// Transforms the specified event data and publishes the transformed event to the event bus.
            /// </summary>
            /// <typeparam name="T">The type of the source event, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <typeparam name="Tu">The type of the target event, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be transformed and published.</param>
            /// <param name="transform">A function that transforms the source event data into the target event data.</param>
            public static void FireAs<T, Tu>(this T eventData, Func<T, Tu> transform) where T : struct, IEvent where Tu : struct, IEvent
            {
                  EventBus.Publish(transform(eventData));
            }

            /// <summary>
            /// Publishes the specified event to the event bus if the given condition evaluates to true.
            /// </summary>
            /// <typeparam name="T">The type of the event being published, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be published.</param>
            /// <param name="condition">A boolean value indicating whether the event should be published.</param>
            public static void FireIf<T>(this T eventData, bool condition) where T : struct, IEvent
            {
                  if (condition)
                  {
                        EventBus.Publish(eventData);
                  }
            }

            /// <summary>
            /// Publishes the event data to the event bus if the specified condition evaluates to true.
            /// </summary>
            /// <typeparam name="T">The type of the event being published, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be published.</param>
            /// <param name="condition">A function that evaluates whether the event should be published.</param>
            /// <remarks>Allocates a new function delegate for the condition check.</remarks>
            public static void FireIf<T>(this T eventData, Func<bool> condition) where T : struct, IEvent
            {
                  if (condition())
                  {
                        EventBus.Publish(eventData);
                  }
            }

            /// <summary>
            /// Fires the event if the given predicate evaluates to true.
            /// </summary>
            /// <typeparam name="T">The type of the event, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data that may be fired.</param>
            /// <param name="predicate">A function that evaluates a condition against the given event data.</param>
            /// <remarks>Allocates a new function delegate for the predicate check.</remarks>
            public static void FireIf<T>(this T eventData, Func<T, bool> predicate) where T : struct, IEvent
            {
                  if (predicate(eventData))
                  {
                        EventBus.Publish(eventData);
                  }
            }

            /// <summary>
            /// Fires the specified tracked event from a source GameObject to a target GameObject.
            /// </summary>
            /// <typeparam name="T">The type of the tracked event being fired, which must implement <see cref="ITrackedEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be fired.</param>
            /// <param name="source">The source GameObject from which the event originates.</param>
            /// <param name="target">The target GameObject to which the event is directed. If null, the target is considered undefined.</param>
            public static void FireFromTo<T>(this T eventData, GameObject source, GameObject target) where T : struct, ITrackedEvent
            {
                  eventData.SourceId = source.GetInstanceID();
                  eventData.TargetId = target != null ? target.GetInstanceID() : -1;
                  EventBus.Publish(eventData);
            }

            /// <summary>
            /// Fires the specified tracked event from a source GameObject to a target identified by an optional target ID.
            /// </summary>
            /// <typeparam name="T">The type of the tracked event being published, which must implement <see cref="ITrackedEvent"/>.</typeparam>
            /// <param name="eventData">The event data to be published. Its source and target information will be updated.</param>
            /// <param name="source">The GameObject representing the source of the event.</param>
            /// <param name="targetId">The optional ID of the target the event is directed to. Defaults to -1 if not specified.</param>
            public static void FireFrom<T>(this T eventData, GameObject source, int targetId = -1) where T : struct, ITrackedEvent
            {
                  eventData.SourceId = source.GetInstanceID();
                  eventData.TargetId = targetId;
                  EventBus.Publish(eventData);
            }

            /// <summary>
            /// Fires the specified tracked event from the given source ID to the target GameObject by setting the appropriate source and target identifiers,
            /// then publishes the event to the event bus.
            /// </summary>
            /// <typeparam name="T">The type of the event to be fired, which must implement <see cref="ITrackedEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be fired.</param>
            /// <param name="target">The target GameObject to which the event is directed.</param>
            /// <param name="sourceId">The identifier of the source from which the event originates. Defaults to -1 if no specific source ID is provided.</param>
            public static void FireTo<T>(this T eventData, GameObject target, int sourceId = -1) where T : struct, ITrackedEvent
            {
                  eventData.SourceId = sourceId;
                  eventData.TargetId = target.GetInstanceID();
                  EventBus.Publish(eventData);
            }

            /// <summary>
            /// Publishes the specified event to the event bus and returns the event data.
            /// </summary>
            /// <typeparam name="T">The type of the event being published, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be published.</param>
            /// <returns>Returns the published event data.</returns>
            public static T FireAndReturn<T>(this T eventData) where T : struct, IEvent
            {
                  EventBus.Publish(eventData);

                  return eventData;
            }
      }
}