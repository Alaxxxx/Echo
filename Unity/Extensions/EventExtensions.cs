using System;
using System.Collections;
using OpalStudio.Echo.Interface;
using UnityEngine;

namespace OpalStudio.Echo.Unity.Extensions
{
      public static class EventExtensions
      {
            public static IEnumerator FireDelayed<T>(this T eventData, float delay) where T : struct, IEvent
            {
                  yield return new WaitForSeconds(delay);
                  Core.EventBus.Publish(eventData);
            }

            public static IEnumerator FireNextFrame<T>(this T eventData) where T : struct, IEvent
            {
                  yield return null;
                  Core.EventBus.Publish(eventData);
            }

            public static void FireAs<T, TU>(this T eventData, Func<T, TU> transform) where T : struct, IEvent where TU : struct, IEvent
            {
                  Core.EventBus.Publish(transform(eventData));
            }

            public static void FireIf<T>(this T eventData, Func<bool> condition) where T : struct, IEvent
            {
                  if (condition())
                  {
                        Core.EventBus.Publish(eventData);
                  }
            }

            public static void FireFromTo<T>(this T eventData, GameObject source, GameObject target) where T : struct, ITrackedEvent
            {
                  if (source == null)
                  {
                        throw new ArgumentNullException(nameof(source));
                  }

                  eventData.SourceId = source.GetInstanceID();
                  eventData.TargetId = target != null ? target.GetInstanceID() : -1;
                  Core.EventBus.Publish(eventData);
            }

            public static void FireFrom<T>(this T eventData, GameObject source, int targetId = -1) where T : struct, ITrackedEvent
            {
                  if (source == null)
                  {
                        throw new ArgumentNullException(nameof(source));
                  }

                  eventData.SourceId = source.GetInstanceID();
                  eventData.TargetId = targetId;
                  Core.EventBus.Publish(eventData);
            }

            public static void FireTo<T>(this T eventData, GameObject target, int sourceId = -1) where T : struct, ITrackedEvent
            {
                  if (target == null)
                  {
                        throw new ArgumentNullException(nameof(target));
                  }

                  eventData.SourceId = sourceId;
                  eventData.TargetId = target.GetInstanceID();
                  Core.EventBus.Publish(eventData);
            }
      }
}