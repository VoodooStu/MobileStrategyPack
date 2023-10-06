using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal.CrossPromo.Mobile;
using Voodoo.Sauce.Internal.CrossPromo.Models;

namespace Voodoo.Sauce.Internal.CrossPromo.Canvas
{
    internal abstract class AFormatManager : MonoBehaviour
    {
        public static AFormatManager Instance;

        /// <summary>
        /// Current asset to print
        /// </summary>
        protected AssetModel Asset;

        protected bool IsActive;

        protected bool IsWaiting;

        private const string ITUNES_URL = "https://itunes.apple.com/fr/app/apple-store/id{0}";

        // ReSharper disable once UnusedMember.Local
        private const string PLAYSTORE_URL = "https://play.google.com/store/apps/details?id={0}";

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once SwitchStatementMissingSomeCases

        /// <summary>
        /// Display the ad
        /// </summary>
        public abstract void Display();

        /// <summary>
        /// OnClick event for linking to the stores
        /// </summary>
        public virtual void OnClickEvent()
        {
            if (Asset?.game == null) {
                return;
            }

            if (string.IsNullOrEmpty(Asset.store_ios_url)) {
                SendTrackingLink(Asset.tracking_link);
                OpenAppStoreUsingId(Asset.game.apple_id, Asset.game.bundle_id);
            } else {
                OpenAppStoreUsingUrl(Asset.store_ios_url);
            }
            
            CrossPromoEvents.TriggerClickEvent(Asset);
        }

        private void SendTrackingLink(string trackingLink)
        {
            if (!PlatformUtils.UNITY_EDITOR && !string.IsNullOrEmpty(trackingLink)) {
                UnityWebRequest r = UnityWebRequest.Get(trackingLink);
                r.SendWebRequest();
            }
        }

        private void OpenAppStoreUsingId(long appleId, string bundleId)
        {
            if (PlatformUtils.UNITY_IOS) {
                if (PlatformUtils.UNITY_EDITOR) {
                    Application.OpenURL(string.Format(ITUNES_URL, appleId));
                } else {
                    if (!IOSCrossPromoWrapper.CheckGoodIosVersion()) {
                        IOSCrossPromoWrapper.LoadNativeStore(appleId);
                    }
                    IOSCrossPromoWrapper.OpenNativeStore(appleId);
                }
            }
            
            if (PlatformUtils.UNITY_ANDROID) {
                Application.OpenURL(string.Format(PLAYSTORE_URL, bundleId));
            }
        }

        private void OpenAppStoreUsingUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) {
                return;
            }
            
            Application.OpenURL(url);
        }

        /// <summary>
        /// Hide the current ad
        /// </summary>
        public abstract void Hide();

        protected IEnumerator WaitCrossPromoReady()
        {
            while (!VoodooCrossPromo.Info.CrossPromoIsReady) yield return null;
            if (VoodooCrossPromo.Info.GetInternetStatus() || !IsActive) {
                if (IsActive)
                    CrossPromoEvents.TriggerErrorEvent(null);
                yield break;
            }

            try {
                Asset = VideoManager.ChooseVideo();
                if (Asset == null)
                    CrossPromoEvents.TriggerErrorEvent(null);
            } catch (Exception e) {
                CrossPromoEvents.TriggerErrorEvent(e);
            }
        }
    }
}