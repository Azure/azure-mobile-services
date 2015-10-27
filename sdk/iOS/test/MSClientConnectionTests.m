// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSClientConnection.h"
#import "MSClient.h"
#import "MSTableRequest.h"
#import "MSAPIRequest.h"

@interface MSClientConnectionTests : XCTestCase

@end

@implementation MSClientConnectionTests

- (void)setUp {
    [super setUp];
    // Put setup code here. This method is called before the invocation of each test method in the class.
}

- (void)tearDown {
    // Put teardown code here. This method is called after the invocation of each test method in the class.
    [super tearDown];
}

-(void)testDefaultHeaders
{
    NSURLRequest *request = [NSURLRequest requestWithURL:[NSURL URLWithString:@"http://someURL.com"]];
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    MSClientConnection *clientConnection = [[MSClientConnection alloc] initWithRequest:request
                                                                                client:client
                                                                            completion:nil];
    
    XCTAssertNotNil(clientConnection);
    XCTAssertNil(request.allHTTPHeaderFields[@"ZUMO-API-VERSION"]);
}

-(void)testTableRequestHeaders
{
    MSTableRequest *request = [MSTableRequest requestWithURL:[NSURL URLWithString:@"http://someURL.com"]];
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    MSClientConnection *clientConnection = [[MSClientConnection alloc] initWithRequest:request
                                                                                client:client
                                                                            completion:nil];
    
    XCTAssertNotNil(clientConnection);
    XCTAssertEqualObjects(clientConnection.request.allHTTPHeaderFields[@"ZUMO-API-VERSION"], @"2.0.0");
}

-(void)testApiRequestHeaders
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    MSAPIRequest *request = [MSAPIRequest requestToinvokeAPI:@"test"
                                                        client:client
                                                          body:nil
                                                    HTTPMethod:@"GET"
                                                    parameters:nil
                                                       headers:nil
                                                    completion:nil];
    
    MSClientConnection *clientConnection = [[MSClientConnection alloc] initWithRequest:request
                                                                                client:client
                                                                            completion:nil];
    
    XCTAssertNotNil(clientConnection);
    XCTAssertEqualObjects(clientConnection.request.allHTTPHeaderFields[@"ZUMO-API-VERSION"], @"2.0.0");
}

@end