//
//  NativeAppStore.m
//  NativeAppStore
//
//  Created by Voodoo on 23/11/2017.
//  Copyright © 2017 Voodoo. All rights reserved.
//

#import "NativeAppStore.h"

@implementation NativeAppStore

-(void)loadStoreProduct:(NSInteger)itemId {
    if ([SKStoreProductViewController class]) {
        
        NSDictionary *parameters = @{SKStoreProductParameterITunesItemIdentifier: @(itemId)};
        self.productId = itemId;
        self.productViewController = [[SKStoreProductViewController alloc] init];
        [self.productViewController setDelegate:self];
       
        [self.productViewController loadProductWithParameters:parameters
                                              completionBlock:^(BOOL result, NSError *error) {
            self.error = false;
            if (error) {
                self.error = true;
                NSLog(@"loadStoreProduct ERROR : result: %@ error: %@", (result ? @"YES" : @"NO"), error);
            }
        }];
    }
}

- (void)openStoreProduct:(NSInteger)itemId {
    if ([SKStoreProductViewController class]) {
        UIViewController *rootViewController = [UIApplication sharedApplication].keyWindow.rootViewController;
        UIViewController *presentedViewController = rootViewController.presentedViewController;
        
        if (![presentedViewController isKindOfClass:[SKStoreProductViewController class]]) {
            UnityGetGLViewController().modalPresentationStyle = UIModalPresentationFullScreen;
            
            if (@available(iOS 13.0, *)) {
                [UnityGetGLViewController() setModalInPresentation:true];
            }
            
            if (self.productViewController) {
                [UnityGetGLViewController() presentViewController:self.productViewController animated:YES completion: nil];
            } else {
                [self loadStoreProduct:itemId];
            }
        }
    } else {
        NSString *url = [NSString stringWithFormat:@"https://apps.apple.com/app/id%ld", (long)self.productId];
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:url] options:@{} completionHandler:nil];
    }
}

#pragma mark - SKStoreProductViewControllerDelegate

- (void)productViewControllerDidFinish:(SKStoreProductViewController *)viewController {
    [self.productViewController dismissViewControllerAnimated:YES completion: nil];
    if (self.error)
        UnitySendMessage("CrossPromoGameObject", "AppstoreClosed", "error");
    else
        UnitySendMessage("CrossPromoGameObject", "AppstoreClosed", "");
}

@end

static NativeAppStore *nativeAppstorePlugin = nil;

void _loadNativeStore(long integer) {
    NSInteger appId = integer;

    if (nativeAppstorePlugin == nil) {
        nativeAppstorePlugin = [[NativeAppStore alloc] init];
    }
    [nativeAppstorePlugin loadStoreProduct:appId];
}

void _openNativeStore(long integer) {
    NSInteger appId = integer;
    
    if (nativeAppstorePlugin == nil) {
        nativeAppstorePlugin = [[NativeAppStore alloc] init];
    }
    [nativeAppstorePlugin openStoreProduct:appId];
}