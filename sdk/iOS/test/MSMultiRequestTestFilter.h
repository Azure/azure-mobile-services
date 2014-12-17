// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSTestFilter.h"

@interface MSMultiRequestTestFilter : NSObject <MSFilter>

@property (nonatomic) NSArray *testFilters;
@property (nonatomic) NSArray *actualRequests;
@property (nonatomic) NSUInteger currentIndex;

+(MSMultiRequestTestFilter *) testFilterWithStatusCodes:(NSArray *)statusCodes data:(NSArray *)data appendEmptyRequest:(BOOL)appendEmptyRequest;

@end
