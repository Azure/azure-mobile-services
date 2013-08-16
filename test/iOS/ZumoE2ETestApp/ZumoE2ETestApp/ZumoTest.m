// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTest.h"
#import "ZumoTestGlobals.h"

@implementation ZumoTest

@synthesize testName, execution, delegate, testStatus, startTime, endTime;
@synthesize propertyBag = _propertyBag;
@synthesize canRunUnattended = _canRunUnattended;

- (id)init {
    self = [super init];
    if (self) {
        [self setTestStatus:TSNotRun];
        logs = [[NSMutableArray alloc] init];
        _propertyBag = [[NSMutableDictionary alloc] init];
        _canRunUnattended = YES;
    }
    
    return self;
}

+ (ZumoTest *)createTestWithName:(NSString *)name andExecution:(ZumoTestExecution)steps {
    ZumoTest *result = [[ZumoTest alloc] init];
    [result setTestName:name];
    [result setExecution:steps];
    return result;
}

- (void)startExecutingFrom:(UIViewController *)currentViewController {
    [[self delegate] zumoTestStarted:[self testName]];
    testStatus = TSRunning;
    ZumoTestExecution steps = [self execution];
    __weak ZumoTest *weakSelf = self;
    [self setStartTime:[NSDate date]];
    steps(self, currentViewController, ^(BOOL testPassed) {
        [weakSelf setEndTime:[NSDate date]];
        TestStatus currentStatus = [weakSelf testStatus];
        if (currentStatus != TSSkipped) {
            // if test marked itself as 'skipped', don't set its status.
            currentStatus = testPassed ? TSPassed : TSFailed;
        }
        [weakSelf setTestStatus:currentStatus];
        [[weakSelf delegate] zumoTestFinished:[weakSelf testName] withResult:currentStatus];
    });
}

- (void)resetStatus {
    testStatus = TSNotRun;
    [logs removeAllObjects];
}

- (void)addLog:(NSString *)text {
    NSString *timestamped = [NSString stringWithFormat:@"[%@] %@", [ZumoTestGlobals dateToString:[NSDate date]], text];
    [logs addObject:timestamped];
    NSLog(@"%@", timestamped);
}

- (NSArray *)getLogs {
    return [NSArray arrayWithArray:logs];
}

- (NSString *)description {
    NSString *statusName = [ZumoTest testStatusToString:[self testStatus]];
    return [NSString stringWithFormat:@"%@ - %@", [self testName], statusName];
}

+ (NSString *)testStatusToString:(TestStatus)status {
    NSString *testStatus;
    switch (status) {
        case TSFailed:
            testStatus = @"Failed";
            break;
            
        case TSPassed:
            testStatus = @"Passed";
            break;
            
        case TSNotRun:
            testStatus = @"NotRun";
            break;
            
        case TSRunning:
            testStatus = @"Running";
            break;
            
        case TSSkipped:
            testStatus = @"Skipped";
            break;
            
        default:
            testStatus = @"Unkonwn";
            break;
    }
    
    return testStatus;
}

@end
