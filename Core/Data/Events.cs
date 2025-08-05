using System;
using System.Runtime.CompilerServices;
using Echo.Interface;

namespace Echo.Core.Data
{
      internal static class Events<T> where T : struct, IEvent
      {
            internal static event Action<T> OnEvent;

            private static FilteredHandler<T>[] filteredHandlers;
            private static int filteredCount;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Invoke(T eventData)
            {
                  OnEvent?.Invoke(eventData);

                  for (int i = 0; i < filteredCount; i++)
                  {
                        ref FilteredHandler<T> handler = ref filteredHandlers[i];

                        if (handler.ShouldInvoke(eventData))
                        {
                              handler.Handler(eventData);
                        }
                  }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void InvokeBatch(ReadOnlySpan<T> events)
            {
                  for (int i = 0; i < events.Length; i++)
                  {
                        Invoke(events[i]);
                  }
            }

            internal static void AddFilteredHandler(Action<T> handler, Func<T, bool> filter)
            {
                  EnsureFilteredCapacity();
                  filteredHandlers[filteredCount++] = new FilteredHandler<T>(handler, filter);
            }

            internal static void RemoveFilteredHandler(Action<T> handler)
            {
                  for (int i = 0; i < filteredCount; i++)
                  {
                        if (ReferenceEquals(filteredHandlers[i].Handler, handler))
                        {
                              for (int j = i; j < filteredCount - 1; j++)
                              {
                                    filteredHandlers[j] = filteredHandlers[j + 1];
                              }

                              filteredCount--;

                              return;
                        }
                  }
            }

            private static void EnsureFilteredCapacity()
            {
                  if (filteredHandlers == null)
                  {
                        filteredHandlers = new FilteredHandler<T>[4];
                  }
                  else if (filteredCount >= filteredHandlers.Length)
                  {
                        Array.Resize(ref filteredHandlers, filteredHandlers.Length * 2);
                  }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Clear()
            {
                  OnEvent = null;
                  filteredCount = 0;
            }
      }
}