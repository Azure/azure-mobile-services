//
//  ZumoTestStore.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/9/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoTestStore.h"
#import "ZumoTest.h"
#import "ZumoTestGroup.h"
#import "ZumoRoundTripTests.h"
#import "ZumoQueryTests.h"
#import "ZumoCUDTests.h"
#import "ZumoLoginTests.h"
#import "ZumoMiscTests.h"

@implementation ZumoTestStore

+ (NSArray *)createTests {
    return [NSArray arrayWithObjects:
            [self createInsertAndVerifyTests],
            [self createQueryTests],
            [self createCUDTests],
            [self createLoginTests],
            [self createMiscTests],
            nil];
}

+ (ZumoTestGroup *)createMiscTests {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:@"Other tests"];
    [result setHelpText:[ZumoMiscTests helpText]];
    NSArray *tests = [ZumoMiscTests createTests];
    ZumoTest *test;
    for (test in tests) {
        [result addTest:test];
    }
    
    return result;
}

+ (ZumoTestGroup *)createLoginTests {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:@"Login tests"];
    [result setHelpText:[ZumoLoginTests helpText]];
    NSArray *tests = [ZumoLoginTests createTests];
    ZumoTest *test;
    for (test in tests) {
        [result addTest:test];
    }
    
    return result;
}

+ (ZumoTestGroup *)createCUDTests {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:@"Update / Delete"];
    [result setHelpText:[ZumoCUDTests helpText]];
    
    NSArray *tests = [ZumoCUDTests createTests];
    ZumoTest *test;
    for (test in tests) {
        [result addTest:test];
    }
    
    return result;
}

+ (ZumoTestGroup *)createInsertAndVerifyTests {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:@"Insert and verify"];
    [result setHelpText:[ZumoRoundTripTests helpText]];
    
    NSArray *tests = [ZumoRoundTripTests createTests];
    ZumoTest *test;
    for (test in tests) {
        [result addTest:test];
    }
    
    return result;
}

+ (ZumoTestGroup *)createQueryTests {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:@"Query"];
    [result setHelpText:[ZumoQueryTests helpText]];
    NSArray *tests = [ZumoQueryTests createTests];
    ZumoTest *test;
    for (test in tests) {
        [result addTest:test];
    }
    
    return result;
}

@end
