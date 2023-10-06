using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal;

namespace Voodoo.Sauce.Internal.Ads.FakeMediation
{
    public class FakeAdsManager : IFakeAdsManager
    {
        private const string ENABLE_FAKE_ADS_KEY = "Voodoo_FakeAdsEnabled";
        private const string TAG = "FakeAds";

        private bool _isEnabled;


        public bool IsEnabled() => _isEnabled;

        public void SetEnabled(bool isEnabled)
        {
            // This value can only be changed on development builds on devices.
            if (!PlatformUtils.UNITY_EDITOR && Debug.isDebugBuild)
            {
                UpdateEnabledValue(isEnabled);
            }
        }

        public void Initialize() {
            // In the editor mode the fake ads are always enabled.
            if (PlatformUtils.UNITY_EDITOR) {
                UpdateEnabledValue(true);
                return;
            }

            // On the production builds the fake ads are always disabled.
            if (!Debug.isDebugBuild) {
                UpdateEnabledValue(false);
                return;
            }
            
            _isEnabled = (PlayerPrefs.GetInt(ENABLE_FAKE_ADS_KEY, 0) == 1);
        }

        private void UpdateEnabledValue(bool isEnabled) {
            _isEnabled = isEnabled;
            PlayerPrefs.SetInt(ENABLE_FAKE_ADS_KEY, _isEnabled ? 1 : 0);
            PlayerPrefs.Save();

            VoodooLog.LogDebug(Module.ADS, TAG, $"Fake ads enabled: {PlayerPrefs.GetInt(ENABLE_FAKE_ADS_KEY)}");
        }
    }
}