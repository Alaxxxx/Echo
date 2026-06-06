using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpalStudio.Echo.Profiler.Editor.Model;
using OpalStudio.Echo.Profiler.Editor.Services;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Views
{
      public sealed class LiveFeedView : VisualElement
      {
            private const int MaxVisible = 200;

            private readonly Label _counter;
            private readonly VisualElement _entriesHost;
            private readonly ScrollView _scroll;
            private readonly Toggle _autoScroll;
            private readonly Toggle _paused;
            private readonly TextField _filter;
            private readonly PayloadInspectorPanel _inspector;
            private readonly VisualElement _content;

            private string _filterText = string.Empty;
            private int _totalSinceClear;
            private readonly Queue<FeedEntry> _entryQueue = new();
            private FeedEntry _selectedEntry;

            public LiveFeedView()
            {
                  style.flexDirection = FlexDirection.Column;
                  style.backgroundColor = EchoTheme.Surface0;
                  style.borderTopColor = EchoTheme.BorderSubtle;
                  style.borderTopWidth = 1;
                  style.minHeight = 120;

                  VisualElement headerBar = new()
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              alignItems = Align.Center,
                              backgroundColor = EchoTheme.Surface1,
                              borderBottomColor = EchoTheme.BorderSubtle,
                              borderBottomWidth = 1
                        }
                  };
                  EchoTheme.ApplyPadding(headerBar, EchoTheme.Space3, EchoTheme.Space5);

                  Label header = new("LIVE FEED")
                  {
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeSmall,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              letterSpacing = new StyleLength(1f),
                              marginRight = EchoTheme.Space4
                        }
                  };
                  headerBar.Add(header);

                  _counter = new Label("0 events")
                  {
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeMicro,
                              flexGrow = 1
                        }
                  };
                  headerBar.Add(_counter);

                  _filter = new TextField
                  {
                        style =
                        {
                              width = 140,
                              height = 20,
                              marginRight = EchoTheme.Space3
                        }
                  };
                  _filter.RegisterValueChangedCallback(evt =>
                  {
                        _filterText = evt.newValue ?? string.Empty;
                        ApplyFilter();
                  });
                  headerBar.Add(_filter);

                  _autoScroll = MakeToggle("Auto-scroll", true);
                  headerBar.Add(_autoScroll);

                  _paused = MakeToggle("Pause", EchoMonitorSettings.FeedPaused);
                  _paused.RegisterValueChangedCallback(evt => EchoMonitorSettings.FeedPaused = evt.newValue);
                  headerBar.Add(_paused);

                  Button clear = new(ClearFeed)
                  {
                        text = "Clear",
                        style =
                        {
                              backgroundColor = new StyleColor(StyleKeyword.None),
                              color = EchoTheme.TextSecondary,
                              fontSize = EchoTheme.FontSizeSmall,
                              borderTopWidth = 0,
                              borderBottomWidth = 0,
                              borderLeftWidth = 0,
                              borderRightWidth = 0,
                              paddingTop = 1,
                              paddingBottom = 1,
                              paddingLeft = EchoTheme.Space3,
                              paddingRight = EchoTheme.Space3,
                              marginLeft = EchoTheme.Space2
                        }
                  };
                  EchoTheme.ApplyRadius(clear, EchoTheme.RadiusSmall);
                  clear.AddToClassList("echo-toolbar-button");
                  headerBar.Add(clear);

                  Add(headerBar);

                  _content = new VisualElement
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              flexGrow = 1
                        }
                  };
                  Add(_content);

                  _scroll = new ScrollView(ScrollViewMode.Vertical)
                  {
                        style = { flexGrow = 1 }
                  };
                  _content.Add(_scroll);

                  _entriesHost = new VisualElement();
                  _scroll.Add(_entriesHost);

                  _inspector = new PayloadInspectorPanel(() => CloseInspector());
                  _inspector.style.display = DisplayStyle.None;
                  _content.Add(_inspector);
            }

            public void OnDispatch(DispatchRecord record)
            {
                  if (EchoMonitorSettings.FeedPaused)
                  {
                        return;
                  }

                  _totalSinceClear++;
                  _counter.text = _totalSinceClear == 1 ? "1 event" : $"{_totalSinceClear} events";

                  FeedEntry entry = new(record, OnRowClicked);
                  ApplyFilterToEntry(entry);
                  _entriesHost.Add(entry);
                  _entryQueue.Enqueue(entry);

                  while (_entryQueue.Count > MaxVisible)
                  {
                        FeedEntry old = _entryQueue.Dequeue();
                        _entriesHost.Remove(old);
                  }

                  if (_autoScroll.value)
                  {
                        _scroll.schedule.Execute(() =>
                        {
                              _scroll.verticalScroller.value = _scroll.verticalScroller.highValue;
                        }).ExecuteLater(0);
                  }
            }

            public void ClearFeed()
            {
                  _entriesHost.Clear();
                  _entryQueue.Clear();
                  _totalSinceClear = 0;
                  _counter.text = "0 events";
                  CloseInspector();
            }

            private void OnRowClicked(FeedEntry entry)
            {
                  _selectedEntry?.SetSelected(false);
                  _selectedEntry = entry;
                  entry.SetSelected(true);
                  _inspector.Show(entry.Record);
                  _inspector.style.display = DisplayStyle.Flex;
            }

            private void CloseInspector()
            {
                  _inspector.style.display = DisplayStyle.None;
                  _selectedEntry?.SetSelected(false);
                  _selectedEntry = null;
            }

            private void ApplyFilter()
            {
                  foreach (FeedEntry entry in _entryQueue)
                  {
                        ApplyFilterToEntry(entry);
                  }
            }

            private void ApplyFilterToEntry(FeedEntry entry)
            {
                  if (string.IsNullOrEmpty(_filterText))
                  {
                        entry.style.display = DisplayStyle.Flex;

                        return;
                  }

                  entry.style.display = entry.MatchesFilter(_filterText) ? DisplayStyle.Flex : DisplayStyle.None;
            }

            private static Toggle MakeToggle(string label, bool initial)
            {
                  return new Toggle(label)
                  {
                        value = initial,
                        style =
                        {
                              marginRight = EchoTheme.Space3,
                              color = EchoTheme.TextSecondary
                        }
                  };
            }

#region FeedEntry

            private sealed class FeedEntry : VisualElement
            {
                  public DispatchRecord Record { get; }
                  private readonly string _searchable;
                  private readonly VisualElement _selectionAccent;

                  public FeedEntry(DispatchRecord record, Action<FeedEntry> onClick)
                  {
                        Record = record;
                        AddToClassList("echo-feed-entry");
                        style.flexDirection = FlexDirection.Row;
                        style.alignItems = Align.Center;
                        EchoTheme.ApplyPadding(this, 2, EchoTheme.Space5);

                        _selectionAccent = new VisualElement
                        {
                              style =
                              {
                                    width = 2,
                                    marginLeft = -EchoTheme.Space5,
                                    marginRight = EchoTheme.Space3,
                                    alignSelf = Align.Stretch,
                                    backgroundColor = new StyleColor(StyleKeyword.None)
                              }
                        };
                        Add(_selectionAccent);

                        Label time = new(record.Timestamp.ToLocalTime().ToString("HH:mm:ss.fff"))
                        {
                              style =
                              {
                                    color = EchoTheme.TextMuted,
                                    fontSize = EchoTheme.FontSizeMicro,
                                    width = 88
                              }
                        };
                        Add(time);

                        Label name = new(record.EventType.Name)
                        {
                              style =
                              {
                                    color = EchoTheme.TextPrimary,
                                    fontSize = EchoTheme.FontSizeSmall,
                                    unityFontStyleAndWeight = FontStyle.Bold,
                                    width = 160,
                                    marginLeft = EchoTheme.Space3,
                                    overflow = Overflow.Hidden,
                                    textOverflow = TextOverflow.Ellipsis,
                                    whiteSpace = WhiteSpace.NoWrap
                              }
                        };
                        Add(name);

                        string summary = PayloadInspector.Summarize(record.Payload);
                        Label payload = new(string.IsNullOrEmpty(summary) ? "·" : summary)
                        {
                              style =
                              {
                                    color = EchoTheme.TextSecondary,
                                    fontSize = EchoTheme.FontSizeSmall,
                                    flexGrow = 1,
                                    marginLeft = EchoTheme.Space3,
                                    overflow = Overflow.Hidden,
                                    textOverflow = TextOverflow.Ellipsis,
                                    whiteSpace = WhiteSpace.NoWrap
                              }
                        };
                        Add(payload);

                        double us = record.ElapsedTicks * 1_000_000.0 / Stopwatch.Frequency;
                        Label timing = new($"{us:F0}µs")
                        {
                              style =
                              {
                                    color = us > 1000 ? EchoTheme.Danger : us > 100 ? EchoTheme.Warning : EchoTheme.TextMuted,
                                    fontSize = EchoTheme.FontSizeMicro,
                                    minWidth = 50,
                                    unityTextAlign = TextAnchor.MiddleRight,
                                    marginLeft = EchoTheme.Space3
                              }
                        };
                        Add(timing);

                        _searchable = record.EventType.Name + " " + summary;

                        RegisterCallback<ClickEvent>(evt => onClick?.Invoke(this));

                        this.AddManipulator(new ContextualMenuManipulator(menu =>
                        {
                              menu.menu.AppendAction("Copy summary", _ => EditorGUIUtility.systemCopyBuffer = _searchable);
                              menu.menu.AppendAction("Copy type name", _ => EditorGUIUtility.systemCopyBuffer = record.EventType.FullName);
                              menu.menu.AppendAction("Open source", _ => ScriptLocator.OpenType(record.EventType));
                        }));
                  }

                  public bool MatchesFilter(string filter)
                  {
                        return _searchable.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
                  }

                  public void SetSelected(bool selected)
                  {
                        _selectionAccent.style.backgroundColor = selected ? EchoTheme.Accent : new StyleColor(StyleKeyword.None);
                        style.backgroundColor = selected ? EchoTheme.SurfaceSelected : new StyleColor(StyleKeyword.None);
                  }
            }

#endregion

#region PayloadInspectorPanel

            private sealed class PayloadInspectorPanel : VisualElement
            {
                  private readonly Label _typeLabel;
                  private readonly Label _timeLabel;
                  private readonly VisualElement _fieldsHost;

                  public PayloadInspectorPanel(Action onClose)
                  {
                        style.width = 320;
                        style.borderLeftWidth = 1;
                        style.borderLeftColor = EchoTheme.BorderSubtle;
                        style.backgroundColor = EchoTheme.Surface1;
                        style.flexDirection = FlexDirection.Column;

                        VisualElement header = new()
                        {
                              style =
                              {
                                    flexDirection = FlexDirection.Row,
                                    alignItems = Align.Center,
                                    borderBottomColor = EchoTheme.BorderSubtle,
                                    borderBottomWidth = 1
                              }
                        };
                        EchoTheme.ApplyPadding(header, EchoTheme.Space3, EchoTheme.Space5);

                        VisualElement headerText = new()
                        {
                              style =
                              {
                                    flexDirection = FlexDirection.Column,
                                    flexGrow = 1
                              }
                        };
                        _typeLabel = new Label("PAYLOAD")
                        {
                              style =
                              {
                                    color = EchoTheme.TextPrimary,
                                    fontSize = EchoTheme.FontSizeBody,
                                    unityFontStyleAndWeight = FontStyle.Bold
                              }
                        };
                        _timeLabel = new Label(string.Empty)
                        {
                              style =
                              {
                                    color = EchoTheme.TextMuted,
                                    fontSize = EchoTheme.FontSizeMicro,
                                    marginTop = 1
                              }
                        };
                        headerText.Add(_typeLabel);
                        headerText.Add(_timeLabel);
                        header.Add(headerText);

                        Button close = new(() => onClose?.Invoke())
                        {
                              text = "×",
                              style =
                              {
                                    backgroundColor = new StyleColor(StyleKeyword.None),
                                    color = EchoTheme.TextMuted,
                                    fontSize = 16,
                                    unityFontStyleAndWeight = FontStyle.Bold,
                                    width = 22,
                                    height = 22,
                                    paddingTop = 0,
                                    paddingBottom = 0,
                                    paddingLeft = 0,
                                    paddingRight = 0,
                                    marginTop = 0,
                                    marginBottom = 0,
                                    marginLeft = 0,
                                    marginRight = 0,
                                    borderTopWidth = 0,
                                    borderBottomWidth = 0,
                                    borderLeftWidth = 0,
                                    borderRightWidth = 0
                              }
                        };
                        close.AddToClassList("echo-toolbar-button");
                        EchoTheme.ApplyRadius(close, EchoTheme.RadiusSmall);
                        header.Add(close);

                        Add(header);

                        ScrollView scroll = new(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
                        EchoTheme.ApplyPadding(scroll, EchoTheme.Space4, EchoTheme.Space5);
                        Add(scroll);

                        _fieldsHost = new VisualElement();
                        scroll.Add(_fieldsHost);
                  }

                  public void Show(DispatchRecord record)
                  {
                        _typeLabel.text = record.EventType.Name;
                        _timeLabel.text = record.Timestamp.ToLocalTime().ToString("HH:mm:ss.fff")
                              + "  ·  frame " + record.FrameIndex;

                        _fieldsHost.Clear();

                        if (record.Payload == null)
                        {
                              _fieldsHost.Add(MakeEmptyMessage("Payload capture is disabled or the event has no fields."));

                              return;
                        }

                        IReadOnlyList<(string Key, string Value)> fields = PayloadInspector.Describe(record.Payload);

                        if (fields.Count == 0)
                        {
                              _fieldsHost.Add(MakeEmptyMessage("This event type has no public fields or properties."));

                              return;
                        }

                        for (int i = 0; i < fields.Count; i++)
                        {
                              _fieldsHost.Add(MakeFieldRow(fields[i].Key, fields[i].Value));
                        }
                  }

                  private static VisualElement MakeFieldRow(string key, string value)
                  {
                        VisualElement row = new()
                        {
                              style =
                              {
                                    flexDirection = FlexDirection.Column,
                                    marginBottom = EchoTheme.Space3,
                                    paddingBottom = EchoTheme.Space2,
                                    borderBottomColor = EchoTheme.BorderSubtle,
                                    borderBottomWidth = 1
                              }
                        };
                        row.Add(new Label(key)
                        {
                              style =
                              {
                                    color = EchoTheme.TextMuted,
                                    fontSize = EchoTheme.FontSizeMicro,
                                    letterSpacing = new StyleLength(0.5f),
                                    marginBottom = EchoTheme.Space1
                              }
                        });
                        row.Add(new Label(value)
                        {
                              style =
                              {
                                    color = EchoTheme.TextPrimary,
                                    fontSize = EchoTheme.FontSizeBody,
                                    whiteSpace = WhiteSpace.Normal,
                                    unityFontStyleAndWeight = FontStyle.Bold
                              }
                        });

                        return row;
                  }

                  private static Label MakeEmptyMessage(string text)
                  {
                        return new Label(text)
                        {
                              style =
                              {
                                    color = EchoTheme.TextMuted,
                                    fontSize = EchoTheme.FontSizeSmall,
                                    whiteSpace = WhiteSpace.Normal,
                                    marginTop = EchoTheme.Space4
                              }
                        };
                  }
            }

#endregion
      }
}