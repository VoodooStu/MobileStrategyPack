//
//  VoodooAppUpdateFramework.h
//  VoodooAppUpdateFramework
//
//  Created by sarra_srairi on 06/08/2020.
//  Copyright © 2020 voodoo. All rights reserved.
//

#import <UIKit/UIKit.h>
#include <VoodooAppUpdateFramework/VoodooAppUpdateFramework-Swift.h>
 
#pragma mark - C interface

 
extern "C" {

// Define the C/C++ request method
typedef void (*RequestAppUpdateCallback)(const int statusAvaiblity, const char* versionTime);
    
typedef void (*openAppleStore);
 
// Set the appropritate delegate methods to iOS / native project
void _setRequestTrackingDelegatesConfiguration(RequestAppUpdateCallback requestDelegate)
    {
      
        [[VoodooAppUpdater shared] requestRequestAppUpdateConfigurationWithDelegate:[[UnityBridge alloc] init]];
         VoodooAppUpdater.fallbackAppUpdateStatusDelegate = requestDelegate;
    }


void _openStoreApple()
{ 
    [[VoodooAppUpdater shared] openAppleStore];
}
 
}
