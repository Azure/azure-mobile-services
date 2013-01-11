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
@interface FilterToCaptureHttpTraffic : NSObject <MSFilter>

@property (nonatomic, copy) NSString *userAgent;
@property (nonatomic, copy) NSString *responseContent;
@property (nonatomic, strong) NSDictionary *requestHeaders;
@property (nonatomic, strong) NSDictionary *responseHeaders;

@end

@implementation FilterToCaptureHttpTraffic

@synthesize userAgent, responseContent;
@synthesize requestHeaders = _requestHeaders;
@synthesize responseHeaders = _responseHeaders;

- (id)init {
    self = [super init];
    if (self) {
        _requestHeaders = [[NSMutableDictionary alloc] init];
        _responseHeaders = [[NSMutableDictionary alloc] init];
    }
    
    return self;
}

- (void)handleRequest:(NSURLRequest *)request onNext:(MSFilterNextBlock)onNext onResponse:(MSFilterResponseBlock)onResponse {
    NSDictionary *headers = [request allHTTPHeaderFields];
    _requestHeaders = [NSMutableDictionary new];
    [_requestHeaders setValuesForKeysWithDictionary:headers];
    userAgent = request.allHTTPHeaderFields[@"User-Agent"];
    onNext(request, ^(NSHTTPURLResponse *response, NSData *data, NSError *error) {
        NSDictionary *respHeaders = [response allHeaderFields];
        _responseHeaders = [NSMutableDictionary new];
        [_responseHeaders setValuesForKeysWithDictionary:respHeaders];
        onResponse(response, data, error);
    });
}

@end

// Filter which will bypass the service, responding directly to the caller
@interface FilterToBypassService : NSObject <MSFilter>

@property (nonatomic) int statusCode;
@property (nonatomic, copy) NSString *contentType;
@property (nonatomic, copy) NSString *body;
@property (nonatomic, strong) NSError *errorToReturn;

@end

@implementation FilterToBypassService

@synthesize statusCode, contentType, body, errorToReturn;

- (void)handleRequest:(NSURLRequest *)request onNext:(MSFilterNextBlock)onNext onResponse:(MSFilterResponseBlock)onResponse {
    NSHTTPURLResponse *resp = nil;
    NSData *data = nil;
    NSError *error = nil;
    if ([self errorToReturn]) {
        error = [self errorToReturn];
    } else {
        resp = [[NSHTTPURLResponse alloc] initWithURL:[request URL] statusCode:[self statusCode] HTTPVersion:@"1.1" headerFields:@{@"Content-Type": [self contentType]}];
        data = [[self body] dataUsingEncoding:NSUTF8StringEncoding];
    }
    
    onResponse(resp, data, error);
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
    [result addObject:[self createFilterTestWhichBypassesService]];
    [result addObject:[self createFilterTestToEnsureWithFilterDoesNotChangeClient]];
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
    ZumoTest *result = [ZumoTest createTestWithName:@"User-Agent test" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        FilterToCaptureHttpTraffic *filter = [[FilterToCaptureHttpTraffic alloc] init];
        MSClient *filteredClient = [client clientwithFilter:filter];
        MSTable *table = [filteredClient getTable:@"iosTodoItem"];
        NSDictionary *item = @{@"name":@"john doe"};
        [table insert:item completion:^(NSDictionary *inserted, NSError *error) {
            BOOL passed = NO;
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error: %@", error]];
            } else {
                NSNumber *itemId = inserted[@"id"];
                MSTable *unfilteredTable = [client getTable:@"iosTodoItem"];
                [unfilteredTable deleteWithId:itemId completion:nil]; // clean-up after this test
                NSString *userAgent = [filter userAgent];
                if ([userAgent rangeOfString:@"objective-c"].location == NSNotFound) {
                    [test addLog:[NSString stringWithFormat:@"Error, user-agent does not contain 'objective-c': %@", userAgent]];
                } else {
                    passed = YES;
                    [test addLog:[NSString stringWithFormat:@"User-Agent: %@", userAgent]];
                }
            }

            [test setTestStatus:(passed ? TSPassed : TSFailed)];
            completion(passed);
        }];
    }];
    
    return result;
}

+ (ZumoTest *)createFilterTestWhichBypassesService {
    return [ZumoTest createTestWithName:@"Filter which bypasses service" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        FilterToBypassService *filter = [[FilterToBypassService alloc] init];
        NSString *json = @"{\"id\":1,\"name\":\"John Doe\",\"age\":33}";
        [filter setBody:json];
        [filter setContentType:@"application/json"];
        [filter setStatusCode:201];
        [filter setErrorToReturn:nil];
        MSClient *mockedClient = [client clientwithFilter:filter];
        MSTable *table = [mockedClient getTable:@"TableWhichDoesNotExist"];
        [table insert:@{@"does":@"not matter"} completion:^(NSDictionary *item, NSError *error) {
            BOOL passed = NO;
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error during insert: %@", error]];
            } else {
                [test addLog:[NSString stringWithFormat:@"Inserted item in the mocked service: %@", item]];
                NSString *name = item[@"name"];
                NSNumber *age = item[@"age"];
                if ([name isEqualToString:@"John Doe"] && [age intValue] == 33) {
                    [test addLog:@"Received the correct value from the filter"];
                    passed = YES;
                } else {
                    [test addLog:@"Error, value received from the filter is not correct"];
                }
            }
            
            [test setTestStatus:(passed ? TSPassed : TSFailed)];
            completion(passed);
        }];
    }];
}

+ (ZumoTest *)createFilterTestToEnsureWithFilterDoesNotChangeClient {
    return [ZumoTest createTestWithName:@"MSClient clientWithFilter does not change client" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        FilterToBypassService *filter = [[FilterToBypassService alloc] init];
        NSError *errorToReturn = [NSError errorWithDomain:@"MyDomain" code:-1234 userInfo:@{@"one":@"two"}];
        [filter setErrorToReturn:errorToReturn];
        MSClient *mockedClient = [client clientwithFilter:filter];
        [test addLog:[NSString stringWithFormat:@"Created a client with filter: %@", mockedClient.filters]];
        MSTable *table = [client getTable:@"iosTodoItem"];
        [table insert:@{@"string1":@"does not matter"} completion:^(NSDictionary *item, NSError *error) {
            BOOL passed = NO;
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error during insert: %@", error]];
            } else {
                [test addLog:[NSString stringWithFormat:@"Inserted succeeded: %@", item]];
                passed = YES;
            }
            
            [test setTestStatus:(passed ? TSPassed : TSFailed)];
            completion(passed);
        }];
    }];
}

+ (ZumoTest *)createFilterWithMultipleRequestsTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Filter which maps one request to many" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        SendMultipleRequestsFilter *filter = [[SendMultipleRequestsFilter alloc] init];
        int numberOfRequests = rand() % 3 + 2; // between 2 and 4 requests sent
        [test addLog:[NSString stringWithFormat:@"Using a filter to send %d requests", numberOfRequests]];
        [filter setNumberOfRequests:numberOfRequests];
        client = [client clientwithFilter:filter];
        MSTable *table = [client getTable:@"iosTodoItem"];
        NSString *uuid = [[NSUUID UUID] UUIDString];
        NSDictionary *item = @{@"name":uuid};
        [table insert:item completion:^(NSDictionary *inserted, NSError *error) {
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error inserting: %@", error]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                if ([filter testFailed]) {
                    for (NSString *log in [filter testLogs]) {
                        [test addLog:log];
                        [test setTestStatus:TSFailed];
                        completion(NO);
                    }
                } else {
                    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"name = %@", uuid];
                    MSQuery *query = [table queryWhere:predicate];
                    [filter setNumberOfRequests:1];
                    [query setSelectFields:@[@"name"]];
                    [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
                        BOOL passed = NO;
                        [test addLog:@"Filter logs:"];
                        for (NSString *log in [filter testLogs]) {
                            [test addLog:log];
                        }

                        if (error || [filter testFailed]) {
                            [test addLog:[NSString stringWithFormat:@"Error reading: %@", error]];
                        } else {
                            if ([items count] == numberOfRequests) {
                                [test addLog:@"Got correct number of items inserted"];
                                passed = YES;
                            } else {
                                [test addLog:[NSString stringWithFormat:@"Error, expected %d items to be returned, but this is what was: %@", numberOfRequests, items]];
                            }
                        }
                        
                        [test setTestStatus:(passed ? TSPassed : TSFailed)];
                        completion(passed);
                    }];
                }
            }
        }];
    }];
    
    return result;
}

@end
