// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSSDKFeatures.h"

#pragma mark * Telemetry Features String Constants

NSString *const MSFeaturesHeaderName = @"X-ZUMO-FEATURES";
NSString *const MSFeatureCodeApiJson = @"AJ";
NSString *const MSFeatureCodeApiGeneric = @"AG";
NSString *const MSFeatureCodeQueryParameters = @"QS";

@implementation MSSDKFeatures

+(NSString *)httpHeaderForFeatures:(MSFeatures)features {
    if (features == MSFeatureNone) return nil;

    NSMutableArray *allFeatures = [[NSMutableArray alloc] init];
    if (features & MSFeatureApiGeneric) {
        [allFeatures addObject:MSFeatureCodeApiGeneric];
    }
    if (features & MSFeatureApiJson) {
        [allFeatures addObject:MSFeatureCodeApiJson];
    }
    if (features & MSFeatureQueryParameters) {
        [allFeatures addObject:MSFeatureCodeQueryParameters];
    }

    return [allFeatures componentsJoinedByString:@","];
}

@end
