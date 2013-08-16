// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "ZumoTestCallbacks.h"

@protocol ZumoTestGroupCallbacks <NSObject>

@required

- (void)zumoTestGroupStarted:(NSString *)groupName;
- (void)zumoTestGroupFinished:(NSString *)groupName withPassed:(int)passedTests andFailed:(int)failedTests andSkipped:(int)skippedTests;
- (void)zumoTestGroupSingleTestStarted:(int)testIndex;
- (void)zumoTestGroupSingleTestFinished:(int)testIndex withResult:(TestStatus)testStatus;

@end
