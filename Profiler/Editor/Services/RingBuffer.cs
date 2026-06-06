using System.Collections;
using System.Collections.Generic;

namespace OpalStudio.Echo.Profiler.Editor.Services
{
      public sealed class RingBuffer<T> : IEnumerable<T>
      {
            private readonly T[] _buffer;
            private int _head;
            private int _count;

            public int Capacity { get; }
            public int Count => _count;

            public RingBuffer(int capacity)
            {
                  Capacity = capacity;
                  _buffer = new T[capacity];
            }

            public void Push(T item)
            {
                  _buffer[_head] = item;
                  _head = (_head + 1) % Capacity;

                  if (_count < Capacity)
                  {
                        _count++;
                  }
            }

            public void Clear()
            {
                  for (int i = 0; i < Capacity; i++)
                  {
                        _buffer[i] = default;
                  }

                  _head = 0;
                  _count = 0;
            }
            
            public T[] ToArray()
            {
                  T[] copy = new T[_count];
                  int start = _count < Capacity ? 0 : _head;

                  for (int i = 0; i < _count; i++)
                  {
                        copy[i] = _buffer[(start + i) % Capacity];
                  }

                  return copy;
            }

            public IEnumerator<T> GetEnumerator()
            {
                  int start = _count < Capacity ? 0 : _head;

                  for (int i = 0; i < _count; i++)
                  {
                        yield return _buffer[(start + i) % Capacity];
                  }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
      }
}