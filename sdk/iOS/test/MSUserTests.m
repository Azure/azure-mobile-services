// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSUser.h"

@interface MSUserTests : SenTestCase

@end


@implementation MSUserTests


#pragma mark * Setup and TearDown


-(void) setUp
{    
    NSLog(@"%@ setUp", self.name);
}

-(void) tearDown
{    
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * Init Method Tests


-(void) testInitWithUserId
{
    MSUser *user = [[MSUser alloc] initWithUserId:@"SomeUserId"];
    
    STAssertNotNil(user, @"user should not be nil.");
    STAssertTrue([user.userId isEqualToString:@"SomeUserId"],
                 @"user id should have been set.");
}

-(void) testInitWithUserIdAllowsNilUserId
{
    MSUser *user = [[MSUser alloc] initWithUserId:nil];
    
    STAssertNotNil(user, @"user should not be nil.");
    STAssertNil(user.userId, @"user id should have been nil.");
}

@end
