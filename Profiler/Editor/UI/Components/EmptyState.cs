using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Components
{
      public sealed class EmptyState : VisualElement
      {
            private readonly Label _title;
            private readonly Label _hint;

            public EmptyState(string title, string hint, IconKind icon = IconKind.Spark)
            {
                  style.flexGrow = 1;
                  style.alignItems = Align.Center;
                  style.justifyContent = Justify.Center;
                  style.paddingTop = EchoTheme.Space7;
                  style.paddingBottom = EchoTheme.Space7;

                  VisualElement glyph = new() { style = { width = 48, height = 48, marginBottom = EchoTheme.Space5 } };
                  glyph.generateVisualContent += ctx => DrawIcon(ctx, glyph.contentRect, icon);
                  Add(glyph);

                  _title = new Label(title)
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeStrong,
                              color = EchoTheme.TextSecondary,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              marginBottom = EchoTheme.Space3
                        }
                  };
                  Add(_title);

                  _hint = new Label(hint)
                  {
                        style =
                        {
                              fontSize = EchoTheme.FontSizeSmall,
                              color = EchoTheme.TextMuted,
                              whiteSpace = WhiteSpace.Normal,
                              maxWidth = 320,
                              unityTextAlign = TextAnchor.MiddleCenter
                        }
                  };
                  Add(_hint);
            }

            public void SetTitle(string title) => _title.text = title;
            public void SetHint(string hint) => _hint.text = hint;

            public enum IconKind
            {
                  Spark,
                  Ghost,
                  Empty
            }

            private static void DrawIcon(MeshGenerationContext ctx, Rect rect, IconKind kind)
            {
                  Painter2D p = ctx.painter2D;
                  Vector2 c = rect.center;
                  float r = Mathf.Min(rect.width, rect.height) * 0.5f;
                  Color baseColor = new(EchoTheme.TextMuted.r, EchoTheme.TextMuted.g, EchoTheme.TextMuted.b, 0.35f);

                  switch (kind)
                  {
                        case IconKind.Spark:
                              for (int i = 0; i < 3; i++)
                              {
                                    float t = i / 3f;
                                    p.strokeColor = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t));
                                    p.lineWidth = 1f;
                                    p.BeginPath();
                                    p.Arc(c, r * (0.4f + t * 0.6f), 0f, 360f);
                                    p.Stroke();
                              }

                              p.fillColor = baseColor;
                              p.BeginPath();
                              p.Arc(c, r * 0.25f, 0f, 360f);
                              p.Fill();

                              break;

                        case IconKind.Ghost:
                              p.fillColor = new Color(EchoTheme.Ghost.r, EchoTheme.Ghost.g, EchoTheme.Ghost.b, 0.35f);
                              p.BeginPath();
                              p.MoveTo(new Vector2(c.x - r * 0.7f, c.y + r * 0.6f));
                              p.LineTo(new Vector2(c.x - r * 0.7f, c.y - r * 0.2f));
                              p.BezierCurveTo(
                                    new Vector2(c.x - r * 0.7f, c.y - r * 0.9f),
                                    new Vector2(c.x + r * 0.7f, c.y - r * 0.9f),
                                    new Vector2(c.x + r * 0.7f, c.y - r * 0.2f));
                              p.LineTo(new Vector2(c.x + r * 0.7f, c.y + r * 0.6f));
                              p.LineTo(new Vector2(c.x + r * 0.45f, c.y + r * 0.4f));
                              p.LineTo(new Vector2(c.x + r * 0.20f, c.y + r * 0.65f));
                              p.LineTo(new Vector2(c.x - r * 0.05f, c.y + r * 0.4f));
                              p.LineTo(new Vector2(c.x - r * 0.30f, c.y + r * 0.65f));
                              p.LineTo(new Vector2(c.x - r * 0.55f, c.y + r * 0.4f));
                              p.LineTo(new Vector2(c.x - r * 0.7f, c.y + r * 0.6f));
                              p.ClosePath();
                              p.Fill();
                              p.fillColor = EchoTheme.Surface0;
                              p.BeginPath();
                              p.Arc(new Vector2(c.x - r * 0.25f, c.y - r * 0.1f), r * 0.10f, 0f, 360f);
                              p.Fill();
                              p.BeginPath();
                              p.Arc(new Vector2(c.x + r * 0.25f, c.y - r * 0.1f), r * 0.10f, 0f, 360f);
                              p.Fill();

                              break;

                        case IconKind.Empty:
                              p.strokeColor = baseColor;
                              p.lineWidth = 1.5f;
                              p.BeginPath();
                              p.Arc(c, r * 0.65f, 0f, 360f);
                              p.Stroke();

                              break;
                  }
            }
      }
}