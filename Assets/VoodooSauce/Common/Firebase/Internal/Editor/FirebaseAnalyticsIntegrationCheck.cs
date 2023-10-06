using System.Collections.Generic;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.Firebase
{
    public class FirebaseAnalyticsIntegrationCheck : IIntegrationCheck
    {
        private static readonly string SettingsNoAndroidFirebaseFilesError =
            FirebaseHelper.GetAndroidConfigFilePath() + " is missing. Please synchronize your Voodoo Settings.";
        private static readonly string SettingsNoAndroidGeneratedFirebaseFilesError = "Generated Firebase Android Resources file "
            + FirebaseHelper.GetAndroidGeneratedFilePath() +
            " is missing. Please remove " +  FirebaseHelper.GetAndroidConfigFilePath() +
            " and synchronize your Voodoo Settings.";
        private static readonly string SettingsNoIOSFirebaseFilesError = FirebaseHelper.GetIOSConfigFilePath() +
            " is missing. Please synchronize your Voodoo Settings.";

        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var list = new List<IntegrationCheckMessage>();
#if UNITY_ANDROID
            if (!FirebaseHelper.AndroidConfigFileExists()) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SettingsNoAndroidFirebaseFilesError));
            }

            if (!FirebaseHelper.AndroidGeneratedConfigFileExists()) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SettingsNoAndroidGeneratedFirebaseFilesError));
            }

#elif UNITY_IOS
            if (!FirebaseHelper.IOSConfigFileExists()) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SettingsNoIOSFirebaseFilesError));
            }

#endif
            return list;
        }
    }
}
