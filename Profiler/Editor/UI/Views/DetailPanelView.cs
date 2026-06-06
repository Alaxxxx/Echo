using System.Collections.Generic;
using OpalStudio.Echo.Profiler.Editor.Model;
using OpalStudio.Echo.Profiler.Editor.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Views
{
      /// <summary>
      /// Right-side detail panel. Header bar with the channel's full name,
      /// thin tab strip, and a scrollable content area.
      /// </summary>
      public sealed class DetailPanelView : VisualElement
      {
            private readonly Label _channelTitle;
            private readonly Label _channelSubtitle;
            private readonly VisualElement _tabStrip;
            private readonly ScrollView _content;
            private readonly EmptyState _empty;

            private OverviewView _overview;
            private SubscribersView _subscribers;

            private readonly List<TabButton> _tabs = new();
            private int _activeTab;
            private ChannelModel _current;

            public DetailPanelView()
            {
                  style.flexGrow = 1;
                  style.flexDirection = FlexDirection.Column;
                  style.backgroundColor = EchoTheme.Surface0;

                  VisualElement header = new()
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Column,
                              borderBottomColor = EchoTheme.BorderSubtle,
                              borderBottomWidth = 1
                        }
                  };
                  EchoTheme.ApplyPadding(header, EchoTheme.Space5, EchoTheme.Space6);

                  _channelTitle = new Label("(no channel selected)")
                  {
                        style =
                        {
                              color = EchoTheme.TextPrimary,
                              fontSize = EchoTheme.FontSizeTitle,
                              unityFontStyleAndWeight = FontStyle.Bold
                        }
                  };
                  header.Add(_channelTitle);

                  _channelSubtitle = new Label(string.Empty)
                  {
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeSmall,
                              marginTop = EchoTheme.Space1
                        }
                  };
                  header.Add(_channelSubtitle);
                  Add(header);

                  _tabStrip = new VisualElement
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              borderBottomColor = EchoTheme.BorderSubtle,
                              borderBottomWidth = 1,
                              paddingLeft = EchoTheme.Space5
                        }
                  };
                  Add(_tabStrip);

                  AddTab("Overview");
                  AddTab("Subscribers");

                  _content = new ScrollView(ScrollViewMode.Vertical)
                  {
                        style = { flexGrow = 1 }
                  };
                  EchoTheme.ApplyPadding(_content, EchoTheme.Space5, EchoTheme.Space6);
                  Add(_content);

                  _empty = new EmptyState(
                        "Select a channel",
                        "Pick a channel from the sidebar to see its statistics, subscribers, and recent dispatches. If the sidebar is empty, publish or subscribe to an event in Play mode and channels will appear in real time.",
                        EmptyState.IconKind.Spark);

                  ShowEmptyState();
            }

            public void ShowEmptyState()
            {
                  _current = null;
                  _channelTitle.text = "(no channel selected)";
                  _channelSubtitle.text = string.Empty;
                  _content.Clear();
                  _content.Add(_empty);
            }

            public void Show(ChannelModel model)
            {
                  _current = model;
                  _channelTitle.text = model.EventType.Name;
                  _channelSubtitle.text = model.EventType.FullName;
                  RenderActiveTab();
            }

            public void Refresh(ChannelModel model)
            {
                  if (_current != model)
                  {
                        return;
                  }

                  if (_activeTab == 0)
                  {
                        _overview?.Refresh();
                  }
                  else
                  {
                        _subscribers?.Refresh();
                  }
            }

            private void AddTab(string label)
            {
                  int index = _tabs.Count;
                  TabButton tab = new(label, () => SelectTab(index));
                  _tabs.Add(tab);
                  _tabStrip.Add(tab);

                  if (index == 0)
                  {
                        tab.SetActive(true);
                  }
            }

            private void SelectTab(int index)
            {
                  if (_activeTab == index)
                  {
                        return;
                  }

                  _tabs[_activeTab].SetActive(false);
                  _activeTab = index;
                  _tabs[_activeTab].SetActive(true);
                  RenderActiveTab();
            }

            private void RenderActiveTab()
            {
                  if (_current == null)
                  {
                        ShowEmptyState();

                        return;
                  }

                  _content.Clear();

                  switch (_activeTab)
                  {
                        case 0:
                              _overview ??= new OverviewView();
                              _overview.Bind(_current);
                              _content.Add(_overview);

                              break;

                        case 1:
                              _subscribers ??= new SubscribersView();
                              _subscribers.Bind(_current);
                              _content.Add(_subscribers);

                              break;
                  }
            }

            private sealed class TabButton : VisualElement
            {
                  private readonly Label _label;

                  public TabButton(string text, System.Action onClick)
                  {
                        AddToClassList("echo-tab");
                        style.paddingTop = EchoTheme.Space3;
                        style.paddingBottom = EchoTheme.Space3;
                        style.paddingLeft = EchoTheme.Space4;
                        style.paddingRight = EchoTheme.Space4;
                        style.marginRight = EchoTheme.Space3;
                        RegisterCallback<ClickEvent>(_ => onClick?.Invoke());

                        _label = new Label(text)
                        {
                              style =
                              {
                                    color = EchoTheme.TextMuted,
                                    fontSize = EchoTheme.FontSizeBody
                              }
                        };
                        Add(_label);
                  }

                  public void SetActive(bool active)
                  {
                        if (active)
                        {
                              AddToClassList("echo-tab--active");
                              _label.style.color = EchoTheme.TextPrimary;
                              _label.style.unityFontStyleAndWeight = FontStyle.Bold;
                        }
                        else
                        {
                              RemoveFromClassList("echo-tab--active");
                              _label.style.color = EchoTheme.TextMuted;
                              _label.style.unityFontStyleAndWeight = FontStyle.Normal;
                        }
                  }
            }
      }
}