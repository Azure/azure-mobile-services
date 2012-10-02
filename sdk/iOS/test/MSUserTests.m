// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import <SenTestingKit/SenTestingKit.h>
#import "MSUser.h"

@interface MSUserTests : SenTestCase

@end


@implementation MSUserTests


#pragma mark * Setup and TearDown


-(void) setUp {
    
    NSLog(@"%@ setUp", self.name);
}

-(void) tearDown {
    
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
