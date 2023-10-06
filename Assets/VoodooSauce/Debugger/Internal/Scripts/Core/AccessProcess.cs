using UnityEngine;

namespace Voodoo.Sauce.Tools.AccessButton
{
    public static class AccessProcess
    {
        public delegate void AccessButtonEvent();

        public static AccessButtonEvent InstantiateAccessButton;
        public static AccessButtonEvent DisposeAccessButton;

        public static GameObject ButtonInstance;

        private const string ACCESS_BUTTON_KEY = "accessButton";
        
        private const string TRUSTED_USER_KEY = "trustedUser";

        public static void SetTrustedUser(bool isTrusted)
        {
            PlayerPrefs.SetInt(TRUSTED_USER_KEY, isTrusted ? 1 : 0);
            SetAccess(isTrusted);
        }

        public static bool IsTrusted() => PlayerPrefs.GetInt(TRUSTED_USER_KEY, 0) == 1;
        
        public static void SetAccess(bool hasAccess)
        {
            PlayerPrefs.SetInt(ACCESS_BUTTON_KEY, hasAccess ? 1 : 0);

            if (IsTrusted() && hasAccess) {
                CheckInstantiateButton();
            } 
            else {
                CheckDisposeButton();
            }
        }

        public static bool HasAccess() => PlayerPrefs.GetInt(ACCESS_BUTTON_KEY, 0) == 1;

        public static void CheckInstantiateButton()
        {
            if (ButtonInstance != null) return;

            if (IsTrusted() && HasAccess()) InstantiateAccessButton?.Invoke();
        }

        private static void CheckDisposeButton()
        {
            if (ButtonInstance == null) return;

            DisposeAccessButton?.Invoke();
        }
    }
}