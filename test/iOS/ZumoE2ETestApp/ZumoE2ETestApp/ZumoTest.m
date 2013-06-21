// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTest.h"

@implementation ZumoTest

@synthesize testName, execution, delegate, testStatus;
@synthesize propertyBag = _propertyBag;

- (id)init {
    self = [super init];
    if (self) {
        [self setTestStatus:TSNotRun];
        logs = [[NSMutableArray alloc] init];
        _propertyBag = [[NSMutableDictionary alloc] init];
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
    steps(self, currentViewController, ^(BOOL testPassed) {
        [weakSelf setTestStatus: (testPassed ? TSPassed : TSFailed)];
        [[weakSelf delegate] zumoTestFinished:[weakSelf testName] withResult:testPassed];
    });
}

- (void)resetStatus {
    testStatus = TSNotRun;
    [logs removeAllObjects];
}

- (void)addLog:(NSString *)text {
    NSString *timestamped = [NSString stringWithFormat:@"[%@] %@", [[NSDate date] description], text];
    [logs addObject:timestamped];
    NSLog(@"%@", timestamped);
}

- (NSArray *)getLogs {
    return [NSArray arrayWithArray:logs];
}

- (NSString *)description {
    NSString *statusName = nil;
    switch ([self testStatus]) {
        case TSFailed:
            statusName = @"Failed";
            break;
            
        case TSPassed:
            statusName = @"Passed";
            break;
            
        case TSRunning:
            statusName = @"Running";
            break;
            
        default:
            statusName = @"NotRun";
            break;
    }
    
    return [NSString stringWithFormat:@"%@ - %@", [self testName], statusName];
}

@end
