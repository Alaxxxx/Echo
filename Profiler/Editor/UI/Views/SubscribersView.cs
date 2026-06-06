using System.Collections.Generic;
using System.Linq;
using OpalStudio.Echo.Core.Diagnostics;
using OpalStudio.Echo.Profiler.Editor.Model;
using OpalStudio.Echo.Profiler.Editor.Services;
using OpalStudio.Echo.Profiler.Editor.UI.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Views
{
      public sealed class SubscribersView : VisualElement
      {
            private readonly VisualElement _ghostPanel;
            private readonly Label _ghostExplain;
            private readonly VisualElement _ghostList;
            private readonly VisualElement _liveList;
            private readonly Label _summary;
            private readonly EmptyState _emptyLive;

            private ChannelModel _model;

            public SubscribersView()
            {
                  style.flexDirection = FlexDirection.Column;

                  _summary = new Label("0 active · 0 ghosts")
                  {
                        style =
                        {
                              color = EchoTheme.TextSecondary,
                              fontSize = EchoTheme.FontSizeBody,
                              marginBottom = EchoTheme.Space5,
                              marginLeft = 3
                        }
                  };
                  Add(_summary);

                  _ghostPanel = new VisualElement();
                  _ghostPanel.style.backgroundColor = EchoTheme.GhostMuted;
                  EchoTheme.ApplyBorder(_ghostPanel, new Color(EchoTheme.Ghost.r, EchoTheme.Ghost.g, EchoTheme.Ghost.b, 0.35f));
                  EchoTheme.ApplyRadius(_ghostPanel, EchoTheme.RadiusMedium);
                  EchoTheme.ApplyPadding(_ghostPanel, EchoTheme.Space5, EchoTheme.Space6);
                  _ghostPanel.style.marginBottom = EchoTheme.Space5;
                  _ghostPanel.style.borderLeftWidth = 3;
                  _ghostPanel.style.borderLeftColor = EchoTheme.Ghost;

                  VisualElement ghostHeader = new()
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              alignItems = Align.Center,
                              marginBottom = EchoTheme.Space3
                        }
                  };
                  StatusDot ghostDot = new(10);
                  ghostDot.SetColor(EchoTheme.Ghost);
                  ghostDot.SetPulse(true);
                  ghostDot.style.marginRight = EchoTheme.Space3;
                  ghostHeader.Add(ghostDot);

                  Label ghostTitle = new("Ghost subscribers detected")
                  {
                        style =
                        {
                              color = EchoTheme.Ghost,
                              fontSize = EchoTheme.FontSizeStrong,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              flexGrow = 1
                        }
                  };
                  ghostHeader.Add(ghostTitle);
                  _ghostPanel.Add(ghostHeader);

                  _ghostExplain = new Label(BuildGhostExplanation(0))
                  {
                        style =
                        {
                              color = EchoTheme.TextSecondary,
                              fontSize = EchoTheme.FontSizeBody,
                              whiteSpace = WhiteSpace.Normal,
                              marginBottom = EchoTheme.Space4
                        }
                  };
                  _ghostPanel.Add(_ghostExplain);

                  _ghostList = new VisualElement();
                  _ghostPanel.Add(_ghostList);

                  Add(_ghostPanel);

                  Add(new Label("ACTIVE SUBSCRIBERS")
                  {
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeSmall,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              letterSpacing = new StyleLength(1.5f),
                              marginTop = EchoTheme.Space3,
                              marginBottom = EchoTheme.Space3,
                              marginLeft = 3
                        }
                  });

                  _liveList = new VisualElement
                  {
                        style =
                        {
                              backgroundColor = EchoTheme.Surface2,
                              marginLeft = 3,
                              marginRight = 3
                        }
                  };
                  EchoTheme.ApplyBorder(_liveList, EchoTheme.BorderSubtle);
                  EchoTheme.ApplyRadius(_liveList, EchoTheme.RadiusMedium);
                  Add(_liveList);

                  _emptyLive = new EmptyState(
                        "No active subscribers",
                        "Nothing is currently listening to this event type. Calls to Publish() will be no-ops.",
                        EmptyState.IconKind.Empty);
            }

            public void Bind(ChannelModel model)
            {
                  _model = model;
                  Render();
            }

            public void Refresh() => Render();

            private void Render()
            {
                  _ghostList.Clear();
                  _liveList.Clear();

                  if (_model == null)
                  {
                        _summary.text = "No channel.";
                        _ghostPanel.style.display = DisplayStyle.None;

                        return;
                  }

                  List<SubscriberInfo> ghosts = _model.Subscribers.Where(s => s.IsGhost).ToList();
                  List<SubscriberInfo> live = _model.Subscribers.Where(s => !s.IsGhost).ToList();

                  int totalHits = 0;

                  for (int i = 0; i < live.Count; i++)
                  {
                        totalHits += live[i].Hits;
                  }

                  _summary.text = $"{live.Count} active subscriber{(live.Count != 1 ? "s" : string.Empty)}"
                        + $" · {FormatNumber(totalHits)} total hit{(totalHits != 1 ? "s" : string.Empty)}"
                        + (ghosts.Count > 0 ? $" · {ghosts.Count} ghost{(ghosts.Count != 1 ? "s" : string.Empty)}" : string.Empty);

                  if (ghosts.Count == 0)
                  {
                        _ghostPanel.style.display = DisplayStyle.None;
                  }
                  else
                  {
                        _ghostPanel.style.display = DisplayStyle.Flex;
                        _ghostExplain.text = BuildGhostExplanation(ghosts.Count);

                        foreach (SubscriberInfo info in ghosts)
                        {
                              _ghostList.Add(BuildRow(info, true));
                        }
                  }

                  if (live.Count == 0)
                  {
                        _liveList.Add(_emptyLive);

                        return;
                  }

                  for (int i = 0; i < live.Count; i++)
                  {
                        VisualElement row = BuildRow(live[i], false);

                        if (i < live.Count - 1)
                        {
                              row.style.borderBottomColor = EchoTheme.BorderSubtle;
                              row.style.borderBottomWidth = 1;
                        }

                        _liveList.Add(row);
                  }
            }

            private static string BuildGhostExplanation(int count)
            {
                  return count == 0
                        ? string.Empty
                        : "A ghost subscriber is a handler whose target GameObject or MonoBehaviour has been destroyed, "
                          + "but whose subscription is still registered. The handler will keep firing on a dead object, "
                          + "leaking memory and possibly throwing exceptions. "
                          + "Most commonly this means a Subscribe() in OnEnable was not paired with an Unsubscribe() in OnDisable. "
                          + "Click Prune to remove all ghost entries from this channel.";
            }

            private static VisualElement BuildRow(SubscriberInfo info, bool isGhost)
            {
                  VisualElement row = new();
                  row.AddToClassList("echo-subscriber-row");
                  row.style.flexDirection = FlexDirection.Row;
                  row.style.alignItems = Align.Center;
                  EchoTheme.ApplyPadding(row, EchoTheme.Space3, EchoTheme.Space5);

                  Badge kindBadge = new(info.Kind.ToString(), ColorForKind(info.Kind));
                  kindBadge.style.width = 70;
                  row.Add(kindBadge);

                  VisualElement nameStack = new()
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Column,
                              flexGrow = 1,
                              flexShrink = 1,
                              minWidth = 0,
                              marginLeft = EchoTheme.Space4
                        }
                  };

                  Label target = new(info.DeclaringTypeName)
                  {
                        style =
                        {
                              color = isGhost ? EchoTheme.TextMuted : EchoTheme.TextPrimary,
                              fontSize = EchoTheme.FontSizeBody,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              overflow = Overflow.Hidden,
                              textOverflow = TextOverflow.Ellipsis,
                              whiteSpace = WhiteSpace.NoWrap
                        }
                  };
                  nameStack.Add(target);

                  Label method = new(info.MethodName + "()")
                  {
                        style =
                        {
                              color = isGhost ? EchoTheme.TextDisabled : EchoTheme.TextSecondary,
                              fontSize = EchoTheme.FontSizeMicro,
                              overflow = Overflow.Hidden,
                              textOverflow = TextOverflow.Ellipsis,
                              whiteSpace = WhiteSpace.NoWrap
                        }
                  };
                  nameStack.Add(method);

                  row.Add(nameStack);

                  if (!isGhost)
                  {
                        Label hits = new(FormatNumber(info.Hits))
                        {
                              style =
                              {
                                    color = info.Hits > 0 ? EchoTheme.TextSecondary : EchoTheme.TextDisabled,
                                    fontSize = EchoTheme.FontSizeSmall,
                                    unityFontStyleAndWeight = FontStyle.Bold,
                                    minWidth = 50,
                                    unityTextAlign = TextAnchor.MiddleRight,
                                    marginRight = EchoTheme.Space3
                              }
                        };
                        Label hitsLabel = new("hits")
                        {
                              style =
                              {
                                    color = EchoTheme.TextDisabled,
                                    fontSize = EchoTheme.FontSizeMicro,
                                    marginRight = EchoTheme.Space4
                              }
                        };
                        row.Add(hits);
                        row.Add(hitsLabel);
                  }

                  if (isGhost)
                  {
                        Label reason = new("destroyed")
                        {
                              style =
                              {
                                    color = EchoTheme.Ghost,
                                    fontSize = EchoTheme.FontSizeMicro,
                                    unityFontStyleAndWeight = FontStyle.Bold,
                                    marginRight = EchoTheme.Space3
                              }
                        };
                        row.Add(reason);
                  }
                  else
                  {
                        UnityEngine.Object unityTarget = info.TryGetUnityTarget();

                        if (unityTarget != null)
                        {
                              row.Add(MakeInlineAction("ping", () => EditorGUIUtility.PingObject(unityTarget)));
                        }

                        row.Add(MakeInlineAction("open", () => ScriptLocator.OpenType(info.DeclaringType, unityTarget)));
                  }

                  if (!isGhost)
                  {
                        UnityEngine.Object liveTarget = info.TryGetUnityTarget();
                        row.RegisterCallback<ClickEvent>(evt =>
                        {
                              if (evt.clickCount == 2)
                              {
                                    ScriptLocator.OpenType(info.DeclaringType, liveTarget);
                              }
                        });

                        row.AddManipulator(new ContextualMenuManipulator(menu =>
                        {
                              menu.menu.AppendAction("Open source", _ => ScriptLocator.OpenType(info.DeclaringType, liveTarget));

                              if (liveTarget != null)
                              {
                                    menu.menu.AppendAction("Ping in Hierarchy", _ => EditorGUIUtility.PingObject(liveTarget));
                                    menu.menu.AppendAction("Select", _ => Selection.activeObject = liveTarget);
                              }
                              else
                              {
                                    menu.menu.AppendAction("Ping in Hierarchy", _ => { }, DropdownMenuAction.Status.Disabled);
                              }

                              menu.menu.AppendSeparator();
                              menu.menu.AppendAction("Copy type name", _ => EditorGUIUtility.systemCopyBuffer = info.DeclaringType?.FullName ?? info.DeclaringTypeName);
                        }));
                  }

                  return row;
            }

            private static Button MakeInlineAction(string label, System.Action onClick)
            {
                  Button button = new(onClick)
                  {
                        text = label,
                        style =
                        {
                              color = EchoTheme.TextMuted,
                              fontSize = EchoTheme.FontSizeMicro,
                              backgroundColor = new StyleColor(StyleKeyword.None),
                              borderTopWidth = 0,
                              borderBottomWidth = 0,
                              borderLeftWidth = 0,
                              borderRightWidth = 0,
                              paddingTop = 1,
                              paddingBottom = 1,
                              paddingLeft = EchoTheme.Space3,
                              paddingRight = EchoTheme.Space3,
                              marginTop = 0,
                              marginBottom = 0,
                              marginLeft = 0,
                              marginRight = EchoTheme.Space2
                        }
                  };
                  EchoTheme.ApplyRadius(button, EchoTheme.RadiusSmall);
                  button.AddToClassList("echo-inline-action");

                  return button;
            }

            private static string FormatNumber(int n)
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

            private static Color ColorForKind(SubscriptionKind kind)
            {
                  return kind switch
                  {
                        SubscriptionKind.Plain => EchoTheme.KindPlain,
                        SubscriptionKind.Scoped => EchoTheme.KindScoped,
                        SubscriptionKind.Filtered => EchoTheme.KindFiltered,
                        SubscriptionKind.Once => EchoTheme.KindOnce,
                        _ => EchoTheme.TextMuted
                  };
            }
      }
}