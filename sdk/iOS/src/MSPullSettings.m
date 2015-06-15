// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSPullSettings.h"

#pragma mark * MSPullSettings Implementation

@implementation MSPullSettings

static NSInteger const _defaultPageSize = 50;

#pragma mark * Initializer method(s)

- (id)init {
    self = [super init];
    if (self) {
        _pageSize = _defaultPageSize;
    }
    
    return self;
}

#pragma mark * Accessor method(s)

- (void)setPageSize:(NSInteger)pageSize {
    _pageSize = pageSize > 0 ? pageSize : _defaultPageSize;
}

#pragma mark * Internal method(s)

+ (NSInteger)defaultPageSize {
    return _defaultPageSize;
}

@end

