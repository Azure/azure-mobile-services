//
//  ZumoTestCallbacks.h
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import <Foundation/Foundation.h>

@protocol ZumoTestCallbacks <NSObject>

@required
- (void)zumoTestStarted:(NSString *)testName;
- (void)zumoTestFinished:(NSString *)testName withResult:(BOOL)testResult;

@end
