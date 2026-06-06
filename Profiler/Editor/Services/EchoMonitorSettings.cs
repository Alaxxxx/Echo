using UnityEditor;

namespace OpalStudio.Echo.Profiler.Editor.Services
{
      public static class EchoMonitorSettings
      {
            private const string Prefix = "OpalStudio.Echo.Monitor.";

            private const string KeyCapturePayloads = Prefix + "CapturePayloads";
            private const string KeyAutoOpenOnPlay = Prefix + "AutoOpenOnPlay";
            private const string KeyPollingHz = Prefix + "PollingHz";
            private const string KeyShowFeed = Prefix + "ShowFeed";
            private const string KeyFeedPaused = Prefix + "FeedPaused";
            private const string KeyChannelSortMode = Prefix + "ChannelSortMode";
            private const string KeySidebarWidth = Prefix + "SidebarWidth";

            public static bool CapturePayloads
            {
                  get => EditorPrefs.GetBool(KeyCapturePayloads, true);
                  set => EditorPrefs.SetBool(KeyCapturePayloads, value);
            }

            public static bool AutoOpenOnPlay
            {
                  get => EditorPrefs.GetBool(KeyAutoOpenOnPlay, false);
                  set => EditorPrefs.SetBool(KeyAutoOpenOnPlay, value);
            }
            
            public static int PollingHz
            {
                  get => EditorPrefs.GetInt(KeyPollingHz, 10);
                  set => EditorPrefs.SetInt(KeyPollingHz, value);
            }

            public static bool ShowFeed
            {
                  get => EditorPrefs.GetBool(KeyShowFeed, true);
                  set => EditorPrefs.SetBool(KeyShowFeed, value);
            }

            public static bool FeedPaused
            {
                  get => EditorPrefs.GetBool(KeyFeedPaused, false);
                  set => EditorPrefs.SetBool(KeyFeedPaused, value);
            }

            public enum ChannelSortMode
            {
                  Alphabetical,
                  Activity,
                  LastFired,
                  TotalCount
            }

            public static ChannelSortMode SortMode
            {
                  get => (ChannelSortMode)EditorPrefs.GetInt(KeyChannelSortMode, (int)ChannelSortMode.Activity);
                  set => EditorPrefs.SetInt(KeyChannelSortMode, (int)value);
            }

            public static float SidebarWidth
            {
                  get => EditorPrefs.GetFloat(KeySidebarWidth, 280f);
                  set => EditorPrefs.SetFloat(KeySidebarWidth, value);
            }
      }
}