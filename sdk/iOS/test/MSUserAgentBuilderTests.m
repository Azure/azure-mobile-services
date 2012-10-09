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
#import "MSUserAgentBuilder.h"

@interface MSUserAgentBuilderTests : SenTestCase

@end

@implementation MSUserAgentBuilderTests

#pragma mark * Setup and TearDown


-(void) setUp
{
    NSLog(@"%@ setUp", self.name);
}

-(void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * UserAgent Method Tests


-(void) testURLForTable
{
    NSString *userAgent = [MSUserAgentBuilder userAgent];
    
    STAssertTrue([userAgent isEqualToString:@"ZUMO/1.0 (iOSSimulator -- -- objective-c) --/--"],
                 @"user agent was: %@", userAgent);
}

@end
