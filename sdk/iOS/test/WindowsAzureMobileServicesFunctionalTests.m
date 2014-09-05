// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "WindowsAzureMobileServices.h"
#import "MSTestFilter.h"
#import "MSTable+MSTableTestUtilities.h"

@interface WindowsAzureMobileServicesFunctionalTests : XCTestCase {
    MSClient *client;
    BOOL done;
}
   
@end

@implementation WindowsAzureMobileServicesFunctionalTests


#pragma mark * Setup and TearDown


- (void) setUp
{
    [super setUp];
    self.continueAfterFailure = NO;
    
    NSLog(@"%@ setUp", self.name);
    
    // These functional tests requires a working Windows Mobile Azure Service
    // with a table named "todoItem". Simply enter the application URL and
    // application key for the Windows Mobile Azure Service below.
    
    client = [MSClient
                clientWithApplicationURLString:@"<Microsoft Azure Mobile Service App URL>"
                applicationKey:@"<Application Key>"];
    
    XCTAssertTrue([client.applicationURL.description hasPrefix:@"https://"], @"The functional tests are currently disabled.");

    self.continueAfterFailure = YES;
    done = NO;
    
    XCTAssertNotNil(client, @"Could not create test client.");
}

- (void) tearDown
{
    self.continueAfterFailure = YES;
    
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
            XCTAssertTrue(FALSE, @"Insert failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        // Verify that the insert succeeded
        XCTAssertNotNil(newItem, @"newItem should not be nil.");
        XCTAssertNotNil(newItem[@"id"],
                       @"newItem should now have an id.");
        
        // Update the item
        NSDictionary *itemToUpdate = @{
            @"id" :newItem[@"id"],
            @"text":@"Write E2E test!",
            @"complete": @(YES)
        };
        
        [todoTable update:itemToUpdate completion:^(NSDictionary *updatedItem, NSError *error) {
            
            // Check for an error
            if (error) {
                XCTAssertTrue(FALSE, @"Update failed with error: %@", error.localizedDescription);
                done = YES;
            }
            
            // Verify that the update succeeded
            XCTAssertNotNil(updatedItem, @"updatedItem should not be nil.");
            XCTAssertTrue([updatedItem[@"complete"] boolValue],
                           @"updatedItem should now be completed.");
            
            // Delete the item
            [todoTable delete:updatedItem completion:^(NSNumber *itemId, NSError *error) {
 
                // Check for an error
                if (error) {
                    XCTAssertTrue(FALSE, @"Delete failed with error: %@", error.localizedDescription);
                    done = YES;
                }
                
                // Verify that the delete succeeded
                XCTAssertTrue([itemId longLongValue] ==
                            [updatedItem[@"id"] longLongValue],
                            @"itemId deleted was: %@.", itemId);
                done = YES;
                
            }];
        }];
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
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
            XCTAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        XCTAssertTrue(items.count == 2, @"items.count was: %lu", (unsigned long)items.count);
        XCTAssertTrue(totalCount == 3, @"totalCount was: %ld", (long)totalCount);
        
        [todoTable deleteAllItemsWithCompletion:^(NSError *error) { done = YES; }];
    };
    
    id query6AfterQuery5 = ^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        // Check for an error
        if (error) {
            XCTAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        XCTAssertTrue(items.count == 2, @"items.count was: %lu", (unsigned long)items.count);
        XCTAssertTrue(totalCount == 3, @"totalCount was: %ld", (long)totalCount);
        
        MSQuery *query = [todoTable query];
        query.fetchOffset = 1;
        query.includeTotalCount = YES;
        [query readWithCompletion:query7AfterQuery6];
    };
    
    id query5AfterQuery4 = ^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        // Check for an error
        if (error) {
            XCTAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        XCTAssertTrue(items.count == 3, @"items.count was: %lu", (unsigned long)items.count);
        XCTAssertTrue(totalCount == -1, @"totalCount was: %ld", (long)totalCount);
        
        [todoTable readWithQueryString:@"$top=2&$inlinecount=allpages"
                            completion:query6AfterQuery5];
    };
    
    id query4AfterQuery3 = ^(NSDictionary *item, NSError *error) {
        
        // Check for an error
        if (error) {
            XCTAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        XCTAssertNotNil(item, @"item should not have been nil.");
        
        [todoTable readWithQueryString:nil completion:query5AfterQuery4];
    };
    
    id query3AfterQuery2 = ^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        // Check for an error
        if (error) {
            XCTAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        XCTAssertTrue(items.count == 1, @"items.count was: %lu", (unsigned long)items.count);
        XCTAssertTrue(totalCount == -1, @"totalCount was: %ld", (long)totalCount);
        
        [todoTable readWithId:[items[0] valueForKey:@"id"]
                    completion:query4AfterQuery3];
    };
    
    id query2AfterQuery1 = ^(NSArray *items, NSInteger totalCount, NSError *error) {
        
        // Check for an error
        if (error) {
            XCTAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        XCTAssertTrue(items.count == 2, @"items.count was: %lu", (unsigned long)items.count);
        XCTAssertTrue(totalCount == -1, @"totalCount was: %ld", (long)totalCount);
        
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"text ENDSWITH 'B' AND complete == TRUE"];
        [todoTable readWithPredicate:predicate completion:query3AfterQuery2];
    };
    
    id query1AfterInsert = ^(NSError *error) {
        
        // Check for an error
        if (error) {
            XCTAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        NSPredicate *predicate = [NSPredicate predicateWithFormat:@"text ENDSWITH 'B'"];
        [todoTable readWithPredicate:predicate completion:query2AfterQuery1];
    };
    
    id insertAfterDeleteAll = ^(NSError *error){
        
        // Check for an error
        if (error) {
            XCTAssertTrue(FALSE, @"Test failed with error: %@", error.localizedDescription);
            done = YES;
        }
        
        [todoTable insertItems:items completion:query1AfterInsert];
    };
    
    [todoTable deleteAllItemsWithCompletion:insertAfterDeleteAll];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
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

        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"Error: Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;

    }];
        
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
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

        XCTAssertNil(item, @"item should have been nil.");

        XCTAssertNotNil(error, @"error was nil after deserializing item.");
        XCTAssertTrue([error domain] == MSErrorDomain,
                     @"error domain was: %@", [error domain]);
        XCTAssertTrue([error code] == MSErrorMessageErrorCode,
                     @"error code was: %ld",(long)[error code]);
        XCTAssertTrue([[error localizedDescription] isEqualToString:
                      @"This is an Error Message for the testFilterThatModifiesResponse test!"],
                     @"error description was: %@", [error localizedDescription]);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
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

        XCTAssertNil(item, @"item should have been nil.");

        XCTAssertNotNil(error, @"error was nil after deserializing item.");
        XCTAssertTrue([error.domain isEqualToString:@"SomeDomain"],
                     @"error domain was: %@", [error domain]);
        XCTAssertTrue([error code] == -102,
                     @"error code was: %ld",(long)[error code]);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
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
        
        XCTAssertNotNil(item, @"item should not have been nil.");
        XCTAssertNil(error, @"error from insert should have been nil.");
        
        // Now try to query the item and make sure we don't error
        [todoTable readWithPredicate:predicate completion:^(MSQueryResult *result,
                                                    NSError *error) {
            
            XCTAssertNotNil(result.items, @"items should not have been nil.");
            XCTAssertTrue([result.items count] > 0, @"items should have matched something.");
            XCTAssertNil(error, @"error from query should have been nil.");
            
            // Now delete the inserted item so as not to corrupt future tests
            NSNumber *itemIdToDelete = item[@"id"];
            [todoTable deleteWithId:itemIdToDelete completion:^(NSNumber *itemId,
                                                                NSError *error) {
                XCTAssertNil(error, @"error from delete should have been nil.");
                done = YES;
            }];
        }];
    }];

    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testUserParametersAreURLEncodedWithQuery
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    MSQuery *query = [todoTable query];
    query.parameters = @{@"encodeMe$?": @"No really $#%& encode me!"};
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
        
        XCTAssertNotNil(result.items, @"items should not have been nil.");
        XCTAssertNil(error, @"error from query was: %@", error.localizedDescription);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
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
        
        XCTAssertNotNil(item, @"item should not have been nil.");
        XCTAssertNil(error, @"error from insert should have been nil.");
        
        // Now update the inserted item
        [todoTable update:item
               parameters:parameters
               completion:^(NSDictionary *updatedItem, NSError *error) {
                          
            XCTAssertNotNil(updatedItem, @"updatedItem should not have been nil.");
            XCTAssertNil(error, @"error from update should have been nil.");

            // Now delete the inserted item so as not to corrupt future tests
            NSNumber *itemIdToDelete = updatedItem[@"id"];
            [todoTable deleteWithId:itemIdToDelete
                        parameters:parameters
                         completion:^(NSNumber *itemId, NSError *error) {
                XCTAssertNil(error, @"error from delete should have been nil.");
                done = YES;
            }];
        }];
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * Negative Insert Tests


-(void) testInsertItemForNonExistentTable
{
    MSTable *todoTable = [client tableWithName:@"NoSuchTable"];
    
    // Create the item
    NSDictionary *item = @{ @"text":@"Write E2E test!", @"complete": @(NO) };
    
    // Insert the item
    [todoTable insert:item completion:^(NSDictionary *item, NSError *error) {
        
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"Error: Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
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
        
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"Error: Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
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
        
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"Error: An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
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
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"Error: Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
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
        
        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"Error: An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testDeleteItemWithIdForNonExistentItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Delete the item
    [todoTable deleteWithId:@-5 completion:^(NSNumber *itemId, NSError *error) {

        XCTAssertNil(itemId, @"itemId should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"Error: An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
}


#pragma mark * Negative ReadWithId Tests


-(void) testReadWithIdForNonExistentTable
{
    MSTable *todoTable = [client tableWithName:@"NoSuchTable"];

    // Insert the item
    [todoTable readWithId:@100 completion:^(NSDictionary *item, NSError *error) {
    
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"Error: Table 'NoSuchTable' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testReadWithIdForNonExistentItemId
{
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    
    // Insert the item
    [todoTable readWithId:@-5 completion:^(NSDictionary *item, NSError *error) {
    
        XCTAssertNil(item, @"item should have been nil.");
        
        XCTAssertNotNil(error, @"error should not have been nil.");
        XCTAssertTrue(error.domain == MSErrorDomain,
                     @"error domain should have been MSErrorDomain.");
        XCTAssertTrue(error.code == MSErrorMessageErrorCode,
                     @"error code should have been MSErrorMessageErrorCode.");
        
        NSString *description = (error.userInfo)[NSLocalizedDescriptionKey];
        XCTAssertTrue([description isEqualToString:@"Error: An item with id '-5' does not exist."],
                     @"description was: %@", description);
        
        done = YES;
    }];
    
    XCTAssertTrue([self waitForTest:90.0], @"Test timed out.");
}

-(void) testConnectionWithDelegateQueue
{
    // Verify default behavior (callback on thread calling function, which will be the main thread here
    MSTable *todoTable = [client tableWithName:@"todoItem"];
    [todoTable readWithId:@1 completion:^(NSDictionary *item, NSError *error) {
        XCTAssertTrue([NSThread isMainThread], @"expected to be on main thread");
        done = YES;
    }];
    done = NO;
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");
    
    [client invokeAPI:@"testapi" body:nil HTTPMethod:@"GET" parameters:nil headers:nil completion:^(id result, NSHTTPURLResponse *response, NSError *error) {
        XCTAssertTrue([NSThread isMainThread], @"expected to be on main thread");
        done = YES;
    }];
    done = NO;
    XCTAssertTrue([self waitForTest:30.0], @"Test timed out.");

    // Now verify moved to the operation queue
    client.connectionDelegateQueue = [[NSOperationQueue alloc] init];
    client.connectionDelegateQueue.name = @"azure.mobileservices.testing";

    todoTable = [client tableWithName:@"todoItem"];
    [todoTable readWithId:@1 completion:^(NSDictionary *item, NSError *error) {
        XCTAssertFalse([NSThread isMainThread], @"expected to not be on main thread");
        done = YES;
    }];
    
    done = NO;
    XCTAssertTrue([self waitForTest:30.0 forLoopMode:NSRunLoopCommonModes], @"Test timed out.");
    
    [client invokeAPI:@"testapi" body:nil HTTPMethod:@"GET" parameters:nil headers:nil completion:^(id result, NSHTTPURLResponse *response, NSError *error) {
        XCTAssertFalse([NSThread isMainThread], @"expected to not be on main thread");
        done = YES;
    }];
    XCTAssertTrue([self waitForTest:30.0 forLoopMode:NSRunLoopCommonModes], @"Test timed out.");
}


#pragma mark * Async Test Helper Method


-(BOOL) waitForTest:(NSTimeInterval)testDuration {
    return [self waitForTest:testDuration forLoopMode:NSDefaultRunLoopMode];
}

-(BOOL) waitForTest:(NSTimeInterval)testDuration forLoopMode:(NSString *)loopMode {

    NSDate *timeoutAt = [NSDate dateWithTimeIntervalSinceNow:testDuration];

    while (!done) {
        [[NSRunLoop currentRunLoop] runMode:loopMode
                                 beforeDate:timeoutAt];
        if([timeoutAt timeIntervalSinceNow] <= 0.0) {
            break;
        }
    };
    
    return done;
}

@end
