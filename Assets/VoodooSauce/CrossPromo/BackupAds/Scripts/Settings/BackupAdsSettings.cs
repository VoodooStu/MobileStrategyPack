using UnityEngine;
using UnityEngine.Serialization;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Canvas;

namespace Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Settings
{
    public class BackupAdsSettings : ScriptableObject
    {
        [Header("Internal Config")]
        public BackupAdsCanvas adsPrefab;

        private static BackupAdsSettings _settings;
        public static BackupAdsSettings Settings {
            get {
                if (!_settings) {
                    _settings = Resources.Load<BackupAdsSettings>("BackupAdsSettings");
                }

                return _settings;
            }
        }
    }
}