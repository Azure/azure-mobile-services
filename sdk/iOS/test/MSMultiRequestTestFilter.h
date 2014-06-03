// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSTestFilter.h"

@interface MSMultiRequestTestFilter : NSObject <MSFilter>

@property (nonatomic) NSArray *testFilters;
@property (nonatomic) NSUInteger currentIndex;

@end
