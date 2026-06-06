using System;
using System.Diagnostics;

namespace OpalStudio.Echo.Profiler.Editor.Model
{
      public sealed class ChannelStats
      {
            public const int FrameWindowSize = 120;

            public const int TimingSampleSize = 256;

            private readonly int[] _dispatchesPerFrame = new int[FrameWindowSize];
            private int _windowHead;
            private int _lastFrameIndex = -1;

            // Ring buffer of recent dispatch durations for percentile calculation.
            // Stored as ticks; converted to microseconds on demand.
            private readonly long[] _timingSamples = new long[TimingSampleSize];
            private int _timingCount;
            private int _timingHead;

            // Sliding window of recent dispatch timestamps for "per second" rate.
            // We use a fixed 1-second window; older entries are pruned lazily on read.
            private readonly long[] _recentTimestamps = new long[1024];
            private int _recentCount;
            private int _recentHead;

            public long TotalDispatches { get; private set; }
            public long TotalElapsedTicks { get; private set; }
            public long PeakElapsedTicks { get; private set; }
            public int PeakDispatchesPerFrame { get; private set; }
            public DateTime LastDispatchTime { get; private set; }

            public double AvgMicroseconds => TotalDispatches == 0 ? 0.0 : TicksToMicroseconds(TotalElapsedTicks / (double)TotalDispatches);

            public double PeakMicroseconds => TicksToMicroseconds(PeakElapsedTicks);

            public int CurrentFrameDispatches => _dispatchesPerFrame[_windowHead];

            public int[] CopyFrameWindow()
            {
                  var copy = new int[FrameWindowSize];

                  for (var i = 0; i < FrameWindowSize; i++)
                  {
                        int idx = (_windowHead + 1 + i) % FrameWindowSize;
                        copy[i] = _dispatchesPerFrame[idx];
                  }

                  return copy;
            }

            internal void RecordDispatch(long elapsedTicks, int frameIndex, DateTime timestamp)
            {
                  TotalDispatches++;
                  TotalElapsedTicks += elapsedTicks;
                  LastDispatchTime = timestamp;

                  if (elapsedTicks > PeakElapsedTicks)
                  {
                        PeakElapsedTicks = elapsedTicks;
                  }

                  AdvanceWindowToFrame(frameIndex);
                  _dispatchesPerFrame[_windowHead]++;

                  if (_dispatchesPerFrame[_windowHead] > PeakDispatchesPerFrame)
                  {
                        PeakDispatchesPerFrame = _dispatchesPerFrame[_windowHead];
                  }

                  _timingSamples[_timingHead] = elapsedTicks;
                  _timingHead = (_timingHead + 1) % TimingSampleSize;

                  if (_timingCount < TimingSampleSize)
                  {
                        _timingCount++;
                  }

                  _recentTimestamps[_recentHead] = timestamp.Ticks;
                  _recentHead = (_recentHead + 1) % _recentTimestamps.Length;

                  if (_recentCount < _recentTimestamps.Length)
                  {
                        _recentCount++;
                  }
            }

            public (double P50, double P95, double P99) ComputePercentiles()
            {
                  if (_timingCount == 0)
                  {
                        return (0, 0, 0);
                  }

                  var sorted = new long[_timingCount];
                  Array.Copy(_timingSamples, sorted, _timingCount);
                  Array.Sort(sorted);

                  return (
                        TicksToMicroseconds(sorted[Percentile(_timingCount, 0.50)]),
                        TicksToMicroseconds(sorted[Percentile(_timingCount, 0.95)]),
                        TicksToMicroseconds(sorted[Percentile(_timingCount, 0.99)])
                  );
            }

            public int ComputeRatePerSecond()
            {
                  if (_recentCount == 0)
                  {
                        return 0;
                  }

                  long threshold = DateTime.UtcNow.Ticks - TimeSpan.TicksPerSecond;
                  var count = 0;

                  for (var i = 0; i < _recentCount; i++)
                  {
                        int idx = (_recentHead - 1 - i + _recentTimestamps.Length) % _recentTimestamps.Length;

                        if (_recentTimestamps[idx] >= threshold)
                        {
                              count++;
                        }
                        else
                        {
                              break;
                        }
                  }

                  return count;
            }

            private static int Percentile(int count, double pct)
            {
                  var idx = (int)Math.Floor(pct * (count - 1));

                  return Math.Clamp(idx, 0, count - 1);
            }

            internal void Reset()
            {
                  Array.Clear(_dispatchesPerFrame, 0, FrameWindowSize);
                  Array.Clear(_timingSamples, 0, TimingSampleSize);
                  Array.Clear(_recentTimestamps, 0, _recentTimestamps.Length);
                  _windowHead = 0;
                  _lastFrameIndex = -1;
                  _timingCount = 0;
                  _timingHead = 0;
                  _recentCount = 0;
                  _recentHead = 0;
                  TotalDispatches = 0;
                  TotalElapsedTicks = 0;
                  PeakElapsedTicks = 0;
                  PeakDispatchesPerFrame = 0;
                  LastDispatchTime = default;
            }

            private void AdvanceWindowToFrame(int frameIndex)
            {
                  if (_lastFrameIndex == frameIndex)
                  {
                        return;
                  }

                  if (_lastFrameIndex < 0)
                  {
                        _lastFrameIndex = frameIndex;

                        return;
                  }

                  int framesPassed = frameIndex - _lastFrameIndex;

                  if (framesPassed >= FrameWindowSize)
                  {
                        Array.Clear(_dispatchesPerFrame, 0, FrameWindowSize);
                        _windowHead = 0;
                        _lastFrameIndex = frameIndex;

                        return;
                  }

                  for (var i = 0; i < framesPassed; i++)
                  {
                        _windowHead = (_windowHead + 1) % FrameWindowSize;
                        _dispatchesPerFrame[_windowHead] = 0;
                  }

                  _lastFrameIndex = frameIndex;
            }

            private static double TicksToMicroseconds(double ticks)
            {
                  return ticks * 1_000_000.0 / Stopwatch.Frequency;
            }
      }
}