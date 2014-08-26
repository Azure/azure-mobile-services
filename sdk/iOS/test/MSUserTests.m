// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSUser.h"

@interface MSUserTests : XCTestCase

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
    
    XCTAssertNotNil(user, @"user should not be nil.");
    XCTAssertTrue([user.userId isEqualToString:@"SomeUserId"],
                 @"user id should have been set.");
}

-(void) testInitWithUserIdAllowsNilUserId
{
    MSUser *user = [[MSUser alloc] initWithUserId:nil];
    
    XCTAssertNotNil(user, @"user should not be nil.");
    XCTAssertNil(user.userId, @"user id should have been nil.");
}

@end
