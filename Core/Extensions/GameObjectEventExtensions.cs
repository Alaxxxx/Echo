using System;
using System.Runtime.CompilerServices;
using OpalStudio.Echo.Interface;
using UnityEngine;

namespace OpalStudio.Echo.Core.Extensions
{
      public static class GameObjectEventExtensions
      {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SubscribeFromThis<T>(this GameObject gameObject, Action<T> action) where T : struct, ITrackedEvent
            {
                  EventBus.SubscribeFromSource(action, gameObject.GetInstanceID());
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SubscribeToThis<T>(this GameObject gameObject, Action<T> action) where T : struct, ITrackedEvent
            {
                  EventBus.SubscribeToTarget(action, gameObject.GetInstanceID());
            }
      }
}