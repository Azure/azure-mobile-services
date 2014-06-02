// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSLocalStorage.h"

@interface MSLocalStorageTests : SenTestCase

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

-(void)testBasicState
{
    // initialize MSLocalStorage
    MSLocalStorage *storage = [[MSLocalStorage alloc] initWithMobileServiceHost:@"foo.mobileservices.net"];
    STAssertNil(storage.deviceToken, @"Device Token should be nil when MSLocalStorage is initialized with empty defaults.");
    STAssertEquals(storage.isRefreshNeeded, YES, @"isRefreshNeeded should be YES when MSLocalStorage is initialized with empty defaults.");

    // Add an item
    [storage updateRegistrationWithName:@"regName" registrationId:@"regId" deviceToken:@"token"];
    STAssertEquals(storage.deviceToken, @"token", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Get that item and ensure it is accurate
    NSString *reg = [storage getRegistrationIdWithName:@"regName"];
    STAssertEquals(reg, @"regId", @"Expected registrationId to be regId");
    
    // Update by registration name
    [storage updateRegistrationWithName:@"regName2" registrationId:@"regId2" deviceToken:@"token4"];
    STAssertEquals(storage.deviceToken, @"token4", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Get that item and ensure it is accurate
    NSString *reg2New = [storage getRegistrationIdWithName:@"regName2"];
    STAssertEquals(reg2New, @"regId2", @"Expected registrationId to be regId2");

    // Add a new registration name
    [storage updateRegistrationWithName:@"regName4" registrationId:@"regId4" deviceToken:@"token4"];
    STAssertEquals(storage.deviceToken, @"token4", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Get the original item and ensure it is accurate
    reg2New = [storage getRegistrationIdWithName:@"regName2"];
    STAssertEquals(reg2New, @"regId2", @"Expected registrationId to be regId2");

    // Get that item and ensure it is accurate
    NSString *reg4 = [storage getRegistrationIdWithName:@"regName4"];
    STAssertEquals(reg4, @"regId4", @"Expected registrationId to be regId4");
    
    // initialize MSLocalStorage to test loading and saving to local storage
    MSLocalStorage *storage2 = [[MSLocalStorage alloc] initWithMobileServiceHost:@"foo.mobileservices.net"];
    STAssertEquals(storage2.deviceToken, @"token4", @"Token is expected to be set correctly loaded from storage.");
    STAssertEquals(storage2.isRefreshNeeded, NO, @"isRefreshNeeded should be NO when MSLocalStorage is initialized with empty defaults.");

    // Get the original item and ensure it is accurate
    reg2New = [storage2 getRegistrationIdWithName:@"regName2"];
    STAssertEquals(reg2New, @"regId2", @"Expected registrationId to be regId2");
    
    // Get that item and ensure it is accurate
    reg4 = [storage2 getRegistrationIdWithName:@"regName4"];
    STAssertEquals(reg4, @"regId4", @"Expected registrationId to be regId4");
    
    // Test delete
    [storage2 deleteRegistrationWithName:@"regName4"];
    
    // Get the original item and ensure it is accurate
    reg2New = [storage2 getRegistrationIdWithName:@"regName2"];
    STAssertEquals(reg2New, @"regId2", @"Expected registrationId to be regId2");
    
    // Get that item and ensure it is accurate
    reg4 = [storage2 getRegistrationIdWithName:@"regName4"];
    STAssertNil(reg4, @"reg4 should be Nil after being deleted.");
    
    // Re-initialize storage from storage
    storage2 = [[MSLocalStorage alloc] initWithMobileServiceHost:@"foo.mobileservices.net"];
    STAssertEquals(storage2.deviceToken, @"token4", @"Token is expected to be set correctly loaded from storage.");
    STAssertEquals(storage2.isRefreshNeeded, NO, @"isRefreshNeeded should be NO when MSLocalStorage is initialized with empty defaults.");
    
    // Get the original item and ensure it is accurate
    reg2New = [storage2 getRegistrationIdWithName:@"regName2"];
    STAssertEquals(reg2New, @"regId2", @"Expected registrationId to be regId2");
    
    // Get that item and ensure it is accurate
    reg4 = [storage2 getRegistrationIdWithName:@"regName4"];
    STAssertNil(reg4, @"reg4 should be Nil after being deleted.");
    
    [storage2 deleteAllRegistrations];
    
    // Get the original item and ensure it is accurate
    reg2New = [storage2 getRegistrationIdWithName:@"regName2"];
    STAssertNil(reg2New, @"reg2New should be Nil after being deleted.");
    
    // Get that item and ensure it is accurate
    reg4 = [storage2 getRegistrationIdWithName:@"regName4"];
    STAssertNil(reg4, @"reg4 should be Nil after being deleted.");
}

@end
