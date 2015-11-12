// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSClient.h"
#import "MSMultiRequestTestFilter.h"
#import "MSPush.h"
#import "MSClientInternal.h"

@interface MSPushTests : XCTestCase
@property (nonatomic) BOOL done;
@property (nonatomic) MSClient *client;
@property (nonatomic) NSURL *url;
@end

@implementation MSPushTests

- (void)setUp
{
    [super setUp];
    self.url = [[NSURL alloc] initWithString:@"https://aurl.azure-mobilefake.net"];
    self.client = [[MSClient alloc] initWithApplicationURL:self.url];

    self.done = NO;
}

- (void)tearDown
{
    // Put teardown code here. This method is called after the invocation of each test method in the class.
    [super tearDown];
}

-(void) testRegisterWithoutTemplateSuccess
{
    MSTestFilter *filter = [MSTestFilter testFilterWithStatusCode:201];
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    
    NSURL *expectedURL = [self.url URLByAppendingPathComponent:@"push/installations"];
    expectedURL = [expectedURL URLByAppendingPathComponent:self.client.installId];
    
    filter.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertEqualObjects(request.HTTPMethod, @"PUT");
        XCTAssertEqualObjects(request.URL, expectedURL);
        
        NSString *bodyString = [[NSString alloc] initWithData:request.HTTPBody
                                                     encoding:NSUTF8StringEncoding];
        XCTAssertEqualObjects(bodyString, @"{\"platform\":\"apns\",\"pushChannel\":\"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8\"}");
        return request;
    };
    
    MSClient *testClient = [self.client clientWithFilter:filter];
    
    
    [testClient.push registerDeviceToken:deviceToken completion:^(NSError *error) {
        XCTAssertNil(error);
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

-(void) testRegisterWithoutTemplateNoTokenError
{
    MSTestFilter *filter = [MSTestFilter testFilterWithStatusCode:201];
    
    MSClient *testClient = [self.client clientWithFilter:filter];
    
    [testClient.push registerDeviceToken:nil completion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSPushRequiredParameter);
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

-(void) testRegisterWithoutTemplateHTTPError
{
    MSTestFilter *filter = [MSTestFilter testFilterWithStatusCode:400];
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    
    MSClient *testClient = [self.client clientWithFilter:filter];
    
    [testClient.push registerDeviceToken:deviceToken completion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSErrorNoMessageErrorCode);
        NSHTTPURLResponse *response = error.userInfo[MSErrorResponseKey];
        XCTAssertNotNil(response);
        XCTAssertEqual(response.statusCode, 400);
        
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

-(void) testRegisterWithoutTemplateError
{
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    MSTestFilter *filter = [MSTestFilter testFilterWithStatusCode:404];
    
    MSClient *testClient = [self.client clientWithFilter:filter];
    
    [testClient.push registerDeviceToken:deviceToken completion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSErrorNoMessageErrorCode);
        NSHTTPURLResponse *response = error.userInfo[MSErrorResponseKey];
        XCTAssertNotNil(response);
        XCTAssertEqual(response.statusCode, 404);
        
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

-(void) testRegisterTemplateSuccess
{
    MSTestFilter *filter = [MSTestFilter testFilterWithStatusCode:201];
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    
    NSURL *expectedURL = [self.url URLByAppendingPathComponent:@"push/installations"];
    expectedURL = [expectedURL URLByAppendingPathComponent:self.client.installId];
    
    filter.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertEqualObjects(request.HTTPMethod, @"PUT");
        XCTAssertEqualObjects(request.URL, expectedURL);
        
        NSString *bodyString = [[NSString alloc] initWithData:request.HTTPBody
                                                     encoding:NSUTF8StringEncoding];
        
        
        // Check the core components of the body, as 32 v 64 format differently
        XCTAssertTrue([bodyString containsString:@"\"platform\":\"apns\""]);
        XCTAssertTrue([bodyString containsString:@"\"pushChannel\":\"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8\""]);
        XCTAssertTrue([bodyString containsString:@"\"templates\":{"]);
        XCTAssertTrue([bodyString containsString:@"\"t2\":{\"body\":\"{\\\"aps\\\":{\\\"alert\\\":\\\"$(message)\\\"}}\"}"]);
        XCTAssertTrue([bodyString containsString:@"\"t1\":{\"body\":\"{\\\"aps\\\":{\\\"alert\\\":\\\"$(message)\\\""]);
        
        return request;
    };
    
    MSClient *testClient = [self.client clientWithFilter:filter];
    
    NSDictionary *template = @{
            @"t1": @{ @"body" : @{ @"aps" : @{ @"alert": @"$(message)" } } },
            @"t2": @{ @"body": @"{\"aps\":{\"alert\":\"$(message)\"}}" }
        };
    
    [testClient.push registerDeviceToken:deviceToken template:template completion:^(NSError *error) {
        XCTAssertNil(error);
        
        // Verify we didn't mess with input template
        id t1Body = template[@"t1"][@"body"];
        XCTAssertTrue([t1Body isKindOfClass:[NSDictionary class]]);
        
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

-(void) testUnregisterSuccess
{
    MSTestFilter *filter = [MSTestFilter testFilterWithStatusCode:204];
    NSURL *expectedURL = [self.url URLByAppendingPathComponent:@"push/installations"];
    expectedURL = [expectedURL URLByAppendingPathComponent:self.client.installId];
    
    filter.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertEqualObjects(request.HTTPMethod, @"DELETE");
        XCTAssertEqualObjects(request.URL, expectedURL);
        
        return request;
    };
    
    MSClient *testClient = [self.client clientWithFilter:filter];
    
    [testClient.push unregisterWithCompletion:^(NSError *error) {
        XCTAssertNil(error);
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

-(void) testUnregisterHTTPError
{
    MSTestFilter *filter = [MSTestFilter testFilterWithStatusCode:400];
    NSURL *expectedURL = [self.url URLByAppendingPathComponent:@"push/installations"];
    expectedURL = [expectedURL URLByAppendingPathComponent:self.client.installId];
    
    MSClient *testClient = [self.client clientWithFilter:filter];
    [testClient.push unregisterWithCompletion:^(NSError *error) {
        XCTAssertNotNil(error);
        XCTAssertEqual(error.code, MSErrorNoMessageErrorCode);
        NSHTTPURLResponse *response = error.userInfo[MSErrorResponseKey];
        XCTAssertNotNil(response);
        XCTAssertEqual(response.statusCode, 400);
        
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

-(BOOL) waitForTest:(NSTimeInterval)testDuration {
    
    NSDate *timeoutAt = [NSDate dateWithTimeIntervalSinceNow:testDuration];
    
    while (!self.done) {
        [[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode
                                 beforeDate:timeoutAt];
        if([timeoutAt timeIntervalSinceNow] <= 0.0) {
            break;
        }
    };
    
    return self.done;
}

-(NSData*) bytesFromHexString:(NSString *)hexString;
{
    NSMutableData* data = [NSMutableData data];
    for (int idx = 0; idx+2 <= hexString.length; idx+=2) {
        NSRange range = NSMakeRange(idx, 2);
        NSString* hexStr = [hexString substringWithRange:range];
        NSScanner* scanner = [NSScanner scannerWithString:hexStr];
        unsigned int intValue;
        if ([scanner scanHexInt:&intValue])
            [data appendBytes:&intValue length:1];
    }
    return data;
}

@end
