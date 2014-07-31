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
        _requiredFeatures = [[NSMutableArray alloc] init];
    }
    
    return self;
}

+ (ZumoTest *)createTestWithName:(NSString *)name andExecution:(ZumoTestExecution)steps {
    ZumoTest *result = [[ZumoTest alloc] init];
    [result setTestName:name];
    [result setExecution:steps];
    return result;
}

- (void)addRequiredFeature:(NSString *)featureName {
    [self.requiredFeatures addObject:featureName];
}

- (BOOL)shouldBeSkipped {
    NSDictionary *globalTestParams = [[ZumoTestGlobals sharedInstance] globalTestParameters];
    NSDictionary *features = [globalTestParams objectForKey:RUNTIME_FEATURES_KEY];
    if (!features) {
        // By default, do not skip
        return NO;
    }
    
    for (NSString *requiredFeature in self.requiredFeatures) {
        NSNumber *featureEnabled = [features objectForKey:requiredFeature];
        if (!featureEnabled) {
            [self addLog:[NSString stringWithFormat:@"Test requires '%@' feature, but the value wasn't present in the runtime information.", featureEnabled]];
            [self setTestStatus:TSFailed];
            return YES;
        }
        BOOL isEnabled = [featureEnabled boolValue];
        if (!isEnabled) {
            // Test requires feature, but it's not enabled. Skip it.
            return YES;
        }
    }
    
    return NO;
}

- (void)startExecutingFrom:(UIViewController *)currentViewController {
    [[self delegate] zumoTestStarted:[self testName]];
    testStatus = TSRunning;
    ZumoTestExecution steps = [self execution];
    __weak ZumoTest *weakSelf = self;
    [self setStartTime:[NSDate date]];
    if ([self shouldBeSkipped]) {
        [self setEndTime:[NSDate date]];
        if ([self testStatus] == TSRunning) {
            [self setTestStatus:TSSkipped];
        }
        [[self delegate] zumoTestFinished:[self testName] withResult:[self testStatus]];
    } else {
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
