using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using Voodoo.Sauce.Common.Utils;

namespace Voodoo.Sauce.Internal.Firebase
{
    public static class FirebaseInitializer
    {
        private const string TAG = "FirebaseInitializer";

        private enum InitState
        {
            NONE,
            INITIALIZING,
            SUCCESS,
            FAIL,
        }

        private static InitState _initState = InitState.NONE;
        private static readonly List<Action<bool>> _Callbacks = new List<Action<bool>>();

        /// <summary>
        /// Inits once Firebase, and call the callback as soon as the answer is received. Instantly if already init.
        /// </summary>
        /// <returns></returns>
        public static void Start()
        {
            if (PlatformUtils.UNITY_EDITOR) {
                VoodooLog.LogWarning(Module.ANALYTICS, TAG,"Firebase Crashlytics & Analytics can not run on Editor, it will not be initialized.");
                return;
            }
            if (_initState == InitState.INITIALIZING) {
                VoodooLog.LogWarning(Module.ANALYTICS, TAG,"Firebase is already initializing.");
                return;
            }
            if (_initState == InitState.SUCCESS || _initState == InitState.FAIL) {
                OnInitComplete();
                return;
            }

            _initState = InitState.INITIALIZING;
            FirebaseApp.CheckAndFixDependenciesAsync()
               .ContinueWithOnMainThread(task => {
                    DependencyStatus dependencyStatus = task.Result;
                    if (dependencyStatus == DependencyStatus.Available) {
                        // Set a flag here for indicating that your project is ready to use Firebase.
                        _initState = FirebaseApp.DefaultInstance != null ? InitState.SUCCESS : InitState.FAIL;
                    } else {
                        VoodooLog.LogError(Module.ANALYTICS, TAG, $"Could not resolve all Firebase dependencies: {dependencyStatus}");
                        _initState = InitState.FAIL;
                    }
                    OnInitComplete();
                });
        }

        public static void Subscribe(Action<bool> callback)
        {
            if (_initState == InitState.SUCCESS || _initState == InitState.FAIL) {
                callback.Invoke(_initState == InitState.SUCCESS);
            } else {
                _Callbacks.Add(callback);
            }
        }

        private static void OnInitComplete()
        {
            foreach (Action<bool> callback in _Callbacks) {
                callback?.Invoke(_initState == InitState.SUCCESS);
            }
            _Callbacks.Clear();
        }
    }
}