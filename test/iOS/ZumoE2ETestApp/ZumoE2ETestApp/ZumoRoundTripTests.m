//
//  ZumoRoundTripTests.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/9/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoRoundTripTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>

@implementation ZumoRoundTripTests

typedef enum { RTTString, RTTDouble, RTTBool, RTTInt, RTTLong, RTTDate } RoundTripTestColumnType;

+ (NSArray *)createTests {
    ZumoTest *firstTest = [ZumoTest createTestWithName:@"Setup dynamic schema" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client getTable:@"TodoItem"];
        NSDictionary *item = @{@"string1":@"test", @"date1": [ZumoTestGlobals createDateWithYear:2011 month:11 day:11], @"bool1": [NSNumber numberWithBool:NO], @"number": [NSNumber numberWithInt:-1], @"longnum":[NSNumber numberWithLong:0], @"intnum":[NSNumber numberWithInt:0], @"setindex":@"setindex"};
        [table insert:item completion:^(NSDictionary *inserted, NSError *err) {
            if (err) {
                [test addLog:@"Error inserting data to create schema"];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                [test addLog:@"Inserted item to create schema"];
                [test setTestStatus:TSPassed];
                completion(YES);
            }
        }];
    }];
    
    NSMutableArray *result = [[NSMutableArray alloc] init];
    [result addObject:firstTest];
    
    [result addObject:[self createRoundTripForType:RTTString withValue:@"" andName:@"Round trip empty string"]];
    NSString *simpleString = [NSString stringWithFormat:@"%c%c%c%c%c",
                              ' ' + (rand() % 95),
                              ' ' + (rand() % 95),
                              ' ' + (rand() % 95),
                              ' ' + (rand() % 95),
                              ' ' + (rand() % 95)];
    [[ZumoTestGlobals propertyBag] setValue:simpleString forKey:ZumoKeyStringValue];
    [result addObject:[self createRoundTripForType:RTTString withValue:simpleString andName:@"Round trip simple string"]];
    [result addObject:[self createRoundTripForType:RTTString withValue:[NSNull null] andName:@"Round trip nil string"]];
    
    [result addObject:[self createRoundTripForType:RTTString withValue:[@"" stringByPaddingToLength:1000 withString:@"*" startingAtIndex:0] andName:@"Round trip large (1000) string"]];
    [result addObject:[self createRoundTripForType:RTTString withValue:[@"" stringByPaddingToLength:10000 withString:@"*" startingAtIndex:0] andName:@"Round trip large (10000) string"]];
    
    // Date scenarios
    [result addObject:[self createRoundTripForType:RTTDate withValue:[NSDate date] andName:@"Round trip current date"]];
    [result addObject:[self createRoundTripForType:RTTDate withValue:[ZumoTestGlobals createDateWithYear:2012 month:12 day:12] andName:@"Round trip specific date"]];
    [result addObject:[self createRoundTripForType:RTTDate withValue:[NSNull null] andName:@"Round trip null date"]];
    
    // Bool scenarios
    [result addObject:[self createRoundTripForType:RTTBool withValue:[NSNumber numberWithBool:YES] andName:@"Round trip (BOOL)YES"]];
    [result addObject:[self createRoundTripForType:RTTBool withValue:[NSNumber numberWithBool:NO] andName:@"Round trip (BOOL)NO"]];
    
    // Number scenarios
    [result addObject:[self createRoundTripForType:RTTInt withValue:[NSNumber numberWithInt:rand()] andName:@"Round trip positive number"]];
    [result addObject:[self createRoundTripForType:RTTInt withValue:[NSNumber numberWithInt:-rand()] andName:@"Round trip negative number"]];
    [result addObject:[self createRoundTripForType:RTTInt withValue:[NSNumber numberWithInt:0] andName:@"Round trip zero"]];
    [result addObject:[self createRoundTripForType:RTTDouble withValue:[NSNumber numberWithDouble:MAXFLOAT] andName:@"Round trip MAXFLOAT"]];
    [result addObject:[self createRoundTripForType:RTTLong withValue:[NSNumber numberWithLong:123456789012345L] andName:@"Round trip long number"]];
    [result addObject:[self createRoundTripForType:RTTLong withValue:[NSNumber numberWithLong:-123456789012345L] andName:@"Round trip negative long"]];
    
    // Negative scenarios
    ZumoTest *test = [ZumoTest createTestWithName:@"New column with null value" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client getTable:@"TodoItem"];
        [table insert:@{@"ColumnWhichDoesNotExist":[NSNull null]} completion:^(NSDictionary *item, NSError *err) {
            BOOL passed;
            if (!err) {
                [test addLog:[NSString stringWithFormat:@"Error, adding new column with null element should fail, but insert worked: %@", item]];
                passed = NO;
            } else {
                if (err.code == MSErrorMessageErrorCode) {
                    [test addLog:@"Test passed, got correct error"];
                    passed = YES;
                } else {
                    [test addLog:[NSString stringWithFormat:@"Expected error code %d, got %d", MSErrorMessageErrorCode, err.code]];
                    passed = NO;
                }
            }
            
            [test setTestStatus:(passed ? TSPassed : TSFailed)];
            completion(passed);
        }];
    }];
    [result addObject:test];
    
    return result;
}

+ (ZumoTest *)createRoundTripForType:(RoundTripTestColumnType)type withValue:(id)value andName:(NSString *)testName {
    ZumoTest *result = [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client getTable:@"TodoItem"];
        NSMutableDictionary *item = [[NSMutableDictionary alloc] init];
        if (type == RTTString) {
            [item setObject:value forKey:@"string1"];
        } else if (type == RTTDouble) {
            [item setObject:value forKey:@"number"];
        } else if (type == RTTBool) {
            [item setObject:value forKey:@"bool1"];
        } else if (type == RTTInt) {
            [item setObject:value forKey:@"intnum"];
        } else if (type == RTTLong) {
            [item setObject:value forKey:@"longnum"];
        } else if (type == RTTDate) {
            [item setObject:value forKey:@"date1"];
        }
        
        [table insert:item completion:^(NSDictionary *inserted, NSError *err) {
            if (err) {
                [test addLog:[NSString stringWithFormat:@"Error inserting: %@", err]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                NSNumber *itemId = [inserted objectForKey:@"id"];
                [table readWithId:itemId completion:^(NSDictionary *retrieved, NSError *err2) {
                    if (err2) {
                        [test addLog:[NSString stringWithFormat:@"Error retrieving: %@", err2]];
                        [test setTestStatus:TSFailed];
                        completion(NO);
                    } else {
                        BOOL failed = NO;
                        if (type == RTTString) {
                            NSString *rtString = [retrieved objectForKey:@"string1"];
                            if ([value isKindOfClass:[NSNull class]] && [rtString isKindOfClass:[NSNull class]]) {
                                // all are null, ok
                            } else {
                                if (![rtString isEqualToString:value]) {
                                    [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtString]];
                                    failed = YES;
                                }
                            }
                        } else if (type == RTTBool) {
                            NSNumber *rtBool = [retrieved objectForKey:@"bool1"];
                            if ([rtBool boolValue] != [((NSNumber *)value) boolValue]) {
                                failed = YES;
                                [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtBool]];
                            }
                        } else if (type == RTTDate) {
                            NSDate *rtDate = [retrieved objectForKey:@"date1"];
                            if (![ZumoTestGlobals compareDate:rtDate withDate:((NSDate *)value)]) {
                                failed = YES;
                                [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtDate]];
                            }
                        } else if (type == RTTDouble) {
                            NSNumber *rtNumber = [retrieved objectForKey:@"number"];
                            double dbl1 = [((NSNumber *)value) doubleValue];
                            double dbl2 = [rtNumber doubleValue];
                            double delta = fabs(dbl1 - dbl2);
                            double error = delta / dbl1;
                            if (error > 0.000000001) {
                                failed = YES;
                                [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtNumber]];
                            }
                        } else if (type == RTTInt) {
                            NSNumber *rtNumber = [retrieved objectForKey:@"intnum"];
                            if (![rtNumber isEqualToNumber:value]) {
                                failed = YES;
                                [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtNumber]];
                            }
                        } else if (type == RTTLong) {
                            NSNumber *rtNumber = [retrieved objectForKey:@"longnum"];
                            if (![rtNumber isEqualToNumber:value]) {
                                failed = YES;
                                [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtNumber]];
                            }
                        } else {
                            failed = YES;
                            [test addLog:@"Test not implemented for this type"];
                        }
                        
                        if (failed) {
                            [test setTestStatus:TSFailed];
                            completion(NO);
                        } else {
                            [test addLog:@"Test passed"];
                            [test setTestStatus:TSPassed];
                            completion(YES);
                        }
                    }
                }];
            }
        }];
    }];
    
    return result;
}

+ (NSString *)helpText {
    NSArray *lines = [NSArray arrayWithObjects:
                      @"1. Create an application on Windows azure portal.",
                      @"2. Create TodoItem table in portal.",
                      @"3. Add Valid Application URL and Application Key.",
                      @"4. Click on the '1.1 RoundTripDataType' button.",
                      @"5. Make sure all the tests pass.",
                      nil];
    return [lines componentsJoinedByString:@"\n"];
}

@end
