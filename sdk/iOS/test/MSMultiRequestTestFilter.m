// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSMultiRequestTestFilter.h"

@implementation MSMultiRequestTestFilter

-(MSMultiRequestTestFilter *) init {
    self = [super init];
    if (self)
    {
        _testFilters = [NSArray array];
        _currentIndex = 0;
    }
    
    return self;
}


-(void) handleRequest:(NSURLRequest *)request
                 next:(MSFilterNextBlock)next
             response:(MSFilterResponseBlock)response
{
    MSTestFilter *currentFilter = self.testFilters[self.currentIndex];
    self.currentIndex++;
    
    [currentFilter handleRequest:request next:next response:response];
}

@end