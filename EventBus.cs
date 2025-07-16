using System;
using System.Runtime.CompilerServices;

namespace Echo
{
      public static class EventBus
      {
            private static class Events<T> where T : struct
            {
                  public static event Action<T> OnEvent;

                  public static void Invoke(T eventData)
                  {
                        OnEvent?.Invoke(eventData);
                  }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Publish<T>(T eventData) where T : struct
            {
                  Events<T>.Invoke(eventData);
            }

            public static void Subscribe<T>(Action<T> action) where T : struct
            {
                  Events<T>.OnEvent += action;
            }

            public static void Unsubscribe<T>(Action<T> action) where T : struct
            {
                  Events<T>.OnEvent -= action;
            }
      }
}
