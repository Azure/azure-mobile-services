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
#import "MSTable.h"


@interface MSTableTests : SenTestCase {
    MSClient *client;
    BOOL done;
}

@end


@implementation MSTableTests


#pragma mark * Setup and TearDown


-(void) setUp
{
    NSLog(@"%@ setUp", self.name);
    
    client = [MSClient clientWithApplicationURL:nil];
    
    done = NO;
}

-(void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * Init Method Tests


-(void) testInitWithNameAndClient
{
    MSTable *table = [[MSTable alloc] initWithName:@"SomeName" andClient:client];
    
    STAssertNotNil(table, @"table should not be nil.");
    
    STAssertNotNil(table.client, @"table.client should not be nil.");
    STAssertTrue([table.name isEqualToString:@"SomeName"],
                 @"table.name shouldbe 'SomeName'");
}

-(void) testInitWithNameAndClientAllowsNil
{
    MSTable *table = [[MSTable alloc] initWithName:nil andClient:nil];
    
    STAssertNotNil(table, @"table should not be nil.");
    
    STAssertNil(table.client, @"table.client should be nil.");
    STAssertNil(table.name, @"table.name should be nil.");
}


#pragma mark * Insert Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// insert tests that require a working Windows Azure Mobile Service.

-(void) testInsertItemWithNilItem
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Insert the item
    [todoTable insert:nil onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSExpectedItemWithRequest,
                     @"error code should have been MSExpectedItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"No item was provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:1.0], @"Test timed out.");
}

-(void) testInsertItemWithInvalidItem
{
    MSTable *todoTable = [client getTable:@"NoSuchTable"];
    
    // Create the item
    id item = [[NSDate alloc] initWithTimeIntervalSinceReferenceDate:0.0];
    
    // Insert the item
    [todoTable insert:item onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemWithRequest,
                     @"error code should have been MSInvalidItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided was not valid."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:1.0], @"Test timed out.");
}


#pragma mark * Update Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// update tests that require a working Windows Azure Mobile Service.

-(void) testUpdateItemWithNilItem
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Update the item
    [todoTable update:nil onSuccess:^(id updatedItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSExpectedItemWithRequest,
                     @"error code should have been MSExpectedItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"No item was provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithInvalidItem
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    id item = [[NSDate alloc] initWithTimeIntervalSinceReferenceDate:0.0];
    
    // Update the item
    [todoTable update:item onSuccess:^(id updatedItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemWithRequest,
                     @"error code should have been MSInvalidItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided was not valid."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:1.0], @"Test timed out.");
}

-(void) testUpdateItemWithNoItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"complete": @(NO) };
    
    // Update the item
    [todoTable update:item onSuccess:^(id updatedItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSMissingItemIdWithRequest,
                     @"error code should have been MSMissingItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have an id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testUpdateItemWithInvalidItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@"I'm not valid." };
    
    // Update the item
    [todoTable update:item onSuccess:^(id updatedItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSMissingItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * Delete Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// delete tests that require a working Windows Azure Mobile Service.

-(void) testDeleteItemWithNilItem
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Update the item
    [todoTable delete:nil onSuccess:^(id itemId) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSExpectedItemWithRequest,
                     @"error code should have been MSExpectedItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"No item was provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithInvalidItem
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    id item = [[NSDate alloc] initWithTimeIntervalSinceReferenceDate:0.0];
    
    // Update the item
    [todoTable delete:item onSuccess:^(id itemId) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemWithRequest,
                     @"error code should have been MSInvalidItemWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided was not valid."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithNoItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"complete": @(NO) };
    
    // Update the item
    [todoTable delete:item onSuccess:^(id itemId) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSMissingItemIdWithRequest,
                     @"error code should have been MSMissingItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have an id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithInvalidItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write unit tests!", @"id":@"I'm not valid." };
    
    // Update the item
    [todoTable delete:item onSuccess:^(id itemId) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSMissingItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithNoItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];

    // Update the item
    [todoTable deleteWithId:nil onSuccess:^(id itemId) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSExpectedItemIdWithRequest,
                     @"error code should have been MSExpectedItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item id was not provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testDeleteItemWithIdWithInvalidItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    id itemId = [[NSDate alloc] initWithTimeIntervalSince1970:0.0];
    
    // Update the item
    [todoTable deleteWithId:itemId onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSMissingItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * ReadWithId Method Tests


// See the WindowsAzureMobileServicesFunctionalTests.m tests for additional
// readWithId tests that require a working Windows Azure Mobile Service.


-(void) testReadItemWithIdWithNoItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Update the item
    [todoTable readWithId:nil onSuccess:^(id itemId) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSExpectedItemIdWithRequest,
                     @"error code should have been MSExpectedItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item id was not provided."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}

-(void) testReadItemWithIdWithInvalidItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    id itemId = [[NSDate alloc] initWithTimeIntervalSince1970:0.0];
    
    // Update the item
    [todoTable readWithId:itemId onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                     @"error code should have been MSMissingItemIdWithRequest.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:0.1], @"Test timed out.");
}


#pragma mark * Query Method Tests


-(void) testQueryReturnsNonNil
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    MSQuery *query = [todoTable query];
    
    STAssertNotNil(query, @"query should not have been nil.");    
}

-(void) testQueryWithPredicateReturnsNonNil
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    MSQuery *query = [todoTable queryWhere:nil];
    
    STAssertNotNil(query, @"query should not have been nil.");
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
