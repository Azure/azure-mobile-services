// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

#pragma mark * Telemetry features definitions

typedef NS_OPTIONS(NSUInteger, MSFeatures) {
    MSFeatureNone              = 0,
    MSFeatureApiJson           = 1 << 0,
    MSFeatureApiGeneric        = 1 << 1,
    MSFeatureQueryParameters   = 1 << 2,
};

extern NSString *const MSFeaturesHeaderName;
extern NSString *const MSFeatureCodeApiJson;
extern NSString *const MSFeatureCodeApiGeneric;
extern NSString *const MSFeatureCodeQueryParameters;


// The |MSSDKFeatures| class defines methods to convert between the
// |MSFeatures| enumeration and the value to be sent in HTTP requests
// with telemetry information to the service.
@interface MSSDKFeatures : NSObject

+(NSString *)httpHeaderForFeatures:(MSFeatures)features;

@end
