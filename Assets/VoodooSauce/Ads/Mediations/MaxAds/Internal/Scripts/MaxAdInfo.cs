using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.MaxMediation
{
    internal class MaxAdInfo
    {
        internal MaxSdkBase.AdInfo MaxNativeAdInfo { get; private set; }

        /// <summary>
        /// The waterfall information is removed from the MaxSdkBase.AdInfo before the ad close events.
        /// We need to store it there to access it later. This value doesn't change during a session.
        /// </summary>
        [CanBeNull] internal string WaterfallTestName { get; private set; }
        [CanBeNull] internal string WaterfallName { get; private set; }

        internal MaxAdInfo(MaxSdkBase.AdInfo maxNativeInfo)
        {
            Update(maxNativeInfo);
        }

        internal void Update(MaxSdkBase.AdInfo maxNativeInfo)
        {
            MaxNativeAdInfo = maxNativeInfo;

            if (!string.IsNullOrEmpty(MaxNativeAdInfo.WaterfallInfo?.Name)) {
                WaterfallName = MaxNativeAdInfo.WaterfallInfo?.Name;
            }
            
            if (!string.IsNullOrEmpty(MaxNativeAdInfo.WaterfallInfo?.TestName)) {
                WaterfallTestName = MaxNativeAdInfo.WaterfallInfo?.TestName;
            }
        }
    }
}