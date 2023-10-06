using System;
using System.Collections.Generic;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Editor;
using Voodoo.Sauce.Internal.IntegrationCheck;
using Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public class AdjustIntegrationCheck : IIntegrationCheck
    {
        private const string SettingsNoAndroidAdjustAppTokenError = "VoodooSauce Settings is missing Adjust App Token for Android";
        private static readonly string SettingsNoAndroidAdjustFileError =
            AdjustHelper.GetAndroidSignaturePath() + " is missing. Please synchronize your Voodoo Settings.";
        private const string SettingsNoIOSAdjustAppTokenError = "VoodooSauce Settings is missing Adjust App Token for IOS";
        private static readonly string SettingsNoIOSAdjustFileError =
            AdjustHelper.GetIOSSignaturePath() + " is missing. Please synchronize your Voodoo Settings.";

        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var list = new List<IntegrationCheckMessage>();
            var platform = "";
            
            if (PlatformUtils.UNITY_IOS) {
                platform = KitchenStoreJSON.PLATFORM_IOS;
                if (string.IsNullOrEmpty(settings.AdjustIosAppToken)) {
                    list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SettingsNoIOSAdjustAppTokenError));
                }

                if (!AdjustHelper.IOSSignatureExists()) {
                    list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SettingsNoIOSAdjustFileError));
                }
            } else if (PlatformUtils.UNITY_ANDROID) {
                platform = KitchenStoreJSON.PLATFORM_ANDROID;
                if (string.IsNullOrEmpty(settings.AdjustAndroidAppToken)) {
                    list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SettingsNoAndroidAdjustAppTokenError));
                }

                if (!AdjustHelper.AndroidSignatureExists()) {
                    list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SettingsNoAndroidAdjustFileError));
                }
            }
            
            try {
                VoodooSauceSettingsChecker.CheckAdjustSignature(settings.Store, platform);
            } catch (Exception exception) {
                if (exception is BadFormattedSettingValueException) {
                    list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING, $"Adjust signature file: {exception.Message}"));
                } else {
                    list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, $"Adjust signature file: {exception.Message}"));
                }
            }
            return list;
        }
    }
}