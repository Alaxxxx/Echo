using System;
using System.Runtime.CompilerServices;
using Echo.Interface;

namespace Echo
{
      public static class EventBus
      {
            private static class Events<T> where T : struct, IEvent
            {
                  public static event Action<T> OnEvent;

                  public static void Invoke(T eventData)
                  {
                        OnEvent?.Invoke(eventData);
                  }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Publish<T>(T eventData) where T : struct, IEvent
            {
                  Events<T>.Invoke(eventData);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Subscribe<T>(Action<T> action) where T : struct, IEvent
            {
                  Events<T>.OnEvent += action;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Unsubscribe<T>(Action<T> action) where T : struct, IEvent
            {
                  Events<T>.OnEvent -= action;
            }
      }
}