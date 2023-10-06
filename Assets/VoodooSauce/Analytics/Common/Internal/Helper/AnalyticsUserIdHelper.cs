using System;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Analytics
{
    public static class AnalyticsUserIdHelper
    {
        private const string PlayerPrefUserIdentifierKey = "VOODOO_ANALYTICS_USER_IDENTIFIER";

        private static string _userId;

        // GetUserId() could now be called in a background thread.
        // We need to use a variable instead
        public static string GetUserId()
        {
            if (string.IsNullOrEmpty(_userId)) {
                _userId = GetStoredUserId();
            }

            return _userId;
        }
        
        private static string GetStoredUserId()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefUserIdentifierKey)) {
                PlayerPrefs.SetString(PlayerPrefUserIdentifierKey, Guid.NewGuid().ToString());
            }
            return PlayerPrefs.GetString(PlayerPrefUserIdentifierKey);
        }
    }
}