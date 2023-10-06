using System;
using System.Collections.Generic;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.Editor
{
    public class VoodooAndroidBuildIntegrationCheck : IIntegrationCheck
    {
        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var result = new List<IntegrationCheckMessage>();
            if (!PlatformUtils.UNITY_ANDROID)
                return result;

            //Check if Gradle Tools Version is already correct
            Version currentGradleVersion = GradleCustomConfigHelper.GetCurrentGradleVersion();
            Version recommendedGradleVersion = GradleCustomConfigHelper.GetRecommendedGradleVersion();
            if (currentGradleVersion != null && currentGradleVersion.CompareTo(recommendedGradleVersion) == -1) {
                result.Add(new IntegrationCheckMessage(
                    IntegrationCheckMessage.Type.ERROR,
                    $"Your configured gradle tools version is {currentGradleVersion}. Android "
                    + $"Target API 31 require Gradle tools version {recommendedGradleVersion}. You can update "
                    + "the gradle tools by following the instruction below:\n\n"
                    + $"1. Download Gradle version {recommendedGradleVersion} in https://services.gradle.org/"
                    + $"distributions/gradle-{recommendedGradleVersion}-bin.zip "
                    + "\n2. Extract the downloaded zip file and copy your extracted folder's path"
                    + "\n3. Go to Preferences -> External Tools -> Android (section) and untick the Gradle"
                    + " Installed with Unity"
                    + "\n4. Click Browse and choose the folder to the extracted gradle file earlier"
                ));
            } else if (currentGradleVersion == null) {
                result.Add(new IntegrationCheckMessage(
                    IntegrationCheckMessage.Type.ERROR,
                    "Your configured Gradle Path is invalid. Please check your configuration in "
                    + "Preferences -> External Tools -> Android (section) -> Gradle"
                ));
            }

            return result;
        }
    }
}