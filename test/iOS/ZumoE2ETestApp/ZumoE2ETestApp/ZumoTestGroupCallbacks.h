//
//  ZumoTestGroupCallbacks.h
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import <Foundation/Foundation.h>

@protocol ZumoTestGroupCallbacks <NSObject>

@required

- (void)zumoTestGroupStarted:(NSString *)groupName;
- (void)zumoTestGroupFinished:(NSString *)groupName withPassed:(int)passedTests andFailed:(int)failedTests;
- (void)zumoTestGroupSingleTestStarted:(int)testIndex;
- (void)zumoTestGroupSingleTestFinished:(int)testIndex withResult:(BOOL)testPassed;

@end
