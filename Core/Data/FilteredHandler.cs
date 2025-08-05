using System;
using System.Runtime.CompilerServices;
using Echo.Interface;

namespace Echo.Core.Data
{
      internal readonly struct FilteredHandler<T> : IEquatable<FilteredHandler<T>> where T : struct, IEvent
      {
            public readonly Action<T> Handler;
            public readonly Func<T, bool> Filter;

            public FilteredHandler(Action<T> handler, Func<T, bool> filter)
            {
                  Handler = handler;
                  Filter = filter;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ShouldInvoke(T eventData) => Filter?.Invoke(eventData) ?? true;

            public bool Equals(FilteredHandler<T> other)
            {
                  return Equals(Handler, other.Handler) && Equals(Filter, other.Filter);
            }

            public override bool Equals(object obj)
            {
                  return obj is FilteredHandler<T> other && Equals(other);
            }

            public override int GetHashCode()
            {
                  return HashCode.Combine(Handler, Filter);
            }
      }
}