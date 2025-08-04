using System;
using System.Runtime.CompilerServices;
using Echo.Interface;

namespace Echo.Core
{
      internal static class Events<T> where T : struct, IEvent
      {
            internal static event Action<T> OnEvent;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Invoke(T eventData)
            {
                  OnEvent?.Invoke(eventData);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void InvokeBatch(ReadOnlySpan<T> events)
            {
                  Action<T> handler = OnEvent;

                  if (handler == null)
                  {
                        return;
                  }

                  for (int i = 0; i < events.Length; i++)
                  {
                        handler(events[i]);
                  }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Clear()
            {
                  OnEvent = null;
            }
      }
}