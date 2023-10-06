using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.CrashReport;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.AppRater;
using Voodoo.Sauce.Internal.CrossPromo.Models;
using Voodoo.Sauce.Firebase;
using Voodoo.Sauce.Internal.CrossPromo;
using Voodoo.Sauce.Internal.IAP;
using Voodoo.Sauce.Internal.VoodooTune;
using Voodoo.Sauce.Privacy;

// ReSharper disable once CheckNamespace
public static class VoodooSauce
{
    private const string TAG = "VoodooSauce";
    
    public static string Version() => VoodooConfig.Version();
    
    public delegate void AppResumedEventHandler();

    public static bool IsChinaApp() => !IsIAPEnabled();
    
    /// <summary>
    /// Use this to check if the game supports in-app purchases in the current store configuration
    /// </summary>
    /// <returns>Returns true if the game supports in-app-purchases, false otherwise.</returns>
    public static bool IsIAPEnabled()
    {
        VoodooSettings settings = VoodooSettings.Load();
        var enabled = true;
#if UNITY_IOS
        enabled = settings.iOSIAPEnabled;
#elif UNITY_ANDROID
        enabled = settings.AndroidIAPEnabled;
#endif
        return enabled;
    }
    
    private static PrivacyCore Privacy => VoodooSauceCore.GetPrivacy(true);

    public static bool UserRequestedToBeForgotten => VoodooSauceCore.GetPrivacy(true).UserRequestedToBeForgotten();

	private static IAPCore Iap => VoodooSauceCore.GetInAppPurchase(true);
    
    private static CrashReportCore CrashReport => VoodooSauceCore.GetCrashReport();

    /// <summary>
    /// Subscribe to this event to know when Voodoo Sauce is initialized.
    /// </summary>
    /// <param name="onInitFinished"></param>
    public static void SubscribeOnInitFinishedEvent(Action<VoodooSauceInitCallbackResult> onInitFinished)
    {
        VoodooSauceBehaviour.SubscribeOnInitFinishedEvent(onInitFinished);
    }

    /// <summary>
    /// Unsubscribe from the event fired when Voodoo Sauce is initialized.
    /// </summary>
    /// <param name="onInitFinished"></param>
    internal static void UnSubscribeOnInitFinishedEvent(Action<VoodooSauceInitCallbackResult> onInitFinished)
    {
        VoodooSauceBehaviour.UnSubscribeOnInitFinishedEvent(onInitFinished);
    }

    /// <summary>
    /// Subscribe to this event in your MonoBehaviour "Start" to be notified when subscription 
    /// is refreshed on app start and app foregrounding. 
    /// </summary>
    public static event AppResumedEventHandler SubscriptionsRefreshed;

    // Subscriptions event: refreshed when 3rd party studios should recheck their  
    // active subscriptions 
    internal static void InvokeAppResumed()
    {
        SubscriptionsRefreshed?.Invoke();
    }

    /// <summary>
    /// Returns the current user's cohort. If the user is not in an A/B test, null is returned.
    /// </summary>
    /// <returns></returns>
    [Obsolete("GetPlayerCohort is deprecated, please use GetPlayerCohorts or GetPlayerCohortName instead")]
    public static string GetPlayerCohort() => AbTestManager.GetPlayerCohort();

    /// <summary>
    /// Returns the current user's cohorts. If the user is not in an A/B test, an empty array is returned.
    /// </summary>
    /// <returns></returns>
    public static string[] GetPlayerCohorts() => AbTestManager.GetPlayerCohorts();

    /// <summary>
    /// Returns the current user's cohort name. If the user is not in an A/B test, null is returned.
    /// </summary>
    /// <returns></returns>
    public static string GetPlayerCohortName()
    {
        string cohortName = AbTestManager.GetPlayerCohort();
        return string.IsNullOrEmpty(cohortName) ? "Default" : cohortName;
    }

    /// <summary>
    /// Method to call whenever the user starts a game.
    /// </summary>
    /// <param name="parameters"></param>
    public static void OnGameStarted(GameStartedParameters parameters)
    {
        if (!VoodooSauceBehaviour.IsInitFinished()) {
            VoodooLog.LogError(Module.COMMON, TAG, "OnGameStarted is called too early. "
                + "Please subscribe to VoodooSauce.SubscribeOnInitFinishedEvent and wait until the Voodoo Sauce is initialized.");
        }

        CrashReport.LogLevelStart(parameters.level);
        AnalyticsManager.TrackGameStarted(parameters);
    }

    /// <summary>
    /// Method to call whenever the user finishes a game, even when leaving a game.
    /// </summary>
    /// <param name="parameters"></param>
    public static void OnGameFinished(GameFinishedParameters parameters)
    {
        CrashReport.LogLevelFinish(parameters.level);
        AnalyticsManager.TrackGameFinished(parameters);
        AdsManager.IncrementGamesPlayed();
    }
    
    /// <summary>
    /// Method to call whenever the user starts a game.
    /// </summary>
    public static void OnGameStarted()
    {
        OnGameStarted(new GameStartedParameters());
    }

    /// <summary>
    /// Method to call whenever the user starts a game.
    /// </summary>
    /// <param name="levelNumber">The level of the game</param>
    /// <param name="eventProperties">An optional list of properties to send along with the event</param>
    /// <param name="eventContextProperties">An optional list of properties to send along with the event</param>
    public static void OnGameStarted(string levelNumber, Dictionary<string, object> eventProperties = null,
                                     Dictionary<string, object> eventContextProperties = null)
    {
        OnGameStarted(new GameStartedParameters {
            level = levelNumber,
            eventProperties = eventProperties,
            eventContextProperties = eventContextProperties,
        });
    }

    /// <summary>
    /// Method to call whenever the user completes a game.
    /// </summary>
    /// <param name="score">The score of the game</param>
    public static void OnGameFinished(float score)
    {
        OnGameFinished(new GameFinishedParameters {
            status = true,
            score = score,
        });
    }

    /// <summary>
    /// Method to call whenever the user finishes a game, even when leaving a game.
    /// </summary>
    /// <param name="levelComplete">Whether the user finished the game</param>
    /// <param name="score">The score of the game</param>
    /// <param name="eventProperties">An optional list of properties to send along with the event</param>
    /// <param name="eventContextProperties">An optional list of properties to send along with the event</param>
    public static void OnGameFinished(bool levelComplete, float score,
        Dictionary<string, object> eventProperties = null,
        Dictionary<string, object> eventContextProperties = null)
    {
        OnGameFinished(new GameFinishedParameters {
            status = levelComplete,
            score = score,
            eventProperties = eventProperties,
            eventContextProperties = eventContextProperties,
        });
    }

    /// <summary>
    /// Method to call whenever the user finishes a game, even when leaving a game.
    /// </summary>
    /// <param name="levelComplete">Whether the user finished the game</param>
    /// <param name="score">The score of the game</param>
    /// <param name="levelNumber">The level number of the game</param>
    /// <param name="eventProperties">An optional list of properties to send along with the event</param>
    /// <param name="eventContextProperties">An optional list of properties to send along with the event</param>
    public static void OnGameFinished(bool levelComplete, float score, string levelNumber,
        Dictionary<string, object> eventProperties = null,
        Dictionary<string, object> eventContextProperties = null)
    {
        OnGameFinished(new GameFinishedParameters {
            status = levelComplete,
            score = score,
            level = levelNumber,
            eventProperties = eventProperties,
            eventContextProperties = eventContextProperties,
        });
    }
    
    
    /// <summary>
    /// Method to call whenever the user click on pause button to pause or resume the game
    /// </summary>
    /// <param name="pauseStatus">True if the Game is paused, else False.</param>
    public static void OnGamePauseButtonClicked(bool pauseStatus)
    {
        AnalyticsManager.TrackGamePauseButtonClicked(pauseStatus);
    }
    
    
    
    
    /// <summary>
    /// Method to call when any item transaction (soft or hard currency) is done by the player. Used to collect detailed statistics.
    /// </summary>
    /// <param name="parameters">Parameters to send as data for the event</param>
    public static void OnItemTransaction(ItemTransactionParameters parameters)
    {
        AnalyticsManager.TrackItemTransaction(parameters);
    }

    /// <summary>
    /// Call this method to track any custom event you want.
    /// </summary>
    /// <param name="eventName">The name of the event to track.
    /// For Adjust, create your event in Adjust > Apps > YOUR_APP > All Settings > Events, then use the token as eventName 
    /// </param>
    /// <param name="eventProperties">An optional list of properties to send along with the event
    /// - The dictionary values will be passed as strings (ToString() will be applied). If the value has a specific formatting, please convert it
    /// to string before passing the parameter
    /// - VoodooAnalytics: sent as cvars, if a key does not exist in eventContextProperties merge into it
    /// - Adjust: sent as CallbackParameters and PartnerParameters
    /// - Mixpanel: sent as event value
    /// - FirebaseAnalytics: sent as event parameter
    /// </param>
    /// <param name="type">string value used by Voodoo Analytics to identify the event type</param>
    /// <param name="analyticsProviders">The list of analytics provider you want to track your custom event to.
    /// If this list is null or empty, the event will be tracked in GameAnalytics</param>
    /// <param name="eventContextProperties">An optional list of properties to send along with the event, the properties' names must only contain letters and underscore characters</param>
    public static void TrackCustomEvent(string eventName,
        Dictionary<string, object> eventProperties = null,
        string type = null,
        List<AnalyticsProvider> analyticsProviders = null,
        Dictionary<string, object> eventContextProperties = null)
    {
        AnalyticsManager.TrackCustomEvent(eventName, eventProperties, type, analyticsProviders, eventContextProperties);
    }

    /// <summary>
    /// Set the identifier of the user to track in the crash reporter.
    /// - Firebase: the method Crashlytics.SetUserId is called
    /// - Embrace: the method Embrace.SetUserIdentifier is called
    /// </summary>
    /// <param name="userId">User identifier to track in the crash reporter</param>
    public static void SetCrashReportingUserId(string userId)
    {
        CrashReport.SetUserId(userId);
    }

    /// <summary>
    /// Set the profile of the user to track in the crash reporter.
    /// - Firebase: this feature can not be applied to Firebase
    /// - Embrace: the method Embrace.SetUserPersona is called (up to ten profiles can be added)
    /// </summary>
    /// <param name="profile">User profile to track in the crash reporter</param>
    public static void SetCrashReportingUserProfile(string profile)
    {
        CrashReport.SetUserProfile(profile);
    }

    /// <summary>
    /// Set a key value pair to be sent when crashes are registered in the crash reporter.
    /// - Firebase: the method Crashlytics.SetCustomKey is called
    /// - Embrace: the method Embrace.AddSessionProperty is called
    /// </summary>
    /// <param name="key">Key to track in the crash reporter</param>
    /// <param name="value">Associated value to track in the crash reporter</param>
    public static void SetCrashReportingCustomData(string key, string value)
    {
        CrashReport.SetCustomData(key, value);
    }

    /// <summary>
    /// Log a simple message in the crash reporter.
    /// - Firebase: the method Crashlytics.Log is called
    /// - Embrace: the method Embrace.LogBreadcrumb is called
    /// </summary>
    /// <param name="message">Message to log in the crash reporter</param>
    public static void LogCrashReportingMessage(string message)
    {
        CrashReport.Log(message);
    }

    /// <summary>
    /// Log an exception in the crash reporter.
    /// </summary>
    /// <param name="exception">The exception to log</param>
    public static void LogException(Exception exception)
    {
        CrashReport.LogException(exception);
    }
    
    /// <summary>
    /// Register your purchase delegate at the startup of your app. This delegate's methods will be called when a purchase
    /// completes or fails, or when trying to restore a purchase. This method use IPurchaseDelegateWithInfo instead
    /// of the old IPurchaseDelegate
    /// </summary>
    public static void RegisterPurchaseDelegate(IPurchaseDelegateWithInfo purchaseDelegate)
    {
        Iap.AddIapPurchaseDelegate(purchaseDelegate);
    }

    /// <summary>
    /// Unregister your purchase delegate. 
    /// </summary>
    public static void UnregisterPurchaseDelegate(IPurchaseDelegateWithInfo purchaseDelegate)
    {
        Iap.RemoveIapPurchaseDelegate(purchaseDelegate);
    }
    
    /// <summary>
    /// Starts the purchase process for the given Product ID. Make sure you did not forget to register your purchase 
    /// delegate beforehand.
    /// </summary>
    /// <param name="productId">The product ID to buy, as registered in VoodooSettings</param>
    /// <param name="caller">Optional parameter. Use this if you want to get the callback only on a specific object instead of every registered delegates</param>
    /// <param name="customPurchaseValidator">Optional parameter. Use this if you want to use a custom purchase validator.
    /// This won't be used in the Unity editor.</param>
    public static void Purchase(string productId, IPurchaseDelegateWithInfo caller, IPurchaseValidator customPurchaseValidator = null)
    {
        Iap.BuyIAPProduct(productId, caller, customPurchaseValidator);
    }
    
    /// <summary>
    /// Starts the purchase process for the given Product ID. Make sure you did not forget to register your purchase 
    /// delegate beforehand.
    /// </summary>
    /// <param name="productId">The product ID to buy, as registered in VoodooSettings</param>
    /// <param name="caller">Optional parameter. Use this if you want to get the callback only on a specific object instead of every registered delegates</param>
    /// <param name="customPurchaseValidator">Optional parameter. Use this if you want to use a custom purchase validator.
    /// This won't be used in the Unity editor.</param>
    public static void Purchase(string productId, IPurchaseValidator customPurchaseValidator = null)
    {
        Iap.BuyIAPProduct(productId, null, customPurchaseValidator);
    }

    /// <summary>
    /// Triggers the restore purchases process for all products registered as Non Consummable and Subscriptions in VoodooSettings. For each
    /// successful restoration, your delegate's OnPurchaseComplete() method will be called.
    ///
    /// SubscriptionInfo for subscription products can be accessed afterwards using "GetSubscriptionInfo".  
    /// </summary>
    /// <param name="successCallback">successCallback, called with enum result of RestorePurchase attempt
    /// </param>
    public static void RestorePurchases(Action<RestorePurchasesResult> successCallback = null)
    {
        Iap.RestoreIAPProduct(successCallback);
    }

    /// <summary>
    /// Returns the SubscriptionInfo of a subscription product.
    ///
    /// In the case an IOS user is reinstalling your app, call "RestorePurchases" before calling this method.
    /// Otherwise SubscriptionInfo will not hold the user's correct subscription status.  
    /// </summary>
    /// <param name="productId">The product ID to retrieve SubscriptionInfo for</param>
    /// <returns>SubscriptionInfo of the product ID.</returns>
    [CanBeNull]
    public static SubscriptionInfoContainer GetSubscriptionInfo(string productId)
    {
        return Iap.GetSubscriptionDetails(productId);
    }

    /// <summary>
    /// Returns if a productId is subscribed to or not.
    ///
    /// Call "RestorePurchases" in case of fresh installs on iOS. 
    /// </summary>
    /// <param name="productId">The product ID to check is subscribed</param>
    /// <returns>If the product ID is subscribed to or not</returns>
    public static bool IsSubscribed(string productId)
    {
        return Iap.IsSubscribedProduct(productId);
    }

    /// <summary>
    /// Return product info based on the provided productId
    /// </summary>
    /// <param name="productId">The product get the infos</param>
    /// <returns>return the product information or null if its not found</returns>
    public static ProductReceivedInfo GetProduct(string productId)
    {
        return Iap.GetProductWithId(productId);
    }

    /// <summary>
    /// This method should only be called when you are not using the IAP Module and a purchase is successful.
    /// It will send the purchase data to VAN, Adjust, GA and Firebase.
    /// The revenue tracking is done automatically when using the IAP Module. Thus, if you are calling this method, it will be done twice.
    /// If you are not using the IAP Module, it's your responsibility to send the revenue data.
    /// </summary>
    /// <param name="productId">The id of the product that has been bought. It should be as defined on the stores.</param>
    /// <param name="productType">The product type (Consumable, NonConsumable or Subscription)</param>
    /// <param name="transactionId">The id of the transaction that was made.</param>
    /// <param name="currency">The currency code in which the transaction was done (ie: EUR, USD,...)</param>
    /// <param name="price">The price of the transaction (in the currency defined above)</param>
    /// <param name="productName">The name of the product</param>
    /// <param name="isRestoredPurchase">This should be set to true only if the purchase was a restoration purchase which means that the user didn't re-pay for it.</param>
    public static void TrackIAPRevenues(string productId, PurchaseProductType productType, string transactionId, string currency, double price, string productName, bool isRestoredPurchase)
    {
        var productReceivedInfo = new ProductReceivedInfo(productId, productType, transactionId, currency, price, null, productName, true);
        TrackIAPRevenues(productReceivedInfo, isRestoredPurchase);
    }

    /// <summary>
    /// This method should only be called when you are not using the IAP Module and a purchase is successful.
    /// It will send the purchase data to VAN, Adjust, GA and Firebase.
    /// The revenue tracking is done automatically when using the IAP Module. Thus, if you are calling this method, it will be done twice.
    /// If you are not using the IAP Module, it's your responsibility to send the revenue data.
    /// </summary>
    /// <param name="product">The product that got bought</param>
    /// <param name="isRestoredPurchase">This should be set to true only if the purchase was a restoration purchase which means that the user didn't re-pay for it.</param>
    public static void TrackIAPRevenues(ProductReceivedInfo product, bool isRestoredPurchase)
    {
        IAPAnalytics.TrackIAPRevenues(product, isRestoredPurchase, true);
    }
    
    /// <summary>
    /// Tries to display a banner. If the user has purchased the "No ads" product, no banner will be displayed.
    /// </summary>
    /// <param name="onBannerDisplayed">Called when the banner is displayed with the height as a parameter</param>
    public static void ShowBanner(Action<float> onBannerDisplayed = null)
    {
        AdsManager.Banner.Show(onBannerDisplayed);
    }

    /// <summary>
    /// Hides any eventually displayed banner.
    /// </summary>
    public static void HideBanner()
    {
        AdsManager.Banner.Hide();
    }

    /// <summary>
    /// Tries to display an interstitial ad if all display conditions are met (user is not premium + display conditions 
    /// are fulfilled) and calls the onComplete callback.
    /// </summary>
    /// <param name="onComplete">
    /// This will get called all the time when you can go on with your game (when the interstitial is dismissed, when no 
    /// interstitial is shown because user is premium, etc.)
    /// </param>
    /// <param name="ignoreConditions">
    /// If true, ad display conditions are ignored, ad will be displayed if it is available, regardless of the time elapsed
    /// since last interstitial.
    /// </param>
    /// <param name="interstitialType">
    /// Additional tag to track specific event
    /// </param>
    public static void ShowInterstitial(Action onComplete = null, bool ignoreConditions = false,
        string interstitialType = null)
    {
        AdsManager.Interstitial.Show(onComplete, ignoreConditions, interstitialType);
    }

    /// <summary>
    /// Returns true when a rewarded video ad is available for display.
    /// </summary>
    public static bool IsRewardedVideoAvailable() => AdsManager.RewardedVideo.IsAvailable();

    /// <summary>
    /// Set the event that will be triggered when the rewarded video load is complete 
    /// </summary>
    /// <param name="onRewardedVideoLoadComplete">
    /// This will called all the time when the rewarded video load is Complete.
    /// The bool parameter will be true if the rewarded video is available.
    /// </param>
    public static void SubscribeOnRewardedVideoLoaded(Action<bool> onRewardedVideoLoadComplete)
    {
        AdsManager.RewardedVideo.OnRewardedVideoAvailabilityChangeEvent += onRewardedVideoLoadComplete;
        onRewardedVideoLoadComplete?.Invoke(IsRewardedVideoAvailable());
    }

    public static void UnSubscribeOnRewardedVideoLoaded(Action<bool> onRewardedVideoLoadComplete)
    {
        AdsManager.RewardedVideo.OnRewardedVideoAvailabilityChangeEvent -= onRewardedVideoLoadComplete;
    }

    /// <summary>
    /// Returns true when an interstitial ad is available for display.
    /// </summary>
    public static bool IsInterstitialAvailable() => AdsManager.Interstitial.IsAvailable();

    /// <summary>
    /// Returns true when a banner ad is available.
    /// </summary>
    public static bool IsBannerAvailable() => AdsManager.Banner.IsAvailable();

    /// <summary>
    /// Displays a rewarded video ad and calls the onComplete callback. Please ensure beforehand there is a rewarded 
    /// video ad available by calling VoodooSauce.IsRewardedVideoAvailable().
    /// </summary>
    /// <param name="onComplete">
    /// This will get called all the time when you can go on with your game (when the ad is dismissed or complete, when 
    /// the video is expired, etc.). The bool parameter will be true if the user did watch the video until the end.
    ///  </param>
    /// <param name="rewardedType">
    /// Additional tag to track specific event
    /// </param>
    public static void ShowRewardedVideo(Action<bool> onComplete, string rewardedType = null)
    {
        AdsManager.RewardedVideo.Show(onComplete, rewardedType);
    }

    /// <summary>
    /// Call this method whenever you display your show rewarded video button, even if no rewarded video is available.
    /// </summary>
    /// <param name="rewardedType">
    /// Additional tag to track specific event
    /// </param>
    public static void OnRewardedVideoButtonShown(string rewardedType)
    {
        AdsManager.RewardedVideo.OnButtonShown(rewardedType);
    }

    /// <summary>
    /// <para>
    /// Configure the conditions that control when an interstitial ad can be displayed.
    /// This method overrides the conditions set in VoodooTune.
    /// </para>
    /// <para>
    /// When can my interstitial be displayed?
    /// </para>
    /// <para>
    /// Suppose you have waited enough time after the last rewarded video was played.
    /// In that case, your interstitial ad will be displayed after the <c>delayInSecondsBeforeFirstInterstitialAd</c>.
    /// Otherwise, your interstitial ad can be displayed if you played enough games <b>else or</b> if the corresponding timer is respected.
    /// </para>
    /// </summary>
    /// <param name="delayInSecondsBeforeFirstInterstitialAd">
    /// The number of seconds to wait before the first-ever interstitial ad can be displayed.
    /// </param>
    /// <param name="delayInSecondsBetweenInterstitialAds">
    /// The number of seconds to wait before an interstitial can be displayed.
    /// </param>
    /// <param name="maxGamesBetweenInterstitialAds">
    /// The maximal number of games between two interstitial ads display
    /// </param>
    /// <param name="delayInSecondsBetweenRewardedVideoAndInterstitial">
    /// Delay in second between rewarded and interstitial
    /// </param>
    /// <param name="delayInSecondsBeforeSessionFirstInterstitial">
    /// The number of seconds to wait before the session's first interstitial ad can be displayed.
    /// This parameter can be disabled with a negative value.
    /// </param>
    /// <param name="delayInSecondsBetweenAppOpenAdAndInterstitial">
    /// Delay in second between an appOpen Ad and an interstitial
    /// </param>
    public static void SetInterstitialAdsDisplayConditions(int delayInSecondsBeforeFirstInterstitialAd,
        int delayInSecondsBetweenInterstitialAds,
        int maxGamesBetweenInterstitialAds,
        int delayInSecondsBetweenRewardedVideoAndInterstitial = -1,
        int delayInSecondsBeforeSessionFirstInterstitial = -1,
        float delayInSecondsBetweenAppOpenAdAndInterstitial = -1)
    {
        AdsManager.SetInterstitialAdsDisplayConditions(delayInSecondsBeforeFirstInterstitialAd,
            delayInSecondsBeforeSessionFirstInterstitial,
            delayInSecondsBetweenInterstitialAds,
            maxGamesBetweenInterstitialAds,
            delayInSecondsBetweenRewardedVideoAndInterstitial,
            delayInSecondsBetweenAppOpenAdAndInterstitial);
    }

    /// <summary>
    /// Set interstitial at launch display behavior 
    /// </summary>
    /// <param name="interstitialAtLaunch">Enable/disable interstitial at launch</param>
    /// <param name="interstitialAtLaunchDelay">Set delay in seconds before interstitial at launch shown</param>
    /// <param name="interstitialAtLaunchTimeout">Set max timeout to wait for interstitial at launch</param>
    [Obsolete("This method is no longer used")]   
    public static void SetInterstitialAtLaunchConditions(bool interstitialAtLaunch,
        int interstitialAtLaunchDelay,
        int interstitialAtLaunchTimeout) { }

    /// <summary>   
    /// Overrides the settings set in VoodooSettings. This method is intended solely for A/B testing purposes.  
    /// </summary>  
    /// <param name="adUnitType">The type of ad unit to override</param>    
    /// <param name="adUnit">The ad unit key. Set it to null to deactivate ads for this type of ad unit</param> 
    public static void SetAdUnit(AdUnitType adUnitType, string adUnit)
    {
        AdsManager.SetAdUnit(adUnitType, adUnit);
    }

    /// <summary>
    /// Enables premium status for the user, thus removing ads. You should call this after the user bought your "No ads"
    /// in-app purchase. Any currently loaded/displayed ad will be removed from screen and memory.
    /// </summary>
    public static void EnablePremium()
    {
        VoodooPremium.SetEnabledPremium();
    }

    /// <summary>
    /// Enables/disables temporary premium status for the user, thus removing/ractivating ads.
    /// This is useful for special promotional events (ie amazon prime gaming ad-free period)
    /// notes: ads are reactivated when free period ends (interstitial=immediate, banner=on next showbanner)
    /// <param name="isPremiumPeriodActive">true when the premium period is ongoing</param>
    /// </summary>
    public static void SetPremiumPeriod(bool isPremiumPeriodActive)
    {
        VoodooPremium.SetPremiumPeriod(isPremiumPeriodActive);
    }

    /// <summary>
    /// Returns true if the user is a premium user (no ads).
    /// </summary>
    /// <returns>TRUE when the user IS premium</returns>
    public static bool IsPremium() => VoodooPremium.IsPremium();

    /// <summary>
    /// Checks if the player purchased premium (no ads) through an IAP
    /// </summary>
    public static bool IsIAPPremium() => VoodooPremium.IsIAPPremium();

    /// <summary>
    /// Checks if the player has a temporary premium period (no ads) active (ie free premium week with Amazon Prime)
    /// </summary>
    public static bool HasPremiumPeriod() => VoodooPremium.HasPremiumPeriod();

    /// <summary>
    /// Show the Cross Promotion
    /// </summary>
    /// <param name="onSuccess">Call when an ad is displayed</param>
    /// <param name="onFailure">Call if an error occured and there is nothing to display</param>
    public static void ShowCrossPromo(Action<AssetModel> onSuccess = null, Action<CPShowFailType> onFailure = null)
    {
        VoodooSauceBehaviour.ShowCrossPromo(onSuccess, onFailure);
    }

    /// <summary>
    /// Hide the Cross Promotion
    /// </summary>
    public static void HideCrossPromo()
    {
        VoodooSauceBehaviour.HideCrossPromo();
    }

    /// <summary>
    /// Returns a string that contains the localized price of a product 
    /// </summary>
    /// <param name="productId">Identifier of the product you want to get price of</param>
    /// <returns>
    /// The localized price. For example: "$3.99"
    /// It can be empty if the IAP is not installed/initialized or if the product doesn't exist.
    /// </returns>
    public static string GetLocalizedProductPrice(string productId)
    {
        return Iap.GetLocalizedProductPrice(productId);
    }
    
    /// <summary>
    /// Retrieve the localized price of a product.
    /// </summary>
    /// <param name="productId">The ID of the product you are looking for.</param>
    /// <returns>The price of the product. It can be null if IAP isn't installed/initialized or if the product doesn't exist.</returns>
    public static LocalizedPriceInfo GetLocalizedProductPriceInfo(string productId)
    {
        return Iap.GetLocalizedProductPriceInfo(productId);
    }

    /// <summary>
    /// Attempts to show the app rater. App Rater will be shown only after the two delays:
    /// Delay After Launch and Postpone Cooldown.
    /// </summary>
    public static void TryToShowAppRater()
    {
        AppRater.TryToShow();
    }

    /// <summary>
    /// Your app MUST contains a setting button in your settings menu that should call this method.
    /// Shows the GDPR pop-up that will allow the user to edit its privacy preferences.
    /// </summary>
    /// <param name="onSettingsClosed">Callback when the setting popup is closed. Can be null.</param>
    public static void ShowGDPRSettings(Action onSettingsClosed = null)
    {
        Privacy.OpenPrivacySettings(onSettingsClosed);
    }

    /// <summary>
    /// Check whether or not the GDPR is applicable in the user's current country
    /// </summary>
    /// <param name="callback">Method called when the response is received, with 'is_applicable' as parameter</param>
    [Obsolete("There is no more restriction for non GDPR users. Please stop using this method.",false)]
    public static void RequestGdprApplicability(Action<bool> callback)
    {
        callback?.Invoke(true);
    }
    
    /// <summary>
    /// Shows the GDPR top banner if the user has not given its full consent.
    /// </summary>
    [Obsolete("THE GDPR banner doesn't comply with Apple policy.",false)]
    public static void ShowGDPRBanner()
    {
    }

    /// <summary>
    /// Returns the current list of AB tests added in VoodooSettings.
    /// Obsolete if you use Voodoo Remote Config
    /// </summary>
    /// <returns>Returns all the names of AB tests in a string array</returns>
    public static string[] GetAbTests() => AbTestManager.GetAbTests();

    /// <summary>
    /// FOR TEST PURPOSE ONLY.
    /// Change the current player cohort
    /// Obsolete if you use Voodoo Remote Config
    /// </summary>
    /// <param name="cohortName">The name of the cohort you want to force to the user</param>
    public static void SetPlayerCohort(string cohortName)
    {
        AbTestManager.SetPlayerCohort(cohortName);
    }

    /// <summary>
    /// FOR TEST PURPOSE ONLY.
    /// Shows a cohort debug menu that will allow use to change cohort during runtime. (Needs app restart)
    /// </summary>
    public static void ShowCohortDebugMenu()
    {
        VoodooSauceBehaviour.ShowCohortDebugMenu();
    }

    /// <summary>
    /// Returns a list of instances of the given type T and its subtypes, from the VoodooTune cache.
    /// if there is no instance present in the cache, the list will contain an instance with the default values defined in the class T.
    /// </summary>
    /// <returns>A list of instances of the given type T and its subtypes.</returns>
    public static List<T> GetSubclassesItems<T>() where T : class, new() => VoodooTuneManager.GetSubclassesItems<T>();

    /// <summary>
    /// Returns a list of instances of the given type T, from the VoodooTune cache.
    /// if there is no instance present in the cache, the list will contain an instance with the default values defined in the class T.
    /// </summary>
    /// <returns>A list of instances of the given type T.</returns>
    public static List<T> GetItems<T>() where T : class, new() => VoodooTuneManager.GetItems<T>();

    /// <summary>
    /// Returns a list of instances of the given type T, from the VoodooTune cache.
    /// if there is no instance present in the cache, the method returns a list of instances with the default values defined in the Resources json files.
    /// </summary>
    /// <returns>An instance of the given type T.</returns>
    public static List<T> GetItemsOrDefaults<T>() where T : class, new() => VoodooTuneManager.GetItemsOrDefaults<T>();

    /// <summary>
    /// Returns the first instances of the given type T, from the VoodooTune cache.
    /// if there is no instance present in the cache, the method returns null.
    /// </summary>
    /// <returns>An instance of the given type T or null.</returns>
    public static T GetItem<T>() where T : class, new() => VoodooTuneManager.GetItem<T>();

    /// <summary>
    /// Returns the first instances of the given type T, from the VoodooTune cache.
    /// if there is no instance present in the cache, the method returns an instance with the default values defined in the class T.
    /// </summary>
    /// <returns>An instance of the given type T. </returns>
    public static T GetItemOrDefault<T>() where T : class, new() => VoodooTuneManager.GetItemOrDefault<T>();

    public enum AnalyticsProvider
    {
        VoodooAnalytics,
        GameAnalytics,
        Mixpanel,
        FirebaseAnalytics,
        Adjust
    }

    /// <summary>
    /// Indicates whether the user accepted the ads consent
    /// </summary>
    /// <returns>Returns true if the user has accepted the ads consent, false otherwise</returns>
    public static bool AdsConsentGiven() => Privacy.HasAdsConsent();

    /// <summary>
    /// Indicates whether the user accepted the analytics consent
    /// </summary>
    /// <returns>Returns true if the user has accepted the analytics consent, false otherwise</returns>
    public static bool AnalyticsConsentGiven() => Privacy.HasAnalyticsConsent();

    public static void SubscribeToOnPrivacyOpened(Action onPrivacyOpen)
    {
        VoodooSauceBehaviour.OnPrivacyOpened += onPrivacyOpen;
    }

    public static void UnsubscribeToOnPrivacyOpened(Action onPrivacyOpen)
    {
        VoodooSauceBehaviour.OnPrivacyOpened -= onPrivacyOpen;
    }

    public static void SubscribeToOnPrivacyClosed(Action onPrivacyClose)
    {
        VoodooSauceBehaviour.OnPrivacyClosed += onPrivacyClose;
    }

    public static void UnsubscribeToOnPrivacyClosed(Action onPrivacyClose)
    {
        VoodooSauceBehaviour.OnPrivacyClosed -= onPrivacyClose;
    }

    public static void SubscribeToDeleteDataRequested(Action onDeleteDataRequest)
    {
        VoodooSauceBehaviour.onDeleteDataRequested += onDeleteDataRequest;
    }
    
    public static void UnsubscribeToDeleteDataRequested(Action onDeleteDataRequest)
    {
        VoodooSauceBehaviour.onDeleteDataRequested -= onDeleteDataRequest;
    }

	/// <summary>
    /// Returns true if interstitial can be displayed. It does not check if the interstitial has been loaded
    /// Please use VoodooSauce.IsInterstitialAvailable() to check if the interstitial was successfully loaded
    /// </summary>
    /// <returns>True if interstitial can be displayed, false otherwise</returns>
    public static bool AreInterstitialDisplayConditionsMet() => AdsManager.AreInterstitialDisplayConditionsMet();

    /// <summary>
    /// Track a custom funnel defined in your VoodooSettings within VAN.
    /// A funnel is composed of multiple unique steps with corresponding positions.
    /// Funnels are a general Analytics concept for tracking your users.
    /// You can learn more by searching online: https://support.google.com/analytics/answer/9327974?hl=en 
    /// </summary>
    /// <param name="funnelName"> Name of funnel </param>
    /// <param name="funnelStep"> Step name in the funnel </param>
    public static void TrackFunnel(string funnelName, string funnelStep)
    {
        VoodooFunnelsManager.TrackFunnel(funnelName, funnelStep); 
    }
    
    /// <summary>
    /// Method to call as soon as the game becomes interactive for the player,
    /// i.e. as soon as the player can do the very first action in the game
    /// like starting a game, choosing a menu item, etc. 
    /// </summary>
    public static void OnGameInteractable()
    {
        AnalyticsManager.OnGameInteractable();
    }

    /// <summary>
    /// Set the position where the audio ad will be displayed.
    /// You can also place your audio ad by using the AudioAdPosition prefab (Assets/VoodooSauce/Ads/AudioAds/AudioAdPosition.prefab).
    /// The prefab position will override this method.
    /// </summary>
    /// <param name="position">Position where you want to see your audio ad.</param>
    /// <param name="offset">Offset in pixels.</param>
    public static void SetAudioAdPosition(AudioAdPosition position, Vector2Int offset)
    {
        AudioAdsManager.Instance.SetAudioAdPosition(position, offset);
    }
    
    /// <summary>
    /// Get the native Banner's height in pt / dp (NOT IN PIXEL), it will return the banner height or -1
    /// if the feature is not supported yet by the mediation. Currently for IronSource it is not supported yet
    /// hence it will always return -1
    /// </summary>
    public static float GetNativeBannerHeightInDp() => AdsManager.Banner.GetNativeBannerHeightInDp();
    
    /// <summary>
    /// Get the native Banner's height in Pixel it will return the banner height or -1
    /// if the feature is not supported yet by the mediation. Currently for IronSource it is not supported yet
    /// hence it will always return -1
    /// </summary>
    public static float GetNativeBannerHeightInPx() => AdsManager.Banner.GetNativeBannerHeightInPx();
}
