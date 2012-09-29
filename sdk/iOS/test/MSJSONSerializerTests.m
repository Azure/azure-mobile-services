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
#import "MSJSONSerializer.h"

#import "MSClientConnection.h"


@interface MSJSONSerializerTests : SenTestCase

@end


static MSJSONSerializer *serializer;


@implementation MSJSONSerializerTests


# pragma mark * Setup and TearDown Methods


- (void) setUp {
    
    NSLog(@"%@ setUp", self.name);
    
    if (serializer == nil) {
        serializer = [MSJSONSerializer JSONSerializer];
    }
}

- (void) tearDown {

    NSLog(@"%@ tearDown", self.name);
}


# pragma mark * dataFromItem:orError: Tests


-(void)testDataFromItemReturnsData
{
    NSLog(@"%@ start", self.name);
    
    NSDictionary *item = @{ @"id" : @5, @"name" : @"bob" };
    
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:item orError:&error];
    
    STAssertNotNil(data, @"data was nil after serializing item.");
    STAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"id\":5,\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);

    NSLog(@"%@ end", self.name);
}


# pragma mark * itemIdFromItem: Tests


-(void)testItemIdFromItemReturnsId
{
    NSLog(@"%@ start", self.name);
    
    NSDictionary *item = @{ @"id" : @5, @"name" : @"bob" };
    NSError *error = nil;
    NSNumber *itemId = [serializer itemIdFromItem:item orError:&error];
    long expected = 5;
    
    STAssertNil(error, @"error should have been nil.");
    STAssertEquals(expected, [itemId longValue], @"itemId was not correct.");
    
    NSLog(@"%@ end", self.name);
}

-(void)testItemIdFromItemThrowsForMissingId
{
    NSLog(@"%@ start", self.name);
    
    NSDictionary *item = @{ @"name" : @"bob" };

    NSError *error = nil;
    NSNumber *itemId = [serializer itemIdFromItem:item orError:&error];
    
    STAssertNotNil(error, @"error should not have been nil.");
    STAssertNil(itemId, @"itemId should have been nil.");
    
    NSLog(@"%@ end", self.name);
}

-(void)testItemIdFromItemThrowsForNonNumericMissingId
{
    NSLog(@"%@ start", self.name);
    
    NSDictionary *item = @{ @"id" : @"anId", @"name" : @"bob" };
    
    NSError *error = nil;
    NSNumber *itemId = [serializer itemIdFromItem:item orError:&error];
    
    STAssertNotNil(error, @"error should not have been nil.");
    STAssertNil(itemId, @"itemId should have been nil.");
    
    NSLog(@"%@ end", self.name);
}


# pragma mark * itemFromData: Tests


-(void)testItemFromDataReturnsOriginalItemUpdated
{
    NSLog(@"%@ start", self.name);
    
    NSDictionary *originalItem = @{ @"id" : @5, @"name" : @"fred" };
    NSMutableDictionary *mutableOriginalItem =
                    [NSMutableDictionary dictionaryWithDictionary:originalItem];
    
    NSString* stringData = @"{\"id\":5,\"name\":\"bob\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];

    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:mutableOriginalItem
                                  orError:&error];
    
    STAssertNotNil(newItem, @"item was nil after deserializing item.");
    STAssertNil(error, @"error was not nil after deserializing item.");
    STAssertTrue([[newItem objectForKey:@"name"] isEqualToString:@"bob"],
                 @"The name key should have been updated to 'bob'.");
    
    NSLog(@"%@ end", self.name);
}

-(void)testItemFromDataReturnsNewItem
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"{\"id\":5,\"name\":\"bob\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:nil
                                  orError:&error];
    
    STAssertNotNil(newItem, @"item was nil after deserializing item.");
    STAssertNil(error, @"error was not nil after deserializing item.");
    STAssertTrue([[newItem objectForKey:@"name"] isEqualToString:@"bob"],
                 @"The name key should have been updated to 'bob'.");
    
    NSLog(@"%@ end", self.name);
}

-(void)testItemFromDataReturnsErrorIfReadFails
{
    NSLog(@"%@ start", self.name);

    NSString* stringData = @"This isn't proper JSON";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:nil
                                  orError:&error];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertNil(newItem, @"error was not nil after deserializing item.");
    STAssertTrue([[error domain] isEqualToString:@"NSCocoaErrorDomain"],
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == 3840, // JSON Parse Error
                 @"error code was: %d",[error code]);
    
    NSLog(@"%@ end", self.name);
}

-(void)testItemFromDataReturnsErrorIfItemIsNotObject
{
    NSLog(@"%@ start", self.name);

    NSString* stringData = @"[ 5, \"This is not an object!\"  ]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:nil
                                  orError:&error];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertNil(newItem, @"error was not nil after deserializing item.");
    STAssertTrue([error domain] == MSErrorDomain,
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSExpectedItemWithResponse,
                 @"error code was: %d",[error code]);
    
    NSLog(@"%@ end", self.name);
}

# pragma mark * totalCountAndItems: Tests

-(void)testTotalCountAndItemsReturnsItems
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"[{\"id\":5,\"name\":\"bob\"},{\"id\":6,\"name\":\"mary\"}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    NSArray *items = nil;
    NSInteger totalCount = [serializer totalCountAndItems:&items
                                                 fromData:data
                                                  orError:&error];
    
    STAssertNotNil(items, @"items was nil after deserializing item.");
    STAssertNil(error, @"error was not nil after deserializing item.");
    STAssertTrue(totalCount == -1,
                 @"The totalCount should have been -1 since it was not given.");
    STAssertTrue(items.count == 2,
                 @"The items array should have had 2 items in it.");
    
    STAssertTrue([[[items objectAtIndex:0] objectForKey:@"name"]
                  isEqualToString:@"bob"],
                 @"The name key should have been updated to 'bob'.");
    
    NSLog(@"%@ end", self.name);
}

-(void)testTotalCountAndItemsReturnsTotalCount
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"{\"results\":[{\"id\":5,\"name\":\"bob\"},{\"id\":6,\"name\":\"mary\"}],\"count\":50}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    NSArray *items = nil;
    NSInteger totalCount = [serializer totalCountAndItems:&items
                                                 fromData:data
                                                  orError:&error];
    
    STAssertNotNil(items, @"items was nil after deserializing item.");
    STAssertNil(error, @"error was not nil after deserializing item.");
    STAssertTrue(totalCount == 50,
                 @"The totalCount should have been 50 since it was given.");
    STAssertTrue(items.count == 2,
                 @"The items array should have had 2 items in it.");
    
    STAssertTrue([[[items objectAtIndex:0] objectForKey:@"name"]
                  isEqualToString:@"bob"],
                 @"The name key should have been updated to 'bob'.");
    
    NSLog(@"%@ end", self.name);
}

-(void)testTotalCountAndItemsReturnsErrorIfReadFails
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"invalid JSON!";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    NSArray *items = nil;
    NSInteger totalCount = [serializer totalCountAndItems:&items
                                                 fromData:data
                                                  orError:&error];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertNil(items, @"error was not nil after deserializing item.");
    STAssertTrue(totalCount == -1,
                 @"The totalCount should have been -1 since there was an error.");
    STAssertTrue([[error domain] isEqualToString:@"NSCocoaErrorDomain"],
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == 3840, // JSON Parse Error
                 @"error code was: %d",[error code]);
    
    NSLog(@"%@ end", self.name);
}

-(void)testTotalCountAndItemsReturnsErrorIfMissingCount
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"{\"results\":[{\"id\":5,\"name\":\"bob\"},{\"id\":6,\"name\":\"mary\"}]}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    NSArray *items = nil;
    NSInteger totalCount = [serializer totalCountAndItems:&items
                                                 fromData:data
                                                  orError:&error];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertNil(items, @"error was not nil after deserializing item.");
    STAssertTrue(totalCount == -1,
                 @"The totalCount should have been -1 since there was an error.");
    STAssertTrue([error domain] == MSErrorDomain,
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSExpectedTotalCountWithResponse,
                 @"error code was: %d",[error code]);
    
    NSLog(@"%@ end", self.name);
}

-(void)testTotalCountAndItemsReturnsErrorIfMissingResults
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"{\"results\":5,\"count\":50}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    NSArray *items = nil;
    NSInteger totalCount = [serializer totalCountAndItems:&items
                                                 fromData:data
                                                  orError:&error];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertNil(items, @"error was not nil after deserializing item.");
    STAssertTrue(totalCount == -1,
                 @"The totalCount should have been -1 since there was an error.");
    STAssertTrue([error domain] == MSErrorDomain,
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSExpectedItemsWithResponse,
                 @"error code was: %d",[error code]);
    
    NSLog(@"%@ end", self.name);
}

# pragma mark * errorFromData: Tests

-(void)testErrorFromDataReturnsError
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"\"This is an Error Message!\"";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([error domain] == MSErrorDomain,
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSErrorMessageErrorCode,
                 @"error code was: %d",[error code]);
    STAssertTrue([[error localizedDescription] isEqualToString:
                  @"This is an Error Message!"],
                 @"error description was: %@", [error localizedDescription]);
        
    NSLog(@"%@ end", self.name);
}

-(void)testErrorFromDataReturnsErrorFromObjectWithErrorKey
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"{\"error\":\"This is another Error Message!\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([error domain] == MSErrorDomain,
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSErrorMessageErrorCode,
                 @"error code was: %d",[error code]);
    STAssertTrue([[error localizedDescription] isEqualToString:
                  @"This is another Error Message!"],
                 @"error description was: %@", [error localizedDescription]);
    
    NSLog(@"%@ end", self.name);
}

-(void)testErrorFromDataReturnsErrorFromObjectWithDescriptionKey
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"{\"description\":\"This is another Error Message!\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([error domain] == MSErrorDomain,
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSErrorMessageErrorCode,
                 @"error code was: %d",[error code]);
    STAssertTrue([[error localizedDescription] isEqualToString:
                  @"This is another Error Message!"],
                 @"error description was: %@", [error localizedDescription]);
    
    NSLog(@"%@ end", self.name);
}

-(void)testErrorFromDataReturnsErrorIfReadFails
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"invalid JSON!";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([[error domain] isEqualToString:@"NSCocoaErrorDomain"],
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == 3840, // JSON Parse Error
                 @"error code was: %d",[error code]);
    
    NSLog(@"%@ end", self.name);
}

-(void)testErrorFromDataReturnsNilIfNoError
{
    NSLog(@"%@ start", self.name);
    
    NSString* stringData = @"{}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([[error domain] isEqualToString:MSErrorDomain],
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSErrorNoMessageErrorCode,
                 @"error code was: %d",[error code]);

    NSLog(@"%@ end", self.name);
}

@end
