using OpalStudio.Echo.Interface;
using OpalStudio.Echo.Unity.Data;
using UnityEngine;

namespace OpalStudio.Echo.Unity.Extensions
{
      public static class TrackedEventFilterExtensions
      {
            public static EventFilterBuilder<T> FromSource<T>(this EventFilterBuilder<T> builder, int sourceId) where T : struct, ITrackedEvent
            {
                  return builder.And(evt => evt.SourceId == sourceId);
            }

            public static EventFilterBuilder<T> FromSource<T>(this EventFilterBuilder<T> builder, GameObject source) where T : struct, ITrackedEvent
            {
                  int sourceId = source.GetInstanceID();

                  return builder.And(evt => evt.SourceId == sourceId);
            }

            public static EventFilterBuilder<T> ToTarget<T>(this EventFilterBuilder<T> builder, int targetId) where T : struct, ITrackedEvent
            {
                  return builder.And(evt => evt.TargetId == targetId);
            }

            public static EventFilterBuilder<T> ToTarget<T>(this EventFilterBuilder<T> builder, GameObject target) where T : struct, ITrackedEvent
            {
                  int targetId = target.GetInstanceID();

                  return builder.And(evt => evt.TargetId == targetId);
            }

            public static EventFilterBuilder<T> Between<T>(this EventFilterBuilder<T> builder, GameObject source, GameObject target) where T : struct, ITrackedEvent
            {
                  int sourceId = source.GetInstanceID();
                  int targetId = target.GetInstanceID();

                  return builder.And(evt => evt.SourceId == sourceId && evt.TargetId == targetId);
            }
      }
}