using PaperPlaneTools;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;

namespace Voodoo.Sauce.Internal.AppRater
{
    internal class AppRater : MonoBehaviour
    {
        private static int _minimumGamesPlayed;

        private static bool _isEnabled;
        
        private void Awake()
        {
            VoodooSettings settings = VoodooSettings.Load();
            _isEnabled = true;
#if UNITY_IOS
            _isEnabled = settings.iOSAppRaterEnabled;
#elif UNITY_ANDROID
            _isEnabled = settings.AndroidAppRaterEnabled;
#endif
            
            if (!_isEnabled) {
                gameObject.SetActive(false);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!_isEnabled) return;

            AnalyticsManager.OnGamePlayed += OnGamePlayed;
        }

        private void OnGamePlayed(int gameCount, bool isHighscore)
        {
            if (_minimumGamesPlayed > 0) {
                if (gameCount >= _minimumGamesPlayed) {
                    if (gameCount == _minimumGamesPlayed || isHighscore)
                        TryToShow();
                }
            }
        }

        internal static void Initialize(string appstoreAppId,
                                        string playstoreAppId,
                                        int delayAfterLaunchInSeconds,
                                        int postponeCooldownInSeconds,
                                        int minimumGamesPlayed)
        {
            if (!_isEnabled) return;

            VoodooLog.LogDebug(Module.APP_RATER, "AppRater.cs", "Initializing AppRater-RateBox");

            _minimumGamesPlayed = minimumGamesPlayed;

            RateBox.Instance.Init(
                RateBox.GetStoreUrl(appstoreAppId, playstoreAppId),
                new RateBoxConditions {
                    MinSessionCount = 0,
                    MinCustomEventsCount = 0,
                    DelayAfterInstallInSeconds = delayAfterLaunchInSeconds,
                    DelayAfterLaunchInSeconds = delayAfterLaunchInSeconds,
                    PostponeCooldownInSeconds = postponeCooldownInSeconds,
                    RequireInternetConnection = true,
                },
                new RateBoxTextSettings {
                    Title = "Like the game?",
                    RateButtonTitle = "Rate",
                    RejectButtonTitle = "",
                    PostponeButtonTitle = "Later",
                    Message = "Please take a moment to rate us.",
                },
                new RateBoxSettings {UseIOSReview = true}
            );
        }

        internal static void SetCustomAlert(IAlertPlatformAdapter customAlert)
        {
            if (!_isEnabled) return;

            RateBox.Instance.AlertAdapter = customAlert;
        }

        internal static void TryToShow()
        {
            if (!_isEnabled) return;

            RateBox.Instance.Show();
        }

        internal static void ForceShow()
        {
            if (!_isEnabled) return;

            RateBox.Instance.ForceShow();
        }
    }
}