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

+ (ZumoTest *)createTestWithName:(NSString *)name andExecution:(ZumoTestExecution)steps;

- (void)resetStatus;
- (void)startExecutingFrom:(UIViewController *)currentViewController;
- (void)addLog:(NSString *)text;
- (NSArray *)getLogs;

+ (NSString *)testStatusToString:(TestStatus)status;

@end
