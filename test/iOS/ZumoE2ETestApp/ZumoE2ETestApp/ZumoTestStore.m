// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTestStore.h"
#import "ZumoTest.h"
#import "ZumoTestGroup.h"
#import "ZumoRoundTripTests.h"
#import "ZumoQueryTests.h"
#import "ZumoCUDTests.h"
#import "ZumoLoginTests.h"
#import "ZumoMiscTests.h"
#import "ZumoPushTests.h"
#import "ZumoCustomApiTests.h"
#import "ZumoTestRunSetup.h"

NSString * const ALL_TESTS_GROUP_NAME = @"All tests";
NSString * const ALL_UNATTENDED_TESTS_GROUP_NAME = @"All tests (unattended)";

@implementation ZumoTestStore

+ (NSArray *)createTests {
    ZumoTestGroup *setupGroup = [[ZumoTestGroup alloc] init];
    [setupGroup setName:@"Test run setup"];
    NSMutableArray *result = [NSMutableArray arrayWithObjects:
                              [self createGroupWithName:@"Test run setup" description:[ZumoTestRunSetup description] tests:[ZumoTestRunSetup createTests]],
                              [self createGroupWithName:@"Insert and verify" description:[ZumoRoundTripTests description] tests:[ZumoRoundTripTests createTests]],
                              [self createGroupWithName:@"Query" description:[ZumoQueryTests description] tests:[ZumoQueryTests createTests]],
                              [self createGroupWithName:@"Update / Delete" description:[ZumoCUDTests description] tests:[ZumoCUDTests createTests]],
                              [self createGroupWithName:@"Login" description:[ZumoLoginTests description] tests:[ZumoLoginTests createTests]],
                              [self createGroupWithName:@"Push notification" description:[ZumoPushTests description] tests:[ZumoPushTests createTests]],
                              [self createGroupWithName:@"Other" description:[ZumoMiscTests description] tests:[ZumoMiscTests createTests]],
                              [self createGroupWithName:@"Custom API" description:[ZumoCustomApiTests description] tests:[ZumoCustomApiTests createTests]],
            nil];
    
    ZumoTestGroup *allTests = [self createGroupWithAllTestsFromIndividualGroups:result onlyIncludeUnattended:NO];
    ZumoTestGroup *allUnattendedTests = [self createGroupWithAllTestsFromIndividualGroups:result onlyIncludeUnattended:YES];
    [result addObject:allUnattendedTests];
    [result addObject:allTests];
    
    return result;
}

+ (ZumoTestGroup *)createGroupWithName:(NSString *)testGroupName description:(NSString *)description tests:(NSArray *)tests {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:testGroupName];
    [result setGroupDescription:description];
    ZumoTest *test;
    for (test in tests) {
        [result addTest:test];
    }
    
    return result;
}

+ (ZumoTestGroup *)createGroupWithAllTestsFromIndividualGroups:(NSArray *)groups onlyIncludeUnattended:(BOOL)unattendedOnly {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:(unattendedOnly ? ALL_UNATTENDED_TESTS_GROUP_NAME : ALL_TESTS_GROUP_NAME)];
    for (ZumoTestGroup *group in groups) {
        NSString *groupHeaderName = [NSString stringWithFormat:@"Start of group: %@", [group name]];
        [result addTest:[self createSeparatorTestWithName:groupHeaderName]];
        
        for (ZumoTest *test in [group tests]) {
            if ([test canRunUnattended] || !unattendedOnly) {
                ZumoTest *newTest = [[ZumoTest alloc] init];
                [newTest setTestName:[test testName]];
                [newTest setExecution:[test execution]];
                newTest.requiredFeatures = test.requiredFeatures;
                
                [result addTest:newTest];
            }
        }
        
        [result addTest:[self createSeparatorTestWithName:@"------------------"]];
    }
    return result;
}

+ (ZumoTest *)createSeparatorTestWithName:(NSString *)testName {
    return [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        completion(YES);
    }];
}

@end
