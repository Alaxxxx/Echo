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
            private static int nextId = 1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Invoke(T eventData)
            {
                  OnEvent?.Invoke(eventData);

                  for (int i = 0; i < filteredCount; i++)
                  {
                        ref var handler = ref filteredHandlers[i];

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

            internal static int AddFilteredHandler(Action<T> handler, Func<T, bool> filter)
            {
                  EnsureFilteredCapacity();
                  int id = nextId++;
                  filteredHandlers[filteredCount++] = new FilteredHandler<T>(handler, filter, id);

                  return id;
            }

            internal static void RemoveFilteredHandler(Action<T> handler)
            {
                  for (int i = 0; i < filteredCount; i++)
                  {
                        if (ReferenceEquals(filteredHandlers[i].Handler, handler))
                        {
                              RemoveAt(i);

                              return;
                        }
                  }
            }

            internal static void RemoveFilteredHandler(int id)
            {
                  for (int i = 0; i < filteredCount; i++)
                  {
                        if (filteredHandlers[i].Id == id)
                        {
                              RemoveAt(i);

                              return;
                        }
                  }
            }

            private static void RemoveAt(int index)
            {
                  for (int j = index; j < filteredCount - 1; j++)
                  {
                        filteredHandlers[j] = filteredHandlers[j + 1];
                  }

                  filteredCount--;
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
                  nextId = 1;
            }
      }
}