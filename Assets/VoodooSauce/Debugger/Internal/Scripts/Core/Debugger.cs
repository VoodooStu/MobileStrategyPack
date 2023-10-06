using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.StoreUtility;
using Voodoo.Sauce.Internal.VoodooTune;

namespace Voodoo.Sauce.Debugger
{
    public static class Debugger
    {
        static DebuggerCanvas _debugger;
        
        static Debugger() 
        {
            _debugger = GameObject.FindObjectOfType<DebuggerCanvas>();
            
            if (_debugger == null) {
                _debugger = WidgetUtility.InstantiateDebugger();
            }
        }

        public static void TryOpen()
        {
            var isVsTestApp = Application.identifier == VoodooConstants.TEST_APP_BUNDLE;
            var useVoodooTune = VoodooSettings.Load().UseRemoteConfig;
            var isDebugBuild  = Debug.isDebugBuild;
            var isWhiteListed = VoodooTuneManager.GetDebuggerAuthorization();

            if (!isVsTestApp && useVoodooTune && !isDebugBuild && !isWhiteListed)
            {
                AppInfo.GetAppUpdateStatus(OnAppUpdateStatusReceived);
                return;
            }
            
            _debugger.Open();
        }

        public static void TryClose()
        {
            if (_debugger) {
                _debugger.Close();
            }
        }

        static void OnAppUpdateStatusReceived(AppUpdateStatus status)
        {
            if (status != AppUpdateStatus.TEST_MODE)
            {
                string instructionText;
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    instructionText = DebuggerConstants.ERROR_MESSAGE_NO_INTERNET;
                }
                else if (status == AppUpdateStatus.UNKNOWN)
                {
                    instructionText = DebuggerConstants.ERROR_MESSAGE_VOODOOTUNE;
                }
                else
                {
                    instructionText = DebuggerConstants.ERROR_MESSAGE_VERSION_OR_VOODOOTUNE;
                }

                var privacy = VoodooSauceCore.GetPrivacy();
                string vendorId = privacy.GetVendorId();
                var debuggerPopupConfig = new DebuggerPopupConfig
                {
                    message = instructionText,
                    showCancelButton = false,
                    confirmText = "Close",
                    showIdfv = string.IsNullOrEmpty(vendorId) == false,
                    idfv = vendorId,
                    showIdfa = privacy.HasAdsConsent(),
                    idfa = privacy.GetAdvertisingId(false)
                };
                
                DisplayPopup(debuggerPopupConfig);
                
                return;
            }

            _debugger.Open();
        }

        public static void Show(Screen screen)
        {
            if (_debugger.IsOpened == false)
            {
                _debugger.Open();
            }

            _debugger.Push(screen);
        }

        public static void Previous()
        {
            if (_debugger.IsHome)
            {
                _debugger.Close();
                return;
            }

            _debugger.Pop();
        }

        public static void Toggle(Screen screen)
        {
            if (_debugger.IsOpened == false)
            {
                _debugger.Open();
            }

            _debugger.Toggle(screen);
        }

        internal static void DisplayPopup(string message)
        {
            DisplayPopup(new DebuggerPopupConfig {
                message = message,
                showCancelButton = false,
                confirmText = "Ok",
            });
        }
        
        internal static void DisplayPopup(string title, string message)
        {
            DisplayPopup(new DebuggerPopupConfig {
                title = title,
                message = message,
                showCancelButton = false,
                confirmText = "Ok",
            });
        }

        internal static void DisplayPopup(DebuggerPopupConfig config)
        {
            if (_debugger) {
                _debugger.ShowPopup(config);
            }
        }
    }
}
