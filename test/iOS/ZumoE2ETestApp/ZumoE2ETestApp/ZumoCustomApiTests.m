// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoCustomApiTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"
#import "ZumoLoginTests.h"

@implementation ZumoCustomApiTests

static NSString *apiPublicName = @"public";
static NSString *apiApplicationName = @"application";
static NSString *apiUserName = @"user";
static NSString *apiAdminName = @"admin";

typedef enum { DataFormatJson, DataFormatXml, DataFormatOther } ApiDataFormat;

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    [result addObject:[self createJsonBasedTestWithName:@"Simple object - POST" apiName:apiApplicationName httpMethod:@"POST" body:@{@"name":@"value"} headers:nil query:nil statusCode:200]];
    [result addObject:[self createJsonBasedTestWithName:@"Simple call - GET" apiName:apiApplicationName httpMethod:@"GET" body:nil headers:nil query:@{@"param": @"value"} statusCode:200]];
    [result addObject:[self createJsonBasedTestWithName:@"Simple object - PUT" apiName:apiApplicationName httpMethod:@"PUT" body:@{@"array":@[@1, @YES, @"str"]} headers:nil query:@{@"method": @"PUT"} statusCode:200]];
    [result addObject:[self createJsonBasedTestWithName:@"Simple object - PATCH" apiName:apiApplicationName httpMethod:@"PATCH" body:@{@"array":@[@1, @YES, @"str"]} headers:nil query:@{@"method": @"PATCH"} statusCode:200]];
    [result addObject:[self createJsonBasedTestWithName:@"Simple call - DELETE" apiName:apiApplicationName httpMethod:@"DELETE" body:nil headers:nil query:@{@"method": @"DELETE"} statusCode:200]];
    
    [result addObject:[self createJsonBasedTestWithName:@"POST - array body" apiName:apiApplicationName httpMethod:@"POST" body:@[@1,@NO,@2] headers:nil query:nil statusCode:200]];
    [result addObject:[self createJsonBasedTestWithName:@"POST - empty array body" apiName:apiApplicationName httpMethod:@"POST" body:@[] headers:nil query:nil statusCode:200]];
    [result addObject:[self createJsonBasedTestWithName:@"POST - empty object body" apiName:apiApplicationName httpMethod:@"POST" body:@{} headers:nil query:nil statusCode:200]];
    
    [result addObject:[self createJsonBasedTestWithName:@"GET - custom headers" apiName:apiApplicationName httpMethod:@"GET" body:nil headers:@{@"x-test-zumo-1": @"header value"} query:nil statusCode:200]];
    
    [result addObject:[self createJsonBasedTestWithName:@"PATCH - query parameters" apiName:apiApplicationName httpMethod:@"PATCH" body:@{@"name":@123} headers:nil query:@{@"key":@"value"} statusCode:200]];
    [result addObject:[self createJsonBasedTestWithName:@"GET - query parameters non-ASCII" apiName:apiApplicationName httpMethod:@"GET" body:nil headers:nil query:@{@"latin":@"áñüø",@"arabic":@"الكتاب على الطاولة", @"japanese":@"本は机の上に", @"name needs & escape": @"value too"} statusCode:200]];
    
    [result addObject:[self createJsonBasedTestWithName:@"GET - 500 response" apiName:apiApplicationName httpMethod:@"GET" body:nil headers:nil query:@{@"name":@"value"} statusCode:500]];
    [result addObject:[self createJsonBasedTestWithName:@"GET - 400 response" apiName:apiApplicationName httpMethod:@"GET" body:nil headers:@{@"x-test-zumo-x":@"header value"} query:@{@"name":@"value"} statusCode:400]];

    [result addObject:[self createDataBasedTestWithName:@"GET - JSON result" apiName:apiApplicationName httpMethod:@"GET" body:nil headers:nil query:nil statusCode:200 inputFormat:DataFormatJson outputFormat:DataFormatJson]];
    [result addObject:[self createDataBasedTestWithName:@"GET - XML result" apiName:apiApplicationName httpMethod:@"GET" body:nil headers:nil query:nil statusCode:200 inputFormat:DataFormatJson outputFormat:DataFormatXml]];
    [result addObject:[self createDataBasedTestWithName:@"GET - Text result" apiName:apiApplicationName httpMethod:@"GET" body:nil headers:nil query:nil statusCode:200 inputFormat:DataFormatJson outputFormat:DataFormatOther]];
 
    [result addObject:[self createDataBasedTestWithName:@"POST - JSON input, XML output" apiName:apiApplicationName httpMethod:@"POST" body:@{@"values":@[@1, @YES]} headers:nil query:nil statusCode:200 inputFormat:DataFormatJson outputFormat:DataFormatXml]];
    [result addObject:[self createDataBasedTestWithName:@"POST - text input, JSON output" apiName:apiApplicationName httpMethod:@"POST" body:@"This is a text body" headers:nil query:nil statusCode:200 inputFormat:DataFormatOther outputFormat:DataFormatJson]];
    [result addObject:[self createDataBasedTestWithName:@"POST - XML input, text output" apiName:apiApplicationName httpMethod:@"POST" body:@"<hello id=\"1\">world</hello>" headers:nil query:nil statusCode:200 inputFormat:DataFormatXml outputFormat:DataFormatOther]];
    
    [result addObject:[self createDataBasedTestWithName:@"PUT - JSON input, XML output, custom headers" apiName:apiApplicationName httpMethod:@"PUT" body:@{} headers:@{@"x-test-zumo-1":@"first header", @"x-test-zumo-2":@"second header"} query:nil statusCode:200 inputFormat:DataFormatJson outputFormat:DataFormatXml]];
    [result addObject:[self createDataBasedTestWithName:@"PATCH - JSON input, text output, custom query parameters" apiName:apiApplicationName httpMethod:@"PATCH" body:@{@"values":@[@1, @2]} headers:@{@"x-test-zumo-1":@"first header"} query:@{@"latin":@"ñøî†é", @"hebrew":@"הספר הוא על השולחן"} statusCode:200 inputFormat:DataFormatJson outputFormat:DataFormatOther]];
    [result addObject:[self createDataBasedTestWithName:@"DELETE - XML output, 400 response" apiName:apiApplicationName httpMethod:@"DELETE" body:nil headers:nil query:nil statusCode:400 inputFormat:DataFormatJson outputFormat:DataFormatXml]];
    [result addObject:[self createDataBasedTestWithName:@"GET - JSON output, 500 response" apiName:apiApplicationName httpMethod:@"GET" body:nil headers:@{@"x-test-zumo-1":@"header value"} query:nil statusCode:500 inputFormat:DataFormatJson outputFormat:DataFormatJson]];

    [result addObject:[ZumoLoginTests createLogoutTest]];
    [result addObject:[self createApiPermissionsTestWithName:@"Public API - no keys" apiName:apiPublicName shouldSucceed:YES]];
    [result addObject:[self createApiPermissionsTestWithName:@"Application API - logged out" apiName:apiApplicationName shouldSucceed:YES]];
    [result addObject:[self createApiPermissionsTestWithName:@"Authenticated API - logged out" apiName:apiUserName shouldSucceed:NO]];
    [result addObject:[self createApiPermissionsTestWithName:@"Admin API - logged out" apiName:apiAdminName shouldSucceed:NO]];

    int indexOfLastUnattendedTest = [result count];
    
    [result addObject:[ZumoLoginTests createLoginTestForProvider:@"facebook" usingSimplifiedMode:YES]];
    [result addObject:[self createApiPermissionsTestWithName:@"Application API - logged in" apiName:apiApplicationName shouldSucceed:YES]];
    [result addObject:[self createApiPermissionsTestWithName:@"Authenticated API - logged in" apiName:apiUserName shouldSucceed:YES]];
    [result addObject:[self createApiPermissionsTestWithName:@"Admin API - logged in" apiName:apiAdminName shouldSucceed:NO]];

    for (int i = indexOfLastUnattendedTest; i < [result count]; i++) {
        ZumoTest *test = result[i];
        [test setCanRunUnattended:NO];
    }
    
    [result addObject:[ZumoLoginTests createLogoutTest]];

    return result;
}

+ (ZumoTest *)createApiPermissionsTestWithName:(NSString *)testName apiName:(NSString *)apiName shouldSucceed:(BOOL)shouldSucceed {
    return [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        if (apiName == apiPublicName) {
            // No key necessary
            client = [MSClient clientWithApplicationURL:[client applicationURL]];
        }
        [client invokeAPI:apiName body:nil HTTPMethod:@"GET" parameters:@{@"x":@"1"} headers:nil completion:^(id result, NSURLResponse *response, NSError *error) {
            if (shouldSucceed) {
                if (error) {
                    [test addLog:[NSString stringWithFormat:@"Error, should have succeeded, but error = %@", error]];
                    completion(NO);
                } else {
                    [test addLog:[NSString stringWithFormat:@"Ok, call should have succeeded and it did. Result = %@", result]];
                    completion(YES);
                }
            } else {
                [test addLog:[NSString stringWithFormat:@"Should have failed, and got an error (%@). Validating error code.", error]];
                NSHTTPURLResponse *httpResponse = [[error userInfo] objectForKey:MSErrorResponseKey];
                if ([httpResponse statusCode] == 401) {
                    [test addLog:@"Received expected status code."];
                    completion(YES);
                } else {
                    [test addLog:[NSString stringWithFormat:@"Error, expected 401, received %d", [httpResponse statusCode]]];
                    completion(NO);
                }
            }
        }];
    }];
}

+ (ZumoTest *)createJsonBasedTestWithName:(NSString *)testName apiName:(NSString *)apiName httpMethod:(NSString *)httpMethod body:(id)body headers:(NSDictionary *)headers query:(NSDictionary *)query statusCode:(NSInteger)statusCode {

    NSDictionary *queryParameters = query;
    if (statusCode != 200) {
        NSMutableDictionary *newQuery = [[NSMutableDictionary alloc] initWithDictionary:query copyItems:YES];
        [newQuery setObject:[NSNumber numberWithInteger:statusCode] forKey:@"status"];
        queryParameters = newQuery;
    }

    testName = [NSString stringWithFormat:@"JSON-style selector: %@", testName];

    return [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        [client invokeAPI:apiName body:body HTTPMethod:httpMethod parameters:queryParameters headers:headers completion:^(id result, NSHTTPURLResponse *httpResponse, NSError *error) {
            BOOL testPassed = YES;
            
            if (error && !httpResponse) {
                httpResponse = [[error userInfo] objectForKey:MSErrorResponseKey];
            }
            
            if (![self validateErrorForTest:test error:error expectedStatus:statusCode response:httpResponse result:result]) {
                completion(NO);
                return;
            }
            
            if (![self validateResponseHeadersForTest:test expectedHeaders:headers httpResponse:httpResponse]) {
                completion(NO);
                return;
            }
            
            NSMutableDictionary *expectedBody = [self createExpectedBodyForTest:test httpMethod:httpMethod query:query body:body];
            
            NSMutableArray *errors = [[NSMutableArray alloc] init];
            id actualBody;
            if (!error) {
                actualBody = result;
            } else {
                NSString *bodyContents = [error localizedDescription];
                NSData *bodyData = [bodyContents dataUsingEncoding:NSUTF8StringEncoding];
                NSError *jsonError;
                actualBody = [NSJSONSerialization JSONObjectWithData:bodyData options:0 error:&jsonError];
                if (jsonError) {
                    [test addLog:[NSString stringWithFormat:@"Error reading response body as JSON. Body contents = %@", bodyContents]];
                    completion(NO);
                    return;
                }
            }
            
            if ([ZumoTestGlobals compareJson:expectedBody with:actualBody log:errors]) {
                [test addLog:@"Comparison succeeded"];
            } else {
                [test addLog:[NSString stringWithFormat:@"Error comparing response. Expected: %@, actual: %@", expectedBody, result]];
                testPassed = NO;
                for (NSString *log in errors) {
                    [test addLog:log];
                }
                completion(NO);
                return;
            }

            completion(testPassed);
        }];
    }];
}

+ (NSMutableDictionary *)createExpectedBodyForTest:(ZumoTest *)test httpMethod:(NSString *)httpMethod query:(NSDictionary *)query body:(id)body {
    NSMutableDictionary *expectedBody = [[NSMutableDictionary alloc] init];
    [expectedBody setObject:httpMethod forKey:@"method"];
    if (query && [query count]) {
        NSMutableDictionary *expectedQuery = [[NSMutableDictionary alloc] init];
        [expectedBody setObject:expectedQuery forKey:@"query"];
        for (NSString *key in [query allKeys]) {
            [expectedQuery setObject:query[key] forKey:key];
        }
    }
    
    if (body) {
        [expectedBody setObject:body forKey:@"body"];
    }
    
    NSDictionary *userLevel = @{@"level":@"anonymous"};
    [expectedBody setObject:userLevel forKey:@"user"];
    
    return expectedBody;
}

+ (BOOL)validateResponseHeadersForTest:(ZumoTest *)test expectedHeaders:(NSDictionary *)headers httpResponse:(NSHTTPURLResponse *)httpResponse; {
    BOOL testPassed = YES;
    if (headers && [headers count]) {
        NSDictionary *responseHeaders = [httpResponse allHeaderFields];
        for (NSString *key in [headers allKeys]) {
            if ([key hasPrefix:@"x-test-zumo-"]) {
                NSString *respHeader = [responseHeaders objectForKey:key];
                NSString *expectedHeader = headers[key];
                if (![expectedHeader isEqualToString:respHeader]) {
                    [test addLog:[NSString stringWithFormat:@"Invalid response header for %@. Expected: %@, actual: %@", key, expectedHeader, respHeader]];
                    testPassed = NO;
                    break;
                }
            }
        }
        
        if (testPassed) {
            [test addLog:@"All headers validated successfully"];
        } else {
            testPassed = NO;
        }
    }
    
    return testPassed;
}

+ (BOOL)validateErrorForTest:(ZumoTest *)test error:(NSError *)error expectedStatus:(NSInteger)statusCode response:(NSHTTPURLResponse *)response result:(id)result {
    BOOL testPassed = YES;
    if (statusCode >= 400) {
        // expected error
        if (error) {
            [test addLog:[NSString stringWithFormat:@"Expected error, received: %@", error]];
            NSInteger actualStatusCode = [((NSHTTPURLResponse *)response) statusCode];
            if (statusCode == actualStatusCode) {
                [test addLog:@"Received expected status code"];
            } else {
                [test addLog:[NSString stringWithFormat:@"Received incorrect status code. Expected %d, received %d", statusCode, actualStatusCode]];
                testPassed = NO;
            }
        } else {
            [test addLog:[NSString stringWithFormat:@"Error, expected error, but no error was received. Result: %@", result]];
            testPassed = NO;
        }
        
        if (!testPassed) {
            [test addLog:@"Got expected non-2XX result."];
        }
        
    } else {
        if (error) {
            [test addLog:[NSString stringWithFormat:@"Unexpected error: %@", error]];
            NSHTTPURLResponse *errorResponse = [[error userInfo] objectForKey:MSErrorResponseKey];
            [test addLog:[NSString stringWithFormat:@"Response status code: %d; header: %@", errorResponse.statusCode, errorResponse.allHeaderFields]];
            testPassed = NO;
        }
    }
    
    return testPassed;
}

+ (NSString *)formatToString:(ApiDataFormat)format {
    switch (format) {
        case DataFormatJson:
            return @"json";
        case DataFormatXml:
            return @"xml";
        default:
            return @"other";
    }
}

+ (NSString *)convertToXmlFromJson:(NSDictionary *)json {
    return [NSString stringWithFormat:@"<root>%@</root>", [self toXmlFromJson:json]];
}

+ (NSString *)toXmlFromJson:(id)json {
    if (!json || [json isKindOfClass:[NSNull class]]) return @"null";
    if ([json isKindOfClass:[NSString class]]) {
        return (NSString *)json;
    }
    if ([json isKindOfClass:[NSNumber class]]) {
        const char *cType = [json objCType];
        if (strcmp(@encode(BOOL), cType) == 0) {
            return [json boolValue] ? @"true" : @"false";
        } else {
            return [json description];
        }
    }
    if ([json isKindOfClass:[NSArray class]]) {
        NSMutableArray *xmlArray = [[NSMutableArray alloc] init];
        for (id item in json) {
            [xmlArray addObject:[NSString stringWithFormat:@"<item>%@</item>", [self toXmlFromJson:item]]];
        }
        return [NSString stringWithFormat:@"<array>%@</array>", [xmlArray componentsJoinedByString:@""]];
    }
    if ([json isKindOfClass:[NSDictionary class]]) {
        NSDictionary *jsonObject = json;
        NSArray *keys = [[jsonObject allKeys] sortedArrayUsingComparator:^NSComparisonResult(id obj1, id obj2) {
            NSString *str1 = obj1;
            NSString *str2 = obj2;
            return [str1 compare:str2];
        }];
        NSMutableArray *xmlArray = [[NSMutableArray alloc] init];
        for (NSString *key in keys) {
            [xmlArray addObject:[NSString stringWithFormat:@"<%@>%@</%@>", key, [self toXmlFromJson:jsonObject[key]], key]];
        }
        return [xmlArray componentsJoinedByString:@""];
    }
    return [NSString stringWithFormat:@"Error, invalid JSON type: %@", json];
}

+ (ZumoTest *)createDataBasedTestWithName:(NSString *)testName apiName:(NSString *)apiName httpMethod:(NSString *)httpMethod body:(id)body headers:(NSDictionary *)headers query:(NSDictionary *)query statusCode:(NSInteger)statusCode inputFormat:(ApiDataFormat)inputFormat outputFormat:(ApiDataFormat)outputFormat {
    
    NSMutableDictionary *queryParameters = [[NSMutableDictionary alloc] initWithDictionary:query copyItems:YES];
    [queryParameters setObject:[self formatToString:outputFormat] forKey:@"format"];
    if (statusCode != 200) {
        [queryParameters setObject:[NSNumber numberWithInteger:statusCode] forKey:@"status"];
    }
    
    NSMutableDictionary *requestHeaders = [[NSMutableDictionary alloc] initWithDictionary:headers copyItems:YES];
    
    testName = [NSString stringWithFormat:@"NSData-style selector: %@", testName];
    
    return [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        NSData *requestBody = nil;
        if (body) {
            if ([body isKindOfClass:[NSString class]]) {
                requestBody = [((NSString *)body) dataUsingEncoding:NSUTF8StringEncoding];
            } else {
                // assuming a JSON body
                NSError *jsonError;
                requestBody = [NSJSONSerialization dataWithJSONObject:body options:0 error:&jsonError];
                if (jsonError) {
                    [test addLog:[NSString stringWithFormat:@"Error converting body to JSON: %@", jsonError]];
                    completion(NO);
                    return;
                }
            }
            
            switch (inputFormat) {
                case DataFormatJson:
                    [requestHeaders setObject:@"application/json" forKey:@"Content-Type"];
                    break;
                    
                case DataFormatXml:
                    [requestHeaders setObject:@"text/xml" forKey:@"Content-Type"];
                    break;
                    
                default:
                    [requestHeaders setObject:@"text/plain" forKey:@"Content-Type"];
                    break;
            }
        }

        [client invokeAPI:apiName data:requestBody HTTPMethod:httpMethod parameters:queryParameters headers:requestHeaders completion:^(NSData *result, NSHTTPURLResponse *httpResponse, NSError *error) {

            BOOL testPassed = YES;
            if (error && !httpResponse) {
                httpResponse = [[error userInfo] objectForKey:MSErrorResponseKey];
            }
            
            if (![self validateErrorForTest:test error:error expectedStatus:statusCode response:httpResponse result:result]) {
                completion(NO);
                return;
            }
            
            if (![self validateResponseHeadersForTest:test expectedHeaders:headers httpResponse:httpResponse]) {
                completion(NO);
                return;
            }

            NSMutableDictionary *expectedBody = [self createExpectedBodyForTest:test httpMethod:httpMethod query:query body:body];
            if (outputFormat == DataFormatJson || outputFormat == DataFormatOther) {
                NSMutableArray *errors = [[NSMutableArray alloc] init];
                NSError *jsonError;
                if (outputFormat == DataFormatOther) {
                    // need to unescape the brackets and curly braces
                    NSString *temp = [[NSString alloc] initWithData:result encoding:NSUTF8StringEncoding];
                    [test addLog:[NSString stringWithFormat:@"Actual response: %@", temp]];
                    temp = [temp stringByReplacingOccurrencesOfString:@"__[__" withString:@"["];
                    temp = [temp stringByReplacingOccurrencesOfString:@"__]__" withString:@"]"];
                    temp = [temp stringByReplacingOccurrencesOfString:@"__{__" withString:@"{"];
                    temp = [temp stringByReplacingOccurrencesOfString:@"__}__" withString:@"}"];
                    [test addLog:[NSString stringWithFormat:@"Unescaped response: %@", temp]];
                    result = [temp dataUsingEncoding:NSUTF8StringEncoding];
                }
                
                NSData *responseBodyData = error == nil ? result : [[error localizedDescription] dataUsingEncoding:NSUTF8StringEncoding];
                id resultJson = [NSJSONSerialization JSONObjectWithData:responseBodyData options:0 error:&jsonError];
                if (jsonError) {
                    [test addLog:[NSString stringWithFormat:@"Error converting expected body to JSON: %@", jsonError]];
                    completion(NO);
                    return;
                }
                if ([ZumoTestGlobals compareJson:expectedBody with:resultJson log:errors]) {
                    [test addLog:@"Comparison succeeded"];
                } else {
                    [test addLog:[NSString stringWithFormat:@"Error comparing response. Expected: %@, actual: %@", expectedBody, resultJson]];
                    testPassed = NO;
                    for (NSString *log in errors) {
                        [test addLog:log];
                    }
                }
            } else {
                NSString *expectedBodyString = [self convertToXmlFromJson:expectedBody];
                NSString *actualResponseBody = error == nil ? [[NSString alloc] initWithData:result encoding:NSUTF8StringEncoding] : [error localizedDescription];
                if ([actualResponseBody isEqualToString:expectedBodyString]) {
                    [test addLog:@"Result is expected"];
                } else {
                    testPassed = NO;
                    [test addLog:[NSString stringWithFormat:@"Error comparing response body. Expected: %@; actual: %@", expectedBodyString, actualResponseBody]];
                }
            }
            
            completion(testPassed);
        }];
    }];
}

+ (NSString *)groupDescription {
    return @"Tests to validate that the client SDK can correctly invoke custom APIs on the mobile service.";
}

@end
