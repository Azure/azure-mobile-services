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

@implementation ZumoTestStore

+ (NSArray *)createTests {
    return [NSArray arrayWithObjects:
            [self createInsertAndVerifyTests],
            [self createQueryTests],
            [self createCUDTests],
            [self createLoginTests],
            [self createPushTests],
            [self createMiscTests],
            [self createCustomApiTests],
            nil];
}

+ (ZumoTestGroup *)createCustomApiTests {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:@"Custom API tests"];
    [result setGroupDescription:[ZumoCustomApiTests description]];
    NSArray *tests = [ZumoCustomApiTests createTests];
    ZumoTest *test;
    for (test in tests) {
        [result addTest:test];
    }
    
    return result;
}

+ (ZumoTestGroup *)createMiscTests {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:@"Other tests"];
    [result setGroupDescription:[ZumoMiscTests description]];
    NSArray *tests = [ZumoMiscTests createTests];
    ZumoTest *test;
    for (test in tests) {
        [result addTest:test];
    }
    
    return result;
}

+ (ZumoTestGroup *)createPushTests {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:@"Push notification tests"];
    [result setGroupDescription:[ZumoPushTests description]];
    NSArray *tests = [ZumoPushTests createTests];
    ZumoTest *test;
    for (test in tests) {
        [result addTest:test];
    }
    
    return result;
}

+ (ZumoTestGroup *)createLoginTests {
    ZumoTestGroup *result = [[ZumoTestGroup alloc] init];
    [result setName:@"Login tests"];
    [result setGroupDescription:[ZumoLoginTests description]];
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
    [result setGroupDescription:[ZumoCUDTests description]];
    
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
    [result setGroupDescription:[ZumoRoundTripTests description]];
    
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
    [result setGroupDescription:[ZumoQueryTests description]];
    NSArray *tests = [ZumoQueryTests createTests];
    ZumoTest *test;
    for (test in tests) {
        [result addTest:test];
    }
    
    return result;
}

@end
