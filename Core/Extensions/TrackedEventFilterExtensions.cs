using System.Runtime.CompilerServices;
using OpalStudio.Echo.Core.Data;
using OpalStudio.Echo.Interface;
using UnityEngine;

namespace OpalStudio.Echo.Core.Extensions
{
      public static class TrackedEventFilterExtensions
      {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static EventFilterBuilder<T> FromSource<T>(this EventFilterBuilder<T> builder, int sourceId) where T : struct, ITrackedEvent
            {
                  return builder.And(evt => evt.SourceId == sourceId);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static EventFilterBuilder<T> FromSource<T>(this EventFilterBuilder<T> builder, GameObject source) where T : struct, ITrackedEvent
            {
                  int sourceId = source.GetInstanceID();

                  return builder.And(evt => evt.SourceId == sourceId);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static EventFilterBuilder<T> ToTarget<T>(this EventFilterBuilder<T> builder, int targetId) where T : struct, ITrackedEvent
            {
                  return builder.And(evt => evt.TargetId == targetId);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static EventFilterBuilder<T> ToTarget<T>(this EventFilterBuilder<T> builder, GameObject target) where T : struct, ITrackedEvent
            {
                  int targetId = target.GetInstanceID();

                  return builder.And(evt => evt.TargetId == targetId);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static EventFilterBuilder<T> Between<T>(this EventFilterBuilder<T> builder, GameObject source, GameObject target) where T : struct, ITrackedEvent
            {
                  int sourceId = source.GetInstanceID();
                  int targetId = target.GetInstanceID();

                  return builder.And(evt => evt.SourceId == sourceId && evt.TargetId == targetId);
            }
      }
}