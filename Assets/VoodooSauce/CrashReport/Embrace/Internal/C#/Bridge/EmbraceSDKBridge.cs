using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport.Embrace
{
    public static class EmbraceSDKBridge
    {
        public static void Start()
        {
#if VS_EMBRACE_SDK
            EmbraceSDK.Embrace.Instance.StartSDK();
            EmbraceSDK.Embrace.Instance.EndAppStartup();

            if (Debug.isDebugBuild) {
                EmbraceSDK.Embrace.Instance.EnableDebugLogging();
            }
#endif
        }

        public static void SetUserIdentifier(string userId)
        {
#if VS_EMBRACE_SDK
            EmbraceSDK.Embrace.Instance.SetUserIdentifier(userId);
#endif
        }

        public static void SetUserAsPayer()
        {
#if VS_EMBRACE_SDK
            EmbraceSDK.Embrace.Instance.SetUserAsPayer();
#endif
        }

        public static void AddSessionProperty(string key, string value, bool permanent)
        {
#if VS_EMBRACE_SDK
            EmbraceSDK.Embrace.Instance.AddSessionProperty(key, value, permanent);
#endif
        }

        public static void SetUserPersona(string persona)
        {
#if VS_EMBRACE_SDK
            EmbraceSDK.Embrace.Instance.SetUserPersona(persona);
#endif
        }

        public static void LogBreadcrumb(string message)
        {
#if VS_EMBRACE_SDK
            EmbraceSDK.Embrace.Instance.LogBreadcrumb(message);
#endif
        }

        public static void LogException(Exception exception)
        {
#if VS_EMBRACE_SDK
            EmbraceSDK.Embrace.Instance.LogMessage(
                exception.Message,
                EmbraceSDK.EMBSeverity.Error,
                new Dictionary<string, string> {
                    { "trace", exception.StackTrace }
                });
#endif
        }
    }
}