//
//  VoodooAppUpdateFramework.h
//  VoodooAppUpdateFramework
//
//  Created by sarra_srairi on 06/08/2020.
//  Copyright © 2020 voodoo. All rights reserved.
//

#import <Foundation/Foundation.h>

//! Project version number for VoodooAppUpdateFramework.
FOUNDATION_EXPORT double VoodooAppUpdateFrameworkVersionNumber;

//! Project version string for VoodooAppUpdateFramework.
FOUNDATION_EXPORT const unsigned char VoodooAppUpdateFrameworkVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <VoodooAppUpdateFramework/PublicHeader.h>

typedef void (*RequestAppUpdateCallback)(const int statusAvaiblity, const char* versionTime);

typedef void (*openAppleStore);

