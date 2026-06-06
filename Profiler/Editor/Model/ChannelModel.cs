using System;
using System.Collections.Generic;
using OpalStudio.Echo.Profiler.Editor.Services;

namespace OpalStudio.Echo.Profiler.Editor.Model
{
      /// <summary>
      /// Per-event-type model: statistics, subscribers, recent dispatches.
      /// Owned by <see cref="EchoProfilerService"/>.
      /// </summary>
      public sealed class ChannelModel
      {
            public const int RecentCapacity = 64;

            public Type EventType { get; }
            public ChannelStats Stats { get; } = new();

            private readonly List<SubscriberInfo> _subscribers = new();
            private readonly RingBuffer<DispatchRecord> _recent = new(RecentCapacity);

            public IReadOnlyList<SubscriberInfo> Subscribers => _subscribers;
            public RingBuffer<DispatchRecord> Recent => _recent;

            public ChannelModel(Type eventType)
            {
                  EventType = eventType;
            }

            internal void AddSubscriber(SubscriberInfo info)
            {
                  _subscribers.Add(info);
            }

            internal void RemoveSubscriber(Delegate handler)
            {
                  for (int i = _subscribers.Count - 1; i >= 0; i--)
                  {
                        SubscriberInfo info = _subscribers[i];

                        if (!info.TryGetDelegate(out Delegate existing))
                        {
                              _subscribers.RemoveAt(i);

                              continue;
                        }

                        if (ReferenceEquals(existing, handler))
                        {
                              _subscribers.RemoveAt(i);

                              return;
                        }
                  }
            }

            internal void RecordDispatch(in DispatchRecord record)
            {
                  Stats.RecordDispatch(record.ElapsedTicks, record.FrameIndex, record.Timestamp);
                  _recent.Push(record);
            }

            internal void Reset()
            {
                  Stats.Reset();
                  _recent.Clear();
            }
      }
}