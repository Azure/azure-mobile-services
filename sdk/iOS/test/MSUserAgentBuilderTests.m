// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

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
    
    STAssertTrue([userAgent isEqualToString:@"ZUMO/1.1 (lang=objective-c; os=--; os_version=--; arch=iOSSimulator; version=1.1.0.0)"],
                 @"user agent was: %@", userAgent);
}

@end
