// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

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
    if (_payload) {
        [_test addLog:@"Push notification not received within the allowed time. Need to retry?"];
        [_test setTestStatus:TSFailed];
        _completion(NO);
    } else {
        [_test addLog:@"Push notification not received for invalid payload - success."];
        [_test setTestStatus:TSPassed];
        _completion(YES);
    }
}

- (void)pushReceived:(NSDictionary *)userInfo {
    [timer invalidate];
    [_test addLog:[NSString stringWithFormat:@"Push notification received: %@", userInfo]];
    if (_payload) {
        NSDictionary *expectedPushInfo = [self zumoPayloadToApsPayload:_payload];
        if ([self compareExpectedPayload:expectedPushInfo withActual:userInfo]) {
            [_test setTestStatus:TSPassed];
            _completion(YES);
        } else {
            [_test addLog:[NSString stringWithFormat:@"Error, payloads are different. Expected: %@, actual: %@", expectedPushInfo, userInfo]];
            [_test setTestStatus:TSFailed];
            _completion(NO);
        }
    } else {
        [_test addLog:@"This is a negative test, the payload should not have been received!"];
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
    if ([self isRunningOnSimulator]) {
        [result addObject:[ZumoTest createTestWithName:@"No push on simulator" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
            [test addLog:@"Running on a simulator, no push tests can be executed"];
            [test setTestStatus:TSPassed];
            completion(YES);
        }]];
    } else {
        [result addObject:[self createValidatePushRegistrationTest]];
        [result addObject:[self createFeedbackTest]];
        [result addObject:[self createPushTestWithName:@"Push simple alert" forPayload:@{@"alert":@"push received"} withDelay:0]];
        [result addObject:[self createPushTestWithName:@"Push simple badge" forPayload:@{@"badge":@9} withDelay:0]];
        [result addObject:[self createPushTestWithName:@"Push simple sound and alert" forPayload:@{@"alert":@"push received",@"sound":@"default"} withDelay:0]];
        [result addObject:[self createPushTestWithName:@"Push alert with loc info and parameters" forPayload:@{@"alert":@{@"loc-key":@"LOC_STRING",@"loc-args":@[@"first",@"second"]}} withDelay:0]];
        [result addObject:[self createPushTestWithName:@"Push with only custom info (no alert / badge / sound)" forPayload:@{@"payload":@{@"foo":@"bar"}} withDelay:0]];
        [result addObject:[self createPushTestWithName:@"Push with alert, badge and sound" forPayload:@{@"alert":@"simple alert", @"badge":@7, @"sound":@"default", @"payload":@{@"custom":@"value"}} withDelay:0]];
        [result addObject:[self createPushTestWithName:@"Push with alert with non-ASCII characters" forPayload:@{@"alert":@"Latin-ãéìôü ÇñÑ, arabic-لكتاب على الطاولة, chinese-这本书在桌子上"} withDelay:0]];
    
        [result addObject:[self createPushTestWithName:@"(Neg) Push with large payload" forPayload:@{@"alert":[@"" stringByPaddingToLength:256 withString:@"*" startingAtIndex:0]} withDelay:0 isNegativeTest:YES]];
    }
    
    return result;
}

+ (BOOL)isRunningOnSimulator {
    NSString *deviceModel = [[UIDevice currentDevice] model];
    if ([deviceModel rangeOfString:@"Simulator" options:NSCaseInsensitiveSearch].location == NSNotFound) {
        return NO;
    } else {
        return YES;
    }
}

+ (ZumoTest *)createValidatePushRegistrationTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Validate push registration" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        if ([self isRunningOnSimulator]) {
            [test addLog:@"Test running on a simulator, skipping test."];
            [test setTestStatus:TSSkipped];
            completion(YES);
            return;
        }
        
        ZumoTestGlobals *globals = [ZumoTestGlobals sharedInstance];
        [test addLog:[globals remoteNotificationRegistrationStatus]];
        if ([globals deviceToken]) {
            [test addLog:[NSString stringWithFormat:@"Device token: %@", [globals deviceToken]]];
            [test setTestStatus:TSPassed];
            completion(YES);
        } else {
            UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Error" message:@"Push tests will not work on the emulator; if this is the case, all subsequent tests will fail, and that's expected." delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
            [av show];
            [test setTestStatus:TSFailed];
            completion(NO);
        }
    }];
    
    return result;
}

+ (ZumoTest *)createPushTestWithName:(NSString *)name forPayload:(NSDictionary *)payload withDelay:(int)seconds {
    return [self createPushTestWithName:name forPayload:payload withDelay:0 isNegativeTest:NO];
}

+ (ZumoTest *)createPushTestWithName:(NSString *)name forPayload:(NSDictionary *)payload withDelay:(int)seconds isNegativeTest:(BOOL)isNegative {
    ZumoTest *result = [ZumoTest createTestWithName:name andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        if ([self isRunningOnSimulator]) {
            [test addLog:@"Test running on a simulator, skipping test."];
            [test setTestStatus:TSSkipped];
            completion(YES);
            return;
        }

        NSString *deviceToken = [[ZumoTestGlobals sharedInstance] deviceToken];
        if (!deviceToken) {
            [test addLog:@"Device not correctly registered for push"];
            [test setTestStatus:TSFailed];
            completion(NO);
        } else {
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            MSTable *table = [client tableWithName:tableName];
            NSURL *appUrl = [client applicationURL];
            [test addLog:[NSString stringWithFormat:@"Sending a request to %@ / table %@", [appUrl description], tableName]];
            NSDictionary *item = @{@"method" : @"send", @"payload" : payload, @"token": deviceToken, @"delay": @(seconds)};
            [table insert:item completion:^(NSDictionary *insertedItem, NSError *error) {
                if (error) {
                    [test addLog:[NSString stringWithFormat:@"Error requesting push: %@", error]];
                    [test setTestStatus:TSFailed];
                    completion(NO);
                } else {
                    NSTimeInterval timeToWait = 15;
                    NSDictionary *expectedPayload = isNegative ? nil : payload;
                    ZumoPushClient *pushClient = [[ZumoPushClient alloc] initForTest:test withPayload:expectedPayload waitFor:timeToWait withTestCompletion:completion];
                    [[test propertyBag] setValue:pushClient forKey:pushClientKey];
                    
                    // completion will be called on the push client...
                }
            }];
        }
    }];
    
    return result;
}

+ (ZumoTest *)createFeedbackTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Simple feedback test" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        if ([self isRunningOnSimulator]) {
            [test addLog:@"Test running on a simulator, skipping test."];
            [test setTestStatus:TSSkipped];
            completion(YES);
            return;
        }

        if (![[ZumoTestGlobals sharedInstance] deviceToken]) {
            [test addLog:@"Device not correctly registered for push"];
            [test setTestStatus:TSFailed];
            completion(NO);
        } else {
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            MSTable *table = [client tableWithName:tableName];
            NSDictionary *item = @{@"method" : @"getFeedback"};
            [table insert:item completion:^(NSDictionary *item, NSError *error) {
                BOOL passed = NO;
                if (error) {
                    [test addLog:[NSString stringWithFormat:@"Error requesting feedback: %@", error]];
                } else {
                    NSArray *devices = item[@"devices"];
                    if (devices) {
                        [test addLog:[NSString stringWithFormat:@"Retrieved devices from feedback script: %@", devices]];
                        passed = YES;
                    } else {
                        [test addLog:[NSString stringWithFormat:@"No 'devices' field in response: %@", item]];
                    }
                }
                
                [test setTestStatus:(passed ? TSPassed : TSFailed)];
                completion(passed);
            }];
        }
    }];
    
    return result;
}

+ (NSString *)groupDescription {
    return @"Tests to validate that the server-side push module can correctly deliver messages to the iOS client.";
}

@end
