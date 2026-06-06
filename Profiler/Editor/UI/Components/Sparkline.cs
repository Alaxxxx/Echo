using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Components
{
      public sealed class Sparkline : VisualElement
      {
            private int[] _data;
            private Color _strokeColor;
            private bool _showBaseline = true;

            public Sparkline()
            {
                  style.minHeight = 28;
                  style.flexGrow = 1;
                  _strokeColor = EchoTheme.Accent;
                  generateVisualContent += OnGenerate;
            }

            public void SetData(int[] data)
            {
                  _data = data;
                  MarkDirtyRepaint();
            }

            public void SetColor(Color color)
            {
                  _strokeColor = color;
                  MarkDirtyRepaint();
            }

            public void SetShowBaseline(bool show)
            {
                  _showBaseline = show;
                  MarkDirtyRepaint();
            }

            private void OnGenerate(MeshGenerationContext ctx)
            {
                  Rect rect = contentRect;

                  if (rect.width <= 1f || rect.height <= 1f)
                  {
                        return;
                  }

                  Painter2D painter = ctx.painter2D;

                  if (_showBaseline)
                  {
                        painter.BeginPath();
                        painter.strokeColor = new Color(_strokeColor.r, _strokeColor.g, _strokeColor.b, 0.15f);
                        painter.lineWidth = 1f;
                        painter.MoveTo(new Vector2(rect.xMin, rect.yMax - 0.5f));
                        painter.LineTo(new Vector2(rect.xMax, rect.yMax - 0.5f));
                        painter.Stroke();
                  }

                  if (_data == null || _data.Length < 2)
                  {
                        return;
                  }

                  int max = 1;

                  for (int i = 0; i < _data.Length; i++)
                  {
                        if (_data[i] > max)
                        {
                              max = _data[i];
                        }
                  }

                  int n = _data.Length;
                  Vector2[] points = new Vector2[n];

                  for (int i = 0; i < n; i++)
                  {
                        float x = Mathf.Lerp(rect.xMin, rect.xMax, i / (float)(n - 1));
                        float t = _data[i] / (float)max;
                        float y = Mathf.Lerp(rect.yMax - 1f, rect.yMin + 2f, t);
                        points[i] = new Vector2(x, y);
                  }

                  Color fillTop = new(_strokeColor.r, _strokeColor.g, _strokeColor.b, 0.28f);
                  Color fillBottom = new(_strokeColor.r, _strokeColor.g, _strokeColor.b, 0.02f);
                  const int gradientBands = 6;

                  for (int band = 0; band < gradientBands; band++)
                  {
                        float ta = band / (float)gradientBands;
                        float tb = (band + 1) / (float)gradientBands;
                        Color bandColor = Color.Lerp(fillTop, fillBottom, ta);

                        float bandTop = Mathf.Lerp(rect.yMin + 2f, rect.yMax - 1f, ta);
                        float bandBottom = Mathf.Lerp(rect.yMin + 2f, rect.yMax - 1f, tb);

                        painter.fillColor = bandColor;
                        painter.BeginPath();
                        painter.MoveTo(new Vector2(rect.xMin, bandBottom));

                        for (int i = 0; i < n; i++)
                        {
                              Vector2 p = points[i];
                              float yClamped = Mathf.Max(p.y, bandTop);
                              yClamped = Mathf.Min(yClamped, bandBottom);
                              painter.LineTo(new Vector2(p.x, yClamped));
                        }

                        painter.LineTo(new Vector2(rect.xMax, bandBottom));
                        painter.ClosePath();
                        painter.Fill();
                  }

                  painter.BeginPath();
                  painter.strokeColor = _strokeColor;
                  painter.lineWidth = 1.5f;
                  painter.lineJoin = LineJoin.Round;
                  painter.lineCap = LineCap.Round;
                  painter.MoveTo(points[0]);

                  for (int i = 0; i < n - 1; i++)
                  {
                        Vector2 p0 = i == 0 ? points[0] : points[i - 1];
                        Vector2 p1 = points[i];
                        Vector2 p2 = points[i + 1];
                        Vector2 p3 = i + 2 < n ? points[i + 2] : points[n - 1];

                        Vector2 cp1 = p1 + (p2 - p0) / 6f;
                        Vector2 cp2 = p2 - (p3 - p1) / 6f;

                        painter.BezierCurveTo(cp1, cp2, p2);
                  }

                  painter.Stroke();
            }
      }
}