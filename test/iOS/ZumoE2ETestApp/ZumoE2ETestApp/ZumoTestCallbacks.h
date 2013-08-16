// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

typedef enum { TSNotRun, TSRunning, TSSkipped, TSFailed, TSPassed } TestStatus;

@protocol ZumoTestCallbacks <NSObject>

@required
- (void)zumoTestStarted:(NSString *)testName;
- (void)zumoTestFinished:(NSString *)testName withResult:(TestStatus)testResult;

@end
