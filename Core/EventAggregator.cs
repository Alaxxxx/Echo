using System;
using System.Runtime.CompilerServices;
using Echo.Interface;

namespace Echo.Core
{
      /// <summary>
      /// A static, generic class for aggregating and managing events of type <typeparamref name="T"/>.
      /// </summary>
      /// <typeparam name="T">
      /// The type of event to be managed, which must be a value type and implement the <see cref="IEvent"/> interface.
      /// </typeparam>
      /// <remarks>
      /// The EventAggregator provides methods for collecting, batching, reserving, flushing, and managing events,
      /// while optimizing memory usage with operations such as auto-flushing and buffer resizing.
      /// </remarks>
      public static class EventAggregator<T> where T : struct, IEvent
      {
            private static T[] buffer;
            private static int count;
            private static int capacity;

            public static int PendingCount => count;
            public static int Capacity => capacity;

            /// <summary>
            /// Adds a single event of type <typeparamref name="T"/> to the internal event buffer, expanding capacity if necessary.
            /// </summary>
            /// <param name="eventData">
            /// The event data to be collected. Must be a value type and implement the <see cref="IEvent"/> interface.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Collect(T eventData)
            {
                  EnsureCapacity(1);
                  buffer[count++] = eventData;
            }

            /// <summary>
            /// Publishes all collected events of type <typeparamref name="T"/> to the event bus as a single batch
            /// and clears the internal buffer. Does nothing if the buffer is empty.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Flush()
            {
                  if (count > 0)
                  {
                        var span = new ReadOnlySpan<T>(buffer, 0, count);
                        EventBus.PublishBatch(span);
                        count = 0;
                  }
            }

            /// <summary>
            /// Clears all events currently stored in the internal event buffer.
            /// </summary>
            /// <remarks>
            /// This method resets the count of pending events to zero without
            /// modifying the buffer capacity. This is useful for discarding
            /// accumulated events without reallocating the buffer.
            /// </remarks>
            public static void Clear()
            {
                  count = 0;
            }

            /// <summary>
            /// Adds a single event of type <typeparamref name="T"/> to the internal event buffer and automatically flushes the buffer if the specified threshold is reached.
            /// </summary>
            /// <param name="eventData">
            /// The event data to be collected. Must be a value type and implement the <see cref="IEvent"/> interface.
            /// </param>
            /// <param name="flushThreshold">
            /// The number of collected events that triggers an automatic flush. Defaults to 100.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void CollectWithAutoFlush(T eventData, int flushThreshold = 100)
            {
                  Collect(eventData);

                  if (count >= flushThreshold)
                  {
                        Flush();
                  }
            }

            /// <summary>
            /// Adds a batch of events of type <typeparamref name="T"/> to the internal event buffer, expanding capacity if necessary.
            /// </summary>
            /// <param name="events">
            /// A read-only span containing the batch of events to be collected. Each event must be a value type and implement the <see cref="IEvent"/> interface.
            /// </param>
            public static void CollectBatch(ReadOnlySpan<T> events)
            {
                  EnsureCapacity(events.Length);

                  events.CopyTo(buffer.AsSpan(count));
                  count += events.Length;
            }

            /// <summary>
            /// Reserves enough capacity in the internal event buffer to store the specified number of events.
            /// If the current capacity is less than the specified quantity, the buffer is resized accordingly.
            /// </summary>
            /// <param name="quantity">
            /// The minimum capacity required in the buffer. Must be greater than or equal to zero.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Reserve(int quantity)
            {
                  if (buffer == null || quantity > capacity)
                  {
                        capacity = quantity;

                        if (buffer == null)
                        {
                              buffer = new T[quantity];
                        }
                        else
                        {
                              Array.Resize(ref buffer, quantity);
                        }
                  }
            }

            /// <summary>
            /// Reduces the capacity of the internal buffer to minimize memory usage,
            /// ensuring that it retains only the necessary capacity to hold the current pending events.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void TrimExcess()
            {
                  if (count == capacity)
                  {
                        return;
                  }

                  int newCapacity = count;

                  if (newCapacity < 16)
                  {
                        newCapacity = 16;
                  }

                  if (newCapacity < capacity)
                  {
                        capacity = newCapacity;
                        Array.Resize(ref buffer, capacity);
                  }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void EnsureCapacity(int additionalCount)
            {
                  if (buffer == null)
                  {
                        capacity = 16;
                        buffer = new T[capacity];
                  }
                  else if (count + additionalCount > capacity)
                  {
                        capacity = Math.Max(capacity * 2, count + additionalCount);
                        Array.Resize(ref buffer, capacity);
                  }
            }
      }
}