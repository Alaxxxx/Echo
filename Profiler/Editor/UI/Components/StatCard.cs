using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Components
{
      public sealed class StatCard : VisualElement
      {
            private readonly Label _value;
            private readonly Label _caption;

            public StatCard(string caption)
            {
                  style.flexDirection = FlexDirection.Column;
                  style.flexGrow = 1;
                  style.marginLeft = 2;
                  style.marginRight = 2;
                  style.minWidth = 100;

                  _value = new Label("-")
                  {
                        style =
                        {
                              fontSize = 18,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              color = EchoTheme.TextPrimary,
                              marginTop = 0,
                              marginBottom = 2
                        }
                  };
                  _caption = new Label(caption)
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeSmall,
                              color = EchoTheme.TextMuted,
                              marginTop = 0
                        }
                  };

                  Add(_value);
                  Add(_caption);
            }

            public void SetValue(string value)
            {
                  _value.text = value;
            }

            public void SetValueColor(Color color)
            {
                  _value.style.color = color;
            }
      }
}