// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "WindowsAzureMobileServices.h"
#import "MSTestFilter.h"
#import "MSTable+MSTableTestUtilities.h"

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
    STAssertTrue(testsEnabled, @"The functional tests are currently disabled.");
    
    // These functional tests requires a working Windows Mobile Azure Service
    // with a table named "todoItem". Simply enter the application URL and
    // application key for the Windows Mobile Azure Service below and set the
    // 'testsEnabled' BOOL above to YES.
    
    client = [MSClient
                clientWithApplicationURLString:@"<Microsoft Azure Mobile Service App URL>"
                applicationKey:@"<Application Key>"];
    
    done = NO;
    
    STAssertNotNil(client, @"Could not create test client.");
}

- (void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * End-to-End Positive Insert, Update, Delete and Read Tests


-(void) testCreateUpdateAndDeleteTodoItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *newItem, NSError *error) {
        // Check for an error
        if (error) {
            STAssertTrue(FALSE, @"Insert failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
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
        
        [todoTable update:itemToUpdate completion:^(NSDictionary *updatedItem, NSError *error) {
            
            // Check for an error
            if (error) {
                STAssertTrue(FALSE, @"Update failed with error: %@", error.localizedDescription);
                done = YES;
            }
            
            // Verify that the update succeeded
            STAssertNotNil(updatedItem, @"updatedItem should not be nil.");
            STAssertTrue([[updatedItem objectForKey:@"complete"] boolValue],
                           @"updatedItem should now be completed.");
            
            // Delete the item
            [todoTable delete:updatedItem completion:^(NSNumber *itemId, NSError *error) {
 
                // Check for an error
                if (error) {
                    STAssertTrue(FALSE, @"Delete failed with error: %@", error.localizedDescription);
                    done = YES;
                }
                
                // Verify that the delete succeeded
                STAssertTrue([itemId longLongValue] ==
                            [[updatedItem objectForKey:@"id"] longLongValue],
                            @"itemId deleted was: %d.", itemId);
                done = YES;
                
            }];
        }];
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testCreateAndQueryTodoItem
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the items
    NSDictionary *item1 = @{ @"text":@"ItemA", @"complete": @(NO) };
    NSDictionary *item2 = @{ @"text":@"ItemB", @"complete": @(YES) };
    NSDictionary *item3 = @{ @"text":@"ItemB", @"complete": @(NO) };
    NSArray *items = @[item1,item2, item3];

    id query7AfterQuery6 = ^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        // Check for an error
        if (error) {
            STAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        STAssertTrue(items.count == 2, @"items.count was: %d", items.count);
        STAssertTrue(totalCount == 3, @"totalCount was: %d", totalCount);
        
        [todoTable deleteAllItemsWithCompletion:^(NSError *error) { done = YES; }];
    };
    
    id query6AfterQuery5 = ^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        // Check for an error
        if (error) {
            STAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        STAssertTrue(items.count == 2, @"items.count was: %d", items.count);
        STAssertTrue(totalCount == 3, @"totalCount was: %d", totalCount);
        
        MSQuery *query = [todoTable query];
        query.fetchOffset = 1;
        query.includeTotalCount = YES;
        [query readWithCompletion:query7AfterQuery6];
    };
    
    id query5AfterQuery4 = ^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        // Check for an error
        if (error) {
            STAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        STAssertTrue(items.count == 3, @"items.count was: %d", items.count);
        STAssertTrue(totalCount == -1, @"totalCount was: %d", totalCount);
        
        [todoTable readWithQueryString:@"$top=2&$inlinecount=allpages"
                            completion:query6AfterQuery5];
    };
    
    id query4AfterQuery3 = ^(NSDictionary *item, NSError *error) {
        
        // Check for an error
        if (error) {
            STAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        STAssertNotNil(item, @"item should not have been nil.");
        
        [todoTable readWithQueryString:nil completion:query5AfterQuery4];
    };
    
    id query3AfterQuery2 = ^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        // Check for an error
        if (error) {
            STAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        STAssertTrue(items.count == 1, @"items.count was: %d", items.count);
        STAssertTrue(totalCount == -1, @"totalCount was: %d", totalCount);
        
        [todoTable readWithId:[[items objectAtIndex:0] valueForKey:@"id"]
                    completion:query4AfterQuery3];
    };
    
    id query2AfterQuery1 = ^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        // Check for an error
        if (error) {
            STAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        STAssertTrue(items.count == 2, @"items.count was: %d", items.count);
        STAssertTrue(totalCount == -1, @"totalCount was: %d", totalCount);
        
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"text ENDSWITH 'B' AND complete == TRUE"];
        [todoTable readWithPredicate:predicate completion:query3AfterQuery2];
    };
    
    id query1AfterInsert = ^(NSError *error) {
        
        // Check for an error
        if (error) {
            STAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"text ENDSWITH 'B'"];
        [todoTable readWithPredicate:predicate completion:query2AfterQuery1];
    };
    
    id insertAfterDeleteAll = ^(NSError *error){
        
        // Check for an error
        if (error) {
            STAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        [todoTable insertItems:items completion:query1AfterInsert];
    };
    
    [todoTable deleteAllItemsWithCompletion:insertAfterDeleteAll];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

#pragma mark * End-to-End Filter Tests

-(void) testFilterThatModifiesRequest
{
    // Create a filter that will replace the request with one requesting
    // a table that doesn't exist.
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSURL *badURL = [client.applicationURL
                     URLByAppendingPathComponent:@"tables/NoSuchTable"];
    NSURLRequest *badRequest = [NSURLRequest requestWithURL:badURL];
    
    testFilter.requestToUse = badRequest;
    
    // Create the client and the table
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    MSTable *todoTable = [filterClient tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {

        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Error: Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;

    }];
        
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testFilterThatModifiesResponse
{
    // Create a filter that will replace the response with one that has
    // a 400 status code and an error message
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSHTTPURLResponse *response = [[NSHTTPURLResponse alloc]
                                   initWithURL:nil
                                   statusCode:400
                                   HTTPVersion:nil headerFields:nil];
    NSString* stringData = @"This is an Error Message for the testFilterThatModifiesResponse test!";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    testFilter.responseToUse = response;
    testFilter.dataToUse = data;
    
    // Create the client and the table
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    MSTable *todoTable = [filterClient tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {

        STAssertNil(item, @"item should have been nil.");

        STAssertNotNil(error, @"error was nil after deserializing item.");
        STAssertTrue([error domain] == MSErrorDomain,
                     @"error domain was: %@", [error domain]);
        STAssertTrue([error code] == MSErrorMessageErrorCode,
                     @"error code was: %d",[error code]);
        STAssertTrue([[error localizedDescription] isEqualToString:
                      @"This is an Error Message for the testFilterThatModifiesResponse test!"],
                     @"error description was: %@", [error localizedDescription]);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testFilterThatReturnsError
{
    // Create a filter that will replace the error
    MSTestFilter *testFilter = [[MSTestFilter alloc] init];
    
    NSError *error = [NSError errorWithDomain:@"SomeDomain"
                                         code:-102
                                     userInfo:nil];
    testFilter.errorToUse = error;
    
    // Create the client and the table
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    MSTable *todoTable = [filterClient tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {

        STAssertNil(item, @"item should have been nil.");

        STAssertNotNil(error, @"error was nil after deserializing item.");
        STAssertTrue([error.domain isEqualToString:@"SomeDomain"],
                     @"error domain was: %@", [error domain]);
        STAssertTrue([error code] == -102,
                     @"error code was: %d",[error code]);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * End-to-End URL Encoding Tests

-(void) testFilterConstantsAreURLEncoded
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    NSString *predicateString = @"text == '#?&$ encode me!'";
    NSPredicate *predicate = [NSPredicate predicateWithFormat:predicateString];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"#?&$ encode me!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNotNil(item, @"item should not have been nil.");
        STAssertNil(error, @"error from insert should have been nil.");
        
        // Now try to query the item and make sure we don't error
        [todoTable readWithPredicate:predicate completion:^(NSArray *items,
                                                    NSInteger totalCount,
                                                    NSError *error) {
            
            STAssertNotNil(items, @"items should not have been nil.");
            STAssertTrue([items count] > 0, @"items should have matched something.");
            STAssertNil(error, @"error from query should have been nil.");
            
            // Now delete the inserted item so as not to corrupt future tests
            NSNumber *itemIdToDelete = [item objectForKey:@"id"];
            [todoTable deleteWithId:itemIdToDelete completion:^(NSNumber *itemId,
                                                                NSError *error) {
                STAssertNil(error, @"error from delete should have been nil.");
                done = YES;
            }];
        }];
    }];

    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testUserParametersAreURLEncodedWithQuery
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    MSQuery *query = [todoTable query];
    query.parameters = @{@"encodeMe$?": @"No really $#%& encode me!"};
    [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        STAssertNotNil(items, @"items should not have been nil.");
        STAssertNil(error, @"error from query was: %@",
                    [error.userInfo objectForKey:NSLocalizedDescriptionKey]);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testUserParametersAreURLEncodedWithInsertUpdateAndDelete
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];

    // Create the item
    NSDictionary *item = @{ @"text":@"some text", @"complete": @(NO) };
    NSDictionary *parameters = @{@"encodeMe$?": @"No really $#%& encode me!"};
    
    // Insert the item
    [todoTable insert:item
           parameters:parameters
           completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNotNil(item, @"item should not have been nil.");
        STAssertNil(error, @"error from insert should have been nil.");
        
        // Now update the inserted item
        [todoTable update:item
               parameters:parameters
               completion:^(NSDictionary *updatedItem, NSError *error) {
                          
            STAssertNotNil(updatedItem, @"updatedItem should not have been nil.");
            STAssertNil(error, @"error from update should have been nil.");

            // Now delete the inserted item so as not to corrupt future tests
            NSNumber *itemIdToDelete = [updatedItem objectForKey:@"id"];
            [todoTable deleteWithId:itemIdToDelete
                        parameters:parameters
                         completion:^(NSNumber *itemId, NSError *error) {
                STAssertNil(error, @"error from delete should have been nil.");
                done = YES;
            }];
        }];
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}



#pragma mark * Negative Insert Tests

-(void) testInsertItemForNonExistentTable
{
    MSTable *todoTable = [client tableWithName:@"NoSuchTable"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Error: Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * Negative Update Tests

-(void) testUpdateItemForNonExistentTable
{
    MSTable *todoTable = [client tableWithName:@"NoSuchTable"];
    
    // Update the item
    NSDictionary *item = @{
        @"text":@"Write E2E test!",
        @"complete": @(NO),
        @"id":@100
    };
    
    // Insert the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Error: Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testUpdateItemForNonExistentItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{
    @"text":@"Write update E2E test!",
    @"complete": @(NO),
    @"id":@-5
    };
    
    // Update the item
    [todoTable update:item completion:^(NSDictionary *item, NSError *error) {
        
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Error: An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * Negative Delete Tests

-(void) testDeleteItemForNonExistentTable
{
    MSTable *todoTable = [client tableWithName:@"NoSuchTable"];
    
    // Create the item
    NSDictionary *item = @{
        @"text":@"Write E2E test!",
        @"complete": @(NO),
        @"id":@100
    };
    
    // Delete the item
    [todoTable delete:item completion:^(NSNumber *itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Error: Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testDeleteItemForNonExistentItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Create the item
    NSDictionary *item = @{
        @"text":@"Write update E2E test!",
        @"complete": @(NO),
        @"id":@-5
    };
    
    // Delete the item
    [todoTable delete:item completion:^(NSNumber *itemId, NSError *error) {
        
        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Error: An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testDeleteItemWithIdForNonExistentItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Delete the item
    [todoTable deleteWithId:@-5 completion:^(NSNumber *itemId, NSError *error) {

        STAssertNil(itemId, @"itemId should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Error: An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * Negative ReadWithId Tests

-(void) testReadWithIdForNonExistentTable
{
    MSTable *todoTable = [client tableWithName:@"NoSuchTable"];

    // Insert the item
    [todoTable readWithId:@100 completion:^(NSDictionary *item, NSError *error) {
    
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Error: Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testReadWithIdForNonExistentItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Insert the item
    [todoTable readWithId:@-5 completion:^(NSDictionary *item, NSError *error) {
    
        STAssertNil(item, @"item should have been nil.");
        
        STAssertNotNil(error, @"error should not have been nil.");
        STAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        STAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
        STAssertTrue([description isEqualToString:@"Error: An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    STAssertTrue([self waitForTest:90.0], @"Test timed out.");
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
