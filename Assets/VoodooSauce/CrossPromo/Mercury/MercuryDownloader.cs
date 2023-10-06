using System;
using System.Collections;
using UnityEngine.Networking;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.CrossPromo.Mercury.Models;

namespace Voodoo.Sauce.Internal.CrossPromo.Mercury
{
    public static class MercuryDownloader
    {
        private const string TAG = "MercuryDownloader";

        public static void DownloadWaterfall(MercuryWaterfall waterfall, Action<MercuryPromotedAsset> onAssetDownloaded, bool overrideCache)
        {
            for (var index = 0; index < waterfall.promote_assets.Length; index++) {
                MercuryPromotedAsset asset = waterfall.promote_assets[index];
                asset.videoIndex = index;
                if (overrideCache || !MercuryCache.IsCached(asset.id)) {
                    DownloadVideo(asset, (asset2, data) => {
                        OnVideoDownloaded(asset2, data);
                        onAssetDownloaded?.Invoke(MercuryCache.GetAsset(asset.id));
                        AdsManager.RewardedVideo.AvailabilityUpdateFromRewardedInterstitial();
                    });
                } else {
                    VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, $"Get video from cache: {asset.id}");
                    MercuryPromotedAsset cachedAsset = MercuryCache.GetAsset(asset.id);
                    onAssetDownloaded?.Invoke(cachedAsset);
                    AdsManager.RewardedVideo.AvailabilityUpdateFromRewardedInterstitial();
                }
            }
        }

        private static void OnVideoDownloaded(MercuryPromotedAsset asset, byte[] bytes)
        {
            MercuryCache.SaveAsset(asset, bytes);
        }

        private static void DownloadVideo(MercuryPromotedAsset asset, Action<MercuryPromotedAsset, byte[]> onSuccess)
        {
            MercuryAPI.Instance.StartCoroutine(DownloadVideoCoroutine(asset, onSuccess));
        }

        private static IEnumerator DownloadVideoCoroutine(MercuryPromotedAsset asset, Action<MercuryPromotedAsset, byte[]> onSuccess)
        {
            string videoUrl = asset.file_url;
            UnityWebRequest webRequest = UnityWebRequest.Get(videoUrl);
            yield return webRequest.SendWebRequest();

            if (webRequest.isHttpError || webRequest.isNetworkError) {
                VoodooLog.LogWarning(Module.CROSS_PROMO, TAG, $"Couldn't download video: {videoUrl}");
            } else {
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, $"Downloaded video: {videoUrl}");
                onSuccess?.Invoke(asset, webRequest.downloadHandler.data);
            }
        }
    }
}