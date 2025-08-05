using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpalStudio.Echo.Core.Data;
using OpalStudio.Echo.Interface;

namespace OpalStudio.Echo.Core.Extensions
{
      public static class CommonFilterExtensions
      {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static EventFilterBuilder<T> WithValue<T, TValue>(this EventFilterBuilder<T> builder, Func<T, TValue> selector, TValue expectedValue)
                        where T : struct, IEvent
            {
                  return builder.And(evt => EqualityComparer<TValue>.Default.Equals(selector(evt), expectedValue));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static EventFilterBuilder<T> WithRange<T, TValue>(this EventFilterBuilder<T> builder, Func<T, TValue> selector, TValue min, TValue max)
                        where T : struct, IEvent where TValue : IComparable<TValue>
            {
                  return builder.And(evt =>
                  {
                        var value = selector(evt);

                        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
                  });
            }
      }
}