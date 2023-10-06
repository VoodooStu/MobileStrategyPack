using System;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Analytics
{
    public static class VoodooAnalyticsEditorEvents
    {
        /// <summary>
        /// This event is triggered when the game is run/stopped in the Unity editor.
        /// </summary>
        internal static event Action gameRunEvent;

        /// <summary>
        /// This event is triggered when the game is paused/stopped in the Unity editor.
        /// </summary>
        internal static event Action gamePausedEvent;

        /// <summary>
        /// Trigger the game run event.
        /// </summary>
        internal static void TriggerGameRun()
        {
            gameRunEvent?.Invoke();
        }

        /// <summary>
        /// Trigger the game paused event.
        /// </summary>
        internal static void TriggerGamePaused()
        {
            gamePausedEvent?.Invoke();
        }
    }
}