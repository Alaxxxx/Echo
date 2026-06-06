using System;

namespace OpalStudio.Echo.Profiler.Editor.Model
{
      public readonly struct DispatchRecord
      {
            public readonly Type EventType;

            public readonly object Payload;

            public readonly DateTime Timestamp;

            public readonly long ElapsedTicks;

            public readonly int FrameIndex;

            public DispatchRecord(Type eventType, object payload, DateTime timestamp, long elapsedTicks, int frameIndex)
            {
                  EventType = eventType;
                  Payload = payload;
                  Timestamp = timestamp;
                  ElapsedTicks = elapsedTicks;
                  FrameIndex = frameIndex;
            }
      }
}