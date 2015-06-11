// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSPullSettings.h"

#pragma mark * MSPullSettings Implementation

@implementation MSPullSettings

NSInteger const defaultPageSize = 50;

@synthesize pageSize = _pageSize;

#pragma mark * Initializer method(s)

- (id)init {
    self = [super init];
    if (self) {
        _pageSize = defaultPageSize;
    }
    
    return self;
}

#pragma mark * Accessor method(s)

- (void)setPageSize:(NSInteger)pageSize {
    if (pageSize <= 0) {
        _pageSize = defaultPageSize;
    }
    else {
        _pageSize = pageSize;
    }
}

#pragma mark * Class factory method(s)

+ (MSPullSettings *)default {
    return [MSPullSettings new];
}

@end

