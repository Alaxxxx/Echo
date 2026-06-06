using System;
using OpalStudio.Echo.Profiler.Editor.Model;
using OpalStudio.Echo.Profiler.Editor.Services;
using OpalStudio.Echo.Profiler.Editor.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Views
{
      public sealed class ChannelRow : VisualElement
      {
            public ChannelModel Model { get; }
            public event Action OnClick;

            private readonly VisualElement _accent;
            private readonly StatusDot _dot;
            private readonly Label _name;
            private readonly Label _rate;
            private readonly Label _subsCount;
            private readonly Sparkline _spark;
            private readonly Label _meta;
            private readonly VisualElement _ghostFlag;

            private IVisualElementScheduledItem _flashSchedule;

            public ChannelRow(ChannelModel model)
            {
                  Model = model;
                  AddToClassList("echo-channel-row");
                  style.flexDirection = FlexDirection.Row;
                  style.alignItems = Align.Stretch;
                  style.paddingLeft = 11;
                  style.paddingRight = EchoTheme.Space4;
                  style.paddingTop = EchoTheme.Space3;
                  style.paddingBottom = EchoTheme.Space3;
                  RegisterCallback<ClickEvent>(_ => OnClick?.Invoke());

                  this.AddManipulator(new ContextualMenuManipulator(menu =>
                  {
                        menu.menu.AppendAction("Copy type name", _ => UnityEditor.EditorGUIUtility.systemCopyBuffer = Model.EventType.FullName);
                        menu.menu.AppendAction("Open event source", _ => ScriptLocator.OpenType(Model.EventType));
                        menu.menu.AppendSeparator();
                        menu.menu.AppendAction("Reset stats for this channel", _ => Model.Stats.Reset());
                  }));

                  _accent = new VisualElement
                  {
                        style =
                        {
                              width = 2,
                              marginLeft = -11,
                              marginRight = EchoTheme.Space3,
                              backgroundColor = new StyleColor(StyleKeyword.None)
                        }
                  };
                  _accent.AddToClassList("echo-channel-row__accent");
                  Add(_accent);

                  VisualElement main = new()
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Column,
                              flexGrow = 1,
                              flexShrink = 1,
                              minWidth = 0
                        }
                  };
                  Add(main);

                  VisualElement topRow = new()
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              alignItems = Align.Center
                        }
                  };
                  _dot = new StatusDot(7);
                  _dot.style.marginRight = EchoTheme.Space3;
                  topRow.Add(_dot);

                  _name = new Label(model.EventType.Name)
                  {
                        style =
                        {
                              color = EchoTheme.TextPrimary,
                              fontSize = EchoTheme.FontSizeBody,
                              flexGrow = 1,
                              flexShrink = 1,
                              minWidth = 0,
                              overflow = Overflow.Hidden,
                              textOverflow = TextOverflow.Ellipsis,
                              whiteSpace = WhiteSpace.NoWrap
                        }
                  };
                  topRow.Add(_name);

                  _ghostFlag = MakeGhostFlag();
                  _ghostFlag.style.display = DisplayStyle.None;
                  topRow.Add(_ghostFlag);

                  _subsCount = new Label("0")
                  {
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeMicro,
                              marginLeft = EchoTheme.Space3,
                              minWidth = 14,
                              unityTextAlign = TextAnchor.MiddleRight
                        }
                  };
                  topRow.Add(_subsCount);
                  main.Add(topRow);
                  
                  VisualElement botRow = new()
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              alignItems = Align.Center,
                              marginTop = EchoTheme.Space2,
                              marginLeft = 7 + EchoTheme.Space3
                        }
                  };
                  _spark = new Sparkline
                  {
                        style =
                        {
                              height = 12,
                              flexGrow = 1,
                              minWidth = 30
                        }
                  };
                  _spark.SetShowBaseline(true);
                  botRow.Add(_spark);

                  _meta = new Label("idle")
                  {
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeMicro,
                              marginLeft = EchoTheme.Space3,
                              minWidth = 40,
                              unityTextAlign = TextAnchor.MiddleRight
                        }
                  };
                  botRow.Add(_meta);

                  main.Add(botRow);
            }

            public void SetSelected(bool selected)
            {
                  if (selected)
                  {
                        AddToClassList("echo-channel-row--selected");
                        style.backgroundColor = EchoTheme.SurfaceSelected;
                  }
                  else
                  {
                        RemoveFromClassList("echo-channel-row--selected");
                        style.backgroundColor = new StyleColor(StyleKeyword.None);
                  }
            }

            public void Refresh()
            {
                  ChannelStats s = Model.Stats;
                  int current = s.CurrentFrameDispatches;
                  long total = s.TotalDispatches;

                  Color dotColor;
                  bool pulse = false;

                  if (total == 0)
                  {
                        dotColor = EchoTheme.TextDisabled;
                        _meta.text = "idle";
                  }
                  else if (current > 30)
                  {
                        dotColor = EchoTheme.Danger;
                        pulse = true;
                        _meta.text = $"{current}/f";
                  }
                  else if (current > 0)
                  {
                        dotColor = EchoTheme.Warning;
                        _meta.text = $"{current}/f";
                  }
                  else
                  {
                        dotColor = EchoTheme.Success;
                        _meta.text = total >= 1000 ? $"{total / 1000}k" : total.ToString();
                  }

                  _dot.SetColor(dotColor);
                  _dot.SetPulse(pulse);

                  _spark.SetData(s.CopyFrameWindow());
                  _spark.SetColor(dotColor);

                  int subs = Model.Subscribers.Count;
                  int ghosts = 0;

                  for (int i = 0; i < subs; i++)
                  {
                        if (Model.Subscribers[i].IsGhost)
                        {
                              ghosts++;
                        }
                  }

                  _subsCount.text = subs.ToString();
                  _subsCount.style.color = subs == 0 ? EchoTheme.TextDisabled : EchoTheme.TextSecondary;
                  _ghostFlag.style.display = ghosts > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }

            public void Flash()
            {
                  AddToClassList("echo-channel-row--flash");

                  _flashSchedule?.Pause();
                  _flashSchedule = schedule.Execute(() =>
                  {
                        RemoveFromClassList("echo-channel-row--flash");
                        _flashSchedule = null;
                  }).StartingIn(150);
            }

            private static VisualElement MakeGhostFlag()
            {
                  Badge badge = new("ghost", EchoTheme.Ghost);

                  return badge;
            }
      }
}