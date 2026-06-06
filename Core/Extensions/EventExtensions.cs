using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core.Extensions
{
      /// <summary>
      /// Platform-agnostic publish helpers. These extensions live in the Core
      /// assembly and have no dependency on UnityEngine.
      /// </summary>
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
      }
}