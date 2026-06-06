using System;
using OpalStudio.Echo.Profiler.Editor.Model;
using OpalStudio.Echo.Profiler.Editor.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Views
{
      public sealed class OverviewView : VisualElement
      {
            private readonly MetricCard _totalCard;
            private readonly MetricCard _rateFrameCard;
            private readonly MetricCard _rateSecCard;
            private readonly MetricCard _subscribersCard;

            private readonly MetricCard _p50Card;
            private readonly MetricCard _p95Card;
            private readonly MetricCard _p99Card;
            private readonly MetricCard _peakTimeCard;

            private readonly Label _chartTitle;
            private readonly Label _chartSubtitle;
            private readonly Sparkline _chartSpark;
            private readonly Label _lastFired;

            private ChannelModel _model;

            public OverviewView()
            {
                  style.flexDirection = FlexDirection.Column;

                  Add(MakeSectionLabel("ACTIVITY"));

                  VisualElement row1 = MakeCardRow();
                  _totalCard = new MetricCard("Total dispatches");
                  _rateFrameCard = new MetricCard("Current frame");
                  _rateSecCard = new MetricCard("Per second");
                  _subscribersCard = new MetricCard("Subscribers");
                  row1.Add(_totalCard);
                  row1.Add(_rateFrameCard);
                  row1.Add(_rateSecCard);
                  row1.Add(_subscribersCard);
                  Add(row1);

                  Add(MakeSectionLabel("LATENCY (microseconds)"));

                  VisualElement row2 = MakeCardRow();
                  _p50Card = new MetricCard("p50");
                  _p95Card = new MetricCard("p95");
                  _p99Card = new MetricCard("p99");
                  _peakTimeCard = new MetricCard("Peak");
                  row2.Add(_p50Card);
                  row2.Add(_p95Card);
                  row2.Add(_p99Card);
                  row2.Add(_peakTimeCard);
                  Add(row2);

                  Add(MakeSectionLabel("DISPATCHES PER FRAME"));

                  VisualElement chartCard = new();
                  chartCard.AddToClassList("echo-card");
                  chartCard.style.backgroundColor = EchoTheme.Surface2;
                  EchoTheme.ApplyBorder(chartCard, EchoTheme.BorderSubtle);
                  EchoTheme.ApplyRadius(chartCard, EchoTheme.RadiusMedium);
                  EchoTheme.ApplyPadding(chartCard, EchoTheme.Space5, EchoTheme.Space6);
                  chartCard.style.marginLeft = 3;
                  chartCard.style.marginRight = 3;
                  chartCard.style.marginTop = EchoTheme.Space2;

                  VisualElement chartHeader = new()
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              alignItems = Align.FlexEnd,
                              marginBottom = EchoTheme.Space4
                        }
                  };

                  _chartTitle = new Label("0")
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeDisplay,
                              color = EchoTheme.TextPrimary,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              marginRight = EchoTheme.Space2
                        }
                  };
                  chartHeader.Add(_chartTitle);

                  _chartSubtitle = new Label("peak in the last 120 frames")
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeSmall,
                              color = EchoTheme.TextMuted,
                              marginBottom = 5
                        }
                  };
                  chartHeader.Add(_chartSubtitle);

                  chartCard.Add(chartHeader);

                  _chartSpark = new Sparkline
                  {
                        style = { height = 140 }
                  };
                  chartCard.Add(_chartSpark);

                  _lastFired = new Label(string.Empty)
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeSmall,
                              color = EchoTheme.TextMuted,
                              marginTop = EchoTheme.Space3
                        }
                  };
                  chartCard.Add(_lastFired);
                  Add(chartCard);
            }

            public void Bind(ChannelModel model)
            {
                  _model = model;
                  Render();
            }

            public void Refresh() => Render();

            private void Render()
            {
                  if (_model == null)
                  {
                        return;
                  }

                  ChannelStats s = _model.Stats;

                  _totalCard.SetValue(FormatNumber(s.TotalDispatches));
                  _rateFrameCard.SetValue(s.CurrentFrameDispatches.ToString());
                  _rateFrameCard.SetUnit("/ frame");

                  int rps = s.ComputeRatePerSecond();
                  _rateSecCard.SetValue(rps.ToString());
                  _rateSecCard.SetUnit("/ s");

                  _subscribersCard.SetValue(_model.Subscribers.Count.ToString());

                  (double p50, double p95, double p99) = s.ComputePercentiles();
                  _p50Card.SetValue(p50.ToString("F1"));
                  _p50Card.SetUnit("µs");
                  _p95Card.SetValue(p95.ToString("F1"));
                  _p95Card.SetUnit("µs");
                  _p95Card.SetValue(p95.ToString("F1"), ColorForTime(p95));
                  _p99Card.SetValue(p99.ToString("F1"));
                  _p99Card.SetUnit("µs");
                  _p99Card.SetValue(p99.ToString("F1"), ColorForTime(p99));

                  _peakTimeCard.SetValue(s.PeakMicroseconds.ToString("F1"));
                  _peakTimeCard.SetUnit("µs");
                  _peakTimeCard.SetValue(s.PeakMicroseconds.ToString("F1"), ColorForTime(s.PeakMicroseconds));

                  int[] window = s.CopyFrameWindow();
                  _chartSpark.SetData(window);

                  int maxInWindow = 0;

                  for (int i = 0; i < window.Length; i++)
                  {
                        if (window[i] > maxInWindow)
                        {
                              maxInWindow = window[i];
                        }
                  }

                  _chartTitle.text = maxInWindow.ToString();
                  _chartSubtitle.text = $"peak in the last {window.Length} frames";

                  Color chartColor = maxInWindow switch
                  {
                        0 => EchoTheme.TextMuted,
                        < 30 => EchoTheme.Accent,
                        _ => EchoTheme.Warning
                  };
                  _chartSpark.SetColor(chartColor);

                  if (s.LastDispatchTime == default)
                  {
                        _lastFired.text = "Never fired.";
                  }
                  else
                  {
                        TimeSpan ago = DateTime.UtcNow - s.LastDispatchTime;
                        _lastFired.text = $"Last dispatched {FormatTimeAgo(ago)}.";
                  }
            }

            private static Color ColorForTime(double us)
            {
                  return us switch
                  {
                        0 => EchoTheme.TextPrimary,
                        < 100 => EchoTheme.Success,
                        < 1000 => EchoTheme.Warning,
                        _ => EchoTheme.Danger
                  };
            }

            private static VisualElement MakeCardRow()
            {
                  return new VisualElement
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              marginBottom = EchoTheme.Space2
                        }
                  };
            }

            private static Label MakeSectionLabel(string text)
            {
                  return new Label(text)
                  {
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeSmall,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              letterSpacing = new StyleLength(1.5f),
                              marginTop = EchoTheme.Space5,
                              marginBottom = EchoTheme.Space2,
                              marginLeft = 3
                        }
                  };
            }

            private static string FormatNumber(long n)
            {
                  if (n < 1_000)
                  {
                        return n.ToString();
                  }

                  if (n < 1_000_000)
                  {
                        return (n / 1000.0).ToString("F1") + "k";
                  }

                  return (n / 1_000_000.0).ToString("F1") + "M";
            }

            private static string FormatTimeAgo(TimeSpan span)
            {
                  if (span.TotalSeconds < 1)
                  {
                        return "just now";
                  }

                  if (span.TotalSeconds < 60)
                  {
                        return $"{(int)span.TotalSeconds}s ago";
                  }

                  if (span.TotalMinutes < 60)
                  {
                        return $"{(int)span.TotalMinutes}m ago";
                  }

                  return $"{(int)span.TotalHours}h ago";
            }
      }
}