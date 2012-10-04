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
#import "WindowsAzureMobileServices.h"

@interface WindowsAzureMobileServicesFunctionalTests : SenTestCase {
    MSClient *client;
    BOOL done;
    BOOL testsEnabled;
}
   
@end

@implementation WindowsAzureMobileServicesFunctionalTests


#pragma mark * Setup and TearDown


- (void) setUp
{
    NSLog(@"%@ setUp", self.name);

    testsEnabled = NO;
    STAssertTrue(testsEnabled, @"The functiontional tests are currently disabled.");
    
    // These functional tests requires a working Windows Mobile Azure Service
    // with a table named "todoItem". Simply enter the application URL and
    // application key for the Windows Mobile Azure Service below and set the
    // 'testsEnabled' BOOL above to YES.
        
    client = [MSClient
              clientWithApplicationURLString:@"<Windows Azure Mobile Service App URL>"
              withApplicationKey:@"<Application Key>"];
    
    done = NO;
    
    STAssertNotNil(client, @"Could not create test client.");
}

- (void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * End-to-End Positive Insert, Update, Delete and Read Tests


- (void) testCreateUpdateAndDeleteTodoItem
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item onSuccess:^(id newItem) {
        
        // Verify that the insert succeeded
        STAssertNotNil(newItem, @"newItem should not be nil.");
        STAssertNotNil([newItem objectForKey:@"id"],
                       @"newItem should now have an id.");
        
        // Update the item
        NSDictionary *itemToUpdate = @{
            @"id" :[newItem objectForKey:@"id"],
            @"text":@"Write E2E test!",
            @"complete": @(YES)
        };
        
        [todoTable update:itemToUpdate onSuccess:^(id updatedItem) {
            
            // Verify that the update succeeded
            STAssertNotNil(updatedItem, @"updatedItem should not be nil.");
            STAssertTrue([[updatedItem objectForKey:@"complete"] boolValue],
                           @"updatedItem should now be completed.");
            
            // Delete the item
            [todoTable delete:updatedItem onSuccess:^(NSNumber *itemId) {
                
                // Verify that the delete succeeded
                STAssertTrue([itemId longLongValue] ==
                            [[updatedItem objectForKey:@"id"] longLongValue],
                            @"itemId deleted was: %d.", itemId);
                done = YES;
                
            } onError:^(NSError *error) {
                STAssertTrue(FALSE,
                             @"Delete failed with error: %@",
                             error.localizedDescription);
                done = YES;

            }];

        } onError:^(NSError *error) {
            STAssertTrue(FALSE,
                         @"Update failed with error: %@",
                         error.localizedDescription);
            done = YES;

        }];
        
    } onError:^(NSError *error) {
        STAssertTrue(FALSE,
                     @"Insert failed with error: %@",
                     error.localizedDescription);
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

- (void) testCreateAndQueryTodoItem
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the items
    NSDictionary *item1 = @{ @"text":@"ItemA", @"complete": @(NO) };
    NSDictionary *item2 = @{ @"text":@"ItemB", @"complete": @(YES) };
    NSDictionary *item3 = @{ @"text":@"ItemB", @"complete": @(NO) };
    NSArray *items = @[item1,item2, item3];
    
    // Create an error block
    id errorBlock = ^(NSError *error) {
        STAssertTrue(FALSE,
                     @"Test failed with error: %@",
                     error.localizedDescription);
        done = YES;
    };
    
    
    id query7AfterQuery6 = ^(NSArray *items, NSInteger totalCount) {
        STAssertTrue(items.count == 2, @"items.count was: %d", items.count);
        STAssertTrue(totalCount == 3, @"totalCount was: %d", totalCount);
        
        [self deleteAllItemswithTable:todoTable
                            onSuccess:^() { done = YES; }
                              onError:errorBlock];
    };
    
    id query6AfterQuery5 = ^(NSArray *items, NSInteger totalCount) {
        STAssertTrue(items.count == 2, @"items.count was: %d", items.count);
        STAssertTrue(totalCount == 3, @"totalCount was: %d", totalCount);
        
        MSQuery *query = [todoTable query];
        query.fetchOffset = 1;
        query.includeTotalCount = YES;
        [query readOnSuccess:query7AfterQuery6 onError:errorBlock];
    };
    
    id query5AfterQuery4 = ^(NSArray *items, NSInteger totalCount) {
        STAssertTrue(items.count == 3, @"items.count was: %d", items.count);
        STAssertTrue(totalCount == -1, @"totalCount was: %d", totalCount);
        
        [todoTable readWithQueryString:@"$top=2&$inlinecount=allpages"
                            onSuccess:query6AfterQuery5
                              onError:errorBlock];
    };
    
    id query4AfterQuery3 = ^(NSDictionary *item) { 
        STAssertNotNil(item, @"item should not have been nil.");
        
        [todoTable readWithQueryString:nil
                             onSuccess:query5AfterQuery4
                               onError:errorBlock];
    };
    
    id query3AfterQuery2 = ^(NSArray *items, NSInteger totalCount) {
        STAssertTrue(items.count == 1, @"items.count was: %d", items.count);
        STAssertTrue(totalCount == -1, @"totalCount was: %d", totalCount);
        
        [todoTable readWithId:[[items objectAtIndex:0] valueForKey:@"id"]
                    onSuccess:query4AfterQuery3
                     onError:errorBlock];
    };
    
    id query2AfterQuery1 = ^(NSArray *items, NSInteger totalCount) {
        STAssertTrue(items.count == 2, @"items.count was: %d", items.count);
        STAssertTrue(totalCount == -1, @"totalCount was: %d", totalCount);
        
        NSPredicate *predicate = [NSPredicate predicateWithFormat:
                                  @"text ENDSWITH 'B' AND complete == TRUE"];
        [todoTable readWhere:predicate
                   onSuccess:query3AfterQuery2
                     onError:errorBlock];
    };
    
    id query1AfterInsert = ^() {
        NSPredicate *predicate = [NSPredicate predicateWithFormat:
                                  @"text ENDSWITH 'B'"];
        [todoTable readWhere:predicate
                   onSuccess:query2AfterQuery1
                     onError:errorBlock];
    };
    
    id insertAfterDeleteAll = ^(){
        [self insertItems:items withTable:todoTable
                onSuccess:query1AfterInsert
                  onError:errorBlock];
    };
    
    [self deleteAllItemswithTable:todoTable
                        onSuccess:insertAfterDeleteAll
                          onError:errorBlock];
    
    
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * Negative Insert Tests


- (void) testInsertItemForNonExistentTable
{
    MSTable *todoTable = [client getTable:@"NoSuchTable"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * Negative Update Tests


- (void) testUpdateItemForNonExistentTable
{
    MSTable *todoTable = [client getTable:@"NoSuchTable"];
    
    // Update the item
    NSDictionary *item = @{
    @"text":@"Write E2E test!",
    @"complete": @(NO),
    @"id":@100
    };
    
    // Insert the item
    [todoTable update:item onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

- (void) testUpdateItemForNonExistentItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{
    @"text":@"Write update E2E test!",
    @"complete": @(NO),
    @"id":@-5
    };
    
    // Update the item
    [todoTable update:item onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * Negative Delete Tests


- (void) testDeleteItemForNonExistentTable
{
    MSTable *todoTable = [client getTable:@"NoSuchTable"];
    
    // Create the item
    NSDictionary *item = @{
        @"text":@"Write E2E test!",
        @"complete": @(NO),
        @"id":@100
    };
    
    // Delete the item
    [todoTable delete:item onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

- (void) testDeleteItemForNonExistentItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{
        @"text":@"Write update E2E test!",
        @"complete": @(NO),
        @"id":@-5
    };
    
    // Delete the item
    [todoTable delete:item onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

- (void) testDeleteItemWithIdForNonExistentItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Delete the item
    [todoTable deleteWithId:@-5 onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * Negative ReadWithId Tests


- (void) testReadWithIdForNonExistentTable
{
    MSTable *todoTable = [client getTable:@"NoSuchTable"];

    // Insert the item
    [todoTable readWithId:@100 onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

- (void) testReadWithIdForNonExistentItemId
{
    MSTable *todoTable = [client getTable:@"todoItem"];
    
    // Insert the item
    [todoTable readWithId:@-5 onSuccess:^(id newItem) {
        
        STAssertTrue(FALSE, @"The onSuccess block should not have been called.");
        
    } onError:^(NSError *error) {
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * Async Test Helper Method


-(void) insertItems:(NSArray *)items
          withTable:(MSTable *)table
            onSuccess:(void (^)())onSuccess
            onError:(void (^)(NSError *))onError
{
    __block NSInteger lastItemIndex = -1;
    
    __block void (^nextInsertBlock)(id);
    nextInsertBlock = [^(id newItem) {
        if (lastItemIndex + 1  < items.count) {
            lastItemIndex++;
            id itemToInsert = [items objectAtIndex:lastItemIndex];
            [table insert:itemToInsert
             onSuccess:nextInsertBlock onError:onError];
        }
        else {
            onSuccess();
        }
    } copy];
    
    nextInsertBlock(nil);
}

-(void) deleteAllItemswithTable:(MSTable *)table
               onSuccess:(void (^)())onSuccess
                 onError:(void (^)(NSError *))onError
{
    __block MSReadQuerySuccessBlock readSuccessBlock;
    readSuccessBlock = ^(NSArray *items, NSInteger totalCount) {
        [self deleteItems:items
                withTable:table
         onSuccess:onSuccess
           onError:onError];
    };
    
    [table readWithQueryString:@"$top=500"
             onSuccess:readSuccessBlock
               onError:onError];
}


-(void) deleteItems:(NSArray *)items
          withTable:(MSTable *)table
   onSuccess:(void (^)())onSuccess
     onError:(void (^)(NSError *))onError
{
    __block NSInteger lastItemIndex = -1;
    
    __block void (^nextDeleteBlock)(NSNumber *);
    nextDeleteBlock = [^(NSNumber *itemId) {
        if (lastItemIndex + 1  < items.count) {
            lastItemIndex++;
            id itemToDelete = [items objectAtIndex:lastItemIndex];
            [table delete:itemToDelete
         onSuccess:nextDeleteBlock onError:onError];
        }
        else {
            onSuccess();
        }
    } copy];
    
    nextDeleteBlock(0);
}


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
