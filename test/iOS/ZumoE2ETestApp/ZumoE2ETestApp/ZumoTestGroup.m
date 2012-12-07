//
//  ZumoTestGroup.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/7/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoTestGroup.h"

@implementation ZumoTestGroup

@synthesize name, delegate, tests, helpText;

- (id)init {
    self = [super init];
    if (self) {
        [self setTests:[[NSMutableArray alloc] init]];
        runningTests = NO;
        testsFailed = testsPassed = -1;
    }
    
    return self;
}

- (void)addTest:(ZumoTest *)test {
    [[self tests] addObject:test];
    [test setDelegate:self];
}

- (void)startExecutingFrom:(UIViewController *)viewController {
    testsPassed = testsFailed = 0;
    runningTests = YES;
    associatedViewController = viewController;
    [[self delegate] zumoTestGroupStarted:[self name]];
    [self executeNextTest];
}

- (void)executeNextTest {
    int testIndex = testsFailed + testsPassed;
    if (testIndex >= [tests count]) {
        runningTests = NO;
        associatedViewController = nil;
        [[self delegate] zumoTestGroupFinished:[self name] withPassed:testsPassed andFailed:testsFailed];
    } else {
        [[self delegate] zumoTestGroupSingleTestStarted:testIndex];
        ZumoTest *nextTest = [[self tests] objectAtIndex:testIndex];
        [nextTest startExecutingFrom:associatedViewController];
    }
}

- (void)zumoTestStarted:(NSString *)testName {
    NSLog(@"Starting test %@", testName);
}

- (void)zumoTestFinished:(NSString *)testName withResult:(BOOL)testResult {
    [[self delegate] zumoTestGroupSingleTestFinished:(testsPassed + testsFailed) withResult:testResult];
    NSLog(@"Finished test %@: %@", testName, testResult ? @"PASS" : @"FAIL");
    if (testResult) {
        testsPassed++;
    } else {
        testsFailed++;
    }
    
    [self executeNextTest];
}

@end
