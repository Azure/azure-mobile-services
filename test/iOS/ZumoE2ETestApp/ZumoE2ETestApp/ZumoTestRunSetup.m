//
//  ZumoTestRunSetup.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 2/6/14.
//  Copyright (c) 2014 Microsoft. All rights reserved.
//

#import "ZumoTestRunSetup.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"

@implementation ZumoTestRunSetup

+ (NSArray *)createTests {
    ZumoTest *test = [ZumoTest createTestWithName:@"Retrieve runtime features" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        [client invokeAPI:@"runtimeInfo" body:nil HTTPMethod:@"GET" parameters:nil headers:nil completion:^(id result, NSHTTPURLResponse *response, NSError *error) {
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error retrieving runtime features: %@", error]];
                completion(NO);
            } else {
                NSDictionary *runtimeInfo = result;
                [test addLog:[NSString stringWithFormat:@"Runtime features: %@", runtimeInfo]];
                NSDictionary *features = [runtimeInfo objectForKey:@"features"];
                NSDictionary *globalTestParams = [[ZumoTestGlobals sharedInstance] globalTestParameters];
                [globalTestParams setValue:features forKey:RUNTIME_FEATURES_KEY];
                completion(YES);
            }
        }];
    }];

    return @[test];
}

+ (NSString *)groupDescription {
    return @"Run before other tests to check the features enabled in the runtime.";
}

@end
