// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


#import <Foundation/Foundation.h>

@interface MSTestWaiter : NSObject

@property (nonatomic) BOOL done;

-(BOOL) waitForTest:(NSTimeInterval)testDuration;

@end
