using System;
using OpalStudio.Echo.Core.Data;
using OpalStudio.Echo.Interface;
using UnityEngine;

namespace OpalStudio.Echo.Unity.Extensions
{
      public static class GameObjectEventExtensions
      {
            public static void SubscribeFromThis<T>(this GameObject gameObject, Action<T> action) where T : struct, ITrackedEvent
            {
                  EventBus.SubscribeFromSource(action, gameObject.GetInstanceID());
            }

            public static void SubscribeToThis<T>(this GameObject gameObject, Action<T> action) where T : struct, ITrackedEvent
            {
                  EventBus.SubscribeToTarget(action, gameObject.GetInstanceID());
            }

            public static ScopedSubscription<T> SubscribeFromThisScoped<T>(this GameObject gameObject, Action<T> action) where T : struct, ITrackedEvent
            {
                  int sourceId = gameObject.GetInstanceID();
                  Action<T> wrapper = evt =>
                  {
                        if (evt.SourceId == sourceId)
                        {
                              action(evt);
                        }
                  };

                  return Core.EventBus.SubscribeScoped(wrapper);
            }

            public static ScopedSubscription<T> SubscribeToThisScoped<T>(this GameObject gameObject, Action<T> action) where T : struct, ITrackedEvent
            {
                  int targetId = gameObject.GetInstanceID();
                  Action<T> wrapper = evt =>
                  {
                        if (evt.TargetId == targetId)
                        {
                              action(evt);
                        }
                  };

                  return Core.EventBus.SubscribeScoped(wrapper);
            }
      }
}