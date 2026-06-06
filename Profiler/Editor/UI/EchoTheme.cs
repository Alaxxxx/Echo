using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.UI
{
      public static class EchoTheme
      {
            private static bool IsDark => EditorGUIUtility.isProSkin;

#region Stratified surfaces


            public static Color Surface0 => IsDark ? new Color(0.110f, 0.110f, 0.118f) : new Color(0.965f, 0.965f, 0.970f);
            public static Color Surface1 => IsDark ? new Color(0.137f, 0.137f, 0.149f) : new Color(0.945f, 0.945f, 0.952f);
            public static Color Surface2 => IsDark ? new Color(0.165f, 0.165f, 0.180f) : new Color(0.920f, 0.920f, 0.930f);
            public static Color Surface3 => IsDark ? new Color(0.196f, 0.196f, 0.212f) : new Color(0.890f, 0.890f, 0.900f);
            public static Color SurfaceHover => IsDark ? new Color(0.224f, 0.224f, 0.243f) : new Color(0.855f, 0.855f, 0.870f);

            public static Color SurfaceSelected => IsDark ? new Color(0.196f, 0.243f, 0.314f) : new Color(0.870f, 0.910f, 0.965f);

#endregion

#region Borders and dividers

            public static Color BorderSubtle => IsDark ? new Color(0.220f, 0.220f, 0.235f) : new Color(0.835f, 0.835f, 0.845f);
            public static Color BorderStrong => IsDark ? new Color(0.282f, 0.282f, 0.302f) : new Color(0.780f, 0.780f, 0.790f);

#endregion

#region Typography colors

            public static Color TextPrimary => IsDark ? new Color(0.925f, 0.925f, 0.945f) : new Color(0.090f, 0.090f, 0.110f);
            public static Color TextSecondary => IsDark ? new Color(0.690f, 0.690f, 0.725f) : new Color(0.330f, 0.330f, 0.360f);
            public static Color TextMuted => IsDark ? new Color(0.490f, 0.490f, 0.530f) : new Color(0.510f, 0.510f, 0.540f);
            public static Color TextDisabled => IsDark ? new Color(0.345f, 0.345f, 0.380f) : new Color(0.680f, 0.680f, 0.700f);

#endregion

#region Semantic accents

            public static Color Accent => IsDark ? new Color(0.392f, 0.604f, 1.000f) : new Color(0.220f, 0.450f, 0.900f);
            public static Color AccentMuted => IsDark ? new Color(0.392f, 0.604f, 1.000f, 0.18f) : new Color(0.220f, 0.450f, 0.900f, 0.14f);

            public static Color Success => IsDark ? new Color(0.380f, 0.820f, 0.520f) : new Color(0.200f, 0.620f, 0.380f);
            public static Color Warning => IsDark ? new Color(0.980f, 0.760f, 0.290f) : new Color(0.800f, 0.560f, 0.090f);
            public static Color Danger => IsDark ? new Color(0.965f, 0.420f, 0.430f) : new Color(0.820f, 0.220f, 0.250f);
            public static Color Info => IsDark ? new Color(0.420f, 0.760f, 0.965f) : new Color(0.160f, 0.500f, 0.760f);

            public static Color Ghost => IsDark ? new Color(0.760f, 0.510f, 0.965f) : new Color(0.560f, 0.320f, 0.820f);
            public static Color GhostMuted => IsDark ? new Color(0.760f, 0.510f, 0.965f, 0.14f) : new Color(0.560f, 0.320f, 0.820f, 0.10f);

#endregion

#region Subscription kind colors (for badges)


            public static Color KindPlain => IsDark ? new Color(0.580f, 0.620f, 0.700f) : new Color(0.420f, 0.450f, 0.520f);
            public static Color KindScoped => IsDark ? new Color(0.420f, 0.760f, 0.965f) : new Color(0.160f, 0.500f, 0.760f);
            public static Color KindFiltered => IsDark ? new Color(0.560f, 0.820f, 0.480f) : new Color(0.310f, 0.610f, 0.260f);
            public static Color KindOnce => IsDark ? new Color(0.980f, 0.760f, 0.290f) : new Color(0.760f, 0.510f, 0.090f);

#endregion

#region Type scale


            public const int FontSizeMicro = 9;
            public const int FontSizeSmall = 10;
            public const int FontSizeBody = 11;
            public const int FontSizeStrong = 12;
            public const int FontSizeTitle = 14;
            public const int FontSizeDisplay = 22;

#endregion

#region Spacing scale (a power-of-two-ish ramp)

            public const int Space1 = 2;
            public const int Space2 = 4;
            public const int Space3 = 6;
            public const int Space4 = 8;
            public const int Space5 = 12;
            public const int Space6 = 16;
            public const int Space7 = 24;
            public const int Space8 = 32;

#endregion

#region Radius

            public const int RadiusSmall = 3;
            public const int RadiusMedium = 5;
            public const int RadiusLarge = 8;

#endregion

#region Style helpers

            public static void ApplyBorder(VisualElement el, Color color, float width = 1f)
            {
                  el.style.borderTopColor = color;
                  el.style.borderBottomColor = color;
                  el.style.borderLeftColor = color;
                  el.style.borderRightColor = color;
                  el.style.borderTopWidth = width;
                  el.style.borderBottomWidth = width;
                  el.style.borderLeftWidth = width;
                  el.style.borderRightWidth = width;
            }

            public static void ApplyRadius(VisualElement el, int radius)
            {
                  el.style.borderTopLeftRadius = radius;
                  el.style.borderTopRightRadius = radius;
                  el.style.borderBottomLeftRadius = radius;
                  el.style.borderBottomRightRadius = radius;
            }

            public static void ApplyPadding(VisualElement el, int vertical, int horizontal)
            {
                  el.style.paddingTop = vertical;
                  el.style.paddingBottom = vertical;
                  el.style.paddingLeft = horizontal;
                  el.style.paddingRight = horizontal;
            }

            public static void ApplyMargin(VisualElement el, int vertical, int horizontal)
            {
                  el.style.marginTop = vertical;
                  el.style.marginBottom = vertical;
                  el.style.marginLeft = horizontal;
                  el.style.marginRight = horizontal;
            }

#endregion
      }
}