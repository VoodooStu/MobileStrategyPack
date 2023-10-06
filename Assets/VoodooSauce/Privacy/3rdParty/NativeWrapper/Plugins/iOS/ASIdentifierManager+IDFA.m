//
//  ASIdentifierManager+IDFA.m
//  Unity-iPhone
//
//  Created by Safouane Azzabi on 17/09/2020.
//

#import "ASIdentifierManager+IDFA.h"

@implementation ASIdentifierManager (IDFA)

-(BOOL)isAdvertisingTrackingEnabled {
    NSString *kLatIdfa = @"00000000-0000-0000-0000-000000000000";

    // we can't use ASIdentifierManager.sharedManager.isAdvertisingTrackingEnabled in iOS 14.0 and newer as it's deprecated (will always return false)
    // this is why we rely on a zeroed IDFA to know whether LAT is active
    bool isAdvertisingTrackingEnabled = ![kLatIdfa isEqualToString:ASIdentifierManager.sharedManager.advertisingIdentifier.UUIDString];
    return isAdvertisingTrackingEnabled;
}

@end
