#import "MaxFacebookIos.h"

@implementation MaxFacebookIos

#pragma mark - Publicly available C methods

extern "C"
{
    void _setFANConsent(bool userConsent) {
        if (@available(iOS 14.0, *)) {
            if (userConsent) {
                NSLog(@"FAN consent given");
                [FBAdSettings setAdvertiserTrackingEnabled:YES];
            } else {
                NSLog(@"FAN consent not given");
                [FBAdSettings setAdvertiserTrackingEnabled:NO];
            }
        }
    }
    
    void _setCCPADataProcessing(bool enable) {
        if (@available(iOS 14.0, *)) {
            if (enable) {
                NSLog(@"Enable FAN CCPA Data Processing");
                [FBAdSettings setDataProcessingOptions:@[@"LDU"] country:1 state:1000];
            } else{
                NSLog(@"Disable FAN CCPA Data Processing");
                [FBAdSettings setDataProcessingOptions:@[]];
            }
        }
    }
}
@end

