//
//  NativeAdsLoader.h
//  Unity-iPhone
//
//  Created by Safouane Azzabi on 06/05/2022.
//

#import <Foundation/Foundation.h>
#import <AppLovinSDK/AppLovinSDK.h>

NS_ASSUME_NONNULL_BEGIN

typedef void (*ALUnityNativeAdsCallback)(const char* args);

@interface NativeAdsLoader : NSObject<MANativeAdDelegate, MAAdRevenueDelegate>

@end

NS_ASSUME_NONNULL_END
