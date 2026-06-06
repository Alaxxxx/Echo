using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Components
{
      public sealed class Badge : VisualElement
      {
            private readonly Label _label;

            public Badge(string text, Color tint)
            {
                  _label = new Label(text.ToUpperInvariant())
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeMicro,
                              color = tint,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              letterSpacing = new StyleLength(1f),
                              paddingTop = 1,
                              paddingBottom = 1,
                              paddingLeft = 0,
                              paddingRight = 0
                        }
                  };

                  EchoTheme.ApplyPadding(this, 1, 6);
                  EchoTheme.ApplyRadius(this, EchoTheme.RadiusSmall);
                  style.backgroundColor = new Color(tint.r, tint.g, tint.b, 0.14f);
                  EchoTheme.ApplyBorder(this, new Color(tint.r, tint.g, tint.b, 0.30f));
                  style.alignSelf = Align.Center;
                  style.flexShrink = 0;
                  Add(_label);
            }

            public void SetText(string text)
            {
                  _label.text = text.ToUpperInvariant();
            }

            public void SetTint(Color tint)
            {
                  _label.style.color = tint;
                  style.backgroundColor = new Color(tint.r, tint.g, tint.b, 0.14f);
                  EchoTheme.ApplyBorder(this, new Color(tint.r, tint.g, tint.b, 0.30f));
            }
      }
}