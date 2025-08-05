using System;
using System.Runtime.CompilerServices;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core.Data
{
      internal static class Events<T> where T : struct, IEvent
      {
            internal static event Action<T> OnEvent;

            private static FilteredHandler<T>[] _filteredHandlers;
            private static int _filteredCount;
            private static int _nextId = 1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Invoke(T eventData)
            {
                  OnEvent?.Invoke(eventData);

                  for (int i = 0; i < _filteredCount; i++)
                  {
                        ref var handler = ref _filteredHandlers[i];

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
                  int id = _nextId++;
                  _filteredHandlers[_filteredCount++] = new FilteredHandler<T>(handler, filter, id);

                  return id;
            }

            internal static bool RemoveFilteredHandler(Action<T> handler)
            {
                  for (int i = 0; i < _filteredCount; i++)
                  {
                        if (ReferenceEquals(_filteredHandlers[i].Handler, handler))
                        {
                              RemoveAt(i);

                              return true;
                        }
                  }

                  return false;
            }

            internal static bool RemoveFilteredHandler(int id)
            {
                  for (int i = 0; i < _filteredCount; i++)
                  {
                        if (_filteredHandlers[i].Id == id)
                        {
                              RemoveAt(i);

                              return true;
                        }
                  }

                  return false;
            }

            private static void RemoveAt(int index)
            {
                  for (int j = index; j < _filteredCount - 1; j++)
                  {
                        _filteredHandlers[j] = _filteredHandlers[j + 1];
                  }

                  _filteredCount--;
            }

            private static void EnsureFilteredCapacity()
            {
                  if (_filteredHandlers == null)
                  {
                        _filteredHandlers = new FilteredHandler<T>[4];
                  }
                  else if (_filteredCount >= _filteredHandlers.Length)
                  {
                        Array.Resize(ref _filteredHandlers, _filteredHandlers.Length * 2);
                  }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Clear()
            {
                  OnEvent = null;
                  _filteredCount = 0;
                  _nextId = 1;
            }
      }
}