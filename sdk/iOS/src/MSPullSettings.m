// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSPullSettings.h"

#pragma mark * MSPullSettings Implementation

@implementation MSPullSettings

NSInteger const defaultPageSize = 50;

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
    _pageSize = pageSize <= 0 ? defaultPageSize : pageSize;
}

@end

