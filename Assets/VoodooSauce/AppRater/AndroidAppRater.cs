using System;
using PaperPlaneTools;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;

namespace Voodoo.Sauce.Internal.AppRater
{
    public class AndroidAppRater : MonoBehaviour, IAlertPlatformAdapter
    {
        private const string TAG = "AndroidAppRater";

        private const string UNITY_ACTIVITY_CLASS = "com.unity3d.player.UnityPlayer";
        private const string VOODOO_APP_RATER_CLASS = "io.voodoo.voodooapprater.AppRater";
        private const string VOODOO_EMAIL_ADDRESS = "support@voodoo.io";

        private Action _positiveCallback;
        private Action _neutralCallback;
        private Action _negativeCallback;
        private Action _dismissCallback;

        private void Awake()
        {
            gameObject.name = "VoodooAppRater";

            if (PlatformUtils.UNITY_ANDROID && !PlatformUtils.UNITY_EDITOR) {
                VoodooLog.LogDebug(Module.APP_RATER, TAG, "Setting Android's AppRater custom alert");
                AppRater.SetCustomAlert(this);
            }
        }

        /*
         * RateBox Callbacks
         */

        public void SetOnDismiss(Action action)
        {
            _dismissCallback = action;
        }

        public void Show(Alert alert)
        {
            VoodooLog.LogDebug(Module.APP_RATER, TAG, "Showing rate pop up.");

            ShowNativeAlertDialog();

            _positiveCallback = alert.PositiveButton?.Handler;
            _neutralCallback = alert.NeutralButton?.Handler;
            _negativeCallback = alert.NegativeButton?.Handler;
        }

        public void Dismiss()
        {
            VoodooLog.LogDebug(Module.APP_RATER, TAG, "Dismissing rate pop up.");
            _dismissCallback?.Invoke();
        }

        public void HandleEvent(string eventName, string value)
        {
            VoodooLog.LogDebug(Module.APP_RATER, TAG, "Native event : " + eventName + " -> " + value);
        }

        /*
         * VoodooAppRater Native Android Method
         */

        private static void ShowNativeAlertDialog()
        {
            AndroidJavaClass unityActivity = new AndroidJavaClass(UNITY_ACTIVITY_CLASS);
            AndroidJavaObject currentActivity = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass appRaterClass = new AndroidJavaClass(VOODOO_APP_RATER_CLASS);
            // Param1 : Activity (UnityActivity)
            // Param2 : Title Text
            // Param3 : Dialog Message
            // Param4 : Positive button text
            // Param5 : Negative button text
            // Param6 : Neutral button text
            appRaterClass.CallStatic("Show", currentActivity, currentActivity,
                "Enjoying " + Application.productName + "?",
                "Tap a star to rate it on the store.",
                "Submit", "Not Now");
        }

        /*
         * VoodooAppRater Native Android Callback
         */

        public void OnPositiveButtonClick(string ratingString)
        {
            int rating = int.Parse(ratingString);
            VoodooLog.LogDebug(Module.APP_RATER, TAG, "OnPositiveButtonClick: " + rating);
            if (rating >= 4) {
                _positiveCallback?.Invoke();
            } else {
                _negativeCallback?.Invoke();
                Application.OpenURL($"mailto:{VOODOO_EMAIL_ADDRESS}?subject={Application.productName}%20Issue&body=Tell%20us%20what%20you%20didn't%20like%20:%20");
            }

            Dismiss();
        }

        public void OnNeutralButtonClick()
        {
            VoodooLog.LogDebug(Module.APP_RATER, TAG, "OnNeutralButtonClick");
            _neutralCallback?.Invoke();
            Dismiss();
        }
    }
}