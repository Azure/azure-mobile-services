// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


#import "MSTestWaiter.h"
#import <Foundation/Foundation.h>

@implementation MSTestWaiter

-(id) init {
    self = [super init];
    
    if (self) {
        _done = NO;
    }
    
    return self;
}

-(BOOL) waitForTest:(NSTimeInterval)testDuration {
    
    NSDate *timeoutAt = [NSDate dateWithTimeIntervalSinceNow:testDuration];
    
    while (!self.done) {
        [[NSRunLoop currentRunLoop] runMode:NSRunLoopCommonModes
                                 beforeDate:timeoutAt];
        if([timeoutAt timeIntervalSinceNow] <= 0.0) {
            break;
        }
    };
    
    return self.done;
}

@end