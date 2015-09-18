// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSUserAgentBuilder.h"

@interface MSUserAgentBuilderTests : XCTestCase

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
    
#if TARGET_OS_IPHONE
    XCTAssertTrue([userAgent isEqualToString:@"ZUMO/3.0 (lang=objective-c; os=--; os_version=--; arch=iOSSimulator; version=3.0.0)"],
                 @"user agent was: %@", userAgent);
#endif
}

@end
