// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSLocalStorage.h"

@interface MSLocalStorageTests : XCTestCase

@end

@implementation MSLocalStorageTests

-(void)setUp
{
    [super setUp];
    
    // Clear storage
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    for (NSString* key in [[defaults dictionaryRepresentation] allKeys]) {
        [defaults removeObjectForKey:key];
    }
    [defaults synchronize];
}

-(void)testInitialLoadUpdateGetAndReload
{
    // initialize MSLocalStorage
    MSLocalStorage *storage = [[MSLocalStorage alloc] initWithMobileServiceHost:@"foo.mobileservices.net"];
    XCTAssertNil(storage.deviceToken, @"Device Token should be nil when MSLocalStorage is initialized with empty defaults.");
    XCTAssertEqual(storage.isRefreshNeeded, YES, @"isRefreshNeeded should be YES when MSLocalStorage is initialized with empty defaults.");

    // Add an item
    [storage updateRegistrationWithName:@"regName" registrationId:@"regId" deviceToken:@"token"];
    XCTAssertEqual(storage.deviceToken, @"token", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Get that item and ensure it is accurate
    NSString *reg = [storage getRegistrationIdWithName:@"regName"];
    XCTAssertEqual(reg, @"regId", @"Expected registrationId to be regId");
    
    // Update by registration name to add second registration
    [storage updateRegistrationWithName:@"regName2" registrationId:@"regId2" deviceToken:@"token4"];
    XCTAssertEqual(storage.deviceToken, @"token4", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Get second item and ensure it is accurate
    NSString *reg2New = [storage getRegistrationIdWithName:@"regName2"];
    XCTAssertEqual(reg2New, @"regId2", @"Expected registrationId to be regId2");

    // Update original registration name
    [storage updateRegistrationWithName:@"regName" registrationId:@"regId4" deviceToken:@"token4"];
    XCTAssertEqual(storage.deviceToken, @"token4", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Get the original item and ensure it is accurate
    reg2New = [storage getRegistrationIdWithName:@"regName"];
    XCTAssertEqual(reg2New, @"regId4", @"Expected registrationId to be regId4");

    // Get that item and ensure it is accurate
    reg2New = [storage getRegistrationIdWithName:@"regName2"];
    XCTAssertEqual(reg2New, @"regId2", @"Expected registrationId to be regId2");
    
    // initialize MSLocalStorage to test loading and saving to local storage
    MSLocalStorage *storage2 = [[MSLocalStorage alloc] initWithMobileServiceHost:@"foo.mobileservices.net"];
    XCTAssertEqualObjects(storage2.deviceToken, @"token4", @"Token is expected to be set correctly loaded from storage.");
    XCTAssertEqual(storage2.isRefreshNeeded, NO, @"isRefreshNeeded should be NO when MSLocalStorage is initialized with empty defaults.");

    // Get the original item and ensure it is accurate
    reg2New = [storage2 getRegistrationIdWithName:@"regName2"];
    XCTAssertEqualObjects(reg2New, @"regId2", @"Expected registrationId to be regId2");
    
    // Get that item and ensure it is accurate
    reg2New = [storage2 getRegistrationIdWithName:@"regName"];
    XCTAssertEqualObjects(reg2New, @"regId4", @"Expected registrationId to be regId4");
}

-(void)testBasicState
{
    // initialize MSLocalStorage
    MSLocalStorage *storage = [[MSLocalStorage alloc] initWithMobileServiceHost:@"foo.mobileservices.net"];
    XCTAssertNil(storage.deviceToken, @"Device Token should be nil when MSLocalStorage is initialized with empty defaults.");
    XCTAssertEqual(storage.isRefreshNeeded, YES, @"isRefreshNeeded should be YES when MSLocalStorage is initialized with empty defaults.");
    
    // Add an item
    [storage updateRegistrationWithName:@"regName" registrationId:@"regId" deviceToken:@"token"];
    XCTAssertEqual(storage.deviceToken, @"token", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Add an item
    [storage updateRegistrationWithName:@"regName2" registrationId:@"regId2" deviceToken:@"token"];
    XCTAssertEqual(storage.deviceToken, @"token", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Test delete
    [storage deleteRegistrationWithName:@"regName"];
    
    // Get the original item and ensure it is accurate
    NSString *reg = [storage getRegistrationIdWithName:@"regName2"];
    XCTAssertEqual(reg, @"regId2", @"Expected registrationId to be regId2");
    
    // Get that item and ensure it is accurate
    reg = [storage getRegistrationIdWithName:@"regName"];
    XCTAssertNil(reg, @"reg should be Nil after being deleted.");
}

- (void)testDeleteAllRegistrations
{
    // initialize MSLocalStorage
    MSLocalStorage *storage = [[MSLocalStorage alloc] initWithMobileServiceHost:@"foo.mobileservices.net"];
    XCTAssertNil(storage.deviceToken, @"Device Token should be nil when MSLocalStorage is initialized with empty defaults.");
    XCTAssertEqual(storage.isRefreshNeeded, YES, @"isRefreshNeeded should be YES when MSLocalStorage is initialized with empty defaults.");
    
    // Updare registrations
    [storage updateRegistrations:@[@{@"templateName":@"regName", @"registrationId":@"regId"},@{@"registrationId":@"regId2"}] deviceToken:@"token2"];
    XCTAssertEqual(storage.deviceToken, @"token2", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    XCTAssertEqual(storage.isRefreshNeeded, NO, @"isRefreshNeeded should be NO after updateRegistrations.");
    
    // Get that item and ensure it is accurate
    NSString *reg = [storage getRegistrationIdWithName:@"regName"];
    XCTAssertEqual(reg, @"regId", @"Expected registrationId to be regId");
    
    // Get that item and ensure it is accurate
    reg = [storage getRegistrationIdWithName:@"$Default"];
    XCTAssertEqual(reg, @"regId2", @"Expected registrationId to be regId2");
    
    [storage deleteAllRegistrations];
    
    // Get the original item and ensure it is accurate
    reg = [storage getRegistrationIdWithName:@"regName"];
    XCTAssertNil(reg, @"reg should be Nil after being deleted.");
    
    // Get that item and ensure it is accurate
    reg = [storage getRegistrationIdWithName:@"$Default"];
    XCTAssertNil(reg, @"$Default should be Nil after being deleted.");
}

- (void)testGetRegistrationIds
{
    // initialize MSLocalStorage
    MSLocalStorage *storage = [[MSLocalStorage alloc] initWithMobileServiceHost:@"foo.mobileservices.net"];
    XCTAssertNil(storage.deviceToken, @"Device Token should be nil when MSLocalStorage is initialized with empty defaults.");
    XCTAssertEqual(storage.isRefreshNeeded, YES, @"isRefreshNeeded should be YES when MSLocalStorage is initialized with empty defaults.");
    
    // Updare registrations
    [storage updateRegistrations:@[@{@"templateName":@"regName", @"registrationId":@"regId"},@{@"registrationId":@"regId2"}] deviceToken:@"token2"];
    XCTAssertEqual(storage.deviceToken, @"token2", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    XCTAssertEqual(storage.isRefreshNeeded, NO, @"isRefreshNeeded should be NO after updateRegistrations.");
    
    NSArray *registrationIds = [[storage getRegistrationIds] sortedArrayUsingSelector:@selector(localizedCaseInsensitiveCompare:)];
    XCTAssertEqual(registrationIds[0], @"regId", @"RegistrationIds should match those in storage.");
    XCTAssertEqual(registrationIds[1], @"regId2", @"RegistrationIds should match those in storage.");
}

@end