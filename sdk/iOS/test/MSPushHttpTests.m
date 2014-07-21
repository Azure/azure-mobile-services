// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSClient.h"
#import "MSPushHttp.h"
#import "MSTestFilter.h"

@interface MSPushHttpTests : SenTestCase
@property (nonatomic) BOOL done;
@property (nonatomic) MSClient *client;
@property (nonatomic) NSURL *url;
@end

@implementation MSPushHttpTests

- (void)setUp
{
    [super setUp];
    self.url = [[NSURL alloc] initWithString:@"https://aurl.azure-mobilefake.net"];
    self.client = [[MSClient alloc] initWithApplicationURL:self.url applicationKey:@"QdffoEwYCblcmkvbInMEkEoSemgJHm31"];
    self.done = NO;
}

- (void)tearDown
{
    // Put teardown code here. This method is called after the invocation of each test method in the class.
    [super tearDown];
}

- (void)testCreateRegistrationId
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    NSString *expectedRegistrationId = @"8313603759421994114-6468852488791307573-9";
    NSURL *locationUrl = [[self.url URLByAppendingPathComponent:@"push/registrations/"] URLByAppendingPathComponent:expectedRegistrationId];
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:201
                                   HTTPVersion:nil
                                   headerFields:@{@"Location":[locationUrl absoluteString]}];
    
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        STAssertTrue([request.HTTPMethod isEqualToString:@"POST"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[self.url URLByAppendingPathComponent:@"push/registrationids"] absoluteString];
        STAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        return request;
    };

    MSClient *filteredClient = [self.client clientWithFilter:testFilter];

    MSPushHttp *pushHttp = [[MSPushHttp alloc] initWithClient:filteredClient];
    [pushHttp createRegistrationId:^(NSString *registrationId, NSError *error) {
        STAssertTrue([expectedRegistrationId isEqualToString:registrationId],
                     @"Expected registrationId to be parsed correctly from returned header.");
        self.done = YES;
    }];
    
    STAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testCreateRegistration
{
    // Create the registration
    NSMutableDictionary *registration = [[NSMutableDictionary alloc] init];
    NSArray *tags = @[@"tag1", @"tag2"];
    [registration setValue:@"59d31b14081b92daa98fad91edc0e61fc23767d5b90892c4f22df56e312045c8" forKey:@"deviceId"];
    [registration setValue:@"apns" forKey:@"platform"];
    [registration setValue:tags forKey:@"tags"];
    NSString *registrationId = @"8313603759421994114-6468852488791307573-9";
    [registration setValue:registrationId forKey:@"registrationId"];
    
    // Create the test filter to allow testing request and returning response without connecting to service
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:204
                                   HTTPVersion:nil headerFields:nil];
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;

    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        STAssertTrue([request.HTTPMethod isEqualToString:@"PUT"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:registrationId] absoluteString];
        STAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        NSString *httpBody = [[NSString alloc] initWithData:request.HTTPBody encoding:NSUTF8StringEncoding];
        STAssertTrue([httpBody rangeOfString:registrationId].location == NSNotFound, @"The request body should not included registrationId.");
        STAssertTrue([httpBody rangeOfString:@"\"deviceId\":\"59d31b14081b92daa98fad91edc0e61fc23767d5b90892c4f22df56e312045c8\""].location != NSNotFound,
                     @"The request body should include deviceId.");
        STAssertTrue([httpBody rangeOfString:@"\"platform\":\"apns\""].location != NSNotFound, @"The request body should include platform.");
        STAssertTrue([httpBody rangeOfString:@"\"tags\":[\"tag1\",\"tag2\"]"].location != NSNotFound, @"The request body should include tags.");
        return request;
    };
    
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];

    MSPushHttp *pushHttp = [[MSPushHttp alloc] initWithClient:filteredClient];
    
    [pushHttp upsertRegistration:registration
                      completion:^(NSError *error) {
                          STAssertNil(error, @"error is expected to be nil.");
                          self.done = YES;
                      }];
    
    STAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testListRegistrations
{
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    NSString* stringData = @"[{\"registrationId\": \"8313603759421994114-6468852488791307573-9\", \"deviceId\":\"59d31b14081b92daa98fad91edc0e61fc23767d5b90892c4f22df56e312045c8\", \"tags\":[\"tag1\",\"tag2\"]}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.dataToUse = data;
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        STAssertTrue([request.HTTPMethod isEqualToString:@"GET"], @"Expected request HTTPMethod to be GET.");
        NSString *expectedQueryAndPath = @"push/registrations?deviceId=59d31b14081b92daa98fad91edc0e61fc23767d5b90892c4f22df56e312045c8&platform=apns";
        NSString *expectedUrl = [[self.url URLByAppendingPathComponent:expectedQueryAndPath]  absoluteString];
        STAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri %@, but got %@.", expectedUrl, [request URL]);
        
        return request;
    };
    
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    MSPushHttp *pushHttp = [[MSPushHttp alloc] initWithClient:filteredClient];
    [pushHttp registrationsForDeviceToken:@"59d31b14081b92daa98fad91edc0e61fc23767d5b90892c4f22df56e312045c8"
                     completion:^(NSArray *registrations, NSError *error) {
                         STAssertNil(error, @"error is expected to be nil.");
                         STAssertTrue([registrations[0][@"deviceId"] isEqualToString:@"59d31b14081b92daa98fad91edc0e61fc23767d5b90892c4f22df56e312045c8"],
                                     @"deviceId is expected to deserialize correctly.");
                         STAssertTrue([registrations[0][@"registrationId"] isEqualToString:@"8313603759421994114-6468852488791307573-9"],
                                      @"registrationId is expected to deserialize correctly.");
                         STAssertTrue([registrations[0][@"tags"][0] isEqualToString:@"tag1"],
                                      @"tag1 is expected to deserialize correctly.");
                         STAssertTrue([registrations[0][@"tags"][1] isEqualToString:@"tag2"],
                                      @"tag2 is expected to deserialize correctly.");
                         self.done = YES;
                      }];
    
    STAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testDeleteRegistrations
{
    NSString *registrationId = @"8313603759421994114-6468852488791307573-9";
    
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    testFilter.responseToUse = response;
    testFilter.ignoreNextFilter = YES;
    testFilter.onInspectRequest = ^(NSURLRequest *request) {
        STAssertTrue([request.HTTPMethod isEqualToString:@"DELETE"], @"Expected request HTTPMethod to be DELETE.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:registrationId] absoluteString];
        STAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        return request;
    };
    
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    MSPushHttp *pushHttp = [[MSPushHttp alloc] initWithClient:filteredClient];
    [pushHttp deleteRegistrationById:registrationId
                      completion:^(NSError *error) {
                          STAssertNil(error, @"error is expected to be nil.");
                          self.done = YES;
                      }];
    
    STAssertTrue([self waitForTest:15], @"Test timed out.");
}

#pragma mark * Async Test Helper Method


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

@end
