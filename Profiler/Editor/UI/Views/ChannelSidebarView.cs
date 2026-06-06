using System;
using System.Collections.Generic;
using System.Linq;
using OpalStudio.Echo.Profiler.Editor.Model;
using OpalStudio.Echo.Profiler.Editor.Services;
using OpalStudio.Echo.Profiler.Editor.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Views
{
      public sealed class ChannelSidebarView : VisualElement
      {
            private readonly Label _header;
            private readonly Label _count;
            private readonly ScrollView _list;
            private readonly EmptyState _empty;
            private readonly Dictionary<Type, ChannelRow> _rows = new();
            private ChannelRow _selectedRow;

            private string _filter = string.Empty;
            private EchoMonitorSettings.ChannelSortMode _sortMode = EchoMonitorSettings.SortMode;

            public event Action<ChannelModel> OnChannelSelected;

            public ChannelSidebarView()
            {
                  style.minWidth = 220;
                  style.flexDirection = FlexDirection.Column;
                  style.backgroundColor = EchoTheme.Surface1;
                  style.borderRightColor = EchoTheme.BorderSubtle;
                  style.borderRightWidth = 1;

                  VisualElement headerBar = new()
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              alignItems = Align.Center,
                              borderBottomColor = EchoTheme.BorderSubtle,
                              borderBottomWidth = 1
                        }
                  };
                  EchoTheme.ApplyPadding(headerBar, EchoTheme.Space3, EchoTheme.Space5);

                  _header = new Label("CHANNELS")
                  {
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeSmall,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              letterSpacing = new StyleLength(1f),
                              flexGrow = 1
                        }
                  };
                  headerBar.Add(_header);

                  _count = new Label("0")
                  {
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeSmall,
                              unityFontStyleAndWeight = FontStyle.Bold
                        }
                  };
                  headerBar.Add(_count);
                  Add(headerBar);

                  _empty = new EmptyState(
                        "No channels yet",
                        "Enter Play mode and publish or subscribe to any IEvent struct to see channels appear here in real time.",
                        EmptyState.IconKind.Spark);
                  _empty.style.display = DisplayStyle.Flex;
                  Add(_empty);

                  _list = new ScrollView(ScrollViewMode.Vertical)
                  {
                        style = { flexGrow = 1 }
                  };
                  _list.style.display = DisplayStyle.None;
                  Add(_list);
            }

            public void SetFilter(string text)
            {
                  _filter = text ?? string.Empty;
                  ApplyFilter();
            }

            public void SetSort(EchoMonitorSettings.ChannelSortMode mode)
            {
                  _sortMode = mode;
                  RebuildOrder();
            }

            public void Refresh(IEnumerable<ChannelModel> channels)
            {
                  HashSet<Type> seen = new();
                  List<ChannelModel> list = channels.ToList();

                  foreach (ChannelModel channel in list)
                  {
                        seen.Add(channel.EventType);

                        if (!_rows.TryGetValue(channel.EventType, out ChannelRow row))
                        {
                              row = new ChannelRow(channel);
                              row.OnClick += () => HandleRowClick(row);
                              _rows[channel.EventType] = row;
                              _list.Add(row);
                        }

                        row.Refresh();
                  }
                  
                  List<Type> toRemove = null;

                  foreach (Type t in _rows.Keys)
                  {
                        if (!seen.Contains(t))
                        {
                              toRemove ??= new List<Type>();
                              toRemove.Add(t);
                        }
                  }

                  if (toRemove != null)
                  {
                        foreach (Type t in toRemove)
                        {
                              ChannelRow row = _rows[t];
                              _list.Remove(row);
                              _rows.Remove(t);
                        }
                  }

                  _count.text = list.Count.ToString();
                  RebuildOrder();
                  ApplyFilter();
                  UpdateEmptyState();
            }

            public void PeriodicRefresh()
            {
                  foreach (ChannelRow row in _rows.Values)
                  {
                        row.Refresh();
                  }
            }

            public void OnChannelTick(Type eventType)
            {
                  if (_rows.TryGetValue(eventType, out ChannelRow row))
                  {
                        row.Flash();
                  }
            }

            public void MarkSelected(ChannelModel model)
            {
                  _selectedRow?.SetSelected(false);

                  if (_rows.TryGetValue(model.EventType, out ChannelRow row))
                  {
                        row.SetSelected(true);
                        _selectedRow = row;
                  }
            }

            private void HandleRowClick(ChannelRow row)
            {
                  OnChannelSelected?.Invoke(row.Model);
            }

            private void ApplyFilter()
            {
                  string filter = _filter.Trim();
                  bool anyVisible = false;

                  foreach (ChannelRow row in _rows.Values)
                  {
                        bool visible = string.IsNullOrEmpty(filter)
                              || row.Model.EventType.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
                        row.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                        anyVisible |= visible;
                  }

                  _ = anyVisible;
            }

            private void RebuildOrder()
            {
                  List<ChannelRow> ordered = _rows.Values.ToList();
                  ordered.Sort(CompareRows);

                  for (int i = 0; i < ordered.Count; i++)
                  {
                        ChannelRow row = ordered[i];

                        if (_list.IndexOf(row) != i)
                        {
                              _list.Remove(row);
                              _list.Insert(i, row);
                        }
                  }
            }

            private int CompareRows(ChannelRow a, ChannelRow b)
            {
                  return _sortMode switch
                  {
                        EchoMonitorSettings.ChannelSortMode.Alphabetical
                              => string.CompareOrdinal(a.Model.EventType.Name, b.Model.EventType.Name),
                        EchoMonitorSettings.ChannelSortMode.Activity
                              => b.Model.Stats.CurrentFrameDispatches.CompareTo(a.Model.Stats.CurrentFrameDispatches),
                        EchoMonitorSettings.ChannelSortMode.LastFired
                              => b.Model.Stats.LastDispatchTime.CompareTo(a.Model.Stats.LastDispatchTime),
                        EchoMonitorSettings.ChannelSortMode.TotalCount
                              => b.Model.Stats.TotalDispatches.CompareTo(a.Model.Stats.TotalDispatches),
                        _ => 0
                  };
            }

            private void UpdateEmptyState()
            {
                  bool hasChannels = _rows.Count > 0;
                  _empty.style.display = hasChannels ? DisplayStyle.None : DisplayStyle.Flex;
                  _list.style.display = hasChannels ? DisplayStyle.Flex : DisplayStyle.None;
            }
      }
}