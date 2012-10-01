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

    testsEnabled = YES;
    STAssertTrue(testsEnabled, @"The functiontional tests are currently disabled.");
    
    // These functional tests requires a working Windows Mobile Azure Service
    // with a table named "todoItem". Simply enter the application URL and
    // application key for the Windows Mobile Azure Service below and set the
    // 'testsEnabled' BOOL above to YES.
    
    client = [MSClient
              clientWithApplicationURLString:@"https://iosclientendtoend.azure-mobile.net/"
              withApplicationKey:@"uLnPzbAwamiGDbgxldoKqxZYenkiwG40"];
    done = NO;
    
    STAssertNotNil(client, @"Could not create test client.");
}

- (void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * Create, Update and Delete a TodoItem Tests


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


#pragma mark * Create items and Query Tests


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

    id query3AfterQuery2 = ^(NSArray *items, NSInteger totalCount) {
        STAssertTrue(items.count == 1, @"Should have been one item.");
        [self deleteAllItemswithTable:todoTable
                     onSuccess:^() { done = YES; }
                       onError:errorBlock];
    };

    id query2AfterQuery1 = ^(NSArray *items, NSInteger totalCount) {
        STAssertTrue(items.count == 2, @"Should have been two items.");
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
    
    do {
        [[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode
                                 beforeDate:timeoutAt];
        if([timeoutAt timeIntervalSinceNow] <= 0.0) {
            break;
        }
    } while (!done);
    
    return done;
}

@end
