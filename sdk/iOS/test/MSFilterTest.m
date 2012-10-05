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
#import "MSClient.h"
#import "MSTable.h"
#import "MSTestFilter.h"

@interface MSFilterTest : SenTestCase {
    MSClient *client;
        BOOL done;
}

@end

@implementation MSFilterTest


#pragma mark * Setup and TearDown


-(void) setUp
{
    NSLog(@"%@ setUp", self.name);
    
    client = [MSClient clientWithApplicationURLString:@"http://someApp"];
    
    done = NO;
}

-(void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * Single Filter Tests


-(void) testFilterThatReturnsResponseImmediately
{
    // Create a filter that will replace the response with one that has
    // a 400 status code and an error message
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:400
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"\"This is an Error Message!\"";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    testFilter.ignoreNextFilter = YES;
    
    // Create the client and the table
    MSClient *filterClient = [client clientwithFilter:testFilter];
    
    MSTable *todoTable = [filterClient getTable:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item onSuccess:^(id newItem) {
        
        STAssertTrue(NO, @"onError should have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error was nil after deserializing item.");
        STAssertTrue([error domain] == MSErrorDomain,
                     @"error domain was: %@", [error domain]);
        STAssertTrue([error code] == MSErrorMessageErrorCode,
                     @"error code was: %d",[error code]);
        STAssertTrue([[error localizedDescription] isEqualToString:
                      @"This is an Error Message!"],
                     @"error description was: %@", [error localizedDescription]);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testFilterThatReturnsErrorImmediately
{
    // Create a filter that will replace the error
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSError *error = [NSError errorWithDomain:@"SomeDomain"
                                         code:-102
                                     userInfo:nil];
    testFilter.errorToUse = error;
    testFilter.ignoreNextFilter = YES;
    
    // Create the client and the table
    MSClient *filterClient = [client clientwithFilter:testFilter];
    
    MSTable *todoTable = [filterClient getTable:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item onSuccess:^(id newItem) {
        
        STAssertTrue(NO, @"onError should have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error was nil after deserializing item.");
        STAssertTrue([error domain] == @"SomeDomain",
                     @"error domain was: %@", [error domain]);
        STAssertTrue([error code] == -102,
                     @"error code was: %d",[error code]);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

#pragma mark * Multiple Filters Tests

-(void) testFiltersCalledInCorrectOrder
{
    // Create three filters that each append to the URL of the request
    MSTestFilter *testFilterA = [[MSTestFilter alloc] init];
    testFilterA.onInspectRequest = ^(NSURLRequest *request) {
        NSMutableURLRequest *mutableRequest = [request mutableCopy];
        mutableRequest.URL = [mutableRequest.URL URLByAppendingPathComponent:@"A"];
        return mutableRequest;
    };
    
    MSTestFilter *testFilterB = [[MSTestFilter alloc] init];
    testFilterB.onInspectRequest = ^(NSURLRequest *request) {
        NSMutableURLRequest *mutableRequest = [request mutableCopy];
        mutableRequest.URL = [mutableRequest.URL URLByAppendingPathComponent:@"B"];
        return mutableRequest;
    };
    
    // Capture the final URL value to check with an assert below
    __block NSURL *finalURL = nil;
    
    MSTestFilter *testFilterC = [[MSTestFilter alloc] init];
    testFilterC.onInspectRequest = ^(NSURLRequest *request) {
        NSMutableURLRequest *mutableRequest = [request mutableCopy];
        mutableRequest.URL = [mutableRequest.URL URLByAppendingPathComponent:@"C"];
        finalURL = mutableRequest.URL;
        return mutableRequest;
    };
    
    testFilterC.errorToUse = [NSError errorWithDomain:@"TestErrorDomain" code:-998 userInfo:nil];
    testFilterC.ignoreNextFilter = YES;
    
    
    // Create the client and the table
    MSClient *filterClient = [[[client clientwithFilter:testFilterA]
                                       clientwithFilter:testFilterB]
                                       clientwithFilter:testFilterC];
    
    MSTable *todoTable = [filterClient getTable:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item onSuccess:^(id newItem) {
        
        STAssertTrue(NO, @"onError should have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == @"TestErrorDomain",
                     @"error domain should have been TestErrorDomain.");
        STAssertTrue(error.code == -998,
                     @"error code should have been -998.");
        
        STAssertTrue([finalURL.absoluteString isEqualToString:@"http://someApp/tables/todoItem/A/B/C"],
                     @"description was: %@", finalURL.absoluteString);
        
        done = YES;
        
    }];
    
    STAssertTrue([self waitForTest:1.0], @"Test timed out.");
}

#pragma mark * Async Test Helper Method


-(BOOL) waitForTest:(NSTimeInterval)testDuration {
    
    NSDate *timeoutAt = [NSDate dateWithTimeIntervalSinceNow:testDuration];
    
    while (!done) {
        [[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode
                                 beforeDate:timeoutAt];
        if([timeoutAt timeIntervalSinceNow] <= 0.0) {
            break;
        }
    };
    
    return done;
}


@end
