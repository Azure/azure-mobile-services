// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSClient.h"
#import "MSMultiRequestTestFilter.h"
#import "MSPush.h"

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
    self.client = [[MSClient alloc] initWithApplicationURL:self.url applicationKey:@"QdffoEwYCblcmkvbInMEkEoSemgJHm31"];

    // Clear storage
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    for (NSString* key in [[defaults dictionaryRepresentation] allKeys]) {
        [defaults removeObjectForKey:key];
    }
    [defaults synchronize];
    
    self.done = NO;
}

- (void)tearDown
{
    // Put teardown code here. This method is called after the invocation of each test method in the class.
    [super tearDown];
}

- (void)testRegisterNativeInitial
{
    MSTestFilter *testFilterEmptyListRegistrations = [MSTestFilter new];
    NSString* stringData = @"[]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    testFilterEmptyListRegistrations.responseToUse = response;
    testFilterEmptyListRegistrations.ignoreNextFilter = YES;
    testFilterEmptyListRegistrations.dataToUse = data;
    testFilterEmptyListRegistrations.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"GET"], @"Expected request HTTPMethod to be GET.");
        NSString *expectedQueryAndPath = @"push/registrations?deviceId=59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8&platform=apns";
        NSString *expectedUrl = [[self.url URLByAppendingPathComponent:expectedQueryAndPath]  absoluteString];
        
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri %@, but found %@.", expectedUrl, [request URL]);
        
        return request;
    };
    
    MSTestFilter *testFilterCreateRegistrationId = [MSTestFilter new];
    __block NSString *expectedRegistrationId = @"8313603759421994114-6468852488791307573-9";

    NSURL *locationUrl = [[self.url URLByAppendingPathComponent:@"push/registrations"]
                          URLByAppendingPathComponent:expectedRegistrationId];
    NSHTTPURLResponse *createRegistrationIdResponse = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:201
                                   HTTPVersion:nil
                                   headerFields:@{@"Location":[locationUrl absoluteString]}];
    
    testFilterCreateRegistrationId.responseToUse = createRegistrationIdResponse;
    testFilterCreateRegistrationId.ignoreNextFilter = YES;
    testFilterCreateRegistrationId.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"POST"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[self.url URLByAppendingPathComponent:@"push/registrationids"] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        return request;
    };
    
    // Create the registration
    NSMutableDictionary *registration = [[NSMutableDictionary alloc] init];
    NSArray *tags = @[@"tag1", @"tag2"];
    [registration setValue:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" forKey:@"deviceId"];
    [registration setValue:@"apns" forKey:@"platform"];
    [registration setValue:tags forKey:@"tags"];
    __block NSString *registrationId = @"8313603759421994114-6468852488791307573-9";
    [registration setValue:registrationId forKey:@"registrationId"];
    
    // Create the test filter to allow testing request and returning response without connecting to service
    MSTestFilter *testFilterUpsertRegistration = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *upsertRegistrationResponse = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:204
                                   HTTPVersion:nil headerFields:nil];
    testFilterUpsertRegistration.responseToUse = upsertRegistrationResponse;
    testFilterUpsertRegistration.ignoreNextFilter = YES;
    
    testFilterUpsertRegistration.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"PUT"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:registrationId] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        NSString *httpBody = [[NSString alloc] initWithData:request.HTTPBody encoding:NSUTF8StringEncoding];
        XCTAssertTrue([httpBody rangeOfString:registrationId].location == NSNotFound, @"The request body should not included registrationId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"deviceId\":\"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8\""].location != NSNotFound,
                     @"The request body should include deviceId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"platform\":\"apns\""].location != NSNotFound, @"The request body should include platform.");
        XCTAssertTrue([httpBody rangeOfString:@"\"tags\":[\"tag1\",\"tag2\"]"].location != NSNotFound, @"The request body should include tags.");
        return request;
    };
    
    MSMultiRequestTestFilter *testFilter = [[MSMultiRequestTestFilter alloc] init];
    testFilter.testFilters = @[testFilterEmptyListRegistrations,
                               testFilterCreateRegistrationId,
                               testFilterUpsertRegistration];
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    [filteredClient.push registerNativeWithDeviceToken:deviceToken tags:@[@"tag1",@"tag2"] completion:^(NSError *error) {
        XCTAssertNil(error, @"Error should be nil %@", error.description);
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testRegisterNativeRequiredDeviceToken {
    [self.client.push registerNativeWithDeviceToken:nil tags:@[@"tag1",@"tag2"] completion:^(NSError *error) {
        XCTAssertEqual(error.code, [@MSPushRequiredParameter integerValue], @"Error code was expected to be MSPushRequiredParameter.");
        XCTAssertEqual(error.domain, MSErrorDomain, @"Error code was expected to be MSErrorDomain.");
        XCTAssertTrue([error.description rangeOfString:@"deviceToken"].location != NSNotFound, @"Expected deviceToken in error description.");
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testRegisterTemplateRequiredDeviceToken {
    [self.client.push registerTemplateWithDeviceToken:nil name:nil jsonBodyTemplate:nil expiryTemplate:nil tags:nil completion:^(NSError *error) {
        XCTAssertEqual(error.code, [@MSPushRequiredParameter integerValue], @"Error code was expected to be MSPushRequiredParameter.");
        XCTAssertEqual(error.domain, MSErrorDomain, @"Error code was expected to be MSErrorDomain.");
        XCTAssertTrue([error.description rangeOfString:@"deviceToken"].location != NSNotFound, @"Expected deviceToken in error description.");
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testRegisterTemplateRequiredName {
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    [self.client.push registerTemplateWithDeviceToken:deviceToken name:nil jsonBodyTemplate:nil expiryTemplate:nil tags:nil completion:^(NSError *error) {
        XCTAssertEqual(error.code, [@MSPushRequiredParameter integerValue], @"Error code was expected to be MSPushRequiredParameter.");
        XCTAssertEqual(error.domain, MSErrorDomain, @"Error code was expected to be MSErrorDomain.");
        XCTAssertTrue([error.description rangeOfString:@"name"].location != NSNotFound, @"Expected name in error description.");
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testUnregisterTemplateRequiredName {
    [self.client.push unregisterTemplateWithName:nil completion:^(NSError *error) {
        XCTAssertEqual(error.code, [@MSPushRequiredParameter integerValue], @"Error code was expected to be MSPushRequiredParameter.");
        XCTAssertEqual(error.domain, MSErrorDomain, @"Error code was expected to be MSErrorDomain.");
        XCTAssertTrue([error.description rangeOfString:@"name"].location != NSNotFound, @"Expected name in error description.");
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testUnregisterAllRequiredDeviceToken {
    [self.client.push unregisterAllWithDeviceToken:nil completion:^(NSError *error) {
        XCTAssertEqual(error.code, [@MSPushRequiredParameter integerValue], @"Error code was expected to be MSPushRequiredParameter.");
        XCTAssertEqual(error.domain, MSErrorDomain, @"Error code was expected to be MSErrorDomain.");
        XCTAssertTrue([error.description rangeOfString:@"deviceToken"].location != NSNotFound, @"Expected deviceToken in error description.");
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testRegisterTemplateRequiredTemplateBody {
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    [self.client.push registerTemplateWithDeviceToken:deviceToken name:@"foo" jsonBodyTemplate:nil expiryTemplate:nil tags:nil completion:^(NSError *error) {
        XCTAssertEqual(error.code, [@MSPushRequiredParameter integerValue], @"Error code was expected to be MSPushRequiredParameter.");
        XCTAssertEqual(error.domain, MSErrorDomain, @"Error code was expected to be MSErrorDomain.");
        XCTAssertTrue([error.description rangeOfString:@"bodyTemplate"].location != NSNotFound, @"Expected bodyTemplate in error description.");
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testRegisterTemplateInitial
{
    MSTestFilter *testFilterEmptyListRegistrations = [MSTestFilter new];
    NSString* stringData = @"[]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    testFilterEmptyListRegistrations.responseToUse = response;
    testFilterEmptyListRegistrations.ignoreNextFilter = YES;
    testFilterEmptyListRegistrations.dataToUse = data;
    testFilterEmptyListRegistrations.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"GET"], @"Expected request HTTPMethod to be GET.");
        NSString *expectedQueryAndPath = @"push/registrations?deviceId=59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8&platform=apns";
        NSString *expectedUrl = [[self.url URLByAppendingPathComponent:expectedQueryAndPath]  absoluteString];
        
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        return request;
    };
    
    MSTestFilter *testFilterCreateRegistrationId = [MSTestFilter new];
    __block NSString *expectedRegistrationId = @"8313603759421994114-6468852488791307573-9";
    
    NSURL *locationUrl = [[self.url URLByAppendingPathComponent:@"push/registrations"]
                          URLByAppendingPathComponent:expectedRegistrationId];
    NSHTTPURLResponse *createRegistrationIdResponse = [[NSHTTPURLResponse alloc]
                                                       initWithURL:nil
                                                       statusCode:201
                                                       HTTPVersion:nil
                                                       headerFields:@{@"Location":[locationUrl absoluteString]}];
    
    testFilterCreateRegistrationId.responseToUse = createRegistrationIdResponse;
    testFilterCreateRegistrationId.ignoreNextFilter = YES;
    testFilterCreateRegistrationId.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"POST"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[self.url URLByAppendingPathComponent:@"push/registrationids"] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        return request;
    };
    
    // Create the registration
    NSMutableDictionary *registration = [[NSMutableDictionary alloc] init];
    NSArray *tags = @[@"tag1", @"tag2"];
    [registration setValue:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" forKey:@"deviceId"];
    [registration setValue:@"apns" forKey:@"platform"];
    [registration setValue:tags forKey:@"tags"];
    __block NSString *registrationId = @"8313603759421994114-6468852488791307573-9";
    [registration setValue:registrationId forKey:@"registrationId"];
    
    // Create the test filter to allow testing request and returning response without connecting to service
    MSTestFilter *testFilterUpsertRegistration = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *upsertRegistrationResponse = [[NSHTTPURLResponse alloc]
                                                     initWithURL:nil
                                                     statusCode:204
                                                     HTTPVersion:nil headerFields:nil];
    testFilterUpsertRegistration.responseToUse = upsertRegistrationResponse;
    testFilterUpsertRegistration.ignoreNextFilter = YES;
    
    testFilterUpsertRegistration.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"PUT"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:registrationId] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        NSString *httpBody = [[NSString alloc] initWithData:request.HTTPBody encoding:NSUTF8StringEncoding];
        XCTAssertTrue([httpBody rangeOfString:registrationId].location == NSNotFound, @"The request body should not included registrationId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"deviceId\":\"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8\""].location != NSNotFound,
                     @"The request body should include deviceId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"platform\":\"apns\""].location != NSNotFound, @"The request body should include platform.");
        XCTAssertTrue([httpBody rangeOfString:@"\"tags\":[\"tag1\",\"tag2\"]"].location != NSNotFound, @"The request body should include tags.");
        XCTAssertTrue([httpBody rangeOfString:@"\"templateBody\":\"{aps:{alert:\\\"$(prop1)\\\"}}\""].location != NSNotFound, @"The request body should include a templateBody.");
        XCTAssertTrue([httpBody rangeOfString:@"\"templateName\":\"template1\""].location != NSNotFound, @"The request body should include a templateName.");
        XCTAssertTrue([httpBody rangeOfString:@"\"expiry\":\"$(expiry)\""].location != NSNotFound, @"The request body should include a templateName.");
        return request;
    };
    
    MSMultiRequestTestFilter *testFilter = [[MSMultiRequestTestFilter alloc] init];
    testFilter.testFilters = @[testFilterEmptyListRegistrations,
                               testFilterCreateRegistrationId,
                               testFilterUpsertRegistration];
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    [filteredClient.push registerTemplateWithDeviceToken:deviceToken name:@"template1" jsonBodyTemplate:@"{aps:{alert:\"$(prop1)\"}}" expiryTemplate:@"$(expiry)" tags:@[@"tag1",@"tag2"] completion:^(NSError *error) {
        XCTAssertNil(error, @"Error should be nil %@", error.description);
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testRegisterUpdateWithEmptyStorage
{
    MSTestFilter *testFilterListRegistrations = [MSTestFilter new];
    NSString* stringData = @"[{\"registrationId\":\"8313603759421994114-6468852488791307573-9\", \"deviceId\":\"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8\", \"tags\":[\"tag1\",\"tag2\"]}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    testFilterListRegistrations.responseToUse = response;
    testFilterListRegistrations.ignoreNextFilter = YES;
    testFilterListRegistrations.dataToUse = data;
    testFilterListRegistrations.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"GET"], @"Expected request HTTPMethod to be GET.");
        NSString *expectedQueryAndPath = @"push/registrations?deviceId=59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8&platform=apns";
        NSString *expectedUrl = [[self.url URLByAppendingPathComponent:expectedQueryAndPath] absoluteString];
        
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri %@, but found %@.", expectedUrl, [request URL]);
        
        return request;
    };
    
    // Create the registration
    NSMutableDictionary *registration = [[NSMutableDictionary alloc] init];
    NSArray *tags = @[@"tag1", @"tag2"];
    [registration setValue:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" forKey:@"deviceId"];
    [registration setValue:@"apns" forKey:@"platform"];
    [registration setValue:tags forKey:@"tags"];
    __block NSString *registrationId = @"8313603759421994114-6468852488791307573-9";
    [registration setValue:registrationId forKey:@"registrationId"];
    
    // Create the test filter to allow testing request and returning response without connecting to service
    MSTestFilter *testFilterUpsertRegistration = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *upsertRegistrationResponse = [[NSHTTPURLResponse alloc]
                                                     initWithURL:nil
                                                     statusCode:204
                                                     HTTPVersion:nil headerFields:nil];
    testFilterUpsertRegistration.responseToUse = upsertRegistrationResponse;
    testFilterUpsertRegistration.ignoreNextFilter = YES;
    
    testFilterUpsertRegistration.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"PUT"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:registrationId] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        NSString *httpBody = [[NSString alloc] initWithData:request.HTTPBody encoding:NSUTF8StringEncoding];
        XCTAssertTrue([httpBody rangeOfString:registrationId].location == NSNotFound, @"The request body should not included registrationId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"deviceId\":\"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8\""].location != NSNotFound,
                     @"The request body should include deviceId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"platform\":\"apns\""].location != NSNotFound, @"The request body should include platform.");
        XCTAssertTrue([httpBody rangeOfString:@"\"tags\":[\"tag1\",\"tag2\"]"].location != NSNotFound, @"The request body should include tags.");
        return request;
    };
    
    MSMultiRequestTestFilter *testFilter = [[MSMultiRequestTestFilter alloc] init];
    testFilter.testFilters = @[testFilterListRegistrations,
                               testFilterUpsertRegistration];
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    [filteredClient.push registerNativeWithDeviceToken:deviceToken tags:@[@"tag1",@"tag2"] completion:^(NSError *error) {
        XCTAssertNil(error, @"Error should be nil %@", error.description);
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testRegisterUpdateWithRegistrationInStorage
{
    [self setStorage:[self.url host] deviceToken:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" storageVersion:@"v1.0.0" registrations:@{@"$Default": @"8313603759421994114-6468852488791307573-9"}];
    
    // Create the registration
    NSMutableDictionary *registration = [[NSMutableDictionary alloc] init];
    NSArray *tags = @[@"tag1", @"tag2"];
    [registration setValue:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" forKey:@"deviceId"];
    [registration setValue:@"apns" forKey:@"platform"];
    [registration setValue:tags forKey:@"tags"];
    __block NSString *registrationId = @"8313603759421994114-6468852488791307573-9";
    [registration setValue:registrationId forKey:@"registrationId"];
    
    // Create the test filter to allow testing request and returning response without connecting to service
    MSTestFilter *testFilterUpsertRegistration = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *upsertRegistrationResponse = [[NSHTTPURLResponse alloc]
                                                     initWithURL:nil
                                                     statusCode:204
                                                     HTTPVersion:nil headerFields:nil];
    testFilterUpsertRegistration.responseToUse = upsertRegistrationResponse;
    testFilterUpsertRegistration.ignoreNextFilter = YES;
    
    testFilterUpsertRegistration.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"PUT"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:registrationId] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        NSString *httpBody = [[NSString alloc] initWithData:request.HTTPBody encoding:NSUTF8StringEncoding];
        XCTAssertTrue([httpBody rangeOfString:registrationId].location == NSNotFound, @"The request body should not included registrationId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"deviceId\":\"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8\""].location != NSNotFound,
                     @"The request body should include deviceId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"platform\":\"apns\""].location != NSNotFound, @"The request body should include platform.");
        XCTAssertTrue([httpBody rangeOfString:@"\"tags\":[\"tag1\",\"tag2\"]"].location != NSNotFound, @"The request body should include tags.");
        return request;
    };
    
    MSMultiRequestTestFilter *testFilter = [[MSMultiRequestTestFilter alloc] init];
    testFilter.testFilters = @[testFilterUpsertRegistration];
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    [filteredClient.push registerNativeWithDeviceToken:deviceToken tags:@[@"tag1",@"tag2"] completion:^(NSError *error) {
        XCTAssertNil(error, @"Error should be nil %@", error.description);
        self.done = YES;
    }];

    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testRegisterUpdateWithRegistrationInStorageExpiredRegistrationId
{
    [self setStorage:[self.url host] deviceToken:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" storageVersion:@"v1.0.0" registrations:@{@"$Default": @"8313603759421994114-6468852488791307573-9"}];
    
    // Create the registration
    NSMutableDictionary *registration = [[NSMutableDictionary alloc] init];
    NSArray *tags = @[@"tag1", @"tag2"];
    [registration setValue:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" forKey:@"deviceId"];
    [registration setValue:@"apns" forKey:@"platform"];
    [registration setValue:tags forKey:@"tags"];
    __block NSString *registrationId = @"8313603759421994114-6468852488791307573-9";
    [registration setValue:registrationId forKey:@"registrationId"];
    
    // Create the test filter to allow testing request and returning response without connecting to service
    MSTestFilter *testFilterUpsertRegistrationExpired = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *upsertRegistrationResponseExpired = [[NSHTTPURLResponse alloc]
                                                     initWithURL:nil
                                                     statusCode:410
                                                     HTTPVersion:nil headerFields:nil];
    testFilterUpsertRegistrationExpired.responseToUse = upsertRegistrationResponseExpired;
    testFilterUpsertRegistrationExpired.ignoreNextFilter = YES;
    
    testFilterUpsertRegistrationExpired.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"PUT"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:registrationId] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        NSString *httpBody = [[NSString alloc] initWithData:request.HTTPBody encoding:NSUTF8StringEncoding];
        XCTAssertTrue([httpBody rangeOfString:registrationId].location == NSNotFound, @"The request body should not included registrationId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"deviceId\":\"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8\""].location != NSNotFound,
                     @"The request body should include deviceId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"platform\":\"apns\""].location != NSNotFound, @"The request body should include platform.");
        XCTAssertTrue([httpBody rangeOfString:@"\"tags\":[\"tag1\",\"tag2\"]"].location != NSNotFound, @"The request body should include tags.");
        return request;
    };
    
    MSTestFilter *testFilterCreateRegistrationId = [MSTestFilter new];
    __block NSString *expectedRegistrationId = @"8313603759421994114-6468852488791307573-9";
    
    NSURL *locationUrl = [[self.url URLByAppendingPathComponent:@"push/registrations"]
                          URLByAppendingPathComponent:expectedRegistrationId];
    NSHTTPURLResponse *createRegistrationIdResponse = [[NSHTTPURLResponse alloc]
                                                       initWithURL:nil
                                                       statusCode:201
                                                       HTTPVersion:nil
                                                       headerFields:@{@"Location":[locationUrl absoluteString]}];
    
    testFilterCreateRegistrationId.responseToUse = createRegistrationIdResponse;
    testFilterCreateRegistrationId.ignoreNextFilter = YES;
    testFilterCreateRegistrationId.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"POST"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[self.url URLByAppendingPathComponent:@"push/registrationids"] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        return request;
    };
    
    // Create the test filter to allow testing request and returning response without connecting to service
    MSTestFilter *testFilterUpsertRegistration = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *upsertRegistrationResponse = [[NSHTTPURLResponse alloc]
                                                     initWithURL:nil
                                                     statusCode:204
                                                     HTTPVersion:nil headerFields:nil];
    testFilterUpsertRegistration.responseToUse = upsertRegistrationResponse;
    testFilterUpsertRegistration.ignoreNextFilter = YES;
    
    testFilterUpsertRegistration.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"PUT"], @"Expected request HTTPMethod to be POST.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:registrationId] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        NSString *httpBody = [[NSString alloc] initWithData:request.HTTPBody encoding:NSUTF8StringEncoding];
        XCTAssertTrue([httpBody rangeOfString:registrationId].location == NSNotFound, @"The request body should not included registrationId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"deviceId\":\"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8\""].location != NSNotFound,
                     @"The request body should include deviceId.");
        XCTAssertTrue([httpBody rangeOfString:@"\"platform\":\"apns\""].location != NSNotFound, @"The request body should include platform.");
        XCTAssertTrue([httpBody rangeOfString:@"\"tags\":[\"tag1\",\"tag2\"]"].location != NSNotFound, @"The request body should include tags.");
        return request;
    };

    
    MSMultiRequestTestFilter *testFilter = [[MSMultiRequestTestFilter alloc] init];
    testFilter.testFilters = @[testFilterUpsertRegistrationExpired,
                               testFilterCreateRegistrationId,
                               testFilterUpsertRegistration];
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    [filteredClient.push registerNativeWithDeviceToken:deviceToken tags:@[@"tag1",@"tag2"] completion:^(NSError *error) {
        XCTAssertNil(error, @"Error should be nil %@", error.description);
        self.done = YES;
    }];

    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testDeleteForMissingRegistration
{
    MSTestFilter *testFilterEmptyListRegistrations = [MSTestFilter new];
    NSString* stringData = @"[]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    testFilterEmptyListRegistrations.responseToUse = response;
    testFilterEmptyListRegistrations.ignoreNextFilter = YES;
    testFilterEmptyListRegistrations.dataToUse = data;
    testFilterEmptyListRegistrations.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"GET"], @"Expected request HTTPMethod to be GET.");
        NSString *expectedQuery = @"?deviceId=59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8&platform=apns";
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:expectedQuery]  absoluteString];
        
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        return request;
    };
    
    MSMultiRequestTestFilter *testFilter = [[MSMultiRequestTestFilter alloc] init];
    testFilter.testFilters = @[testFilterEmptyListRegistrations];
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    [filteredClient.push unregisterTemplateWithName:@"template1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Missing local registration is a no-op on delete");
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testDeleteForExistingRegistrationFromRefresh
{
    [self setStorage:[self.url host] deviceToken:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" storageVersion:@"v1.0.0" registrations:@{}];
    
    MSTestFilter *testFilterListRegistrations = [MSTestFilter new];
    NSString* stringData = @"[{\"templateName\":\"template1\",\"registrationId\":\"8313603759421994114-6468852488791307573-9\"}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    testFilterListRegistrations.responseToUse = response;
    testFilterListRegistrations.ignoreNextFilter = YES;
    testFilterListRegistrations.dataToUse = data;
    testFilterListRegistrations.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"GET"], @"Expected request HTTPMethod to be GET.");
        NSString *expectedQuery = @"?deviceId=59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8&platform=apns";
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:expectedQuery]  absoluteString];
        
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        return request;
    };
    
    MSTestFilter *testFilterDeleteRegistration = [MSTestFilter new];
    
    NSHTTPURLResponse *deleteResponse = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    testFilterDeleteRegistration.responseToUse = deleteResponse;
    testFilterDeleteRegistration.ignoreNextFilter = YES;
    testFilterDeleteRegistration.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"DELETE"], @"Expected request HTTPMethod to be DELETE.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:@"8313603759421994114-6468852488791307573-9"] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        return request;
    };
    
    MSMultiRequestTestFilter *testFilter = [[MSMultiRequestTestFilter alloc] init];
    testFilter.testFilters = @[testFilterListRegistrations,
                               testFilterDeleteRegistration];
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    [filteredClient.push unregisterTemplateWithName:@"template1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error should be nil %@", error.description);
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testDeleteForExistingRegistrationFromStorage
{
    [self setStorage:[self.url host] deviceToken:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" storageVersion:@"v1.0.0" registrations:@{@"template1":@"8313603759421994114-6468852488791307573-9"}];
    
    MSTestFilter *testFilterDeleteRegistration = [MSTestFilter new];
    
    NSHTTPURLResponse *deleteResponse = [[NSHTTPURLResponse alloc]
                                         initWithURL:nil
                                         statusCode:200
                                         HTTPVersion:nil headerFields:nil];
    
    testFilterDeleteRegistration.responseToUse = deleteResponse;
    testFilterDeleteRegistration.ignoreNextFilter = YES;
    testFilterDeleteRegistration.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"DELETE"], @"Expected request HTTPMethod to be DELETE.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:@"8313603759421994114-6468852488791307573-9"] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        return request;
    };
    
    MSMultiRequestTestFilter *testFilter = [[MSMultiRequestTestFilter alloc] init];
    testFilter.testFilters = @[testFilterDeleteRegistration];
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    [filteredClient.push unregisterTemplateWithName:@"template1" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error should be nil %@", error.description);
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testDeleteForMissingRegistrationAfterRefresh
{
    [self setStorage:[self.url host] deviceToken:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" storageVersion:@"v1.0.0" registrations:@{}];
    
    MSTestFilter *testFilterListRegistrations = [MSTestFilter new];
    NSString* stringData = @"[{\"templateName\":\"template1\",\"registrationId\":\"8313603759421994114-6468852488791307573-9\"}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    testFilterListRegistrations.responseToUse = response;
    testFilterListRegistrations.ignoreNextFilter = YES;
    testFilterListRegistrations.dataToUse = data;
    testFilterListRegistrations.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"GET"], @"Expected request HTTPMethod to be GET.");
        NSString *expectedQuery = @"?deviceId=59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8&platform=apns";
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:expectedQuery]  absoluteString];
        
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        
        return request;
    };
    
    MSTestFilter *testFilterDeleteRegistration = [MSTestFilter new];
    
    NSHTTPURLResponse *deleteResponse = [[NSHTTPURLResponse alloc]
                                         initWithURL:nil
                                         statusCode:200
                                         HTTPVersion:nil headerFields:nil];
    
    testFilterDeleteRegistration.responseToUse = deleteResponse;
    testFilterDeleteRegistration.ignoreNextFilter = YES;
    testFilterDeleteRegistration.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"DELETE"], @"Expected request HTTPMethod to be DELETE.");
        NSString *expectedUrl = [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:@"8313603759421994114-6468852488791307573-9"] absoluteString];
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri.");
        return request;
    };
    
    MSMultiRequestTestFilter *testFilter = [[MSMultiRequestTestFilter alloc] init];
    testFilter.testFilters = @[testFilterListRegistrations,
                               testFilterDeleteRegistration];
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    [filteredClient.push unregisterTemplateWithName:@"template2" completion:^(NSError *error) {
        XCTAssertNil(error, @"Error should be nil %@", error.description);
        self.done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:15], @"Test timed out.");
}

- (void)testDeleteAllRegistrations
{
    [self setStorage:[self.url host] deviceToken:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8" storageVersion:@"v1.0.0" registrations:@{}];
    
    MSTestFilter *testFilterListRegistrations = [MSTestFilter new];
    NSString* stringData = @"[{\"templateName\":\"template1\",\"registrationId\":\"8313603759421994114-6468852488791307573-9\"},{\"templateName\":\"template2\",\"registrationId\":\"8313603759421994114-6468852488791307573-7\"}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:200
                                   HTTPVersion:nil headerFields:nil];
    
    testFilterListRegistrations.responseToUse = response;
    testFilterListRegistrations.ignoreNextFilter = YES;
    testFilterListRegistrations.dataToUse = data;
    testFilterListRegistrations.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"GET"], @"Expected request HTTPMethod to be GET.");
        NSString *expectedQueryAndPath = @"push/registrations?deviceId=59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8&platform=apns";
        NSString *expectedUrl = [[self.url URLByAppendingPathComponent:expectedQueryAndPath]  absoluteString];
        
        XCTAssertTrue([expectedUrl isEqualToString:[[request URL] absoluteString]], @"Expected request to have expected Uri %@, but found %@.", expectedUrl, [request URL]);
        
        return request;
    };
    
    MSTestFilter *testFilterDeleteRegistration = [MSTestFilter new];
    
    NSHTTPURLResponse *deleteResponse = [[NSHTTPURLResponse alloc]
                                         initWithURL:nil
                                         statusCode:200
                                         HTTPVersion:nil headerFields:nil];
    
    __block NSMutableArray *expectedUrls = [@[
                                              [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:@"8313603759421994114-6468852488791307573-7"] absoluteString],
                                              [[[self.url URLByAppendingPathComponent:@"push/registrations"] URLByAppendingPathComponent:@"8313603759421994114-6468852488791307573-9"] absoluteString]] mutableCopy];
    
    testFilterDeleteRegistration.responseToUse = deleteResponse;
    testFilterDeleteRegistration.ignoreNextFilter = YES;
    testFilterDeleteRegistration.onInspectRequest = ^(NSURLRequest *request) {
        XCTAssertTrue([request.HTTPMethod isEqualToString:@"DELETE"], @"Expected request HTTPMethod to be DELETE.");
        NSString *actualUrl = request.URL.absoluteString;
        XCTAssertTrue([expectedUrls containsObject:actualUrl], @"Expected request to have expected Uri.");
        [expectedUrls removeObject:actualUrl];
        return request;
    };
    
    MSMultiRequestTestFilter *testFilter = [[MSMultiRequestTestFilter alloc] init];
    testFilter.testFilters = @[testFilterListRegistrations,
                               testFilterDeleteRegistration,
                               testFilterDeleteRegistration];
    MSClient *filteredClient = [self.client clientWithFilter:testFilter];
    
    NSData *deviceToken = [self bytesFromHexString:@"59D31B14081B92DAA98FAD91EDC0E61FC23767D5B90892C4F22DF56E312045C8"];
    [filteredClient.push unregisterAllWithDeviceToken:deviceToken completion:^(NSError *error) {
        XCTAssertTrue(expectedUrls.count == 0, @"Expected all URLs to have been hit");
        XCTAssertNil(error, @"Error should be nil %@", error.description);
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

-(void) setStorage:(NSString *)mobileServiceHost
       deviceToken:(NSString *)deviceToken
    storageVersion:(NSString *)storageVersion
     registrations:(NSDictionary *)registrations
{
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    NSString *versionKey = [NSString stringWithFormat:@"%@-version", mobileServiceHost];
    NSString *deviceTokenKey = [NSString stringWithFormat:@"%@-deviceToken", mobileServiceHost];
    NSString *registrationsKey = [NSString stringWithFormat:@"%@-registrations", mobileServiceHost];
    
    [defaults setObject:deviceToken forKey:deviceTokenKey];
    [defaults setObject:storageVersion forKey:versionKey];
    [defaults setObject:registrations forKey:registrationsKey];
    
    [defaults synchronize];
}

@end
