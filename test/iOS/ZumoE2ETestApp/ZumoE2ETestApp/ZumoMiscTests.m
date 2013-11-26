// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoMiscTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>

// Private classes

// Filter which will save the User-Agent request header
@interface FilterToCaptureHttpTraffic : NSObject <MSFilter>

@property (nonatomic, copy) NSString *userAgent;
@property (nonatomic, copy) NSData *responseContent;
@property (nonatomic, strong) NSDictionary *requestHeaders;
@property (nonatomic, strong) NSDictionary *responseHeaders;

@end

@implementation FilterToCaptureHttpTraffic

@synthesize userAgent;
@synthesize requestHeaders = _requestHeaders;
@synthesize responseHeaders = _responseHeaders;
@synthesize responseContent = _responseContent;

- (id)init {
    self = [super init];
    if (self) {
        _requestHeaders = [[NSMutableDictionary alloc] init];
        _responseHeaders = [[NSMutableDictionary alloc] init];
    }
    
    return self;
}

- (void)handleRequest:(NSURLRequest *)request next:(MSFilterNextBlock)onNext response:(MSFilterResponseBlock)onResponse {
    NSDictionary *headers = [request allHTTPHeaderFields];
    _requestHeaders = [NSMutableDictionary new];
    [_requestHeaders setValuesForKeysWithDictionary:headers];
    userAgent = request.allHTTPHeaderFields[@"User-Agent"];
    NSString *clientVersion = [NSString stringWithFormat:@"%d.%d.%d.0", WindowsAzureMobileServicesSdkMajorVersion, WindowsAzureMobileServicesSdkMinorVersion, WindowsAzureMobileServicesSdkBuildVersion];
    [[[ZumoTestGlobals sharedInstance] globalTestParameters] setObject:clientVersion forKey:CLIENT_VERSION_KEY];
    onNext(request, ^(NSHTTPURLResponse *response, NSData *data, NSError *error) {
        NSDictionary *respHeaders = [response allHeaderFields];
        _responseHeaders = [NSMutableDictionary new];
        [_responseHeaders setValuesForKeysWithDictionary:respHeaders];
        NSString *runtimeVersion = [_responseHeaders objectForKey:@"x-zumo-version"];
        if (runtimeVersion) {
            [[[ZumoTestGlobals sharedInstance] globalTestParameters] setObject:runtimeVersion forKey:RUNTIME_VERSION_KEY];
        }
        _responseContent = [NSData dataWithData:data];
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

- (void)handleRequest:(NSURLRequest *)request next:(MSFilterNextBlock)onNext response:(MSFilterResponseBlock)onResponse {
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

- (void)handleRequest:(NSURLRequest *)request next:(MSFilterNextBlock)onNext response:(MSFilterResponseBlock)onResponse {
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

// Filter which will save the User-Agent request header
@interface FilterToSimulateOptimisticConcurrency : NSObject <MSFilter>

@end

@implementation FilterToSimulateOptimisticConcurrency

- (void)handleRequest:(NSURLRequest *)request next:(MSFilterNextBlock)onNext response:(MSFilterResponseBlock)onResponse {

    // Append __systemProperties=* to request URI
    NSString *url = [[request URL] absoluteString];
    NSString *query, *urlWithoutQuery;
    NSRange indexOfQuery = [url rangeOfString:@"?"];
    if (indexOfQuery.location == NSNotFound) {
        query = @"";
        urlWithoutQuery = url;
    } else {
        query = [url substringFromIndex:(indexOfQuery.location + 1)];
        query = [query stringByAppendingString:@"&"];
        urlWithoutQuery = [url substringToIndex:indexOfQuery.location];
    }
    urlWithoutQuery = [urlWithoutQuery stringByAppendingString:@"?"];
    query = [query stringByAppendingString:@"__systemProperties=*"];
    url = [urlWithoutQuery stringByAppendingString:query];

    // Clone the request and headers
    NSMutableURLRequest *newRequest = [[NSMutableURLRequest alloc] initWithURL:[NSURL URLWithString:url]];
    NSString *HTTPMethod = [request HTTPMethod];
    [newRequest setHTTPMethod:HTTPMethod];
    NSDictionary *requestHeaders = [request allHTTPHeaderFields];
    for (NSString *headerName in [requestHeaders keyEnumerator]) {
        NSString *headerValue = [requestHeaders objectForKey:headerName];
        [newRequest setValue:headerValue forHTTPHeaderField:headerName];
    }
    [newRequest setHTTPBody:[request HTTPBody]];
    
    // Remove the system properties from the request
    if ([HTTPMethod isEqualToString:@"PUT"] || [HTTPMethod isEqualToString:@"PATCH"]) {
        NSError *error;
        NSData *requestBody = [request HTTPBody];
        id body = [NSJSONSerialization JSONObjectWithData:requestBody options:0 error:&error];
        if (error) {
            onResponse(nil, nil, error);
            return;
        }
        if ([body isKindOfClass:[NSDictionary class]]) {
            NSDictionary *item = body;
            NSString *version = nil;
            NSMutableDictionary *newItem = [[NSMutableDictionary alloc] init];
            BOOL propertyRemoved = NO;
            for (NSString *key in [item allKeys]) {
                if ([key hasPrefix:@"__"]) {
                    propertyRemoved = YES;
                    if ([key isEqualToString:@"__version"]) {
                        version = [item objectForKey:key];
                    }
                } else {
                    [newItem setValue:[item objectForKey:key] forKey:key];
                }
            }
            
            if (version) {
                NSString *etag = [NSString stringWithFormat:@"\"%@\"", version];
                [newRequest setValue:etag forHTTPHeaderField:@"If-Match"];
            }
            
            if (propertyRemoved) {
                NSData *newBody = [NSJSONSerialization dataWithJSONObject:newItem options:0 error:&error];
                if (error) {
                    onResponse(nil, nil, error);
                    return;
                }
                [newRequest setHTTPBody:newBody];
            }
        }
    }

    // Now send the updated request
    onNext(newRequest, ^(NSHTTPURLResponse *response, NSData *data, NSError *error) {
        if (error) {
            onResponse(response, data, error);
            return;
        }

        // Move the response ETag header into a __version field
        NSDictionary *respHeaders = [response allHeaderFields];
        NSString *contentType = [respHeaders objectForKey:@"Content-Type"];
        if ([contentType isEqualToString:@"application/json"]) {
            NSString *etag = [respHeaders objectForKey:@"ETag"];
            if (etag && data) {
                if ([etag hasPrefix:@"\""]) etag = [etag substringFromIndex:1];
                if ([etag hasSuffix:@"\""]) etag = [etag substringToIndex:([etag length] - 1)];
                id body = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
                if (!error) {
                    if ([body isKindOfClass:[NSDictionary class]]) {
                        NSMutableDictionary *item = [[NSMutableDictionary alloc] initWithDictionary:body];
                        [item setValue:etag forKey:@"__version"];
                        data = [NSJSONSerialization dataWithJSONObject:item options:0 error:&error];
                        if (error) data = nil;
                    }
                }
            }
        }

        onResponse(response, data, error);
    });
}

@end

// Main implementation
@implementation ZumoMiscTests

static NSString *tableName = @"iOSRoundTripTable";
static NSString *stringIdTableName = @"stringIdRoundTripTable";
static NSString *parameterTestTableName = @"ParamsTestTable";

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    [result addObject:[self createUserAgentTest]];
    [result addObject:[self createFilterWithMultipleRequestsTest]];
    [result addObject:[self createFilterTestWhichBypassesService]];
    [result addObject:[self createFilterTestToEnsureWithFilterDoesNotChangeClient]];
    [result addObject:[self createParameterPassingTest]];
    [result addObject:[self createOptimisticConcurrencyWithFilterTest]];
    return result;
}

+ (ZumoTest *)createOptimisticConcurrencyWithFilterTest {
    return [ZumoTest createTestWithName:@"Using filters to access optimistic concurrency feature" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        client = [client clientWithFilter:[[FilterToSimulateOptimisticConcurrency alloc] init]];
        MSTable *table = [client tableWithName:stringIdTableName];
        NSDictionary *item = @{@"name":@"John Doe",@"number":@123};
        [table insert:item completion:^(NSDictionary *inserted, NSError *error) {
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error inserting first item: %@", error]];
                completion(NO);
                return;
            }
            
            [test addLog:[NSString stringWithFormat:@"Inserted: %@", inserted]];
            id itemId = [inserted objectForKey:@"id"];
            NSMutableDictionary *toUpdate = [[NSMutableDictionary alloc] initWithDictionary:inserted];
            [toUpdate setValue:@"Jane Roe" forKey:@"name"];
            [table update:toUpdate completion:^(NSDictionary *updated, NSError *error) {
                if (error) {
                    [test addLog:[NSString stringWithFormat:@"Error updating item: %@", error]];
                    completion(NO);
                    return;
                }

                [test addLog:[NSString stringWithFormat:@"Updated: %@", updated]];
                [test addLog:@"Now updating with incorrect version"];
                [toUpdate setValue:@"incorrect" forKey:@"__version"];
                [table update:toUpdate completion:^(NSDictionary *updated2, NSError *error) {
                    BOOL testPassed = NO;
                    if (error) {
                        [test addLog:[NSString stringWithFormat:@"Got error as expected: %@", error]];
                        testPassed = YES;
                    } else {
                        [test addLog:[NSString stringWithFormat:@"Error, update should not have worked, but it did: %@", updated2]];
                    }
                    
                    [test addLog:@"Cleaning up..."];
                    [table deleteWithId:itemId completion:^(NSNumber *itemId, NSError *error) {
                        [test addLog:[@"Delete item result: " stringByAppendingString:error ? @"failed" : @"succeeded"]];
                        completion(testPassed);
                    }];
                }];
            }];
        }];
    }];
}

+ (ZumoTest *)createParameterPassingTest {
    return [ZumoTest createTestWithName:@"Parameter passing tests" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:parameterTestTableName];
        NSDictionary *baseDict = @{
                               @"item": @"simple",
                               @"item": @"simple" ,
                               @"empty": @"" ,
                               @"spaces": @"with spaces",
                               @"specialChars": @"`!@#$%^&*()-=[]\\;',./~_+{}|:\"<>?",
                               @"latin": @"ãéìôü ÇñÑ",
                               @"arabic": @"الكتاب على الطاولة",
                               @"chinese": @"这本书在桌子上",
                               @"japanese": @"本は机の上に",
                               @"hebrew": @"הספר הוא על השולחן",
                               @"name+with special&chars": @"should just work"
                               };
        NSMutableDictionary *dict = [[NSMutableDictionary alloc] initWithDictionary:baseDict];
        [dict setValue:@"insert" forKey:@"operation"];
        NSMutableDictionary *item = [[NSMutableDictionary alloc] initWithDictionary:@{@"name":@"John Doe"}];
        [table insert:item parameters:dict completion:^(NSDictionary *inserted, NSError *error) {
            if ([self handleIfError:error operation:@"insert" test:test completion:completion]) {
                return;
            }
            if (![self validateParameters:test operation:@"insert" expected:dict actual:[self parseJson:inserted[@"parameters"]]]) {
                completion(NO);
                return;
            }
            
            dict[@"operation"] = @"update";
            [item setValue:@1 forKey:@"id"];
            [table update:item parameters:dict completion:^(NSDictionary *updated, NSError *error) {
                if ([self handleIfError:error operation:@"update" test:test completion:completion]) {
                    return;
                }
                if (![self validateParameters:test operation:@"update" expected:dict actual:[self parseJson:updated[@"parameters"]]]) {
                    completion(NO);
                    return;
                }
                
                dict[@"operation"] = @"lookup";
                [table readWithId:@1 parameters:dict completion:^(NSDictionary *lookedUp, NSError *error) {
                    if ([self handleIfError:error operation:@"lookup" test:test completion:completion]) {
                        return;
                    }
                    if (![self validateParameters:test operation:@"lookup" expected:dict actual:[self parseJson:lookedUp[@"parameters"]]]) {
                        completion(NO);
                        return;
                    }
                    
                    dict[@"operation"] = @"read";
                    MSQuery *query = [table query];
                    [query setParameters:dict];
                    [query readWithCompletion:^(NSArray *readItems, NSInteger totalCount, NSError *error) {
                        if ([self handleIfError:error operation:@"read" test:test completion:completion]) {
                            return;
                        }
                        if (![self validateParameters:test operation:@"read" expected:dict actual:[self parseJson:readItems[0][@"parameters"]]]) {
                            completion(NO);
                            return;
                        }
                        
                        dict[@"operation"] = @"delete";
                        FilterToCaptureHttpTraffic *capturingFilter = [[FilterToCaptureHttpTraffic alloc] init];
                        MSClient *filteredClient = [client clientWithFilter:capturingFilter];
                        MSTable *filteredTable = [filteredClient tableWithName:parameterTestTableName];
                        [filteredTable deleteWithId:@1 parameters:dict completion:^(NSNumber *itemId, NSError *error) {
                            if ([self handleIfError:error operation:@"delete" test:test completion:completion]) {
                                return;
                            }

                            NSString *responseContent = [[NSString alloc] initWithData:[capturingFilter responseContent] encoding:NSUTF8StringEncoding];
                            NSDictionary *deleteBody = [self parseJson:responseContent];
                            if (![self validateParameters:test operation:@"delete" expected:dict actual:[self parseJson:deleteBody[@"parameters"]]]) {
                                completion(NO);
                                return;
                            }
                            
                            [test addLog:@"All validations passed"];
                            completion(YES);
                        }];
                    }];
                }];
            }];
        }];
    }];
}

+ (NSDictionary *)parseJson:(NSString *)json {
    NSData *data = [json dataUsingEncoding:NSUTF8StringEncoding];
    NSError *error;
    NSDictionary *result = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
    return result;
}
+ (BOOL)validateParameters:(ZumoTest *)test operation:(NSString *)operation expected:(NSDictionary *)expected actual:(NSDictionary *)actual {
    BOOL same = YES;
    NSString *paramaterName;
    [test addLog:[NSString stringWithFormat:@"Validating parameters for operation %@", operation]];
    for (paramaterName in [expected keyEnumerator]) {
        NSString *expectedValue = expected[paramaterName];
        NSString *actualValue = actual[paramaterName];
        if (!actualValue) {
            [test addLog:[NSString stringWithFormat:@"Parameter %@ not found in the response", paramaterName]];
            same = NO;
        } else {
            if (![expectedValue isEqualToString:actualValue]) {
                [test addLog:[NSString stringWithFormat:@"Value of parameter %@ is incorrect. Expected: %@; actual: %@", paramaterName, expectedValue, actualValue]];
                same = NO;
            }
        }
    }
    
    if (same) {
        [test addLog:@"All parameter validated correctly"];
    }
    
    return same;
}

+ (BOOL)handleIfError:(NSError *)error operation:(NSString *)operation test:(ZumoTest *)test completion:(ZumoTestCompletion) completion {
    if (error) {
        [test addLog:[[NSString alloc] initWithFormat:@"Error during %@: %@", operation, error]];
        completion(NO);
        return YES;
    } else {
        return NO;
    }
}

+ (ZumoTest *)createUserAgentTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"User-Agent test" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        FilterToCaptureHttpTraffic *filter = [[FilterToCaptureHttpTraffic alloc] init];
        MSClient *filteredClient = [client clientWithFilter:filter];
        MSTable *table = [filteredClient tableWithName:tableName];
        NSDictionary *item = @{@"name":@"john doe"};
        [table insert:item completion:^(NSDictionary *inserted, NSError *error) {
            BOOL passed = NO;
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error: %@", error]];
            } else {
                NSNumber *itemId = inserted[@"id"];
                MSTable *unfilteredTable = [client tableWithName:tableName];
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
        MSClient *mockedClient = [client clientWithFilter:filter];
        MSTable *table = [mockedClient tableWithName:@"TableWhichDoesNotExist"];
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
        MSClient *mockedClient = [client clientWithFilter:filter];
        [test addLog:[NSString stringWithFormat:@"Created a client with filter: %@", mockedClient.filters]];
        MSTable *table = [client tableWithName:tableName];
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
        client = [client clientWithFilter:filter];
        MSTable *table = [client tableWithName:tableName];
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
                    MSQuery *query = [table queryWithPredicate:predicate];
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

+ (NSString *)groupDescription {
    return @"Tests to validate features which don't fit in other groups. Those include filters, correct user-agent header, and some replaying scenarios.";
}

@end
