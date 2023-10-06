//
//  MaxNativeAdViewOptions.m
//  Unity-iPhone
//
//  Created by Cyril Gy on 16/05/2022.
//
#import "MaxNativeAdViewOptions.h"

//pragma MARK: - MaxNativeAdViewOptions Interface

@implementation MaxNativeAdViewOptions

@synthesize x;
@synthesize y;
@synthesize width;
@synthesize height;
@synthesize backgroundColor;
@synthesize titleColor;
@synthesize bodyColor;
@synthesize buttonBackgroundColor;
@synthesize buttonTextColor;
@synthesize advertiserColor;

- (id)initWithData:(NSData *)data {
    if (self = [super init]) {
        
    }
    
    return self;
}

@end
