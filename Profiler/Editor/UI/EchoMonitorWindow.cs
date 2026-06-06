using System;
using OpalStudio.Echo.Profiler.Editor.Model;
using OpalStudio.Echo.Profiler.Editor.Services;
using OpalStudio.Echo.Profiler.Editor.UI.Views;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI
{
      public sealed class EchoMonitorWindow : EditorWindow
      {
            private const string MenuPath = "Window/Echo/Monitor";
            private const string StyleSheetName = "EchoMonitor";

            private ToolbarView _toolbar;
            private ChannelSidebarView _sidebar;
            private DetailPanelView _detailPanel;
            private LiveFeedView _liveFeed;
            private TwoPaneSplitView _mainSplitter;
            private TwoPaneSplitView _bodySplitter;

            private ChannelModel _selected;
            private IVisualElementScheduledItem _refreshSchedule;

            [MenuItem(MenuPath)]
            public static void Open()
            {
                  EchoMonitorWindow window = GetWindow<EchoMonitorWindow>();
                  window.titleContent = new GUIContent("Echo Monitor");
                  window.minSize = new Vector2(820f, 480f);
                  window.Show();
            }

            private void CreateGUI()
            {
                  VisualElement root = rootVisualElement;
                  root.style.flexDirection = FlexDirection.Column;
                  root.style.backgroundColor = EchoTheme.Surface0;

                  StyleSheet sheet = Resources.Load<StyleSheet>(StyleSheetName);

                  if (sheet != null)
                  {
                        root.styleSheets.Add(sheet);
                  }

                  root.focusable = true;
                  root.RegisterCallback<KeyDownEvent>(OnKeyDown);

                  _toolbar = new ToolbarView();
                  _toolbar.OnLiveToggle += SetLive;
                  _toolbar.OnClear += OnClear;
                  _toolbar.OnPruneGhosts += OnPruneGhosts;
                  _toolbar.OnToggleFeed += OnToggleFeed;
                  _toolbar.OnSearchChanged += text => _sidebar?.SetFilter(text);
                  _toolbar.OnSortChanged += mode => _sidebar?.SetSort(mode);
                  root.Add(_toolbar);

                  _mainSplitter = new TwoPaneSplitView(
                        0,
                        420f,
                        TwoPaneSplitViewOrientation.Vertical
                  )
                  {
                        style = { flexGrow = 1 },
                        viewDataKey = "echo.monitor.mainSplit"
                  };
                  root.Add(_mainSplitter);

                  _bodySplitter = new TwoPaneSplitView(
                        0,
                        EchoMonitorSettings.SidebarWidth,
                        TwoPaneSplitViewOrientation.Horizontal
                  )
                  {
                        viewDataKey = "echo.monitor.bodySplit"
                  };

                  _sidebar = new ChannelSidebarView();
                  _sidebar.OnChannelSelected += OnChannelSelected;
                  _bodySplitter.Add(_sidebar);

                  _detailPanel = new DetailPanelView();
                  _bodySplitter.Add(_detailPanel);

                  _mainSplitter.Add(_bodySplitter);

                  _liveFeed = new LiveFeedView();
                  _mainSplitter.Add(_liveFeed);

                  EchoProfilerService.ModelChanged += OnModelChanged;
                  EchoProfilerService.ChannelDiscovered += OnChannelDiscovered;
                  EchoProfilerService.DispatchObserved += OnDispatchObserved;

                  RefreshAll();
                  ApplyFeedVisibility();

                  int pollingMs = Mathf.Max(33, 1000 / EchoMonitorSettings.PollingHz);
                  _refreshSchedule = root.schedule.Execute(PeriodicRefresh).Every(pollingMs);
            }

            private void OnDestroy()
            {
                  EchoProfilerService.ModelChanged -= OnModelChanged;
                  EchoProfilerService.ChannelDiscovered -= OnChannelDiscovered;
                  EchoProfilerService.DispatchObserved -= OnDispatchObserved;
            }

            private void OnModelChanged()
            {
                  _sidebar.Refresh(EchoProfilerService.AllChannels.Values);
                  RefreshSelected();
                  _toolbar.UpdateLeakBadge(EchoProfilerService.CountGhosts());
                  _toolbar.SetLive(EchoProfilerService.IsLive);
            }

            private void OnChannelDiscovered(ChannelModel model)
            {
                  _sidebar.Refresh(EchoProfilerService.AllChannels.Values);

                  if (_selected == null)
                  {
                        OnChannelSelected(model);
                  }
            }

            private void OnDispatchObserved(DispatchRecord record)
            {
                  _liveFeed?.OnDispatch(record);

                  _sidebar?.OnChannelTick(record.EventType);
            }

            private void OnChannelSelected(ChannelModel model)
            {
                  _selected = model;
                  _sidebar.MarkSelected(model);
                  _detailPanel.Show(model);
            }

            private void RefreshAll()
            {
                  _sidebar.Refresh(EchoProfilerService.AllChannels.Values);
                  _toolbar.UpdateLeakBadge(EchoProfilerService.CountGhosts());
                  _toolbar.SetLive(EchoProfilerService.IsLive);
                  RefreshSelected();
            }

            private void RefreshSelected()
            {
                  if (_selected != null)
                  {
                        _detailPanel.Refresh(_selected);
                  }
            }

            private void PeriodicRefresh()
            {
                  _sidebar.PeriodicRefresh();
                  RefreshSelected();
                  _toolbar.UpdateLeakBadge(EchoProfilerService.CountGhosts());
            }

            private void SetLive(bool live) => EchoProfilerService.SetLive(live);

            private void OnClear()
            {
                  EchoProfilerService.Clear();
                  _selected = null;
                  _detailPanel.ShowEmptyState();
                  _liveFeed.ClearFeed();
            }

            private void OnPruneGhosts()
            {
                  int removed = EchoProfilerService.PruneGhosts();
                  ShowNotification(new GUIContent(removed == 0
                        ? "No ghost subscribers found."
                        : $"Pruned {removed} ghost subscriber{(removed > 1 ? "s" : string.Empty)}."));
            }

            private void OnToggleFeed()
            {
                  EchoMonitorSettings.ShowFeed = !EchoMonitorSettings.ShowFeed;
                  ApplyFeedVisibility();
            }

            private void ApplyFeedVisibility()
            {
                  if (_liveFeed == null)
                  {
                        return;
                  }

                  _liveFeed.style.display = EchoMonitorSettings.ShowFeed ? DisplayStyle.Flex : DisplayStyle.None;
                  _toolbar?.SetFeedVisible(EchoMonitorSettings.ShowFeed);
            }

            private void OnKeyDown(KeyDownEvent evt)
            {
                  if (!evt.ctrlKey && !evt.commandKey)
                  {
                        return;
                  }

                  switch (evt.keyCode)
                  {
                        case KeyCode.L:
                              SetLive(!EchoProfilerService.IsLive);
                              evt.StopPropagation();

                              break;

                        case KeyCode.K:
                              OnClear();
                              evt.StopPropagation();

                              break;

                        case KeyCode.F:
                              _toolbar?.FocusSearch();
                              evt.StopPropagation();

                              break;
                  }
            }
      }
}