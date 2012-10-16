// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import "MSUserAgentBuilder.h"
#import <UIKit/UIKit.h>


#pragma mark * User Agent Header String Constants


NSString *const sdkVersion = @"1.0";
NSString *const sdkLanguage = @"objective-c";
NSString *const userAgentValueFormat = @"ZUMO/%@ (%@ %@ %@ %@) %@/%@";
NSString *const simulatorModel = @"iOSSimulator";
NSString *const unknownValue = @"--";


#pragma mark * MSUserAgentBuilder Implementation


@implementation MSUserAgentBuilder


#pragma  mark * Public UserAgent Method


+(NSString *) userAgent
{
    NSString *model = nil;
    NSString *OS = nil;
    NSString *OSversion = nil;
    NSString *appName = nil;
    NSString *appVersion = nil;
    
    // Get the device related info
#if TARGET_IPHONE_SIMULATOR
    model = simulatorModel;
    OS = unknownValue;
    OSversion = unknownValue;
#else
    UIDevice *currentDevice = [UIDevice currentDevice];
    model = [currentDevice model];
    OS = [currentDevice systemName];
    OSversion = [currentDevice systemVersion];
#endif
    
    // Get the app related info
    NSDictionary *bundleInfo = [[NSBundle mainBundle] infoDictionary];
    
    appName = [bundleInfo objectForKey:(NSString*)kCFBundleNameKey];
    if (!appName) {
        appName = unknownValue;
    }
    
    appVersion = [bundleInfo objectForKey:(NSString*)kCFBundleVersionKey];
    if (!appVersion) {
        appVersion = unknownValue;
    }
    
    // Build the user agent string
    NSString *userAgent = [NSString stringWithFormat:userAgentValueFormat,
                           sdkVersion,
                           model,
                           OS,
                           OSversion,
                           sdkLanguage,
                           appName,
                           appVersion];
    
    return userAgent;
}

@end
