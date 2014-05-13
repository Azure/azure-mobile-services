// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSStoredRegistration.h"

@interface MSStoredRegistrationTests : SenTestCase

@end

@implementation MSStoredRegistrationTests

- (void)testInitWithName
{
    MSStoredRegistration* reg = [[MSStoredRegistration alloc] initWithName:@"foo" registrationId:@"fooId"];
    STAssertEquals(@"foo", reg.registrationName, @"Expects the registration name to be set correctly.");
    STAssertEquals(@"fooId", reg.registrationId, @"Expects the registration id to be set correctly.");
}

@end
