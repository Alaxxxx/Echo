using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpalStudio.Echo.Profiler.Editor.Services
{
      public static class EchoMonitorSettingsProvider
      {
            [SettingsProvider]
            public static SettingsProvider CreateProvider()
            {
                  SettingsProvider provider = new("Preferences/Echo Monitor", SettingsScope.User)
                  {
                        label = "Echo Monitor",
                        activateHandler = (_, root) => Build(root),
                        keywords = new[] { "echo", "event", "bus", "monitor", "profiler" }
                  };

                  return provider;
            }

            private static void Build(VisualElement root)
            {
                  root.style.paddingTop = 12;
                  root.style.paddingLeft = 12;
                  root.style.paddingRight = 12;
                  root.style.paddingBottom = 12;

                  Label title = new("Echo Monitor")
                  {
                        style =
                        {
                              fontSize = 19,
                              unityFontStyleAndWeight = FontStyle.Bold,
                              marginBottom = 12
                        }
                  };
                  root.Add(title);

                  Toggle capturePayloads = new("Capture event payloads")
                  {
                        value = EchoMonitorSettings.CapturePayloads,
                        tooltip = "When enabled, the monitor boxes each event payload for inspection in the live feed. Disable to reduce editor overhead when monitoring high-frequency events."
                  };
                  capturePayloads.RegisterValueChangedCallback(evt => EchoMonitorSettings.CapturePayloads = evt.newValue);
                  root.Add(capturePayloads);

                  Toggle autoOpen = new("Auto-open monitor on Play")
                  {
                        value = EchoMonitorSettings.AutoOpenOnPlay,
                        tooltip = "Opens the Echo Monitor window automatically when entering Play Mode."
                  };
                  autoOpen.RegisterValueChangedCallback(evt => EchoMonitorSettings.AutoOpenOnPlay = evt.newValue);
                  root.Add(autoOpen);

                  SliderInt polling = new("Polling rate (Hz)", 5, 60)
                  {
                        value = EchoMonitorSettings.PollingHz,
                        showInputField = true,
                        tooltip = "How often the monitor refreshes its UI. Higher is smoother but costs more CPU. Default: 10 Hz."
                  };
                  polling.RegisterValueChangedCallback(evt => EchoMonitorSettings.PollingHz = evt.newValue);
                  polling.style.marginTop = 8;
                  root.Add(polling);

                  // Reset button.
                  Button reset = new(ResetDefaults)
                  {
                        text = "Reset to defaults",
                        style =
                        {
                              marginTop = 16,
                              alignSelf = Align.FlexStart
                        }
                  };
                  root.Add(reset);
            }

            private static void ResetDefaults()
            {
                  EchoMonitorSettings.CapturePayloads = true;
                  EchoMonitorSettings.AutoOpenOnPlay = false;
                  EchoMonitorSettings.PollingHz = 10;
                  EchoMonitorSettings.ShowFeed = true;
                  EchoMonitorSettings.FeedPaused = false;
                  EchoMonitorSettings.SortMode = EchoMonitorSettings.ChannelSortMode.Activity;
                  EchoMonitorSettings.SidebarWidth = 280f;

                  SettingsService.NotifySettingsProviderChanged();
            }
      }
}