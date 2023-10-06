using UnityEditor;
using Voodoo.Analytics;
using Voodoo.Sauce.Internal.Analytics;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Analytics.Editor
{
    [InitializeOnLoad]
    internal static class VoodooAnalyticsEditorListener
    {
        static VoodooAnalyticsEditorListener()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state) {
                case PlayModeStateChange.EnteredPlayMode:
                    TriggerGameRun();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    TriggerGamePaused();
                    break;
            }
        }

        private static void OnPauseStateChanged(PauseState state)
        {
            switch (state) {
                case PauseState.Unpaused:
                    TriggerGameRun();
                    break;
                case PauseState.Paused:
                    TriggerGamePaused();
                    break;
            }
        }

        private static void TriggerGameRun()
        {
            VoodooAnalyticsManager.StartTracker();
        }

        private static void TriggerGamePaused()
        {
            VoodooAnalyticsManager.StopTracker();
            AnalyticsSessionManager.Instance().StopTimer();
        }
    }
}