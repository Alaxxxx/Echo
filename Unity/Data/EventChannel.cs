using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Unity.Data
{
      /// <summary>
      /// Internal storage for filtered event handlers of a given event type.
      /// Mirrors the Core's <c>EventChannel&lt;T&gt;</c> but adds filtering support.
      /// </summary>
      internal static class EventChannel<T> where T : struct, IEvent
      {
            private static FilteredHandler<T>[] filteredHandlers;
            private static int filteredCount;

            private static int nextId = 1;

            private static int invocationDepth;
            private static List<int> pendingRemovals;

            static EventChannel()
            {
                  Core.Data.EventChannel<T>.SecondaryDispatch = InvokeFiltered;
                  Core.Data.EventChannel<T>.SecondaryHasSubscribers = () => filteredCount > 0;
                  Core.Diagnostics.EventChannelRegistry.Register(typeof(T), Clear);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void InvokeFiltered(T eventData)
            {
                  if (filteredCount == 0)
                  {
                        return;
                  }

                  invocationDepth++;

                  try
                  {
                        FilteredHandler<T>[] handlers = filteredHandlers;
                        int count = filteredCount;

                        for (int i = 0; i < count; i++)
                        {
                              ref FilteredHandler<T> handler = ref handlers[i];

                              if (handler.IsAlive && handler.ShouldInvoke(eventData))
                              {
                                    handler.Handler(eventData);
                              }
                        }
                  }
                  finally
                  {
                        invocationDepth--;

                        if (invocationDepth == 0 && pendingRemovals != null && pendingRemovals.Count > 0)
                        {
                              FlushPendingRemovals();
                        }
                  }
            }

            internal static int AddFilteredHandler(Action<T> handler, Func<T, bool> filter)
            {
                  EnsureFilteredCapacity();
                  int id = nextId++;
                  filteredHandlers[filteredCount++] = new FilteredHandler<T>(handler, filter, id);

                  return id;
            }

            internal static void RemoveFilteredHandler(int id)
            {
                  if (invocationDepth > 0)
                  {
                        for (int i = 0; i < filteredCount; i++)
                        {
                              if (filteredHandlers[i].Id == id)
                              {
                                    Core.Diagnostics.EchoInstrumentation.NotifyUnsubscribe(typeof(T), filteredHandlers[i].Handler);
                                    filteredHandlers[i] = filteredHandlers[i].MarkDead();
                                    pendingRemovals ??= new List<int>(4);
                                    pendingRemovals.Add(id);

                                    return;
                              }
                        }

                        return;
                  }

                  for (int i = 0; i < filteredCount; i++)
                  {
                        if (filteredHandlers[i].Id == id)
                        {
                              Core.Diagnostics.EchoInstrumentation.NotifyUnsubscribe(typeof(T), filteredHandlers[i].Handler);
                              RemoveAt(i);

                              return;
                        }
                  }
            }

            private static void FlushPendingRemovals()
            {
                  for (int p = 0; p < pendingRemovals.Count; p++)
                  {
                        int id = pendingRemovals[p];

                        for (int i = 0; i < filteredCount; i++)
                        {
                              if (filteredHandlers[i].Id == id)
                              {
                                    RemoveAt(i);

                                    break;
                              }
                        }
                  }

                  pendingRemovals.Clear();
            }

            private static void RemoveAt(int index)
            {
                  for (int j = index; j < filteredCount - 1; j++)
                  {
                        filteredHandlers[j] = filteredHandlers[j + 1];
                  }

                  filteredCount--;

                  filteredHandlers[filteredCount] = default;
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
                  if (filteredHandlers != null)
                  {
                        for (int i = 0; i < filteredCount; i++)
                        {
                              filteredHandlers[i] = default;
                        }
                  }

                  filteredCount = 0;

                  pendingRemovals?.Clear();
            }
      }
}