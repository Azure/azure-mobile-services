// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <SenTestingKit/SenTestingKit.h>
#import "MSPredicateTranslator.h"

@interface MSPredicateTranslatorTests : SenTestCase

@end

@implementation MSPredicateTranslatorTests

-(void) testSimplePredicateStrings
{
    // Create some dates with no seconds, seconds and fractional seconds
    NSDateComponents *dateParts = [[NSDateComponents alloc] init];
    dateParts.year = 2012;
    dateParts.month = 5;
    dateParts.day = 21;
    dateParts.calendar = [[NSCalendar alloc]
                           initWithCalendarIdentifier:NSGregorianCalendar];
    dateParts.calendar.timeZone = [NSTimeZone timeZoneForSecondsFromGMT:0];
    
    NSDate *date1 = dateParts.date;
    dateParts.second = 29;
    NSDate *date2 = dateParts.date;
    NSDate *date3 = [date2 dateByAddingTimeInterval:0.3];

    // For each test case: the first element is the expected OData value;
    // the second element is the predicate format string; and all
    // additional elements are the arguments to the format string
    NSArray *testCases = @[
        // BOOL test
        @[ @"(Complete eq true)",
            @"Complete == YES"],
        
        // Test cases for TRUEPREDICATE, FALSEPREDICATE
        @[ @"(1 eq 1)",
           @"TRUEPREDICATE"],
    
        @[ @"((1 eq 1) and startswith(name,'b'))",
           @"TRUEPREDICATE && name BEGINSWITH 'b'"],
    
        @[ @"(1 eq 0)",
           @"FALSEPREDICATE"],
    
        @[ @"((1 eq 0) or (Price le 5.99d))",
           @"FALSEPREDICATE OR Price <= 5.99"],
        
        // Test cases for comparison opperators and number formating.
        // We do not test numberWithInteger or numberWithLong as their translation is different
        // on 32 vs 64-bit systems resulting in slightly different oData queries
        @[ @"(Price gt 50f)",
           @"Price > %@", [NSNumber numberWithFloat:50.0]],
        
        @[ @"(Price ge 22.25d)",
           @"Price >= %@", [NSNumber numberWithDouble:22.25]],
        
        @[ @"(Count lt -25)",
           @"Count < %@", [NSNumber numberWithInt:-25]],

        @[ @"(Count le 500)",
            @"Count =< %@", [NSNumber numberWithInt:500]],
        
        @[ @"(Price eq 1.99m)",
           @"Price == %@", [NSDecimalNumber decimalNumberWithString:@"1.99"]],
        
        @[ @"(Count ne 200l)",
           @"Count != %@", [NSNumber numberWithLongLong:200]],
    
        // Test cases for boolean operators
        @[ @"((Count gt 0) and (Price le 5.99d))",
           @"Count > 0 && Price <= 5.99"],
        
        @[ @"((Count eq 0) or (Price le 5.99d))",
           @"Count = 0 OR Price <= 5.99"],
        
        @[ @"((Count eq 0) and not((Price gt 599.99d)))",
           @"Count = 0 && !(Price > 599.99)"],
    
        // Test cases for math operators and functions
        @[ @"((Count add 200) gt 350)",
           @"Count + 200 > 350"],
        
        @[ @"((Price sub 1.99d) le Cost)",
           @"Price - 1.99 <= Cost"],
        
        @[ @"((Weight mul -1.5f) lt 225f)",
           @"Weight * %@ < %@",
            [NSNumber numberWithFloat:-1.5],
            [NSNumber numberWithFloat:225]],
        
        @[ @"((Weight div 3f) gt 225d)",
           @"Weight / %@ > %@",
            [NSNumber numberWithFloat:3],
            [NSNumber numberWithDouble:225]],
        
        @[ @"((Id mod 2) eq 1)",
           @"modulus:by:(Id,2) == 1"],
        
        @[ @"(floor(Price) eq 3)",
           @"floor:(Price) == 3"],
        
        @[ @"(ceiling(Price) eq 3)",
           @"ceiling:(Price) == 3"],
        
        // Test cases for strings
        @[ @"(name eq 'bob')",
           @"name = 'bob'"],
    
        @[ @"(text eq 'This text has a '' in it!')",
            @"text = %@", @"This text has a ' in it!"],
    
        @[ @"(tolower(name) eq tolower('bob'))",
           @"name =[c] 'bob'"],
    
        @[ @"(tolower(name) eq 'bob')",
           @"lowercase:(name) = 'bob'"],
        
        @[ @"(toupper(name) eq 'BOB')",
           @"uppercase:(name) = 'BOB'"],
        
        @[ @"startswith(name,'b')",
           @"name BEGINSWITH 'b'"],
        
        @[ @"endswith(name,'b')",
           @"name ENDSWITH 'b'"],
        
        @[ @"substringof('b',name)",
           @"name CONTAINS 'b'"],
    
        // Test cases for date times
        @[ @"(Created eq datetime'2012-05-21T00:00:00.000Z')",
           @"Created == %@", date1],
    
        @[ @"(Created eq datetime'2012-05-21T00:00:29.000Z')",
           @"Created == %@", date2],
    
        @[ @"(Created eq datetime'2012-05-21T00:00:29.300Z')",
           @"Created == %@", date3],
    
        // Test cases for IN
        @[ @"((Zipcode eq 98008) or (Zipcode eq 98006) or (Zipcode eq 98007))",
             @"Zipcode IN { 98008, 98006, 98007}"],
    
        @[ @"((Zipcode eq (98007 add 1)) or (Zipcode eq (98007 sub 1)) or (Zipcode eq 98007))",
           @"Zipcode IN { %@ + 1, %@ - 1, %@ }", @98007, @98007, @98007],
    ];
    
    for (NSArray *testCase in testCases) {
        
        NSString *expected = [testCase objectAtIndex:0];
        
        NSLog(@"%@ test case start", expected);
        
        NSString *format = [testCase objectAtIndex:1];
        
        NSRange range;
        range.location = 2;
        range.length = testCase.count - 2;
        NSArray *arguments = [testCase subarrayWithRange:range];
        
        NSPredicate *predicate = [NSPredicate predicateWithFormat:format
                                                    argumentArray:arguments];
        
        NSError *error = nil;
        NSString *actual = [MSPredicateTranslator queryFilterFromPredicate:predicate
                                                                   orError:&error];
        
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([actual isEqualToString:expected],
                     @"Query filter actual: '%@' and expected: '%@'",
                     actual,
                     expected);
        
        NSLog(@"%@ test case end", expected);
    }
}

-(void) testPredicatesWithSubstitutionVariables
{
    // Create some dates with no seconds, seconds and fractional seconds
    NSDateComponents *dateParts = [[NSDateComponents alloc] init];
    dateParts.year = 2012;
    dateParts.month = 5;
    dateParts.day = 21;
    dateParts.calendar = [[NSCalendar alloc]
                          initWithCalendarIdentifier:NSGregorianCalendar];
    dateParts.calendar.timeZone = [NSTimeZone timeZoneForSecondsFromGMT:0];
    
    NSDate *date1 = dateParts.date;
    dateParts.second = 29;
    NSDate *date2 = dateParts.date;
    NSDate *date3 = [date2 dateByAddingTimeInterval:0.3];
    
    // For each test case: the first element is the expected OData value;
    // the second element is the predicate format string; and all
    // additional elements are the substitution variables
    NSArray *testCases = @[
        // Test cases for comparison opperators and number formating.
        @[ @"(Price gt 50f)",
           @"Price > $price", @{ @"price":[NSNumber numberWithFloat:50.0]}],
        
        @[ @"(Price ge 22.25d)",
           @"Price >= $price", @{ @"price":[NSNumber numberWithDouble:22.25]}],
        
        @[ @"(Count lt -25)",
           @"Count < $count", @{ @"count":[NSNumber numberWithInt:-25]}],

        @[ @"(Count le 500)",
           @"Count =< $count", @{ @"count":[NSNumber numberWithInt:500]}],
        
        @[ @"(Price eq 1.99m)",
           @"Price == $price", @{ @"price":[NSDecimalNumber decimalNumberWithString:@"1.99"]}],
        
        @[ @"(Count ne 200l)",
           @"Count != $count", @{ @"count" :[NSNumber numberWithLongLong:200]}],
    
        @[ @"(Complete eq true)",
           @"Complete == $complete", @{ @"complete": @YES }],
        
        // Test cases for strings
        @[ @"(tolower(name) eq 'bob')",
           @"lowercase:(name) = $name", @{ @"name":@"bob"}],
        
        @[ @"(toupper(name) eq 'BOB')",
           @"uppercase:(name) = $name", @{ @"name":@"BOB"}],
        
        @[ @"startswith(name,'b')",
           @"name BEGINSWITH $name", @{ @"name":@"b"}],
        
        // Test cases for date times
        @[ @"(Created eq datetime'2012-05-21T00:00:00.000Z')",
           @"Created == $date", @{@"date":date1}],
        
        @[ @"(Created eq datetime'2012-05-21T00:00:29.000Z')",
           @"Created == $date", @{@"date":date2}],
        
        @[ @"(Created eq datetime'2012-05-21T00:00:29.300Z')",
           @"Created == $date", @{@"date":date3}],
    
        // Test cases for IN
        @[ @"((Zipcode eq 98008) or (Zipcode eq 98006) or (Zipcode eq 98007))",
        @"Zipcode IN $zipcodes", @{ @"zipcodes":@[ @98008, @98006, @98007 ] }],
    
        // Test cases for BETWEEN
        @[ @"((Zipcode ge 98006) and (Zipcode le 98008))",
        @"Zipcode BETWEEN $zipcodes", @{ @"zipcodes":@[ @98006, @98008] }],
    ];
    
    for (NSArray *testCase in testCases) {
        
        NSString *expected = [testCase objectAtIndex:0];
        
        NSLog(@"%@ test case start", expected);
        
        NSString *format = [testCase objectAtIndex:1];
        
        NSDictionary *variables = [testCase objectAtIndex:2];
        
        NSPredicate *predicate = [NSPredicate predicateWithFormat:format];
        NSPredicate *predicatewithVariables = [predicate predicateWithSubstitutionVariables:variables];
        
        NSError *error = nil;
        NSString *actual = [MSPredicateTranslator queryFilterFromPredicate:predicatewithVariables
                                                                   orError:&error];
        
        STAssertNil(error, @"error should have been nil.");
        STAssertTrue([actual isEqualToString:expected],
                     @"Query filter actual: '%@' and expected: '%@'",
                     actual,
                     expected);
        
        NSLog(@"%@ test case end", expected);
    }
}

@end

