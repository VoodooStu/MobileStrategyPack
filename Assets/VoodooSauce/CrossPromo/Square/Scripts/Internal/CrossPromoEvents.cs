using System;
using Voodoo.Sauce.Internal.CrossPromo.Models;

namespace Voodoo.Sauce.Internal.CrossPromo
{
    /// <summary>
    /// Events Handler
    /// </summary>
    internal class CrossPromoEvents
    {
        /// <summary>
        /// Event when the cross promotion is ready
        /// </summary>
        internal static event Action<string> OnInitComplete;

        /// <summary>
        /// Event when the user click on the ad
        /// </summary>
        internal static event Action<AssetModel> OnClick;

        /// <summary>
        /// Event when an ad is shown
        /// </summary>
        internal static event Action<AssetModel> OnShown;

        /// <summary>
        /// Event whenever there is an error
        /// </summary>
        internal static event Action<Exception> OnError;

        internal static void TriggerInitComplete(string format)
        {
            OnInitComplete?.Invoke(format);
        }

        internal static void TriggerClickEvent(AssetModel asset)
        {
            OnClick?.Invoke(asset);
        }
        
        internal static void TriggerShownEvent(AssetModel asset)
        {
            OnShown?.Invoke(asset);
        }

        internal static void TriggerErrorEvent(Exception message)
        {
            OnError?.Invoke(message);
        }
    }
}