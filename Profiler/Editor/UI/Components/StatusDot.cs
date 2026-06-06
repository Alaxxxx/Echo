using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI.Components
{
      public sealed class StatusDot : VisualElement
      {
            private Color _color;
            private bool _pulse;
            private float _phase;
            private IVisualElementScheduledItem _animation;

            public StatusDot(int size = 8)
            {
                  _color = EchoTheme.TextMuted;
                  style.width = size;
                  style.height = size;
                  style.alignSelf = Align.Center;
                  generateVisualContent += OnGenerate;
            }

            public void SetColor(Color color)
            {
                  _color = color;
                  MarkDirtyRepaint();
            }

            public void SetPulse(bool pulse)
            {
                  if (_pulse == pulse)
                  {
                        return;
                  }

                  _pulse = pulse;

                  if (pulse)
                  {
                        _animation = schedule.Execute(() =>
                        {
                              _phase = (Time.realtimeSinceStartup % 1.5f) / 1.5f;
                              MarkDirtyRepaint();
                        }).Every(50);
                  }
                  else
                  {
                        _animation?.Pause();
                        _animation = null;
                        _phase = 0f;
                        MarkDirtyRepaint();
                  }
            }

            private void OnGenerate(MeshGenerationContext ctx)
            {
                  Rect r = contentRect;

                  if (r.width <= 0f || r.height <= 0f)
                  {
                        return;
                  }

                  Painter2D painter = ctx.painter2D;
                  Vector2 center = r.center;
                  float baseRadius = Mathf.Min(r.width, r.height) * 0.5f;

                  if (_pulse)
                  {
                        float t = Mathf.Sin(_phase * Mathf.PI * 2f) * 0.5f + 0.5f;
                        float ringRadius = Mathf.Lerp(baseRadius, baseRadius * 2f, t);
                        float alpha = Mathf.Lerp(0.45f, 0f, t);

                        painter.fillColor = new Color(_color.r, _color.g, _color.b, alpha);
                        painter.BeginPath();
                        painter.Arc(center, ringRadius, 0f, 360f);
                        painter.Fill();
                  }

                  painter.fillColor = _color;
                  painter.BeginPath();
                  painter.Arc(center, baseRadius * 0.75f, 0f, 360f);
                  painter.Fill();
            }
      }
}