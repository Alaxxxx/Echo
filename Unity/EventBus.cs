using System;
using OpalStudio.Echo.Core.Diagnostics;
using OpalStudio.Echo.Interface;
using OpalStudio.Echo.Unity.Data;

namespace OpalStudio.Echo.Unity
{
      public static partial class EventBus
      {
            public static void SubscribeFiltered<T>(Action<T> action, Func<T, bool> filter) where T : struct, IEvent
            {
                  EventChannel<T>.AddFilteredHandler(action, filter);
                  EchoInstrumentation.NotifySubscribe(typeof(T), action, SubscriptionKind.Filtered);
            }

            public static FilteredSubscription<T> SubscribeFilteredScoped<T>(Action<T> action, Func<T, bool> filter) where T : struct, IEvent
            {
                  int id = EventChannel<T>.AddFilteredHandler(action, filter);
                  EchoInstrumentation.NotifySubscribe(typeof(T), action, SubscriptionKind.Filtered);

                  return new FilteredSubscription<T>(id);
            }

            public static void SubscribeFromSource<T>(Action<T> action, int sourceId) where T : struct, ITrackedEvent
            {
                  EventChannel<T>.AddFilteredHandler(action, evt => evt.SourceId == sourceId);
                  EchoInstrumentation.NotifySubscribe(typeof(T), action, SubscriptionKind.Filtered);
            }

            public static void SubscribeToTarget<T>(Action<T> action, int targetId) where T : struct, ITrackedEvent
            {
                  EventChannel<T>.AddFilteredHandler(action, evt => evt.TargetId == targetId);
                  EchoInstrumentation.NotifySubscribe(typeof(T), action, SubscriptionKind.Filtered);
            }

            public static void SubscribeFromTo<T>(Action<T> action, int sourceId, int targetId) where T : struct, ITrackedEvent
            {
                  EventChannel<T>.AddFilteredHandler(action, evt => evt.SourceId == sourceId && evt.TargetId == targetId);
                  EchoInstrumentation.NotifySubscribe(typeof(T), action, SubscriptionKind.Filtered);
            }

            public static void SubscribeOnce<T>(Action<T> action) where T : struct, IEvent
            {
                  Action<T> wrapper = null;

                  wrapper = evt =>
                  {
                        Core.EventBus.Unsubscribe(wrapper);
                        action(evt);
                  };

                  Core.EventBus.Subscribe(wrapper);
                  EchoInstrumentation.NotifySubscribe(typeof(T), action, SubscriptionKind.Once);
            }

            public static EventFilterBuilder<T> Where<T>() where T : struct, IEvent
            {
                  return default;
            }
      }
}