using System;
using OpalStudio.Echo.Profiler.Editor.Services;
using OpalStudio.Echo.Profiler.Editor.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Views
{
      public sealed class ToolbarView : VisualElement
      {
            private readonly Label _liveLabel;
            private readonly StatusDot _liveDot;
            private readonly VisualElement _liveButton;
            private readonly VisualElement _leakBadge;
            private readonly Label _leakLabel;
            private readonly TextField _search;
            private readonly EnumField _sort;
            private readonly Button _feedToggle;

            private bool _live = true;

            public event Action<bool> OnLiveToggle;
            public event Action OnClear;
            public event Action OnPruneGhosts;
            public event Action OnToggleFeed;
            public event Action<string> OnSearchChanged;
            public event Action<EchoMonitorSettings.ChannelSortMode> OnSortChanged;

            public ToolbarView()
            {
                  style.flexDirection = FlexDirection.Row;
                  style.alignItems = Align.Center;
                  style.height = 32;
                  style.backgroundColor = EchoTheme.Surface1;
                  style.borderBottomColor = EchoTheme.BorderSubtle;
                  style.borderBottomWidth = 1;
                  EchoTheme.ApplyPadding(this, 0, EchoTheme.Space4);

                  _liveDot = new StatusDot(8) { style = { marginRight = EchoTheme.Space2 } };
                  _liveLabel = new Label("Live")
                  {
                        style =
                        {
                              color = EchoTheme.TextPrimary,
                              fontSize = EchoTheme.FontSizeBody,
                              unityFontStyleAndWeight = FontStyle.Bold
                        }
                  };

                  _liveButton = new VisualElement
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              alignItems = Align.Center,
                              paddingTop = 3,
                              paddingBottom = 3,
                              paddingLeft = EchoTheme.Space3,
                              paddingRight = EchoTheme.Space4,
                              marginRight = EchoTheme.Space3
                        }
                  };
                  EchoTheme.ApplyRadius(_liveButton, EchoTheme.RadiusSmall);
                  _liveButton.AddToClassList("echo-toolbar-button");
                  _liveButton.Add(_liveDot);
                  _liveButton.Add(_liveLabel);
                  _liveButton.RegisterCallback<ClickEvent>(_ =>
                  {
                        _live = !_live;
                        UpdateLiveButton();
                        OnLiveToggle?.Invoke(_live);
                  });
                  Add(_liveButton);

                  AddSeparator();

                  Add(MakeButton("Clear", () => OnClear?.Invoke()));
                  _feedToggle = MakeButton("Feed", () => OnToggleFeed?.Invoke());
                  Add(_feedToggle);

                  Add(new VisualElement { style = { flexGrow = 1 } });

                  _leakBadge = new VisualElement
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              alignItems = Align.Center,
                              backgroundColor = EchoTheme.GhostMuted,
                              paddingTop = 2,
                              paddingBottom = 2,
                              paddingLeft = EchoTheme.Space3,
                              paddingRight = EchoTheme.Space3,
                              marginRight = EchoTheme.Space3,
                              display = DisplayStyle.None
                        }
                  };
                  EchoTheme.ApplyRadius(_leakBadge, EchoTheme.RadiusSmall);
                  EchoTheme.ApplyBorder(_leakBadge, new Color(EchoTheme.Ghost.r, EchoTheme.Ghost.g, EchoTheme.Ghost.b, 0.35f));

                  StatusDot leakDot = new(7);
                  leakDot.SetColor(EchoTheme.Ghost);
                  leakDot.SetPulse(true);
                  leakDot.style.marginRight = EchoTheme.Space2;
                  _leakBadge.Add(leakDot);

                  _leakLabel = new Label("0 leaks")
                  {
                        style =
                        {
                              color = EchoTheme.Ghost,
                              fontSize = EchoTheme.FontSizeSmall,
                              unityFontStyleAndWeight = FontStyle.Bold
                        }
                  };
                  _leakBadge.Add(_leakLabel);

                  Button pruneInBadge = new(() => OnPruneGhosts?.Invoke())
                  {
                        text = "Prune",
                        style =
                        {
                              color = EchoTheme.Ghost,
                              fontSize = EchoTheme.FontSizeMicro,
                              backgroundColor = new StyleColor(StyleKeyword.None),
                              marginLeft = EchoTheme.Space3,
                              marginRight = 0,
                              marginTop = 0,
                              marginBottom = 0,
                              paddingTop = 0,
                              paddingBottom = 0,
                              paddingLeft = EchoTheme.Space2,
                              paddingRight = EchoTheme.Space2,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              borderTopWidth = 0,
                              borderBottomWidth = 0,
                              borderLeftWidth = 0,
                              borderRightWidth = 0
                        }
                  };
                  _leakBadge.Add(pruneInBadge);

                  Add(_leakBadge);

                  _search = new TextField
                  {
                        style =
                        {
                              width = 180,
                              marginRight = EchoTheme.Space3,
                              height = 22
                        }
                  };
                  _search.AddToClassList("echo-search");
                  _search.RegisterValueChangedCallback(evt => OnSearchChanged?.Invoke(evt.newValue));

                  Label searchLabel = new("Filter")
                  {
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeSmall,
                              marginRight = EchoTheme.Space2
                        }
                  };
                  Add(searchLabel);
                  Add(_search);

                  _sort = new EnumField(EchoMonitorSettings.SortMode)
                  {
                        style =
                        {
                              width = 110,
                              height = 22
                        }
                  };
                  _sort.RegisterValueChangedCallback(evt =>
                  {
                        EchoMonitorSettings.ChannelSortMode mode = (EchoMonitorSettings.ChannelSortMode)evt.newValue;
                        EchoMonitorSettings.SortMode = mode;
                        OnSortChanged?.Invoke(mode);
                  });
                  Add(_sort);

                  UpdateLiveButton();
            }

            public void SetLive(bool live)
            {
                  _live = live;
                  UpdateLiveButton();
            }

            public void UpdateLeakBadge(int count)
            {
                  if (count <= 0)
                  {
                        _leakBadge.style.display = DisplayStyle.None;

                        return;
                  }

                  _leakBadge.style.display = DisplayStyle.Flex;
                  _leakLabel.text = count == 1 ? "1 leak" : $"{count} leaks";
            }

            public void SetFeedVisible(bool visible)
            {
                  if (visible)
                  {
                        _feedToggle.AddToClassList("echo-toolbar-button--active");
                  }
                  else
                  {
                        _feedToggle.RemoveFromClassList("echo-toolbar-button--active");
                  }
            }

            public void FocusSearch()
            {
                  _search?.Focus();
            }

            private void UpdateLiveButton()
            {
                  if (_live)
                  {
                        _liveLabel.text = "Live";
                        _liveLabel.style.color = EchoTheme.TextPrimary;
                        _liveDot.SetColor(EchoTheme.Success);
                        _liveDot.SetPulse(true);
                  }
                  else
                  {
                        _liveLabel.text = "Paused";
                        _liveLabel.style.color = EchoTheme.TextSecondary;
                        _liveDot.SetColor(EchoTheme.TextMuted);
                        _liveDot.SetPulse(false);
                  }
            }

            private void AddSeparator()
            {
                  VisualElement sep = new()
                  {
                        style =
                        {
                              width = 1,
                              height = 16,
                              backgroundColor = EchoTheme.BorderSubtle,
                              marginLeft = EchoTheme.Space2,
                              marginRight = EchoTheme.Space3,
                              alignSelf = Align.Center
                        }
                  };
                  Add(sep);
            }

            private static Button MakeButton(string text, Action onClick)
            {
                  Button button = new(onClick)
                  {
                        text = text,
                        style =
                        {
                              backgroundColor = new StyleColor(StyleKeyword.None),
                              color = EchoTheme.TextSecondary,
                              fontSize = EchoTheme.FontSizeBody,
                              borderTopWidth = 0,
                              borderBottomWidth = 0,
                              borderLeftWidth = 0,
                              borderRightWidth = 0,
                              paddingTop = 3,
                              paddingBottom = 3,
                              paddingLeft = EchoTheme.Space3,
                              paddingRight = EchoTheme.Space3,
                              marginLeft = 0,
                              marginRight = EchoTheme.Space2
                        }
                  };
                  EchoTheme.ApplyRadius(button, EchoTheme.RadiusSmall);
                  button.AddToClassList("echo-toolbar-button");

                  return button;
            }
      }
}