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
#import "MSPredicateTranslator.h"

@interface MSPredicateTranslatorTests : SenTestCase

@end

@implementation MSPredicateTranslatorTests

-(void) testSimplePredicateStrings
{
    NSLog(@"%@ start", self.name);
    
    // Create some dates with no seconds, seconds and fractional seconds
    NSDateComponents *dateParts = [[NSDateComponents alloc] init];
    dateParts.year = 2012;
    dateParts.month = 5;
    dateParts.day = 21;
    dateParts.calendar = [[NSCalendar alloc]
                           initWithCalendarIdentifier:NSGregorianCalendar];
    
    NSDate *date1 = dateParts.date;
    dateParts.second = 29;
    NSDate *date2 = dateParts.date;
    NSDate *date3 = [date2 dateByAddingTimeInterval:0.3];

    // For each test case: the first element is the expected OData value;
    // the second element is the predicate format string; and all
    // additional elements are the arguments to the format string
    NSArray *testCases = @[
    
        // Test cases for comparison opperators and number formating.
        // Note: NSNumber doesn't preserve c-types, so longs become ints
        @[ @"(Price gt 50f)",
           @"Price > %@", [NSNumber numberWithFloat:50.0]],
        
        @[ @"(Price ge 22.25d)",
           @"Price >= %@", [NSNumber numberWithDouble:22.25]],
        
        @[ @"(Count lt -25)",
           @"Count < %@", [NSNumber numberWithInt:-25]],
        
        @[ @"(Count le 500)",
           @"Count =< %@", [NSNumber numberWithInteger:500]],
        
        @[ @"(Price eq 1.99m)",
           @"Price == %@", [NSDecimalNumber decimalNumberWithString:@"1.99"]],
        
        @[ @"(Count ne 200)",
           @"Count != %@", [NSNumber numberWithLong:200]],
    
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
        @[ @"(Created eq datetime'2012-05-21T00:00')",
           @"Created == %@", date1],
    
        @[ @"(Created eq datetime'2012-05-21T00:00:29')",
           @"Created == %@", date2],
    
        @[ @"(Created eq datetime'2012-05-21T00:00:29.3000000')",
           @"Created == %@", date3],
    
        // Test cases for evaluated expressions
        //@[ @"(Count gt -2)",
        //   @"Count > (5+2-9)"],
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
    
    NSLog(@"%@ end", self.name);
}

@end

