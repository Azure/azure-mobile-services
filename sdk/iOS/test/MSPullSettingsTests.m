// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSPullSettingsInternal.h"

@interface MSPullSettingsTests : XCTestCase
@end

@implementation MSPullSettingsTests

#pragma mark * Setup and TearDown

- (void)setUp
{
    NSLog(@"%@ setUp", self.name);
}

- (void)tearDown
{
    NSLog(@"%@ tearDown", self.name);
}

#pragma mark * PageSize tests

- (void)testPullSettings_NegativePageSize
{
    MSPullSettings *pullSettings = [MSPullSettings new];
    pullSettings.pageSize = -1;

    XCTAssertEqual(pullSettings.pageSize, [MSPullSettings defaultPageSize], "Incorrect page size");
}

- (void)testPullSettings_ZeroPageSize
{
    MSPullSettings *pullSettings = [MSPullSettings new];
    pullSettings.pageSize = 0;
    
    XCTAssertEqual(pullSettings.pageSize, [MSPullSettings defaultPageSize], "Incorrect page size");
}

- (void)testPullSettings_ValidPageSize
{
    MSPullSettings *pullSettings = [MSPullSettings new];
    pullSettings.pageSize = 1;
    
    XCTAssertEqual(pullSettings.pageSize, 1, "Incorrect page size");
}

- (void)testPullSettings_DefaultPageSize
{
    MSPullSettings *pullSettings = [MSPullSettings new];
    
    XCTAssertEqual(pullSettings.pageSize, [MSPullSettings defaultPageSize], "Incorrect page size");
}

- (void)testDefaultPageSizeGreaterThanZero
{
    XCTAssertGreaterThan([MSPullSettings defaultPageSize], 0, "Page size must be > 0");
}

@end
