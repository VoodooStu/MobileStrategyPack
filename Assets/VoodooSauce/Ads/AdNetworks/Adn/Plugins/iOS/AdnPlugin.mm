//
// AdnPlugin.mm
// VoodooAdn
//
// Created by Sarra Srairi on 03/10/2022.
//
//

#if __has_include(<VoodooAdn/VoodooAdn.h>) || __has_include("VoodooAdn.h")
#define USE_ADN_FRAMEWORK
#endif

#import <UIKit/UIKit.h>
#ifdef USE_ADN_FRAMEWORK
#include "VoodooAdn/VoodooAdn-Swift.h"
#endif

#define NSSTRING(_X) ( (_X != NULL) ? [NSString stringWithCString: _X encoding: NSStringEncodingConversionAllowLossy].al_stringByTrimmingWhitespace : nil)

#pragma mark - C interface
extern "C" {

  typedef void (*SdkInitCallback)();

  void _AdnSetBidTokenExtraParams(const char* parameters){
    if (@available(iOS 13, *)) {
#ifdef USE_ADN_FRAMEWORK
    NSString *list_string = parameters != NULL ? [NSString stringWithUTF8String:parameters] : nil;
    [AdnSdk setBidRequestParamWithString: list_string];
#endif
      }
}

    void _AdnSubscribeOnSdkInitialized(SdkInitCallback callback){
        if (@available(iOS 13, *)) {
    #ifdef USE_ADN_FRAMEWORK
            [AdnSdk SubscribeOnSdkInitializedWithListner: callback];
    #endif
        }
    }
}
