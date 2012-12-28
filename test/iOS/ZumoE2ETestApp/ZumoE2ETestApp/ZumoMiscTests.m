//
//  ZumoMiscTests.m
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoMiscTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>

// Private classes

// Filter which will save the User-Agent request header
@interface UserAgentGrabberFilter : NSObject <MSFilter>

@property (nonatomic, copy) NSString *userAgent;

@end

@implementation UserAgentGrabberFilter

@synthesize userAgent;

- (void)handleRequest:(NSURLRequest *)request onNext:(MSFilterNextBlock)onNext onResponse:(MSFilterResponseBlock)onResponse {
    userAgent = request.allHTTPHeaderFields[@"User-Agent"];
    onNext(request, onResponse);
}

@end

// Filter which will send multiple requests instead of one,
// returning the response of the last one
@interface SendMultipleRequestsFilter : NSObject <MSFilter>

@property (nonatomic) int numberOfRequests;
@property (nonatomic) BOOL testFailed;
@property (nonatomic) NSMutableArray *testLogs;

@end

@implementation SendMultipleRequestsFilter

@synthesize testFailed = _testFailed;
@synthesize testLogs = _testLogs;
@synthesize numberOfRequests = _numberOfRequests;

-(id)init {
    self = [super init];
    if (self) {
        _numberOfRequests = 2;
        _testFailed = NO;
        _testLogs = [[NSMutableArray alloc] init];
    }
    
    return self;
}

- (void)setNumberOfRequests:(int)numberOfRequests {
    if (numberOfRequests >= 1 || numberOfRequests <= 4) {
        _numberOfRequests = numberOfRequests;
    } else {
        _testFailed = YES;
        [_testLogs addObject:@"Number of requests must be between 1 and 4"];
        _numberOfRequests = 2;
    }
}

- (void)handleRequest:(NSURLRequest *)request onNext:(MSFilterNextBlock)onNext onResponse:(MSFilterResponseBlock)onResponse {
    onNext(request, ^(NSHTTPURLResponse *response, NSData *data, NSError *error) {
        [self addResponseToLog:response forRequest:request];
        if (_numberOfRequests == 1) {
            onResponse(response, data, error);
        } else {
            onNext(request, ^(NSHTTPURLResponse *response, NSData *data, NSError *error) {
                [self addResponseToLog:response forRequest:request];
                if (_numberOfRequests == 2) {
                    onResponse(response, data, error);
                } else {
                    onNext(request, ^(NSHTTPURLResponse *response, NSData *data, NSError *error) {
                        [self addResponseToLog:response forRequest:request];
                        if (_numberOfRequests == 3) {
                            onResponse(response, data, error);
                        } else {
                            onNext(request, ^(NSHTTPURLResponse *response, NSData *data, NSError *error) {
                                [self addResponseToLog:response forRequest:request];
                                onResponse(response, data, error);
                            });
                        }
                    });
                }
            });
        }
    });
}

- (void)addResponseToLog:(NSHTTPURLResponse *)response forRequest:(NSURLRequest *)request {
    [_testLogs addObject:[NSString stringWithFormat:@"%@ request to %@: Response %d", [request HTTPMethod], [request URL], response.statusCode]];
}

@end

// Main implementation
@implementation ZumoMiscTests

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    [result addObject:[self createUserAgentTest]];
    [result addObject:[self createFilterWithMultipleRequestsTest]];
    return result;
}

+ (NSString *)helpText {
    NSArray *lines = [NSArray arrayWithObjects:
                      @"1. Create an application on Windows azure portal.",
                      @"2. Create a table called 'iOSTodoItem'.",
                      @"3. Click on the 'Misc Tests' button.",
                      @"4. Make sure all the tests pass.",
                      nil];
    return [lines componentsJoinedByString:@"\n"];
}

+ (ZumoTest *)createUserAgentTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"User-Agent test" andExecution:nil];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        UserAgentGrabberFilter *filter = [[UserAgentGrabberFilter alloc] init];
        client = [client clientwithFilter:filter];
        MSTable *table = [client getTable:@"iosTodoItem"];
        NSDictionary *item = @{@"name":@"john doe"};
        [table insert:item completion:^(NSDictionary *inserted, NSError *error) {
            BOOL passed = NO;
            if (error) {
                [weakRef addLog:[NSString stringWithFormat:@"Error: %@", error]];
            } else {
                NSNumber *itemId = inserted[@"id"];
                [table deleteWithId:itemId completion:nil]; // clean-up after this test
                NSString *userAgent = [filter userAgent];
                if ([userAgent rangeOfString:@"objective-c"].location == NSNotFound) {
                    [weakRef addLog:[NSString stringWithFormat:@"Error, user-agent does not contain 'objective-c': %@", userAgent]];
                } else {
                    passed = YES;
                    [weakRef addLog:[NSString stringWithFormat:@"User-Agent: %@", userAgent]];
                }
            }

            [weakRef setTestStatus:(passed ? TSPassed : TSFailed)];
            completion(passed);
        }];
    }];
    
    return result;
}

+ (ZumoTest *)createFilterWithMultipleRequestsTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Filter which maps one request to many" andExecution:nil];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        SendMultipleRequestsFilter *filter = [[SendMultipleRequestsFilter alloc] init];
        int numberOfRequests = rand() % 3 + 2; // between 2 and 4 requests sent
        [weakRef addLog:[NSString stringWithFormat:@"Using a filter to send %d requests", numberOfRequests]];
        [filter setNumberOfRequests:numberOfRequests];
        client = [client clientwithFilter:filter];
        MSTable *table = [client getTable:@"iosTodoItem"];
        NSString *uuid = [[NSUUID UUID] UUIDString];
        NSDictionary *item = @{@"name":uuid};
        [table insert:item completion:^(NSDictionary *inserted, NSError *error) {
            if (error) {
                [weakRef addLog:[NSString stringWithFormat:@"Error inserting: %@", error]];
                [weakRef setTestStatus:TSFailed];
                completion(NO);
            } else {
                if ([filter testFailed]) {
                    for (NSString *log in [filter testLogs]) {
                        [weakRef addLog:log];
                        [weakRef setTestStatus:TSFailed];
                        completion(NO);
                    }
                } else {
                    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"name = %@", uuid];
                    MSQuery *query = [table queryWhere:predicate];
                    [filter setNumberOfRequests:1];
                    [query setSelectFields:@[@"name"]];
                    [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
                        BOOL passed = NO;
                        [weakRef addLog:@"Filter logs:"];
                        for (NSString *log in [filter testLogs]) {
                            [weakRef addLog:log];
                        }

                        if (error || [filter testFailed]) {
                            [weakRef addLog:[NSString stringWithFormat:@"Error reading: %@", error]];
                        } else {
                            if ([items count] == numberOfRequests) {
                                [weakRef addLog:@"Got correct number of items inserted"];
                                passed = YES;
                            } else {
                                [weakRef addLog:[NSString stringWithFormat:@"Error, expected %d items to be returned, but this is what was: %@", numberOfRequests, items]];
                            }
                        }
                        
                        [weakRef setTestStatus:(passed ? TSPassed : TSFailed)];
                        completion(passed);
                    }];
                }
            }
        }];
    }];
    
    return result;
}

@end
