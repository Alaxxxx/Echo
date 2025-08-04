using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Echo.Interface;
using UnityEngine;

namespace Echo.Core
{
      public static class EventExtensions
      {
            /// <summary>
            /// Publishes the specified event to the event bus.
            /// </summary>
            /// <typeparam name="T">The type of the event being published, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be published.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static System.Collections.IEnumerator FireDelayed<T>(this T eventData, float delay) where T : struct, IEvent
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static System.Collections.IEnumerator FireNextFrame<T>(this T eventData) where T : struct, IEvent
            {
                  yield return null;
                  EventBus.Publish(eventData);
            }

            /// <summary>
            /// Transforms the specified event data and publishes the transformed event to the event bus.
            /// </summary>
            /// <typeparam name="T">The type of the source event, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <typeparam name="TU">The type of the target event, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be transformed and published.</param>
            /// <param name="transform">A function that transforms the source event data into the target event data.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void FireAs<T, TU>(this T eventData, Func<T, TU> transform) where T : struct, IEvent where TU : struct, IEvent
            {
                  EventBus.Publish(transform(eventData));
            }

            /// <summary>
            /// Publishes the specified event to the event bus if the given condition evaluates to true.
            /// </summary>
            /// <typeparam name="T">The type of the event being published, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be published.</param>
            /// <param name="condition">A boolean value indicating whether the event should be published.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T FireAndReturn<T>(this T eventData) where T : struct, IEvent
            {
                  EventBus.Publish(eventData);

                  return eventData;
            }

            /// <summary>
            /// Publishes a batch of events to the event bus.
            /// </summary>
            /// <typeparam name="T">The type of events being published, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="events">An array of event data to be published.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void FireBatch<T>(this T[] events) where T : struct, IEvent
            {
                  EventBus.PublishBatch(events);
            }

            /// <summary>
            /// Publishes a batch of events to the event bus.
            /// </summary>
            /// <param name="events">The batch of events to be published, provided as a read-only span of event data.</param>
            /// <typeparam name="T">The type of the events being published, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void FireBatch<T>(this ReadOnlySpan<T> events) where T : struct, IEvent
            {
                  EventBus.PublishBatch(events);
            }

            /// <summary>
            /// Collects the specified event data for later aggregation or processing.
            /// </summary>
            /// <typeparam name="T">The type of the event being collected, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be collected.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Collect<T>(this T eventData) where T : struct, IEvent
            {
                  EventAggregator<T>.Collect(eventData);
            }

            /// <summary>
            /// Collects the specified event and flushes the events when the collection reaches the specified threshold.
            /// </summary>
            /// <typeparam name="T">The type of the event being collected, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="eventData">The event data to be collected.</param>
            /// <param name="flushThreshold">The threshold of collected events at which a flush will occur. Defaults to 100 if not specified.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void CollectAndFlush<T>(this T eventData, int flushThreshold = 100) where T : struct, IEvent
            {
                  EventAggregator<T>.CollectWithAutoFlush(eventData, flushThreshold);
            }

            /// <summary>
            /// Collects and aggregates a sequence of events to be processed together.
            /// </summary>
            /// <typeparam name="T">The type of events to collect, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="events">The sequence of events to collect.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void CollectAll<T>(this IEnumerable<T> events) where T : struct, IEvent
            {
                  foreach (T e in events)
                  {
                        EventAggregator<T>.Collect(e);
                  }
            }

            /// <summary>
            /// Collects a batch of events and aggregates them for processing.
            /// </summary>
            /// <typeparam name="T">The type of the events being collected, which must implement <see cref="IEvent"/> and be a value type.</typeparam>
            /// <param name="events">The read-only span of events to be collected.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void CollectAll<T>(this ReadOnlySpan<T> events) where T : struct, IEvent
            {
                  EventAggregator<T>.CollectBatch(events);
            }
      }
}