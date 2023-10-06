using System;
using System.IO;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    //@todo : This is legacy Class compatible with VS (will be updated soon)
    // Expose data fields of Unity SubscriptionInfo
    // Allow data fields to be serialized and reaccessed in case internet connection lost
    [Serializable]
    public partial class SubscriptionInfoContainer : ISerializationCallbackReceiver
    {
        // Filepath location for serialized IAP subscription information
        public static readonly string SubscriptionFilePath = Application.persistentDataPath + "/subscriptions/";


        // Exception cases for non serializable fields 
        [SerializeField] private long _lPurchaseDate;

        //[SerializeField]
        //private long _lRemainingTime;
        [SerializeField] private long _lIntroductoryPricePeriod;

        [SerializeField] private long _lExpireDate;

        [SerializeField] private long _lCancelDate;

        [SerializeField] private long _lFreeTrialPeriod;

        [SerializeField] private long _lSubscriptionPeriod;

        public string productId;
        public SubscriptionResult isSubscribed;
        public SubscriptionResult isExpired;
        public SubscriptionResult isCancelled;
        public SubscriptionResult isFreeTrial;

        public SubscriptionResult isAutoRenewing;

        //[NonSerialized]
        //public TimeSpan remainingTime;
        public SubscriptionResult isIntroductoryPricePeriod;
        public string introductoryPrice;
        public long introductoryPricePeriodCycles;
        public string freeTrialPeriodString;
        public string skuDetails;
        public string subscriptionInfoJson;

        [NonSerialized] public DateTime cancelDate;

        [NonSerialized] public DateTime expireDate;

        [NonSerialized] public TimeSpan freeTrialPeriod;

        [NonSerialized] public TimeSpan introductoryPricePeriod;

        [NonSerialized] public DateTime purchaseDate;

        [NonSerialized] public TimeSpan subscriptionPeriod;

        public SubscriptionInfoContainer()
        {
        }

        public void OnBeforeSerialize()
        {
            _lPurchaseDate = purchaseDate.Ticks;
            //_lRemainingTime = remainingTime.Ticks;
            _lIntroductoryPricePeriod = introductoryPricePeriod.Ticks;
            _lExpireDate = expireDate.Ticks;
            _lCancelDate = cancelDate.Ticks;
            _lFreeTrialPeriod = freeTrialPeriod.Ticks;
            _lSubscriptionPeriod = subscriptionPeriod.Ticks;
        }

        public void OnAfterDeserialize()
        {
            purchaseDate = new DateTime(_lPurchaseDate);
            //remainingTime = new TimeSpan(_lRemainingTime);
            introductoryPricePeriod = new TimeSpan(_lIntroductoryPricePeriod);
            expireDate = new DateTime(_lExpireDate);
            cancelDate = new DateTime(_lCancelDate);
            freeTrialPeriod = new TimeSpan(_lFreeTrialPeriod);
            subscriptionPeriod = new TimeSpan(_lSubscriptionPeriod);
        }

        public static void ClearSavedSubscriptionInfo()
        {
            if (Directory.Exists(SubscriptionFilePath)) Directory.Delete(SubscriptionFilePath, true);
        }
    }

    public enum SubscriptionResult
    {
        True,
        False,
        Unsupported
    }
}