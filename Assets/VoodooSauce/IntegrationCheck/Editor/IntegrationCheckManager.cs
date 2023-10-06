using System.Collections.Generic;
using UnityEngine;

namespace Voodoo.Sauce.Internal.IntegrationCheck.Editor
{
    /// <summary>
    /// For logging build errors and warnings.  This is the manager of IntegrationCheckWindow,
    /// to interact with IntegrationCheckWindow, it needs to go through this class
    /// </summary>
    public static class IntegrationCheckManager
    {
        internal static readonly List<IntegrationCheckMessage> ErrorMessages = new List<IntegrationCheckMessage>();
        internal static readonly List<IntegrationCheckMessage> WarningMessages = new List<IntegrationCheckMessage>();
        
        internal static void Clear()
        {
            ErrorMessages.Clear();
            WarningMessages.Clear();
        }
        
        /// <summary>
        /// Log error or warning message and update the messages in UI window
        /// If UI window is not shown it will automatically show the UI window
        /// </summary>
        internal static void LogMessage(IntegrationCheckMessage message)
        {
            if (message.type == IntegrationCheckMessage.Type.WARNING) {
                WarningMessages.Add(message);
                Debug.LogWarning("Integration warning: " + message.Description);
            } else {
                ErrorMessages.Add(message);
                Debug.LogError("Integration error: " + message.Description);
            }
            ShowWindow();
        }

        internal static void ShowWindow()
        {
            IntegrationCheckWindow.ShowWindow();
        }
    }
}