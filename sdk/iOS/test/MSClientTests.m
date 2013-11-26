// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSClient.h"
#import "MSTestFilter.h"

@interface MSClientTests : SenTestCase {
    BOOL done;
}

@end


@implementation MSClientTests


#pragma mark * Setup and TearDown


-(void) setUp
{
    NSLog(@"%@ setUp", self.name);
    
    done = NO;
}

-(void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * Static Constructor Method Tests


-(void) testStaticConstructor1ReturnsClient
{
    MSClient *client =
    [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNil(client.applicationKey, @"client.applicationKey should be nil.");
}

-(void) testStaticConstructor2ReturnsClient
{
    MSClient *client =
    [MSClient clientWithApplicationURLString:@"http://someURL.com"
                          applicationKey:@"here is some key"];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNotNil(client.applicationKey, @"client.applicationKey should not be nil.");
    STAssertTrue([client.applicationKey isEqualToString:@"here is some key"],
                 @"The client should be using the url it was created with.");
}

-(void) testStaticConstructor3ReturnsClient
{
    NSURL *appURL = [NSURL URLWithString:@"http://someURL.com"];
    
    MSClient *client =
    [MSClient clientWithApplicationURL:appURL];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNil(client.applicationKey, @"client.applicationKey should be nil.");
}

-(void) testStaticConstructor4ReturnsClient
{
    NSURL *appURL = [NSURL URLWithString:@"http://someURL.com"];
    
    MSClient *client =
    [MSClient clientWithApplicationURL:appURL
                    applicationKey:@"here is some key"];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNotNil(client.applicationKey, @"client.applicationKey should not be nil.");
    STAssertTrue([client.applicationKey isEqualToString:@"here is some key"],
                 @"The client should be using the url it was created with.");
}

-(void) testStaticConstructorPercentEncodesURL
{
    MSClient *client =
        [MSClient clientWithApplicationURLString:@"http://yeah! .com"];
    
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://yeah!%20.com"],
                 @"The client should have encoded and normalized the url: %@", urlString);
}


#pragma mark * Init Method Tests


-(void) testInitWithApplicationURL
{
    NSURL *appURL = [NSURL URLWithString:@"http://someURL.com"];
    
    MSClient *client = [[MSClient alloc] initWithApplicationURL:appURL];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNil(client.applicationKey, @"client.applicationKey should be nil.");
}

-(void) testInitWithApplicationURLAllowsNilURL
{    
    MSClient *client = [[MSClient alloc] initWithApplicationURL:nil];
    
    STAssertNotNil(client, @"client should not be nil.");
    STAssertNil(client.applicationURL, @"client.applicationURL should be nil.");
    STAssertNil(client.applicationKey, @"client.applicationKey should be nil.");
}

-(void) testInitWithApplicationURLAndApplicationKey
{
    NSURL *appURL = [NSURL URLWithString:@"http://someURL.com"];
    
    MSClient *client =
    [[MSClient alloc] initWithApplicationURL:appURL
                    applicationKey:@"here is some key"];
    
    STAssertNotNil(client, @"client should not be nil.");
    
    STAssertNotNil(client.applicationURL, @"client.applicationURL should not be nil.");
    NSString *urlString = [client.applicationURL absoluteString];
    STAssertTrue([urlString isEqualToString:@"http://someURL.com"],
                 @"The client should be using the url it was created with.");
    
    STAssertNotNil(client.applicationKey, @"client.applicationKey should not be nil.");
    STAssertTrue([client.applicationKey isEqualToString:@"here is some key"],
                 @"The client should be using the url it was created with.");
}


#pragma mark * Table Method Tests


-(void) testTableWithNameReturnsTable
{
    MSClient *client =
    [MSClient clientWithApplicationURLString:@"http://someURL.com"];

    MSTable *table = [client tableWithName:@"Some Table Name"];
    
    STAssertNotNil(table, @"table should not be nil.");
}

-(void) testTableWithNameAllowsNilTableName
{
    MSClient *client =
    [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    MSTable *table = [client tableWithName:nil];
    
    STAssertNotNil(table, @"table should not be nil.");
}


#pragma mark * Invoke Api Method Tests


-(void) testInvokeAPISetsCorrectUrlMethodAndHeaders
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    __block NSURLRequest *actualRequest = nil;
    
    // Use the filter to capture the request being sent
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.responseToUse = [[NSHTTPURLResponse alloc] initWithURL:nil
                                                           statusCode:200
                                                          HTTPVersion:nil
                                                         headerFields:nil];
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    // Create a client that uses the filter
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient invokeAPI:@"someAPI"
                       body:nil
                 HTTPMethod:@"Get"
                 parameters:@{ @"x" : @24 }
                    headers:@{ @"someHeader" : @"someValue" }
                 completion:
     ^(id result, NSURLResponse *response, NSError *error) {
         
         STAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
         STAssertNil(error, @"error should have been nil.");
               
         NSString *actualUrl = actualRequest.URL.absoluteString;
         STAssertTrue([actualUrl isEqualToString:@"http://someURL.com/api/someAPI?x=24"],
                      @"URL was not as expected.");
         
         NSString *actualHeader = [actualRequest.allHTTPHeaderFields valueForKey:@"someHeader"];
         STAssertNotNil(actualHeader, @"actualHeader should not have been nil.");
         STAssertTrue([actualHeader isEqualToString:@"someValue"],
                      @"Header value was not as expected.");
         
         NSString *actualMethod = actualRequest.HTTPMethod;
         STAssertNotNil(actualMethod, @"actualMethod should not have been nil.");
         STAssertTrue([actualMethod isEqualToString:@"GET"],
                      @"HTTP Method was not as expected.");
         
         done = YES;
     }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInvokeAPISerializesAsJson
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    __block NSURLRequest *actualRequest = nil;
    
    // Use the filter to capture the request being sent
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.responseToUse = [[NSHTTPURLResponse alloc] initWithURL:nil
                                                           statusCode:200
                                                          HTTPVersion:nil
                                                         headerFields:nil];
    
    testFilter.dataToUse = [@"{\"id\":5,\"name\":\"bob\"}"
                            dataUsingEncoding:NSUTF8StringEncoding];
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    // Create a client that uses the filter
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient invokeAPI:@"someAPI"
                       body:@{@"id":@1, @"name":@"jim" }
                 HTTPMethod:@"Get"
                 parameters:nil
                    headers:nil
                 completion:
     ^(id result, NSURLResponse *response, NSError *error) {
         
         STAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
         STAssertNil(error, @"error should have been nil.");
         
         NSData *actualBody = actualRequest.HTTPBody;
         NSString *bodyString = [[NSString alloc] initWithData:actualBody
                                                      encoding:NSUTF8StringEncoding];
         STAssertTrue([bodyString isEqualToString:@"{\"id\":1,\"name\":\"jim\"}"],
                      @"The body was not serialized as expected.");
         
         STAssertNotNil(result, @"result should not have been nil.");
         STAssertTrue([[result valueForKey:@"id"] isEqual:@5], @"The id should have been 5");
         STAssertTrue([[result valueForKey:@"name"] isEqualToString:@"bob"], @"The name should have been 'bob'");
         
         done = YES;
     }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInvokeAPIPassesAlongData
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    __block NSURLRequest *actualRequest = nil;
    
    NSData *testData = [@"{\"id\":5,\"name\":\"bob\"}"
                        dataUsingEncoding:NSUTF8StringEncoding];
    
    // Use the filter to capture the request being sent
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.responseToUse = [[NSHTTPURLResponse alloc] initWithURL:nil
                                                           statusCode:200
                                                          HTTPVersion:nil
                                                         headerFields:nil];
    testFilter.dataToUse = testData;
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    // Create a client that uses the filter
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient invokeAPI:@"someAPI"
                       data:testData
                 HTTPMethod:@"Get"
                 parameters:nil
                    headers:nil
                 completion:
     ^(NSData *result, NSURLResponse *response, NSError *error) {
         
         STAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
         STAssertNil(error, @"error should have been nil.");
         
         NSData *actualBody = actualRequest.HTTPBody;
         STAssertNotNil(actualBody, @"actualBody should not have been nil.");
         STAssertEqualObjects(actualBody, testData, @"Should be the same data instance.");
         
         STAssertNotNil(result, @"result should not have been nil.");
         STAssertEqualObjects(result, testData, @"Should be the same data instance.");

         NSString *contentType = [actualRequest.allHTTPHeaderFields valueForKey:@"Content-Type"];
         STAssertNotNil(contentType, @"contentType should not have been nil.");
         STAssertTrue([contentType isEqualToString:@"application/json"],
                                                   @"Content-Type was not as expected.");
         done = YES;
     }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInvokeAPIHonorsContentTypeIfSet
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    __block NSURLRequest *actualRequest = nil;
    
    NSData *testData = [@"{\"id\":5,\"name\":\"bob\"}"
                        dataUsingEncoding:NSUTF8StringEncoding];
    
    // Use the filter to capture the request being sent
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.responseToUse = [[NSHTTPURLResponse alloc] initWithURL:nil
                                                           statusCode:200
                                                          HTTPVersion:nil
                                                         headerFields:nil];
    testFilter.dataToUse = testData;
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    // Create a client that uses the filter
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient invokeAPI:@"someAPI"
                       data:testData
                 HTTPMethod:@"Get"
                 parameters:nil
                    headers:@{@"Content-Type":@"text/json"}
                 completion:
     ^(NSData *result, NSURLResponse *response, NSError *error) {
         
         STAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
         STAssertNil(error, @"error should have been nil.");
         
         NSData *actualBody = actualRequest.HTTPBody;
         STAssertNotNil(actualBody, @"actualBody should not have been nil.");
         STAssertEqualObjects(actualBody, testData, @"Should be the same data instance.");
         
         STAssertNotNil(result, @"result should not have been nil.");
         STAssertEqualObjects(result, testData, @"Should be the same data instance.");
         
         NSString *contentType = [actualRequest.allHTTPHeaderFields valueForKey:@"Content-Type"];
         STAssertNotNil(contentType, @"contentType should not have been nil.");
         STAssertTrue([contentType isEqualToString:@"text/json"],
                                                   @"Content-Type was not as expected.");
         
         done = YES;
     }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInvokeAPIAllowsNilAPIName
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    __block NSURLRequest *actualRequest = nil;
    
    // Use the filter to capture the request being sent
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.responseToUse = [[NSHTTPURLResponse alloc] initWithURL:nil
                                                           statusCode:200
                                                          HTTPVersion:nil
                                                         headerFields:nil];
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    // Create a client that uses the filter
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient invokeAPI:nil
                       body:nil
                 HTTPMethod:@"Get"
                 parameters:nil
                    headers:nil
                 completion:
     ^(id result, NSURLResponse *response, NSError *error) {
         
         STAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
         STAssertNil(error, @"error should have been nil.");
         
         NSString *actualUrl = actualRequest.URL.absoluteString;
         STAssertTrue([actualUrl isEqualToString:@"http://someURL.com/api/"],
                      @"URL was not as expected.");
         
         done = YES;
     }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInvokeAPIAllowsNilData
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    __block NSURLRequest *actualRequest = nil;
    
    // Use the filter to capture the request being sent
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.responseToUse = [[NSHTTPURLResponse alloc] initWithURL:nil
                                                           statusCode:200
                                                          HTTPVersion:nil
                                                         headerFields:nil];
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    // Create a client that uses the filter
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient invokeAPI:@"someApi"
                       data:nil
                 HTTPMethod:@"Get"
                 parameters:nil
                    headers:nil
                 completion:
     ^(NSData *result, NSURLResponse *response, NSError *error) {
         
         STAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
         STAssertNil(error, @"error should have been nil.");
         
         STAssertNil(actualRequest.HTTPBody, @"body should have been nil.");
         
         done = YES;
     }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInvokeAPIAllowsNilMethod
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    __block NSURLRequest *actualRequest = nil;
    
    // Use the filter to capture the request being sent
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.responseToUse = [[NSHTTPURLResponse alloc] initWithURL:nil
                                                           statusCode:200
                                                          HTTPVersion:nil
                                                         headerFields:nil];
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    // Create a client that uses the filter
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient invokeAPI:@"someApi"
                       body:nil
                 HTTPMethod:nil
                 parameters:nil
                    headers:nil
                 completion:
     ^(id result, NSURLResponse *response, NSError *error) {
         
         STAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
         STAssertNil(error, @"error should have been nil.");
         
         NSString *actualUrl = actualRequest.URL.absoluteString;
         STAssertTrue([actualUrl isEqualToString:@"http://someURL.com/api/someApi"],
                      @"URL was not as expected.");
         
         STAssertTrue([actualRequest.HTTPMethod isEqualToString:@"POST"], @"The default HTTP method should have been 'POST'");
         
         done = YES;
     }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testInvokeReturnsErrorFor400OrGreater
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];

    // Use the filter to capture the request being sent
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.responseToUse = [[NSHTTPURLResponse alloc] initWithURL:nil
                                                           statusCode:500
                                                          HTTPVersion:nil
                                                         headerFields:nil];
    NSString* stringData = @"This is the error msg.";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.dataToUse = data;
    
    // Create a client that uses the filter
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient invokeAPI:@"someApi"
                       data:nil
                 HTTPMethod:@"GET"
                 parameters:nil
                    headers:nil
                 completion:
     ^(NSData *data, NSURLResponse *response, NSError *error) {
         
         STAssertNil(response, @"response should have been nil.");
         STAssertNotNil(error, @"error should not have been nil.");
         STAssertTrue([[error localizedDescription] isEqualToString:
                        @"This is the error msg."],
                        @"error description was: %@", [error localizedDescription]);
         done = YES;
     }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

- (void) testInvokeApiAllowsArrayDataOut
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    __block NSURLRequest *actualRequest = nil;

    // Use the filter to capture the request being sent
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    testFilter.ignoreNextFilter = YES;
    testFilter.responseToUse = [[NSHTTPURLResponse alloc] initWithURL:nil
                                                           statusCode:200
                                                          HTTPVersion:nil
                                                         headerFields:nil];
    
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        actualRequest = request;
        return request;
    };
    
    // Create a client that uses the filter
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient invokeAPI:@"somApi"
                       body:@[@"apple", @"orange", @"banana"]
                 HTTPMethod:@"Get"
                 parameters:nil
                    headers:nil
                 completion:
     ^(id result, NSHTTPURLResponse *response, NSError *error) {
         NSString *bodyString = [[NSString alloc] initWithData:actualRequest.HTTPBody
                                                      encoding:NSUTF8StringEncoding];
         
         STAssertNotNil(actualRequest, @"actualRequest should not have been nil.");
         STAssertNil(error, @"error should have been nil.");
         STAssertEqualObjects(@"[\"apple\",\"orange\",\"banana\"]", bodyString, @"Unexpected body found: %@", bodyString);
         
         done = YES;
     }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

#pragma mark * Async Test Helper Method


-(BOOL) waitForTest:(NSTimeInterval)testDuration {
    
    NSDate *timeoutAt = [NSDate dateWithTimeIntervalSinceNow:testDuration];
    
    while (!done) {
        [[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode
                                 beforeDate:timeoutAt];
        if([timeoutAt timeIntervalSinceNow] <= 0.0) {
            break;
        }
    };
    
    return done;
}

@end
