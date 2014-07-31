// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "ZumoTestCallbacks.h"

// Forward decoration
@class ZumoTest;

typedef void (^ZumoTestCompletion)(BOOL testPassed);
typedef void (^ZumoTestExecution)(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion);

@interface ZumoTest : NSObject
{
    NSMutableArray *logs;
}

@property (nonatomic, weak) id<ZumoTestCallbacks> delegate;

@property (nonatomic, strong) NSString *testName;
@property (nonatomic, copy) ZumoTestExecution execution;
@property (nonatomic) TestStatus testStatus;
@property (nonatomic, strong) NSMutableDictionary *propertyBag;
@property (nonatomic) BOOL canRunUnattended;
@property (nonatomic, copy) NSDate *startTime;
@property (nonatomic, copy) NSDate *endTime;
@property (nonatomic, strong) NSMutableArray *requiredFeatures;

+ (ZumoTest *)createTestWithName:(NSString *)name andExecution:(ZumoTestExecution)steps;

- (void)resetStatus;
- (void)startExecutingFrom:(UIViewController *)currentViewController;
- (void)addLog:(NSString *)text;
- (NSArray *)getLogs;
- (void)addRequiredFeature:(NSString *)featureName;

+ (NSString *)testStatusToString:(TestStatus)status;

@end
