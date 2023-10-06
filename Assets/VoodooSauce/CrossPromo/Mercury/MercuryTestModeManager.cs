using UnityEngine;
using Voodoo.Sauce.Common.Utils;

namespace Voodoo.Sauce.Internal.CrossPromo.Mercury
{
    public class MercuryTestModeManager
    {
        private const string MERCURY_TEST_MODE_PREFS_KEY = "MercuryApiTestModeKey";
        private static MercuryTestModeManager _instance;
        public static MercuryTestModeManager Instance => _instance ?? (_instance = new MercuryTestModeManager());

        private TestModeState _testModeState = TestModeState.Unknown;
        
        public void Initialize()
        {
            _testModeState = (TestModeState) PlayerPrefs.GetInt(MERCURY_TEST_MODE_PREFS_KEY, (int) TestModeState.Unknown);
        }

        public bool IsTestModeEnabled()
        {
            if (PlatformUtils.UNITY_EDITOR) 
                return true;
            return _testModeState == TestModeState.Enabled;
        }

        public void SetTestMode(bool enabled)
        {
            _testModeState = enabled ? TestModeState.Enabled : TestModeState.Disabled;
            PlayerPrefs.SetInt(MERCURY_TEST_MODE_PREFS_KEY, (int)_testModeState);
        }

        enum TestModeState
        {
            Unknown = 0,
            Enabled = 1,
            Disabled = 2
        }
    }
}