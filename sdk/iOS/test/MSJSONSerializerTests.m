// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "WindowsAzureMobileServices.h"
#import "MSJSONSerializer.h"
#import "MSClientConnection.h"
#import "MSTable+MSTableTestUtilities.h"

@interface MSJSONSerializerTests : SenTestCase {
    MSJSONSerializer *serializer;
}

@end

@implementation MSJSONSerializerTests

# pragma mark * Setup and TearDown Methods

- (void) setUp
{
    NSLog(@"%@ setUp", self.name);
    
    serializer = [MSJSONSerializer JSONSerializer];
}

- (void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}

# pragma mark * stringFromItem:orError
-(void)testStringFromItemReturnsId
{
    NSArray *validIds = [MSTable testValidStringIds];
    validIds = [validIds arrayByAddingObjectsFromArray:@[
        @{@"id" : @1, @"string": @"1"},
        @{@"id" : @INT_MAX, @"string": [NSString stringWithFormat:@"%d", INT_MAX]},
        @{@"id" : @LONG_LONG_MAX, @"string": [NSString stringWithFormat:@"%lld", LONG_LONG_MAX]}
        ]];
    
    NSError *error = nil;
    
    for (id test in validIds)
    {
        id testId;
        NSString *expected;
        if([test isKindOfClass:[NSDictionary class]]) {
            testId = [test objectForKey:@"id"];
            expected = [test objectForKey:@"string"];
        } else {
            testId = test;
            expected = test;
        }
        NSString *actualId = [serializer stringFromItemId:testId orError:&error];
        
        STAssertNil(error, @"error was not nil after getting string id for %@", testId);
        STAssertTrue([actualId isEqualToString:expected], @"error string id was %@ and not %@", actualId, expected);
    }
}

-(void)testStringFromItemErrorsOnInvalidIds
{
    NSArray *invalidIds = [MSTable testInvalidStringIds];
    invalidIds = [invalidIds arrayByAddingObjectsFromArray:[MSTable testEmptyStringIdsIncludingNull:YES]];
    invalidIds = [invalidIds arrayByAddingObjectsFromArray:[MSTable testInvalidIntIds]];
    invalidIds = [invalidIds arrayByAddingObjectsFromArray:[MSTable testNonStringNonIntValidJsonIds]];
    invalidIds = [invalidIds arrayByAddingObjectsFromArray:[MSTable testNonStringNonIntIds]];    
                  
    NSError *error = nil;
    for (id testId in invalidIds)
    {
        NSString *actualId = [serializer stringFromItemId:testId orError:&error];
        STAssertNotNil(error, @"error was nil after getting string id %@", actualId);
        
        STAssertEquals([@MSInvalidItemIdWithRequest integerValue], error.code, @"Unexpected error code: %d", error.code);
        STAssertEqualObjects(@"The item provided did not have a valid id.", error.localizedDescription, @"Unexpected messge: %@", error.localizedDescription);
    }
}

-(void)testStringFromItemErrorsOnNilItemId
{
    NSError *error = nil;
    [serializer stringFromItemId:nil orError:&error];
    STAssertNotNil(error, @"error was nil after getting nil item id");
    STAssertEquals([@MSExpectedItemIdWithRequest integerValue], error.code, @"Unexpected error code: %d", error.code);
    STAssertEqualObjects(@"The item id was not provided.", error.localizedDescription, @"Unexpected message: %@", error.localizedDescription);
}

# pragma mark * dataFromItem:idAllowed:ensureDictionary:orError: Tests

-(void)testDataFromItemReturnsData
{
    NSDictionary *item = @{ @"id" : @5, @"name" : @"bob" };
    
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:YES
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNotNil(data, @"data was nil after serializing item.");
    STAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"id\":5,\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

-(void)testDataFromItemWithStringIdReturnsData
{
    NSDictionary *item = @{ @"id" : @"MY-ID", @"name" : @"bob" };
    
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:YES
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNotNil(data, @"data was nil after serializing item.");
    STAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"id\":\"MY-ID\",\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

-(void)testDataFromItemWithStringIdReturnsDataWithIdNotAllowed
{
    NSDictionary *item = @{ @"id" : @"MY-ID", @"name" : @"bob" };
    
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:NO
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNotNil(data, @"data was nil after serializing item.");
    STAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"id\":\"MY-ID\",\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

-(void)testDataFromItemWithNullUppercaseIdReturnsDataWithIdNotAllowed
{
    NSDictionary *item = @{ @"ID" : [NSNull null], @"name" : @"bob" };
    
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:NO
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNotNil(data, @"data was nil after serializing item.");
    STAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"ID\":null,\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

-(void)testDataFromItemWithEmptyUppercaseIdReturnsDataWithIdNotAllowed
{
    NSDictionary *item = @{ @"ID" : @"", @"name" : @"bob" };
    
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:NO
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNotNil(data, @"data was nil after serializing item.");
    STAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"ID\":\"\",\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

-(void)testDataFromItemErrorForNilItem
{
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:nil
                                  idAllowed:YES
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNil(data, @"data was not nil after serializing item.");
    STAssertNotNil(error, @"error was nil after serializing item.");
    STAssertTrue(error.domain == MSErrorDomain,
                 @"error domain should have been MSErrorDomain.");
    STAssertTrue(error.code == MSExpectedItemWithRequest,
                 @"error code should have been MSExpectedItemWithRequest.");
    
    NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
    STAssertTrue([description isEqualToString:@"No item was provided."],
                 @"description was: %@", description);
}

-(void)testDataFromItemReturnsDataWithDatesSerialized
{
    // Create some dates
    NSDateComponents *dateParts = [[NSDateComponents alloc] init];
    dateParts.year = 1999;
    dateParts.month = 12;
    dateParts.day = 3;
    dateParts.hour = 15;
    dateParts.minute = 44;
    dateParts.calendar = [[NSCalendar alloc]
                          initWithCalendarIdentifier:NSGregorianCalendar];
    dateParts.calendar.timeZone = [NSTimeZone timeZoneForSecondsFromGMT:0];
    
    NSDate *date1 = dateParts.date;
    dateParts.second = 29;
    NSDate *date2 = dateParts.date;
    NSDate *date3 = [date2 dateByAddingTimeInterval:0.3];
    
    NSDictionary *item = @{
        @"id" : @5,
        @"x" : @[
            date1,
            @{ @"y" : date2 }
        ],
        @"z" : date3
    };
    
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:YES
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNotNil(data, @"data was nil after serializing item.");
    STAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"id\":5,\"x\":[\"1999-12-03T15:44:00.000Z\",{\"y\":\"1999-12-03T15:44:29.000Z\"}],\"z\":\"1999-12-03T15:44:29.300Z\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

-(void)testDataFromItemErrorWithIdNotAllowed
{
    NSError *error = nil;
    NSDictionary *item = @{ @"id" : @5, @"name" : @"bob" };
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:NO
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNil(data, @"data was not nil after serializing item.");
    STAssertNotNil(error, @"error was nil after serializing item.");
    STAssertTrue(error.domain == MSErrorDomain,
                 @"error domain should have been MSErrorDomain.");
    STAssertTrue(error.code == MSExistingItemIdWithRequest,
                 @"error code should have been MSExistingItemIdWithRequest.");
    
    NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
    STAssertTrue([description isEqualToString:@"The item provided must not have an id."],
                 @"description was: %@", description);
}

-(void)testDataFromItemErrorWithMultipleIdNotAllowed
{
    NSError *error = nil;
    NSDictionary *item = @{ @"id" : @5, @"Id": @10, @"name" : @"bob" };
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:NO
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNil(data, @"data was not nil after serializing item.");
    STAssertNotNil(error, @"error was nil after serializing item.");
    STAssertTrue(error.domain == MSErrorDomain,
                 @"error domain should have been MSErrorDomain.");
    STAssertTrue(error.code == MSExistingItemIdWithRequest,
                 @"error code should have been MSExistingItemIdWithRequest.");
    
    NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
    STAssertTrue([description isEqualToString:@"The item provided must not have an id."],
                 @"description was: %@", description);
}

-(void)testDataFromItemErrorWithStringUpperCaseIdNotAllowed
{
    NSError *error = nil;
    NSDictionary *item = @{ @"iD" : @"MY-ID", @"name" : @"bob" };
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:NO
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNil(data, @"data was not nil after serializing item.");
    STAssertNotNil(error, @"error was nil after serializing item.");
    STAssertTrue(error.domain == MSErrorDomain,
                 @"error domain should have been MSErrorDomain.");
    STAssertTrue(error.code == MSInvalidItemIdWithRequest,
                 @"error code should have been MSInvalidItemIdWithRequest.");
    
    NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
    STAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
                 @"description was: %@", description);
}

-(void)testDataFromItemErrorWithIdUpperCaseNotAllowed
{
    NSError *error = nil;
    NSDictionary *item = @{ @"Id" : @5, @"name" : @"bob" };
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:NO
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNil(data, @"data was not nil after serializing item.");
    STAssertNotNil(error, @"error was nil after serializing item.");
    STAssertTrue(error.domain == MSErrorDomain,
                 @"error domain should have been MSErrorDomain.");
    STAssertTrue(error.code == MSExistingItemIdWithRequest,
                 @"error code should have been MSExistingItemIdWithRequest.");
    
    NSString *description = [error.userInfo objectForKey:NSLocalizedDescriptionKey];
    STAssertTrue([description isEqualToString:@"The item provided must not have an id."],
                 @"description was: %@", description);
}

-(void)testDataFromItemErrorWithIdNotAllowedAndNoId
{
    NSError *error = nil;
    NSDictionary *item = @{ @"name" : @"bob" };
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:NO
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNotNil(data, @"data was nil after serializing item.");
    STAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

-(void)testDataFromItemWithNonDictionaryItem
{
    NSArray *item = @[ @{ @"id" : @5, @"name" : @"bob" } ];
    
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:YES
                           ensureDictionary:NO
                     removeSystemProperties:NO
                                    orError:&error];
    
    STAssertNotNil(data, @"data was nil after serializing item.");
    STAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"[{\"id\":5,\"name\":\"bob\"}]";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

# pragma mark * itemIdFromItem: Tests

-(void)testItemIdFromItemReturnsId
{
    NSDictionary *item = @{ @"id" : @5, @"name" : @"bob" };
    NSError *error = nil;
    NSNumber *itemId = [serializer itemIdFromItem:item orError:&error];
    long long expected = 5;
    
    STAssertNil(error, @"error should have been nil.");
    STAssertEquals(expected, [itemId longLongValue], @"itemId was not correct.");
}

-(void)testItemIdFromItemWithStringIdReturnsId
{
    NSDictionary *item = @{ @"id" : @"my-id", @"name" : @"bob" };
    NSError *error = nil;
    NSString *itemId = [serializer itemIdFromItem:item orError:&error];

    STAssertNil(error, @"error should have been nil.");
    STAssertEquals(@"my-id", itemId, @"itemId was not correct.");
}

-(void)testItemIdFromItemThrowsForMissingId
{
    NSDictionary *item = @{ @"name" : @"bob" };

    NSError *error = nil;
    NSNumber *itemId = [serializer itemIdFromItem:item orError:&error];
    
    STAssertNotNil(error, @"error should not have been nil.");
    STAssertNil(itemId, @"itemId should have been nil.");
}

-(void)testItemIdFromItemThrowsForNonNumericNonStringMissingId
{
    NSDictionary *item = @{ @"id" : [NSNull null], @"name" : @"bob" };
    
    NSError *error = nil;
    id itemId = [serializer itemIdFromItem:item orError:&error];
    
    STAssertNotNil(error, @"error should not have been nil.");
    STAssertNil(itemId, @"itemId should have been nil.");
}

-(void)testItemIdFromItemReturnsErrorIfIdIsNotLowercased
{
    NSDictionary *item = @{ @"Id" : @"5", @"name" : @"bob" };
    
    NSError *error = nil;
    NSNumber *itemId = [serializer itemIdFromItem:item orError:&error];
    
    STAssertNotNil(error, @"error should not have been nil.");
    STAssertNil(itemId, @"itemId should have been nil.");
}


# pragma mark * itemFromData: Tests

-(void)testItemFromDataReturnsOriginalItemUpdated
{
    NSDictionary *originalItem = @{ @"id" : @5, @"name" : @"fred" };
    NSMutableDictionary *mutableOriginalItem =
                    [NSMutableDictionary dictionaryWithDictionary:originalItem];
    
    NSString* stringData = @"{\"id\":5,\"name\":\"bob\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];

    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:mutableOriginalItem
                         ensureDictionary:YES
                                  orError:&error];
    
    STAssertNotNil(newItem, @"item was nil after deserializing item.");
    STAssertNil(error, @"error was not nil after deserializing item.");
    STAssertTrue([[newItem objectForKey:@"name"] isEqualToString:@"bob"],
                 @"The name key should have been updated to 'bob'.");
}

-(void)testItemFromDataReturnsNewItem
{
    NSString* stringData = @"{\"id\":5,\"name\":\"bob\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:nil
                         ensureDictionary:YES
                                  orError:&error];
    
    STAssertNotNil(newItem, @"item was nil after deserializing item.");
    STAssertNil(error, @"error was not nil after deserializing item.");
    STAssertTrue([[newItem objectForKey:@"name"] isEqualToString:@"bob"],
                 @"The name key should have been updated to 'bob'.");
}

-(void)testItemFromDataReturnsNewItemWithDates
{
    NSString* stringData = @"{\"id\":5,\"date\":\"1999-12-03T15:44:29.000Z\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:nil
                         ensureDictionary:YES
                                  orError:&error];
    
    STAssertNotNil(newItem, @"item was nil after deserializing item.");
    STAssertNil(error, @"error was not nil after deserializing item.");
    
    NSDate *date = [newItem objectForKey:@"date"];
    STAssertNotNil(date, @"date was nil after deserializing item.");
    
    NSCalendar *gregorian = [[NSCalendar alloc]
                             initWithCalendarIdentifier:NSGregorianCalendar];
    [gregorian setTimeZone:[NSTimeZone timeZoneForSecondsFromGMT:0]];
    NSDateComponents *dateParts =
        [gregorian components:(NSYearCalendarUnit |
                               NSHourCalendarUnit |
                               NSSecondCalendarUnit)
                     fromDate:date];

    STAssertTrue(dateParts.year == 1999, @"year was: %d", dateParts.year);
    STAssertTrue(dateParts.hour == 15, @"hour was: %d", dateParts.hour);
    STAssertTrue(dateParts.second == 29, @"second was: %d", dateParts.second);
}

-(void)testItemFromDataReturnsNewItemWithNoFractionalSecondsDates
{
    NSString* stringData = @"{\"id\":5,\"date\":\"1999-12-03T15:44:29Z\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:nil
                         ensureDictionary:YES
                                  orError:&error];
    
    STAssertNotNil(newItem, @"item was nil after deserializing item.");
    STAssertNil(error, @"error was not nil after deserializing item.");
    
    NSDate *date = [newItem objectForKey:@"date"];
    STAssertNotNil(date, @"date was nil after deserializing item.");
    
    NSCalendar *gregorian = [[NSCalendar alloc]
                             initWithCalendarIdentifier:NSGregorianCalendar];
    [gregorian setTimeZone:[NSTimeZone timeZoneForSecondsFromGMT:0]];
    NSDateComponents *dateParts =
    [gregorian components:(NSYearCalendarUnit |
                           NSHourCalendarUnit |
                           NSSecondCalendarUnit)
                 fromDate:date];
    
    STAssertTrue(dateParts.year == 1999, @"year was: %d", dateParts.year);
    STAssertTrue(dateParts.hour == 15, @"hour was: %d", dateParts.hour);
    STAssertTrue(dateParts.second == 29, @"second was: %d", dateParts.second);
}

-(void)testItemFromDataReturnsErrorIfReadFails
{
    NSString* stringData = @"This isn't proper JSON";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:nil
                         ensureDictionary:YES
                                  orError:&error];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertNil(newItem, @"error was not nil after deserializing item.");
    STAssertTrue([[error domain] isEqualToString:@"NSCocoaErrorDomain"],
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == 3840, // JSON Parse Error
                 @"error code was: %d",[error code]);
}

-(void)testItemFromDataReturnsErrorIfItemIsNotDictionary
{
    NSString* stringData = @"[ 5, \"This is not an object!\"  ]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:nil
                         ensureDictionary:YES
                                  orError:&error];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertNil(newItem, @"newItem was not nil after deserializing item.");
    STAssertTrue([error domain] == MSErrorDomain,
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSExpectedItemWithResponse,
                 @"error code was: %d",[error code]);
}

-(void)testItemFromDataReturnsNonDictionary
{
    NSString* stringData = @"[ 5, \"This is not an object!\"  ]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    id newItem = [serializer itemFromData:data
                         withOriginalItem:nil
                         ensureDictionary:NO
                                  orError:&error];
    
    STAssertNil(error, @"error was not nil after deserializing item.");
    STAssertNotNil(newItem, @"newItem was nil after deserializing item.");
    STAssertTrue([[newItem objectAtIndex:0] isEqual:@5],
                 @"The first element should have been a 5.");
    STAssertTrue([[newItem objectAtIndex:1] isEqualToString:@"This is not an object!"],
                 @"The second element should have been 'This is not an object!'.");
}


# pragma mark * totalCountAndItems: Tests


-(void)testTotalCountAndItemsReturnsItems
{
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
}

-(void)testTotalCountAndItemsReturnsItemsWithDates
{
    NSString* stringData = @"[{\"id\":5,\"name\":\"bob\",\"dates\":[\"1999-12-03T15:44:29.0Z\"]},{\"id\":6,\"name\":\"mary\",\"date\":\"2012-11-03T8:44:00.0Z\"}]";
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
    
    
    NSArray *dates = [[items objectAtIndex:0] objectForKey:@"dates"];
    STAssertNotNil(dates, @"dates was nil after deserializing item.");
    
    NSDate *date = [dates objectAtIndex:0];
    STAssertNotNil(date, @"date was nil after deserializing item.");
    
    NSCalendar *gregorian = [[NSCalendar alloc]
                             initWithCalendarIdentifier:NSGregorianCalendar];
    [gregorian setTimeZone:[NSTimeZone timeZoneForSecondsFromGMT:0]];
    NSDateComponents *dateParts =
    [gregorian components:(NSYearCalendarUnit |
                           NSHourCalendarUnit |
                           NSSecondCalendarUnit)
                 fromDate:date];
    
    STAssertTrue(dateParts.year == 1999, @"year was: %d", dateParts.year);
    STAssertTrue(dateParts.hour == 15, @"hour was: %d", dateParts.hour);
    STAssertTrue(dateParts.second == 29, @"second was: %d", dateParts.second);
    
    NSDate *date2 = [[items objectAtIndex:1] objectForKey:@"date"];
    STAssertNotNil(date2, @"date was nil after deserializing item.");

    NSDateComponents *dateParts2 =
    [gregorian components:(NSYearCalendarUnit |
                           NSHourCalendarUnit |
                           NSSecondCalendarUnit)
                 fromDate:date2];
    
    STAssertTrue(dateParts2.year == 2012, @"year was: %d", dateParts2.year);
    STAssertTrue(dateParts2.hour == 8, @"hour was: %d", dateParts2.hour);
    STAssertTrue(dateParts2.second == 0, @"second was: %d", dateParts2.second);
}

-(void)testTotalCountAndItemsReturnsTotalCount
{
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
}

-(void)testTotalCountAndItemsReturnsErrorIfMissingCount
{
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
}

-(void)testTotalCountAndItemsReturnsErrorIfMissingResults
{
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
}


# pragma mark * errorFromData: Tests


-(void)testErrorFromDataReturnsError
{
    NSString* stringData = @"\"This is an Error Message!\"";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"text/json"];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([error domain] == MSErrorDomain,
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSErrorMessageErrorCode,
                 @"error code was: %d",[error code]);
    STAssertTrue([[error localizedDescription] isEqualToString:
                  @"This is an Error Message!"],
                 @"error description was: %@", [error localizedDescription]);
}

-(void)testErrorFromDataReturnsErrorFromObjectWithErrorKey
{
    NSString* stringData = @"{\"error\":\"This is another Error Message!\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"text/JSON"];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([error domain] == MSErrorDomain,
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSErrorMessageErrorCode,
                 @"error code was: %d",[error code]);
    STAssertTrue([[error localizedDescription] isEqualToString:
                  @"This is another Error Message!"],
                 @"error description was: %@", [error localizedDescription]);
}

-(void)testErrorFromDataReturnsErrorFromObjectWithDescriptionKey
{    
    NSString* stringData = @"{\"description\":\"This is another Error Message!\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"application/json"];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([error domain] == MSErrorDomain,
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSErrorMessageErrorCode,
                 @"error code was: %d",[error code]);
    STAssertTrue([[error localizedDescription] isEqualToString:
                  @"This is another Error Message!"],
                 @"error description was: %@", [error localizedDescription]);
}

-(void)testErrorFromDataReturnsErrorIfReadFails
{
    NSString* stringData = @"invalid JSON!";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"application/JSON"];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([[error domain] isEqualToString:@"NSCocoaErrorDomain"],
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == 3840, // JSON Parse Error
                 @"error code was: %d",[error code]);
}

-(void)testErrorFromDataReturnsJsonEvenIfNotExpectedJsonForm
{
    NSString* stringData = @"{}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"text/json"];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([[error domain] isEqualToString:MSErrorDomain],
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSErrorMessageErrorCode,
                 @"error code was: %d",[error code]);
    STAssertTrue([[error localizedDescription] isEqualToString:
                  @"{}"],
                  @"error description was: %@", [error localizedDescription]);
}

-(void)testErrorFromDataReturnsNonJson
{
    NSString* stringData = @"<Hey>This sure is some poor xml</Hey>";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"application/xml"];
    
    STAssertNotNil(error, @"error was nil after deserializing item.");
    STAssertTrue([[error domain] isEqualToString:MSErrorDomain],
                 @"error domain was: %@", [error domain]);
    STAssertTrue([error code] == MSErrorMessageErrorCode,
                 @"error code was: %d",[error code]);
    STAssertTrue([[error localizedDescription] isEqualToString:
                  @"<Hey>This sure is some poor xml</Hey>"],
                  @"error description was: %@", [error localizedDescription]);
}

-(void)testSystemPropertiesNotRemovedWithIntId
{
    NSError *error;
    
    NSDictionary *item = @{@"id": @7, @"__Prop1": @6};
    NSData *data = [serializer dataFromItem:item idAllowed:YES ensureDictionary:NO removeSystemProperties:YES orError:&error];
    
    STAssertNil(error, @"An error occurred %d", error.code);
    
    NSString *expected = @"{\"id\":7,\"__Prop1\":6}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

-(void)testSystemPropertiesNotRemovedWithArray
{
    NSError *error;
    
    NSArray *item = @[@{@"id": @7, @"__Prop1": @6}];
    NSData *data = [serializer dataFromItem:item idAllowed:YES ensureDictionary:NO removeSystemProperties:YES orError:&error];
    
    STAssertNil(error, @"An error occurred %d", error.code);
    
    NSString *expected = @"[{\"id\":7,\"__Prop1\":6}]";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

-(void)testSystemPropertiesRemovedWithStringId
{
    NSError *error;
    NSDictionary *item = @{@"id": @"one", @"__Prop1": @6, @"__prop4": @"help"};
    NSData *data = [serializer dataFromItem:item idAllowed:YES ensureDictionary:NO removeSystemProperties:YES orError:&error];
    
    STAssertNil(error, @"An error occurred %d", error.code);
    
    NSString *expected = @"{\"id\":\"one\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                         encoding:NSUTF8StringEncoding];
    
    STAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

@end
