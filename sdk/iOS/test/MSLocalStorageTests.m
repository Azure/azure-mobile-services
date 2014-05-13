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
        [defaults synchronize];
    }
}

-(void)testBasicState
{
    // initialize MSLocalStorage
    MSLocalStorage *storage = [[MSLocalStorage alloc] initWithNotificationHubPath:@"foo.mobileservices.net"];
    STAssertNil(storage.deviceToken, @"Device Token should be nil when MSLocalStorage is initialized with empty defaults.");
    STAssertEquals(storage.isRefreshNeeded, YES, @"isRefreshNeeded should be YES when MSLocalStorage is initialized with empty defaults.");

    // Add an item
    [storage updateWithRegistrationName:@"regName" registrationId:@"regId" deviceToken:@"token"];
    STAssertEquals(storage.deviceToken, @"token", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Get that item and ensure it is accurate
    MSStoredRegistration *reg = [storage getMSStoredRegistrationWithRegistrationName:@"regName"];
    STAssertEquals(reg.registrationId, @"regId", @"Expected registrationId to be regId");
    STAssertEquals(reg.registrationName, @"regName", @"Expected registrationName to be regName");
    
    // Ensure that refreshFinishedWithDeviceToken works
    [storage refreshFinishedWithDeviceToken:@"token2"];
    STAssertEquals(storage.deviceToken, @"token2", @"Token is expected to be set correctly after refreshFinishedWithDeviceToken is called.");
    STAssertEquals(storage.isRefreshNeeded, NO, @"isRefreshNeeded is expected to be NO after refreshFinishedWithDeviceToken is called.");
    
    // Update by registrationId
    [storage updateWithRegistrationId:@"regId" registrationName:@"regName2" deviceToken:@"token3"];
    STAssertEquals(storage.deviceToken, @"token3", @"Token is expected to be set correctly after updateWithRegistrationId is called.");
    
    // Get the original item and ensure it is nil
    MSStoredRegistration *regOrig = [storage getMSStoredRegistrationWithRegistrationName:@"regName"];
    STAssertNil(regOrig, @"Expected entry for original registration to be nil.");
    
    // Get that item and ensure it is accurate
    MSStoredRegistration *reg2 = [storage getMSStoredRegistrationWithRegistrationName:@"regName2"];
    STAssertEquals(reg2.registrationId, @"regId", @"Expected registrationId to be regId");
    STAssertEquals(reg2.registrationName, @"regName2", @"Expected registrationName to be regName2");
    
    // Update by registration name
    [storage updateWithRegistrationName:@"regName2" registrationId:@"regId2" deviceToken:@"token4"];
    STAssertEquals(storage.deviceToken, @"token4", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Get that item and ensure it is accurate
    MSStoredRegistration *reg2New = [storage getMSStoredRegistrationWithRegistrationName:@"regName2"];
    STAssertEquals(reg2New.registrationId, @"regId2", @"Expected registrationId to be regId2");
    STAssertEquals(reg2New.registrationName, @"regName2", @"Expected registrationName to be regName2");

    // Add a new registration name
    [storage updateWithRegistrationName:@"regName4" registrationId:@"regId4" deviceToken:@"token4"];
    STAssertEquals(storage.deviceToken, @"token4", @"Token is expected to be set correctly after updateWithRegistrationName is called.");
    
    // Get the original item and ensure it is accurate
    reg2New = [storage getMSStoredRegistrationWithRegistrationName:@"regName2"];
    STAssertEquals(reg2New.registrationId, @"regId2", @"Expected registrationId to be regId2");
    STAssertEquals(reg2New.registrationName, @"regName2", @"Expected registrationName to be regName2");

    // Get that item and ensure it is accurate
    MSStoredRegistration *reg4 = [storage getMSStoredRegistrationWithRegistrationName:@"regName4"];
    STAssertEquals(reg4.registrationId, @"regId4", @"Expected registrationId to be regId4");
    STAssertEquals(reg4.registrationName, @"regName4", @"Expected registrationName to be regName4");
    
    // initialize MSLocalStorage to test loading and saving to local storage
    MSLocalStorage *storage2 = [[MSLocalStorage alloc] initWithNotificationHubPath:@"foo.mobileservices.net"];
    STAssertEquals(storage2.deviceToken, @"token4", @"Token is expected to be set correctly loaded from storage.");
    STAssertEquals(storage2.isRefreshNeeded, NO, @"isRefreshNeeded should be NO when MSLocalStorage is initialized with empty defaults.");

    // Get the original item and ensure it is accurate
    reg2New = [storage2 getMSStoredRegistrationWithRegistrationName:@"regName2"];
    STAssertEquals(reg2New.registrationId, @"regId2", @"Expected registrationId to be regId2");
    STAssertEquals(reg2New.registrationName, @"regName2", @"Expected registrationName to be regName2");
    
    // Get that item and ensure it is accurate
    reg4 = [storage2 getMSStoredRegistrationWithRegistrationName:@"regName4"];
    STAssertEquals(reg4.registrationId, @"regId4", @"Expected registrationId to be regId4");
    STAssertEquals(reg4.registrationName, @"regName4", @"Expected registrationName to be regName4");
    
    // Test delete
    [storage2 deleteWithRegistrationName:@"regName4"];
    
    // Get the original item and ensure it is accurate
    reg2New = [storage2 getMSStoredRegistrationWithRegistrationName:@"regName2"];
    STAssertEquals(reg2New.registrationId, @"regId2", @"Expected registrationId to be regId2");
    STAssertEquals(reg2New.registrationName, @"regName2", @"Expected registrationName to be regName2");
    
    // Get that item and ensure it is accurate
    reg4 = [storage2 getMSStoredRegistrationWithRegistrationName:@"regName4"];
    STAssertNil(reg4, @"reg4 should be Nil after being deleted.");
    
    // Re-initialize storage from storage
    storage2 = [[MSLocalStorage alloc] initWithNotificationHubPath:@"foo.mobileservices.net"];
    STAssertEquals(storage2.deviceToken, @"token4", @"Token is expected to be set correctly loaded from storage.");
    STAssertEquals(storage2.isRefreshNeeded, NO, @"isRefreshNeeded should be NO when MSLocalStorage is initialized with empty defaults.");
    
    // Get the original item and ensure it is accurate
    reg2New = [storage2 getMSStoredRegistrationWithRegistrationName:@"regName2"];
    STAssertEquals(reg2New.registrationId, @"regId2", @"Expected registrationId to be regId2");
    STAssertEquals(reg2New.registrationName, @"regName2", @"Expected registrationName to be regName2");
    
    // Get that item and ensure it is accurate
    reg4 = [storage2 getMSStoredRegistrationWithRegistrationName:@"regName4"];
    STAssertNil(reg4, @"reg4 should be Nil after being deleted.");
    
    [storage deleteAllRegistrations];
    
    // Get the original item and ensure it is accurate
    reg2New = [storage2 getMSStoredRegistrationWithRegistrationName:@"regName2"];
    STAssertNil(reg2New, @"reg2New should be Nil after being deleted.");
    
    // Get that item and ensure it is accurate
    reg4 = [storage2 getMSStoredRegistrationWithRegistrationName:@"regName4"];
    STAssertNil(reg4, @"reg4 should be Nil after being deleted.");
}

@end
