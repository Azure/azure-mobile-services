// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTestGroup.h"

@implementation ZumoTestGroup

@synthesize name, delegate, tests, groupDescription;
@synthesize testsFailed = _testsFailed, testsPassed = _testsPassed;

- (id)init {
    self = [super init];
    if (self) {
        [self setTests:[[NSMutableArray alloc] init]];
        runningTests = NO;
        _testsFailed = _testsPassed = -1;
    }
    
    return self;
}

- (void)addTest:(ZumoTest *)test {
    [[self tests] addObject:test];
    [test setDelegate:self];
}

- (void)startExecutingFrom:(UIViewController *)viewController {
    _testsPassed = _testsFailed = 0;
    runningTests = YES;
    associatedViewController = viewController;
    [[self delegate] zumoTestGroupStarted:[self name]];
    [self executeNextTest];
}

- (void)executeNextTest {
    int testIndex = _testsFailed + _testsPassed;
    if (testIndex >= [tests count]) {
        runningTests = NO;
        associatedViewController = nil;
        [[self delegate] zumoTestGroupFinished:[self name] withPassed:_testsPassed andFailed:_testsFailed];
    } else {
        ZumoTest *nextTest = [[self tests] objectAtIndex:testIndex];
        [nextTest startExecutingFrom:associatedViewController];
    }
}

- (void)zumoTestStarted:(NSString *)testName {
    [[self delegate] zumoTestGroupSingleTestStarted:(_testsFailed + _testsPassed)];
    NSLog(@"Starting test %@", testName);
}

- (void)zumoTestFinished:(NSString *)testName withResult:(BOOL)testResult {
    [[self delegate] zumoTestGroupSingleTestFinished:(_testsPassed + _testsFailed) withResult:testResult];
    NSLog(@"Finished test %@: %@", testName, testResult ? @"PASS" : @"FAIL");
    if (testResult) {
        _testsPassed++;
    } else {
        _testsFailed++;
    }
    
    [self executeNextTest];
}

@end
