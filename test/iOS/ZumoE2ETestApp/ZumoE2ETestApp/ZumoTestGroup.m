// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTestGroup.h"

@implementation ZumoTestGroup

@synthesize name, delegate, tests, groupDescription;
@synthesize testsFailed = _testsFailed, testsPassed = _testsPassed, testsSkipped = _testsSkipped;
@synthesize startTime, endTime;

- (id)init {
    self = [super init];
    if (self) {
        [self setTests:[[NSMutableArray alloc] init]];
        runningTests = NO;
        _testsFailed = _testsPassed = _testsSkipped = -1;
    }
    
    return self;
}

- (void)addTest:(ZumoTest *)test {
    [[self tests] addObject:test];
    [test setDelegate:self];
}

- (void)startExecutingFrom:(UIViewController *)viewController {
    _testsPassed = _testsFailed = _testsSkipped = 0;
    [self setStartTime:[NSDate date]];
    runningTests = YES;
    associatedViewController = viewController;
    [[self delegate] zumoTestGroupStarted:[self name]];
    [self executeNextTest];
}

- (void)executeNextTest {
    int testIndex = _testsFailed + _testsPassed + _testsSkipped;
    if (testIndex >= [tests count]) {
        runningTests = NO;
        [self setEndTime:[NSDate date]];
        associatedViewController = nil;
        [[self delegate] zumoTestGroupFinished:[self name] withPassed:_testsPassed andFailed:_testsFailed andSkipped:_testsSkipped];
    } else {
        ZumoTest *nextTest = [[self tests] objectAtIndex:testIndex];
        [nextTest startExecutingFrom:associatedViewController];
    }
}

- (void)zumoTestStarted:(NSString *)testName {
    [[self delegate] zumoTestGroupSingleTestStarted:(_testsFailed + _testsPassed + _testsSkipped)];
    NSLog(@"Starting test %@", testName);
}

- (void)zumoTestFinished:(NSString *)testName withResult:(TestStatus)testResult {
    [[self delegate] zumoTestGroupSingleTestFinished:(_testsPassed + _testsFailed + _testsSkipped) withResult:testResult];
    NSString *testResultStr = (testResult == TSPassed) ? @"PASS" : (testResult == TSSkipped ? @"SKIP" : @"FAIL");
    NSLog(@"Finished test %@: %@", testName, testResultStr);
    if (testResult == TSPassed) {
        _testsPassed++;
    } else if (testResult == TSSkipped) {
        _testsSkipped++;
    } else {
        _testsFailed++;
    }
    
    [self executeNextTest];
}

@end
