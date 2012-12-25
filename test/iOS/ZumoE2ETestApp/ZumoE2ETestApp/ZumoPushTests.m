//
//  ZumoPushTests.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/23/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoPushTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"

// Helper class which will receive the push requests, and call a callback either
// after a timer ends or after a push notification is received.
@interface ZumoPushClient : NSObject <PushNotificationReceiver>
{
    NSTimer *timer;
}

@property (nonatomic, weak) ZumoTest *test;
@property (nonatomic, strong) ZumoTestCompletion completion;
@property (nonatomic, strong) NSDictionary *payload;

@end

@implementation ZumoPushClient

@synthesize test = _test, completion = _completion;

- (id)initForTest:(__weak ZumoTest*)test withPayload:(NSDictionary *)payload waitFor:(NSTimeInterval)seconds withTestCompletion:(ZumoTestCompletion)completion {
    self = [super init];
    if (self) {
        _test = test;
        _completion = completion;
        timer = [NSTimer scheduledTimerWithTimeInterval:seconds target:self selector:@selector(timerFired:) userInfo:nil repeats:NO];
        [[ZumoTestGlobals sharedInstance] setPushNotificationDelegate:self];
    }
    
    return self;
}

- (void)timerFired:(NSTimer *)theTimer {
    [_test addLog:@"Push notification not received within the allowed time. Need to retry?"];
    [_test setTestStatus:TSFailed];
    _completion(NO);
}

- (void)pushReceived:(NSDictionary *)userInfo {
    [timer invalidate];
    [_test addLog:[NSString stringWithFormat:@"Push notification received: %@", userInfo]];
    // PPP compare data received with payload passed to it
    [_test setTestStatus:TSPassed];
    _completion(YES);
}

@end

// Main implementation
@implementation ZumoPushTests

static NSString *tableName = @"iosPushTest";
static NSString *pushClientKey = @"PushClientKey";

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    [result addObject:[self createValidatePushRegistrationTest]];
    [result addObject:[self createFeedbackTest]];
    [result addObject:[self createPushTestWithName:@"Push simple alert" forPayload:@{@"alert":@"push received"} withDelay:0]];
    [result addObject:[self createPushTestWithName:@"Push simple badge" forPayload:@{@"badge":@9} withDelay:0]];
    [result addObject:[self createPushTestWithName:@"Push simple sound and alert" forPayload:@{@"alert":@"push received",@"sound":@"default"} withDelay:0]];
    return result;
}

+ (NSString *)helpText {
    NSArray *lines = [NSArray arrayWithObjects:
                      @"1. Create an application on Windows azure portal.",
                      @"2. Create a table called 'iOSPushTest'.",
                      @"3. Set the appropriate script on the table",
                      @"4. Make sure all the tests pass.",
                      nil];
    return [lines componentsJoinedByString:@"\n"];
}

+ (ZumoTest *)createValidatePushRegistrationTest {
    ZumoTest *result = [[ZumoTest alloc] init];
    [result setTestName:@"Validate push registration"];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        ZumoTestGlobals *globals = [ZumoTestGlobals sharedInstance];
        [weakRef addLog:[globals remoteNotificationRegistrationStatus]];
        if ([globals deviceToken]) {
            [weakRef addLog:[NSString stringWithFormat:@"Device token: %@", [globals deviceToken]]];
            [weakRef setTestStatus:TSPassed];
            completion(YES);
        } else {
            [weakRef setTestStatus:TSFailed];
            completion(NO);
        }
    }];
    
    return result;
}

+ (ZumoTest *)createPushTestWithName:(NSString *)name forPayload:(NSDictionary *)payload withDelay:(int)seconds {
    ZumoTest *result = [[ZumoTest alloc] init];
    [result setTestName:name];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        NSString *deviceToken = [[ZumoTestGlobals sharedInstance] deviceToken];
        if (!deviceToken) {
            [weakRef addLog:@"Device not correctly registered for push"];
            [weakRef setTestStatus:TSFailed];
            completion(NO);
        } else {
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            MSTable *table = [client getTable:tableName];
            NSDictionary *item = @{@"method" : @"send", @"payload" : payload, @"token": deviceToken, @"delay": @(seconds)};
            [table insert:item completion:^(NSDictionary *item, NSError *error) {
                if (error) {
                    [weakRef addLog:[NSString stringWithFormat:@"Error requesting push: %@", error]];
                    [weakRef setTestStatus:TSFailed];
                    completion(NO);
                } else {
                    NSTimeInterval timeToWait = 5;
                    ZumoPushClient *pushClient = [[ZumoPushClient alloc] initForTest:weakRef withPayload:payload waitFor:timeToWait withTestCompletion:completion];
                    [[weakRef propertyBag] setValue:pushClient forKey:pushClientKey];
                    
                    // completion will be called on the push client...
                }
            }];
        }
    }];
    
    return result;
}

+ (ZumoTest *)createFeedbackTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Simple feedback test" andExecution:nil];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        if (![[ZumoTestGlobals sharedInstance] deviceToken]) {
            [weakRef addLog:@"Device not correctly registered for push"];
            [weakRef setTestStatus:TSFailed];
            completion(NO);
        } else {
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            MSTable *table = [client getTable:tableName];
            NSDictionary *item = @{@"method" : @"getFeedback"};
            [table insert:item completion:^(NSDictionary *item, NSError *error) {
                BOOL passed = NO;
                if (error) {
                    [weakRef addLog:[NSString stringWithFormat:@"Error requesting feedback: %@", error]];
                } else {
                    NSArray *devices = item[@"devices"];
                    if (devices) {
                        [weakRef addLog:[NSString stringWithFormat:@"Retrieved devices from feedback script: %@", devices]];
                        passed = YES;
                    } else {
                        [weakRef addLog:[NSString stringWithFormat:@"No 'devices' field in response: %@", item]];
                    }
                }
                
                [weakRef setTestStatus:(passed ? TSPassed : TSFailed)];
                completion(passed);
            }];
        }
    }];
    
    return result;
}

@end
