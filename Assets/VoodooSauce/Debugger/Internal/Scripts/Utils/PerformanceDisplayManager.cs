using UnityEngine;
using Voodoo.Sauce.Internal.DebugScreen.CodeStage.AdvancedFPSCounter;

namespace Voodoo.Sauce.Tools.PerformanceDisplay
{
    public static class PerformanceDisplayManager
    {
        public static bool IsEnabled {
            get => _isEnabled;
            set => SetEnabled(value);
        }

        private static bool _isEnabled;
        private static AFPSCounter _prefab;

        private const string SHOW_PERFORMANCE_DISPLAY_KEY = "Voodoo_PerformanceDisplayEnabled";

        public static void Initialize(AFPSCounter prefab)
        {
            _prefab = prefab;

            InitializeInternal();
        }

        private static void InitializeInternal()
        {
            if (PlayerPrefs.GetInt(SHOW_PERFORMANCE_DISPLAY_KEY, 0) == 1) {
                SetEnabled(true);
            }
        }

        private static void SetEnabled(bool setEnabled)
        {
            if (_isEnabled == setEnabled) return;

            if (setEnabled) {
                Object.Instantiate(_prefab);
            } else {
                AFPSCounter.SelfDestroy();
            }

            _isEnabled = setEnabled;

            PlayerPrefs.SetInt(SHOW_PERFORMANCE_DISPLAY_KEY, _isEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}