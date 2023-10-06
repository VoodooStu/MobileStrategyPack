using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

// ReSharper disable All

namespace Voodoo.Sauce.Internal.Editor
{
    /// <summary>
    /// Editor Static Utility Functions 
    /// </summary>
    public static class EditorUtility
    {
        /// <summary>
        /// Allows execution of IEnumerator within UnityEditor scripts 
        /// </summary>
        /// <param name="update">IEnumerator routine to execute</param>
        /// <param name="end">Action to run when routine finishes</param>
        public static void StartBackgroundTask(IEnumerator update, Action end = null)
        {
            if (Application.isBatchMode) {
                StartSyncBackgroundTask(update, end);
                return;
            }

            EditorApplication.CallbackFunction closureCallback = null;
            closureCallback = () => {
                try {
                    if (update.MoveNext() == false) {
                        end?.Invoke();
                        EditorApplication.update -= closureCallback;
                    }
                } catch (Exception e) {
                    Debug.LogException(e);
                    end?.Invoke();
                    EditorApplication.update -= closureCallback;
                }
            };
            EditorApplication.update += closureCallback;
        }
        
        /// <summary>
        /// Performs execution of IEnumerator in Editor Batch mode (with -quit flag on) It have some downside though,
        /// WaitForSeconds, WaitForEndOfFrame and WaitForFixedUpdate will be skipped
        /// </summary>
        /// <param name="update">IEnumerator routine to execute</param>
        /// <param name="end">Action to run when routine finishes</param>
        public static void StartSyncBackgroundTask(IEnumerator update, Action end = null)
        {
            while (update.MoveNext ()) {
                if (update.Current != null) {
                    IEnumerator num;
                    try {
                        num = (IEnumerator)update.Current;
                    } catch (InvalidCastException) {
                        if (update.Current is WaitForSeconds)
                            Debug.LogWarning ("Skipped call to WaitForSeconds due to batchmode. If you still need to Wait, use WaitForSecondsRealtime instead.");
                        continue;  // Skip to next step
                    }
                }
            }
            end?.Invoke();
        }
    }
}