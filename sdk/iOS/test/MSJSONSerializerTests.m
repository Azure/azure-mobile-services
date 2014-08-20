// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "WindowsAzureMobileServices.h"
#import "MSJSONSerializer.h"
#import "MSClientConnection.h"
#import "MSTable+MSTableTestUtilities.h"

@interface MSJSONSerializerTests : XCTestCase {
    MSJSONSerializer *serializer;
}

@end

@implementation MSJSONSerializerTests


#pragma mark * Setup and TearDown Methods


- (void) setUp
{
    NSLog(@"%@ setUp", self.name);
    
    serializer = [MSJSONSerializer JSONSerializer];
}

- (void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}


#pragma mark * stringFromItem:orError


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
            testId = test[@"id"];
            expected = test[@"string"];
        } else {
            testId = test;
            expected = test;
        }
        NSString *actualId = [serializer stringFromItemId:testId orError:&error];
        
        XCTAssertNil(error, @"error was not nil after getting string id for %@", testId);
        XCTAssertTrue([actualId isEqualToString:expected], @"error string id was %@ and not %@", actualId, expected);
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
        XCTAssertNotNil(error, @"error was nil after getting string id %@", actualId);
        
        XCTAssertEqual(error.code, MSInvalidItemIdWithRequest);
        XCTAssertEqualObjects(error.localizedDescription, @"The item provided did not have a valid id.");
    }
}

-(void)testStringFromItemErrorsOnNilItemId
{
    NSError *error = nil;
    [serializer stringFromItemId:nil orError:&error];
    XCTAssertNotNil(error, @"error was nil after getting nil item id");
    XCTAssertEqual(error.code, MSExpectedItemIdWithRequest);
    XCTAssertEqualObjects(error.localizedDescription, @"The item id was not provided.");
}


#pragma mark * dataFromItem:idAllowed:ensureDictionary:orError: Tests


-(void)testDataFromItemReturnsData
{
    NSDictionary *item = @{ @"id" : @5, @"name" : @"bob" };
    
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:item
                                  idAllowed:YES
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    XCTAssertNotNil(data, @"data was nil after serializing item.");
    XCTAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"id\":5,\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    XCTAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
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
    
    XCTAssertNotNil(data, @"data was nil after serializing item.");
    XCTAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"id\":\"MY-ID\",\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    XCTAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
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
    
    XCTAssertNotNil(data, @"data was nil after serializing item.");
    XCTAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"id\":\"MY-ID\",\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    XCTAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
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
    
    XCTAssertNotNil(data, @"data was nil after serializing item.");
    XCTAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"ID\":null,\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    XCTAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
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
    
    XCTAssertNotNil(data, @"data was nil after serializing item.");
    XCTAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"ID\":\"\",\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    XCTAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

-(void)testDataFromItemErrorForNilItem
{
    NSError *error = nil;
    NSData *data = [serializer dataFromItem:nil
                                  idAllowed:YES
                           ensureDictionary:YES
                     removeSystemProperties:NO
                                    orError:&error];
    
    XCTAssertNil(data, @"data was not nil after serializing item.");
    XCTAssertNotNil(error, @"error was nil after serializing item.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertTrue(error.code == MSExpectedItemWithRequest,
                 @"error code should have been MSExpectedItemWithRequest.");
    
    NSString *description = error.localizedDescription;
    XCTAssertTrue([description isEqualToString:@"No item was provided."],
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
    
    XCTAssertNotNil(data, @"data was nil after serializing item.");
    XCTAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"id\":5,\"x\":[\"1999-12-03T15:44:00.000Z\",{\"y\":\"1999-12-03T15:44:29.000Z\"}],\"z\":\"1999-12-03T15:44:29.300Z\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    XCTAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
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
    
    XCTAssertNil(data, @"data was not nil after serializing item.");
    XCTAssertNotNil(error, @"error was nil after serializing item.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertTrue(error.code == MSExistingItemIdWithRequest,
                 @"error code should have been MSExistingItemIdWithRequest.");
    
    NSString *description = error.localizedDescription;
    XCTAssertTrue([description isEqualToString:@"The item provided must not have an id."],
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
    
    XCTAssertNil(data, @"data was not nil after serializing item.");
    XCTAssertNotNil(error, @"error was nil after serializing item.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertTrue(error.code == MSExistingItemIdWithRequest,
                 @"error code should have been MSExistingItemIdWithRequest.");
    
    NSString *description = error.localizedDescription;
    XCTAssertTrue([description isEqualToString:@"The item provided must not have an id."],
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
    
    XCTAssertNil(data, @"data was not nil after serializing item.");
    XCTAssertNotNil(error, @"error was nil after serializing item.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertTrue(error.code == MSInvalidItemIdWithRequest,
                 @"error code should have been MSInvalidItemIdWithRequest.");
    
    NSString *description = error.localizedDescription;
    XCTAssertTrue([description isEqualToString:@"The item provided did not have a valid id."],
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
    
    XCTAssertNil(data, @"data was not nil after serializing item.");
    XCTAssertNotNil(error, @"error was nil after serializing item.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertTrue(error.code == MSExistingItemIdWithRequest,
                 @"error code should have been MSExistingItemIdWithRequest.");
    
    NSString *description = error.localizedDescription;
    XCTAssertTrue([description isEqualToString:@"The item provided must not have an id."],
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
    
    XCTAssertNotNil(data, @"data was nil after serializing item.");
    XCTAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"{\"name\":\"bob\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    XCTAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
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
    
    XCTAssertNotNil(data, @"data was nil after serializing item.");
    XCTAssertNil(error, @"error was not nil after serializing item.");
    
    NSString *expected = @"[{\"id\":5,\"name\":\"bob\"}]";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    XCTAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}


#pragma mark * itemIdFromItem: Tests


-(void)testItemIdFromItemReturnsId
{
    NSDictionary *item = @{ @"id" : @5, @"name" : @"bob" };
    NSError *error = nil;
    NSNumber *itemId = [serializer itemIdFromItem:item orError:&error];
    long long expected = 5;
    
    XCTAssertNil(error, @"error should have been nil.");
    XCTAssertEqual(expected, [itemId longLongValue], @"itemId was not correct.");
}

-(void)testItemIdFromItemWithStringIdReturnsId
{
    NSDictionary *item = @{ @"id" : @"my-id", @"name" : @"bob" };
    NSError *error = nil;
    NSString *itemId = [serializer itemIdFromItem:item orError:&error];

    XCTAssertNil(error, @"error should have been nil.");
    XCTAssertEqual(@"my-id", itemId, @"itemId was not correct.");
}

-(void)testItemIdFromItemThrowsForMissingId
{
    NSDictionary *item = @{ @"name" : @"bob" };

    NSError *error = nil;
    NSNumber *itemId = [serializer itemIdFromItem:item orError:&error];
    
    XCTAssertNotNil(error, @"error should not have been nil.");
    XCTAssertNil(itemId, @"itemId should have been nil.");
}

-(void)testItemIdFromItemThrowsForNonNumericNonStringMissingId
{
    NSDictionary *item = @{ @"id" : [NSNull null], @"name" : @"bob" };
    
    NSError *error = nil;
    id itemId = [serializer itemIdFromItem:item orError:&error];
    
    XCTAssertNotNil(error, @"error should not have been nil.");
    XCTAssertNil(itemId, @"itemId should have been nil.");
}

-(void)testItemIdFromItemReturnsErrorIfIdIsNotLowercased
{
    NSDictionary *item = @{ @"Id" : @"5", @"name" : @"bob" };
    
    NSError *error = nil;
    NSNumber *itemId = [serializer itemIdFromItem:item orError:&error];
    
    XCTAssertNotNil(error, @"error should not have been nil.");
    XCTAssertNil(itemId, @"itemId should have been nil.");
}


#pragma mark * itemFromData: Tests


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
    
    XCTAssertNotNil(newItem, @"item was nil after deserializing item.");
    XCTAssertNil(error, @"error was not nil after deserializing item.");
    XCTAssertTrue([newItem[@"name"] isEqualToString:@"bob"],
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
    
    XCTAssertNotNil(newItem, @"item was nil after deserializing item.");
    XCTAssertNil(error, @"error was not nil after deserializing item.");
    XCTAssertTrue([newItem[@"name"] isEqualToString:@"bob"],
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
    
    XCTAssertNotNil(newItem, @"item was nil after deserializing item.");
    XCTAssertNil(error, @"error was not nil after deserializing item.");
    
    NSDate *date = newItem[@"date"];
    XCTAssertNotNil(date, @"date was nil after deserializing item.");
    
    NSCalendar *gregorian = [[NSCalendar alloc]
                             initWithCalendarIdentifier:NSGregorianCalendar];
    [gregorian setTimeZone:[NSTimeZone timeZoneForSecondsFromGMT:0]];
    NSDateComponents *dateParts =
        [gregorian components:(NSYearCalendarUnit |
                               NSHourCalendarUnit |
                               NSSecondCalendarUnit)
                     fromDate:date];

    XCTAssertEqual(dateParts.year, 1999);
    XCTAssertEqual(dateParts.hour, 15);
    XCTAssertEqual(dateParts.second, 29);
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
    
    XCTAssertNotNil(newItem, @"item was nil after deserializing item.");
    XCTAssertNil(error, @"error was not nil after deserializing item.");
    
    NSDate *date = newItem[@"date"];
    XCTAssertNotNil(date, @"date was nil after deserializing item.");
    
    NSCalendar *gregorian = [[NSCalendar alloc]
                             initWithCalendarIdentifier:NSGregorianCalendar];
    [gregorian setTimeZone:[NSTimeZone timeZoneForSecondsFromGMT:0]];
    NSDateComponents *dateParts =
    [gregorian components:(NSYearCalendarUnit |
                           NSHourCalendarUnit |
                           NSSecondCalendarUnit)
                 fromDate:date];
    
    XCTAssertEqual(dateParts.year, 1999);
    XCTAssertEqual(dateParts.hour, 15);
    XCTAssertEqual(dateParts.second, 29);
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
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    XCTAssertNil(newItem, @"error was not nil after deserializing item.");
    XCTAssertTrue([[error domain] isEqualToString:@"NSCocoaErrorDomain"],
                 @"error domain was: %@", [error domain]);
    XCTAssertEqual(error.code, 3840);
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
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    XCTAssertNil(newItem, @"newItem was not nil after deserializing item.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertEqual(error.code, MSExpectedItemWithResponse);
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
    
    XCTAssertNil(error, @"error was not nil after deserializing item.");
    XCTAssertNotNil(newItem, @"newItem was nil after deserializing item.");
    XCTAssertTrue([newItem[0] isEqual:@5],
                 @"The first element should have been a 5.");
    XCTAssertTrue([newItem[1] isEqualToString:@"This is not an object!"],
                 @"The second element should have been 'This is not an object!'.");
}


#pragma mark * totalCountAndItems: Tests


-(void)testTotalCountAndItemsReturnsItems
{
    NSString* stringData = @"[{\"id\":5,\"name\":\"bob\"},{\"id\":6,\"name\":\"mary\"}]";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = nil;
    NSArray *items = nil;
    NSInteger totalCount = [serializer totalCountAndItems:&items
                                                 fromData:data
                                                  orError:&error];
    
    XCTAssertNotNil(items, @"items was nil after deserializing item.");
    XCTAssertNil(error, @"error was not nil after deserializing item.");
    XCTAssertTrue(totalCount == -1,
                 @"The totalCount should have been -1 since it was not given.");
    XCTAssertTrue(items.count == 2,
                 @"The items array should have had 2 items in it.");
    
    XCTAssertTrue([items[0][@"name"]
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
    
    XCTAssertNotNil(items, @"items was nil after deserializing item.");
    XCTAssertNil(error, @"error was not nil after deserializing item.");
    XCTAssertTrue(totalCount == -1,
                 @"The totalCount should have been -1 since it was not given.");
    XCTAssertTrue(items.count == 2,
                 @"The items array should have had 2 items in it.");
    
    XCTAssertTrue([items[0][@"name"]
                  isEqualToString:@"bob"],
                 @"The name key should have been updated to 'bob'.");
    
    
    NSArray *dates = items[0][@"dates"];
    XCTAssertNotNil(dates, @"dates was nil after deserializing item.");
    
    NSDate *date = dates[0];
    XCTAssertNotNil(date, @"date was nil after deserializing item.");
    
    NSCalendar *gregorian = [[NSCalendar alloc]
                             initWithCalendarIdentifier:NSGregorianCalendar];
    [gregorian setTimeZone:[NSTimeZone timeZoneForSecondsFromGMT:0]];
    NSDateComponents *dateParts =
    [gregorian components:(NSYearCalendarUnit |
                           NSHourCalendarUnit |
                           NSSecondCalendarUnit)
                 fromDate:date];
    
    
    XCTAssertEqual(dateParts.year, 1999);
    XCTAssertEqual(dateParts.hour, 15);
    XCTAssertEqual(dateParts.second, 29);
    
    NSDate *date2 = items[1][@"date"];
    XCTAssertNotNil(date2, @"date was nil after deserializing item.");

    NSDateComponents *dateParts2 =
    [gregorian components:(NSYearCalendarUnit |
                           NSHourCalendarUnit |
                           NSSecondCalendarUnit)
                 fromDate:date2];

    XCTAssertEqual(dateParts2.year, 2012);
    XCTAssertEqual(dateParts2.hour, 8);
    XCTAssertEqual(dateParts2.second, 0);
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
    
    XCTAssertNotNil(items, @"items was nil after deserializing item.");
    XCTAssertNil(error, @"error was not nil after deserializing item.");
    XCTAssertTrue(totalCount == 50,
                 @"The totalCount should have been 50 since it was given.");
    XCTAssertTrue(items.count == 2,
                 @"The items array should have had 2 items in it.");
    
    XCTAssertTrue([items[0][@"name"]
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
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    XCTAssertNil(items, @"error was not nil after deserializing item.");
    XCTAssertTrue(totalCount == -1,
                 @"The totalCount should have been -1 since there was an error.");
    
    XCTAssertEqualObjects(error.domain, @"NSCocoaErrorDomain");
    XCTAssertEqual(error.code, 3840);
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
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    XCTAssertNil(items, @"error was not nil after deserializing item.");
    XCTAssertEqual(totalCount, -1, @"The totalCount should have been -1 since there was an error.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertEqual(error.code, MSExpectedTotalCountWithResponse);
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
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    XCTAssertNil(items, @"error was not nil after deserializing item.");
    
    XCTAssertEqual(totalCount, -1, @"The totalCount should have been -1 since there was an error.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertEqual(error.code, MSExpectedItemsWithResponse);
}


#pragma mark * errorFromData: Tests


-(void)testErrorFromDataReturnsError
{
    NSString* stringData = @"\"This is an Error Message!\"";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"text/json"];
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertEqual(error.code, MSErrorMessageErrorCode);
    XCTAssertEqualObjects(error.localizedDescription, @"This is an Error Message!");
}

-(void)testErrorFromDataReturnsErrorFromObjectWithErrorKey
{
    NSString* stringData = @"{\"error\":\"This is another Error Message!\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"text/JSON"];
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertEqual(error.code, MSErrorMessageErrorCode);
    XCTAssertEqualObjects(error.localizedDescription, @"This is another Error Message!");
}

-(void)testErrorFromDataReturnsErrorFromObjectWithDescriptionKey
{    
    NSString* stringData = @"{\"description\":\"This is another Error Message!\"}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"application/json"];
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertEqual(error.code, MSErrorMessageErrorCode);
    XCTAssertTrue([[error localizedDescription] isEqualToString:
                  @"This is another Error Message!"],
                 @"error description was: %@", [error localizedDescription]);
}

-(void)testErrorFromDataReturnsErrorIfReadFails
{
    NSString* stringData = @"invalid JSON!";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"application/JSON"];
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    XCTAssertEqualObjects(error.domain, @"NSCocoaErrorDomain");
    XCTAssertEqual(error.code, 3840);
}

-(void)testErrorFromDataReturnsJsonEvenIfNotExpectedJsonForm
{
    NSString* stringData = @"{}";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"text/json"];
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertEqual(error.code, MSErrorMessageErrorCode);
    XCTAssertEqualObjects(error.localizedDescription, @"{}");
}

-(void)testErrorFromDataReturnsNonJson
{
    NSString* stringData = @"<Hey>This sure is some poor xml</Hey>";
    NSData* data = [stringData dataUsingEncoding:NSUTF8StringEncoding];
    
    NSError *error = [serializer errorFromData:data MIMEType:@"application/xml"];
    
    XCTAssertNotNil(error, @"error was nil after deserializing item.");
    XCTAssertEqualObjects(error.domain, MSErrorDomain);
    XCTAssertEqual(error.code, MSErrorMessageErrorCode);
    XCTAssertEqualObjects(error.localizedDescription, @"<Hey>This sure is some poor xml</Hey>");
}

-(void)testSystemPropertiesNotRemovedWithIntId
{
    NSError *error;
    
    NSDictionary *item = @{@"id": @7, @"__Prop1": @6};
    NSData *data = [serializer dataFromItem:item idAllowed:YES ensureDictionary:NO removeSystemProperties:YES orError:&error];
    
    XCTAssertNil(error, @"An error occurred: %@", error.localizedDescription);
    
    NSString *expected = @"{\"id\":7,\"__Prop1\":6}";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    XCTAssertEqualObjects(actual, expected);
}

-(void)testSystemPropertiesNotRemovedWithArray
{
    NSError *error;
    
    NSArray *item = @[@{@"id": @7, @"__Prop1": @6}];
    NSData *data = [serializer dataFromItem:item idAllowed:YES ensureDictionary:NO removeSystemProperties:YES orError:&error];
    
    XCTAssertNil(error, @"An error occurred: %@", error.localizedDescription);
    
    NSString *expected = @"[{\"id\":7,\"__Prop1\":6}]";
    NSString *actual = [[NSString alloc] initWithData:data
                                             encoding:NSUTF8StringEncoding];
    
    XCTAssertEqualObjects(actual, expected);
}

-(void)testSystemPropertiesRemovedWithStringId
{
    NSError *error;
    NSDictionary *item = @{@"id": @"one", @"__Prop1": @6, @"__prop4": @"help"};
    NSData *data = [serializer dataFromItem:item idAllowed:YES ensureDictionary:NO removeSystemProperties:YES orError:&error];
    
    XCTAssertNil(error, @"An error occurred: %@", error.localizedDescription);
    
    NSString *expected = @"{\"id\":\"one\"}";
    NSString *actual = [[NSString alloc] initWithData:data
                                         encoding:NSUTF8StringEncoding];
    
    XCTAssertTrue([expected isEqualToString:actual], @"JSON was: %@", actual);
}

@end
