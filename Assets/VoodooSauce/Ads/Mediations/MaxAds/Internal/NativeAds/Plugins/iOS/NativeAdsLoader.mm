//
//  NativeAdsLoader.m
//  Unity-iPhone
//
//  Created by Safouane Azzabi on 06/05/2022.
//

#import "NativeAdsLoader.h"
#import <QuartzCore/QuartzCore.h>
#include "UI/UnityAppController+ViewHandling.h"

//pragma MARK: - NativeAdsLoader Interface

@interface NativeAdsLoader()

@property (nonatomic, strong) MANativeAdLoader *nativeAdLoader;
@property (nonatomic, strong) MAAd *loadedNativeAd;
@property (nonatomic) bool isReadyToShow;
@property (nonatomic, weak) UIView *nativeAdContainerView;
@property (nonatomic, strong) MANativeAdView *nativeAdView;
@property (nonatomic) ALUnityNativeAdsCallback callback;
@property (nonatomic, strong) NSString *adUnitIdentifier;

@end


// MARK: - NativeAdsLoader Implementation

@implementation NativeAdsLoader

    static int TITLE_LABEL_TAG = 11;
    static int BODY_LABEL_TAG = 12;
    static int CALL_TO_ACTION_LABEL_TAG = 13;
    static int ICON_IMAGE_VIEW_TAG = 10;
    static int MEDIA_VIEW_CONTENT_VIEW_TAG = 1;
    static int ADVERTISER_LABEL_TAG = 2;
    static int OPTIONS_CONTENT_VIEW_TAG = 3;
    static int AD_LABEL_VIEW_TAG = 100;

    - (id)initWithAdUnitIdentifier:(NSString *)adUnitIdentifier containerView:(UIView *)containerView andCallback:(ALUnityNativeAdsCallback)callback
    {
        if (self = [super init]) {
            self.nativeAdLoader = [[MANativeAdLoader alloc] initWithAdUnitIdentifier:adUnitIdentifier];
            [self.nativeAdLoader setNativeAdDelegate:self];
            [self.nativeAdLoader setRevenueDelegate:self];
            
            self.nativeAdContainerView = containerView;
            self.callback = callback;
            self.adUnitIdentifier = adUnitIdentifier;
        }

        return self;
    }

    - (void)loadAd
    {
        [self.nativeAdLoader loadAd];
    }
    
    - (bool)isAdAvailable
    {
        return self.loadedNativeAd != nil && self.isReadyToShow;
    }
    
    - (void)showAdView:(NSString *)viewFormat atX:(float)x andY:(float)y withWidth:(float)width andHeight:(float)height
    {
        // Hide the current ad view if it's existing.
        [self hideAdView];
        
        if (![self isAdAvailable]) {
            return;
        }
        
        // Add the native ad view to the layout.
        @try {
            self.nativeAdView = [self createNativeAdView:viewFormat];
            [self.nativeAdLoader renderNativeAdView:self.nativeAdView withAd:self.loadedNativeAd];
            [self.nativeAdContainerView addSubview:self.nativeAdView];
            
            // The size must be scaled to the screen (i.e. a retina screen has a scale equal to 2)
            CGFloat scale = [[UIScreen mainScreen] scale];
            [self.nativeAdView setFrame:CGRectMake(x/scale, y/scale, width/scale, height/scale)];
            
            // Send callback.
            [self forwardEventWithName:@"onNativeAdShown"];
        } @catch (NSException *exception) {
            NSLog(@"VoodooSauce Native Ad: can not show the ad: %@", exception.reason);
        }
    }
    
    - (void)hideAdView
    {
        if (self.nativeAdView == nil) {
            return;
        }
        
        // Remove all the references to the view to avoid memory leaks.
        // Remove the view from its superview before setting the value to nil!
        [self.nativeAdView removeFromSuperview];
        self.nativeAdView = nil;
        
        self.isReadyToShow = false;
        
        // Send callback.
        [self forwardEventWithName:@"onNativeAdHidden"];
    }

    - (void)setCustomData: (NSString*) customData
    {
    	self.nativeAdLoader.customData = customData;
    }

    - (void)didLoadNativeAd:(MANativeAdView *)nativeAdView forAd:(MAAd *)ad
    {
        // Remove all the references to the current ad to avoid memory leaks.
        if (self.loadedNativeAd != nil) {
            [self.nativeAdLoader destroyAd:self.loadedNativeAd];
        }
        self.loadedNativeAd = ad;
        self.isReadyToShow = true;
        
        // Send callback.
        [self forwardEventWithName:@"onNativeAdLoaded"];
    }

    - (void)didFailToLoadNativeAdForAdUnitIdentifier:(NSString *)adUnitIdentifier withError:(MAError *)error
    {
        [self forwardEventWithName:@"onNativeAdLoadFailed" andError:error];
    }

    - (void)didClickNativeAd:(MAAd *)ad
    {
        [self forwardEventWithName:@"onNativeAdClicked" andAd:ad];
    }

    - (void)didPayRevenueForAd:(MAAd *)ad
    {
        [self forwardEventWithName:@"onNativeAdRevenuePaid" andAd:ad];
    }

    - (MANativeAdView *)createNativeAdView:(NSString *)viewFormat
    {
        UINib *nativeAdViewNib = [UINib nibWithNibName:viewFormat bundle:NSBundle.mainBundle];
        MANativeAdView *nativeAdView = [nativeAdViewNib instantiateWithOwner:nil options:nil].firstObject;
        [nativeAdView layer].cornerRadius = 5.0f;
        [nativeAdView layer].borderWidth = 1.0f;
        [nativeAdView layer].borderColor = [UIColor lightGrayColor].CGColor;
        nativeAdView.clipsToBounds = true;
        
        UIView *adLabelView = [nativeAdView viewWithTag:AD_LABEL_VIEW_TAG];
        [adLabelView layer].borderWidth = 1.0f;
        [adLabelView layer].borderColor = [UIColor colorWithRed:((float)((0x696969 & 0xFF0000) >> 16))/255.0 \
                                                          green:((float)((0x696969 & 0x00FF00) >>  8))/255.0 \
                                                           blue:((float)((0x696969 & 0x0000FF) >>  0))/255.0 \
                                                          alpha:1.0].CGColor;
        
        MANativeAdViewBinder *binder = [[MANativeAdViewBinder alloc] initWithBuilderBlock:^(MANativeAdViewBinderBuilder *builder) {
            builder.titleLabelTag = TITLE_LABEL_TAG;
            builder.bodyLabelTag = BODY_LABEL_TAG;
            builder.callToActionButtonTag = CALL_TO_ACTION_LABEL_TAG;
            builder.iconImageViewTag = ICON_IMAGE_VIEW_TAG;
            builder.mediaContentViewTag = MEDIA_VIEW_CONTENT_VIEW_TAG;
            builder.advertiserLabelTag = ADVERTISER_LABEL_TAG;
            builder.optionsContentViewTag = OPTIONS_CONTENT_VIEW_TAG;
        }];
        [nativeAdView bindViewsWithAdViewBinder:binder];
        return nativeAdView;
    }

    - (void)forwardEventWithName:(NSString *)name
    {
        [self forwardEventWithName:name andAd:self.loadedNativeAd];
    }

    - (void)forwardEventWithName:(NSString *)name andAd:(MAAd *)ad
    {
        NSMutableDictionary *values = [[NSMutableDictionary alloc] init];
        
        if (ad != nil) {
            if (ad.adUnitIdentifier == nil) {
                NSLog(@"Field 'adUnitIdentifier' is nil for the MaxAds native ad.");
            } else {
                [values setObject:ad.adUnitIdentifier forKey:@"adUnitId"];
            }
            
            if (ad.networkName == nil) {
                NSLog(@"Field 'networkName' is nil for the MaxAds native ad.");
            } else {
                [values setObject:ad.networkName forKey:@"networkName"];
            }
            
            if (ad.networkPlacement == nil) {
                NSLog(@"Field 'networkPlacement' is nil for the MaxAds native ad.");
            } else {
                [values setObject:ad.networkPlacement forKey:@"networkPlacement"];
            }
            
            if (ad.creativeIdentifier == nil) {
                NSLog(@"Field 'creativeIdentifier' is nil for the MaxAds native ad.");
            } else {
                [values setObject:ad.creativeIdentifier forKey:@"creativeId"];
            }
            
            if (ad.placement == nil) {
                NSLog(@"Field 'placement' is nil for the MaxAds native ad.");
            } else {
                [values setObject:ad.placement forKey:@"placement"];
            }
            
            [values setObject:[NSNumber numberWithDouble:ad.revenue] forKey:@"revenue"];
            
            if (ad.revenuePrecision == nil) {
                NSLog(@"Field 'revenuePrecision' is nil for the MaxAds native ad.");
            } else {
                [values setObject:ad.revenuePrecision forKey:@"revenuePrecision"];
            }
            
            if (ad.waterfall == nil || ad.waterfall.name == nil) {
                NSLog(@"Field 'waterfall' is nil for the MaxAds native ad.");
            } else {
                [values setObject:@{@"name": ad.waterfall.name} forKey:@"waterfallInfo"];
            }
        }
        
        [self forwardEventWithName:name andValues:values];
    }

    - (void)forwardEventWithName:(NSString *)name andError:(MAError *)error
    {
        NSMutableDictionary *values = [[NSMutableDictionary alloc] init];
        
        if (error != nil) {
            if (error.message == nil) {
                NSLog(@"Field 'message' is nil for the MaxAds native ad error.");
            } else {
                [values setObject:error.message forKey:@"errorMessage"];
            }
            
            if (error.adLoadFailureInfo == nil) {
                NSLog(@"Field 'adLoadFailureInfo' is nil for the MaxAds native ad error.");
            } else {
                [values setObject:error.adLoadFailureInfo forKey:@"adLoadFailureInfo"];
            }
            
            [values setObject:[NSNumber numberWithInteger:error.code] forKey:@"errorCode"];
            
            if (error.waterfall == nil || error.waterfall.name == nil) {
                NSLog(@"Field 'waterfall' is nil for the MaxAds native ad error.");
            } else {
                [values setObject:@{@"name": error.waterfall.name} forKey:@"waterfallInfo"];
            }
        }
        
        [self forwardEventWithName:name andValues:values];
    }

    - (void)forwardEventWithName:(NSString *)name andValues:(NSDictionary<NSString *, id> *)dictionary
    {
        NSMutableDictionary *values = [[NSMutableDictionary alloc] initWithObjectsAndKeys:@"native", @"adFormat", nil];
        
        if (name == nil) {
            NSLog(@"Field 'name' is nil for the MaxAds native ad.");
        } else {
            [values setObject:name forKey:@"name"];
        }
        
        if (self.adUnitIdentifier == nil) {
            NSLog(@"Field 'adUnitIdentifier' is nil for the MaxAds native ad.");
        } else {
            [values setObject:self.adUnitIdentifier forKey:@"adUnitId"];
        }
        
        if (dictionary != nil) {
            for (NSString *key in dictionary) {
                [values setObject:dictionary[key] forKey:key];
            }
        }
        
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:values options:0 error:nil];
        NSString *serializedParameters = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        self.callback(serializedParameters.UTF8String);
    }

@end



// MARK: - External

extern "C"
{
    NativeAdsLoader *adLoader = nil;

    extern void _InitializeNativeAd(const char *adUnitIdentifierStr, ALUnityNativeAdsCallback callback) {
        if (adLoader == nil) {
            NSString *adUnitIdentifier = [NSString stringWithUTF8String:adUnitIdentifierStr];
            adLoader = [[NativeAdsLoader alloc] initWithAdUnitIdentifier:adUnitIdentifier containerView:UnityGetGLViewController().view andCallback:callback];
        }
    }
    
      extern void _LoadNativeAd() {
            if (adLoader == nil) {
                return;
            }
            
            [adLoader loadAd];
        }

    extern void _ShowNativeAd(const char *viewFormatStr, float x, float y, float width, float height) {
        if (adLoader == nil) {
            return;
        }
        
        NSString *viewFormat = [NSString stringWithUTF8String:viewFormatStr];
        [adLoader showAdView:viewFormat atX:x andY:y withWidth:width andHeight:height];
    }
    
    extern void _HideNativeAd() {
        if (adLoader == nil) {
            return;
        }
        
        [adLoader hideAdView];
    }
    
    extern bool _IsNativeAdAvailable() {
        return adLoader != nil && [adLoader isAdAvailable];
    }

    extern void _SetCustomData(const char *customData) {
    	if (adLoader == nil) {
    		return;
    	}

    	NSString *data = [NSString stringWithUTF8String:customData];
    	[adLoader setCustomData:data];
    }
}
