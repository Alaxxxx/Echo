using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Echo.Interface;

namespace Echo.Core
{
      internal static class Events<T> where T : struct, IEvent
      {
            internal static event Action<T> OnEvent;

            private readonly static List<Subscriber> _filteredSubscribers = new();

            private readonly struct Subscriber
            {
                  public readonly Action<T> Action { get; }
                  public readonly Func<T, bool> Filter { get; }

                  public Subscriber(Action<T> action, Func<T, bool> filter)
                  {
                        Action = action;
                        Filter = filter;
                  }
            }

            internal static void SubscribeFiltered(Action<T> action, Func<T, bool> filter)
            {
                  if (!_filteredSubscribers.Exists(sub => sub.Action == action))
                  {
                        _filteredSubscribers.Add(new Subscriber(action, filter));
                  }
            }

            internal static void UnsubscribeFiltered(Action<T> action)
            {
                  _filteredSubscribers.RemoveAll(sub => sub.Action == action);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void InvokeFiltered(T eventData)
            {
                  foreach (Subscriber sub in _filteredSubscribers.ToArray())
                  {
                        if (sub.Filter(eventData))
                        {
                              sub.Action(eventData);
                        }
                  }
            }

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