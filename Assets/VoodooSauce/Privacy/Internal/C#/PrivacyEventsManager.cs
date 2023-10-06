using System.Collections.Generic;
using Voodoo.Sauce.Internal.Analytics;

namespace Voodoo.Sauce.Privacy
{
    public static class PrivacyEventsManager
    {
        private static string EventType = "custom";
        private static List<VoodooSauce.AnalyticsProvider> analyticsProviders = new List<VoodooSauce.AnalyticsProvider>()
        {
            VoodooSauce.AnalyticsProvider.VoodooAnalytics
        };

        private enum ScreenName
        {
            learn_more_popup,
            voodoo_disclaimer_popup,
            apple_disclaimer_popup,
            apple_popup,
            gdpr_popup,
            privacy_popup,
            consent_or_purchase_popup,
            purchase_popup
        }

        private enum EventName
        {
            screen_view,
            button_click
        }
        
        private enum Property
        {
            screen_name,
            button,
            toggle_ads,
            toggle_analytics
        }

        #region Learn More Popup
        public static void OnLearnMoreDisplay()
        {
            string eventName = EventName.screen_view.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.learn_more_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }

        public static void OnLearnMoreClick()
        {
            string eventName = EventName.button_click.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.learn_more_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        #endregion
        
        #region Voodoo Disclaimer Popup
        public static void OnVoodooDisclaimerDisplay()
        {
            string eventName = EventName.screen_view.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.voodoo_disclaimer_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }

        public static void OnVoodooDisclaimerClick()
        {
            string eventName = EventName.button_click.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.voodoo_disclaimer_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        #endregion
        
        #region Apple Disclaimer Popup
        public static void OnAppleDisclaimerDisplay()
        {
            string eventName = EventName.screen_view.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.apple_disclaimer_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }

        public static void OnAppleDisclaimerClick()
        {
            string eventName = EventName.button_click.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.apple_disclaimer_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        #endregion
        
        #region Apple Popup
        public enum ApplePopupButton
        {
            allow,
            deny
        }
        
        public static void OnAppleIdfaAuthorizationDisplay()
        {
            AnalyticsManager.StartAttDisplayingTimer();
            string eventName = EventName.screen_view.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.apple_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }

        public static void OnAppleIdfaAuthorizationClick(ApplePopupButton applePopupButton)
        {
            AnalyticsManager.StopAttDisplayingTimer();
            string eventName = EventName.button_click.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.apple_popup.ToString());
            eventProperties.Add(Property.button.ToString(),applePopupButton.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        #endregion
        
        #region GDPR Popup
        public enum GDPRPopupButton
        {
            proceed,
            more_info,
            more_info_sdk,
            more_info_ads,
            more_info_analytics,
            more_info_age_limit
        }
        
        public static void OnGDPRDisplay()
        {
            string eventName = EventName.screen_view.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.gdpr_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        
        public static void OnGDPRProceedClick(bool ads, bool analytics)
        {
            string eventName = EventName.button_click.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.gdpr_popup.ToString());
            eventProperties.Add(Property.button.ToString(),GDPRPopupButton.proceed.ToString());
            eventProperties.Add(Property.toggle_ads.ToString(),ads);
            eventProperties.Add(Property.toggle_analytics.ToString(), analytics);
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        
        public static void OnGDPRClick(GDPRPopupButton GDPRPopupButton)
        {
            string eventName = EventName.button_click.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.gdpr_popup.ToString());
            eventProperties.Add(Property.button.ToString(),GDPRPopupButton.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        #endregion

        #region Privacy Popup
        public enum PrivacyPopupButton
        { 
            proceed,
            more_info,
            more_info_sdk,
            more_info_age_limit
        }
        
        public static void OnPrivacyDisplay()
        {
            string eventName = EventName.screen_view.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.privacy_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        
        public static void OnPrivacyClick(PrivacyPopupButton privacyPopupButton)
        {
            string eventName = EventName.button_click.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.privacy_popup.ToString());
            eventProperties.Add(Property.button.ToString(),privacyPopupButton.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        #endregion
        
        #region Consent_or_purchase_Popup
        public enum ConsentOrPurchasePopupButton
        { 
            consent,
            purchase
        }
        
        public static void OnConsentOrPurchaseDisplay()
        {
            string eventName = EventName.screen_view.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.consent_or_purchase_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        
        public static void OnConsentOrPurchaseClick(ConsentOrPurchasePopupButton consentOrPurchasePopupButton)
        {
            string eventName = EventName.button_click.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.consent_or_purchase_popup.ToString());
            eventProperties.Add(Property.button.ToString(),consentOrPurchasePopupButton.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        #endregion
        
        #region Purchase_Popup
        public enum PurchasePopupButton
        {
            purchase,
            consent,
            limitedPlayTime
        }
        
        public static void OnPurchaseDisplay()
        {
            string eventName = EventName.screen_view.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.purchase_popup.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        
        public static void OnPurchaseClick(PurchasePopupButton purchasePopupButton)
        {
            string eventName = EventName.button_click.ToString();
            Dictionary<string,object> eventProperties = new Dictionary<string, object>();
            eventProperties.Add(Property.screen_name.ToString(),ScreenName.purchase_popup.ToString());
            eventProperties.Add(Property.button.ToString(),purchasePopupButton.ToString());
            VoodooSauce.TrackCustomEvent(eventName, eventProperties, EventType, analyticsProviders);
        }
        #endregion
    }
}