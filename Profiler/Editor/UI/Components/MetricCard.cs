using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Components
{
      public sealed class MetricCard : VisualElement
      {
            private readonly Label _caption;
            private readonly Label _value;
            private readonly Label _unit;
            private readonly Label _trend;
            private readonly Sparkline _sparkline;
            private readonly VisualElement _valueRow;

            public MetricCard(string caption, bool withSparkline = false)
            {
                  AddToClassList("echo-card");
                  style.backgroundColor = EchoTheme.Surface2;
                  EchoTheme.ApplyBorder(this, EchoTheme.BorderSubtle);
                  EchoTheme.ApplyRadius(this, EchoTheme.RadiusMedium);
                  EchoTheme.ApplyPadding(this, EchoTheme.Space4, EchoTheme.Space5);
                  style.flexDirection = FlexDirection.Column;
                  style.flexGrow = 1;
                  style.flexBasis = 0;
                  style.marginLeft = 3;
                  style.marginRight = 3;
                  style.minWidth = 110;

                  _caption = new Label(caption)
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeSmall,
                              color = EchoTheme.TextMuted,
                              unityTextAlign = TextAnchor.UpperLeft,
                              marginBottom = EchoTheme.Space2,
                              unityFontStyleAndWeight = FontStyle.Normal,
                              letterSpacing = new StyleLength(0.5f)
                        }
                  };
                  Add(_caption);

                  _valueRow = new VisualElement
                  {
                        style =
                        {
                              flexDirection = FlexDirection.Row,
                              alignItems = Align.FlexEnd
                        }
                  };
                  Add(_valueRow);

                  _value = new Label("—")
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeDisplay,
                              color = EchoTheme.TextPrimary,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              marginRight = EchoTheme.Space2
                        }
                  };
                  _valueRow.Add(_value);

                  _unit = new Label(string.Empty)
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeSmall,
                              color = EchoTheme.TextMuted,
                              marginBottom = 4
                        }
                  };
                  _valueRow.Add(_unit);

                  _trend = new Label(string.Empty)
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeMicro,
                              color = EchoTheme.TextMuted,
                              marginTop = EchoTheme.Space1
                        }
                  };
                  Add(_trend);

                  if (withSparkline)
                  {
                        _sparkline = new Sparkline
                        {
                              style =
                              {
                                    height = 20,
                                    marginTop = EchoTheme.Space3
                              }
                        };
                        _sparkline.SetShowBaseline(false);
                        Add(_sparkline);
                  }
            }

            public void SetValue(string value)
            {
                  _value.text = value;
            }

            public void SetValue(string value, Color color)
            {
                  _value.text = value;
                  _value.style.color = color;
            }

            public void SetUnit(string unit)
            {
                  _unit.text = unit;
            }

            public void SetTrend(string trend, Color? color = null)
            {
                  _trend.text = trend;
                  _trend.style.color = color ?? EchoTheme.TextMuted;
            }

            public void SetSparkline(int[] data, Color color)
            {
                  if (_sparkline == null)
                  {
                        return;
                  }

                  _sparkline.SetData(data);
                  _sparkline.SetColor(color);
            }
      }
}