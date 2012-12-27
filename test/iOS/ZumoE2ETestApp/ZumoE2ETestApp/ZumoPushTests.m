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

@property (nonatomic, readonly, weak) ZumoTest *test;
@property (nonatomic, readonly, strong) ZumoTestCompletion completion;
@property (nonatomic, readonly, strong) NSDictionary *payload;

@end

@implementation ZumoPushClient

@synthesize test = _test, completion = _completion;

- (id)initForTest:(__weak ZumoTest*)test withPayload:(NSDictionary *)payload waitFor:(NSTimeInterval)seconds withTestCompletion:(ZumoTestCompletion)completion {
    self = [super init];
    if (self) {
        _test = test;
        _completion = completion;
        _payload = [payload copy];
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
    NSDictionary *expectedPushInfo = [self zumoPayloadToApsPayload:_payload];
    if ([self compareExpectedPayload:expectedPushInfo withActual:userInfo]) {
        [_test setTestStatus:TSPassed];
        _completion(YES);
    } else {
        [_test addLog:[NSString stringWithFormat:@"Error, payloads are different. Expected: %@, actual: %@", expectedPushInfo, userInfo]];
        [_test setTestStatus:TSFailed];
        _completion(NO);
    }
}

- (BOOL)compareExpectedPayload:(NSDictionary *)expected withActual:(NSDictionary *)actual {
    BOOL allEqual = YES;
    for (NSString *key in [expected keyEnumerator]) {
        id actualValue = actual[key];
        if (!actualValue) {
            allEqual = NO;
            [_test addLog:[NSString stringWithFormat:@"Key %@ in the expected payload, but not in the push received", key]];
        } else {
            id expectedValue = [expected objectForKey:key];
            if ([actualValue isKindOfClass:[NSDictionary class]] && [expectedValue isKindOfClass:[NSDictionary class]]) {
                // Compare recursively
                if (![self compareExpectedPayload:(NSDictionary *)expectedValue withActual:(NSDictionary *)actualValue]) {
                    [_test addLog:[NSString stringWithFormat:@"Value for key %@ in the expected payload is different than the one on the push received", key]];
                    allEqual = NO;
                }
            } else {
                // Use simple comparison
                if (![expectedValue isEqual:actualValue]) {
                    [_test addLog:[NSString stringWithFormat:@"Value for key %@ in the expected payload (%@) is different than the one on the push received (%@)", key, expectedValue, actualValue]];
                    allEqual = NO;
                }
            }
        }
    }
    
    if (allEqual) {
        for (NSString *key in [actual keyEnumerator]) {
            if (!expected[key]) {
                allEqual = NO;
                [_test addLog:[NSString stringWithFormat:@"Key %@ in the push received, but not in the expected payload", key]];
            }
        }
    }
    
    return allEqual;
}

- (NSDictionary *)zumoPayloadToApsPayload:(NSDictionary *)originalPayload {
    NSMutableDictionary *result = [[NSMutableDictionary alloc] init];
    NSMutableDictionary *aps = [[NSMutableDictionary alloc] init];
    [result setValue:aps forKey:@"aps"];
    id alert = originalPayload[@"alert"];
    if (alert) {
        [aps setValue:alert forKey:@"alert"];
    }
    
    id badge = originalPayload[@"badge"];
    if (badge) {
        [aps setValue:badge forKey:@"badge"];
    }
    
    id sound = originalPayload[@"sound"];
    if (sound) {
        [aps setValue:sound forKey:@"sound"];
    }
    
    NSDictionary *payload = originalPayload[@"payload"];
    if (payload) {
        [result addEntriesFromDictionary:payload];
    }
    
    return result;
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
    [result addObject:[self createPushTestWithName:@"Push alert with loc info and parameters" forPayload:@{@"alert":@{@"loc-key":@"LOC_STRING",@"loc-args":@[@"first",@"second"]}} withDelay:0]];
    [result addObject:[self createPushTestWithName:@"Push with only custom info (no alert / badge / sound)" forPayload:@{@"payload":@{@"foo":@"bar"}} withDelay:0]];
    [result addObject:[self createPushTestWithName:@"Push with alert, badge and sound" forPayload:@{@"alert":@"simple alert", @"badge":@7, @"sound":@"default", @"payload":@{@"custom":@"value"}} withDelay:0]];
    [result addObject:[self createPushTestWithName:@"Push with alert with non-ASCII characters" forPayload:@{@"alert":@"Latin-ãéìôü ÇñÑ, arabic-لكتاب على الطاولة, chinese-这本书在桌子上"} withDelay:0]];
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
