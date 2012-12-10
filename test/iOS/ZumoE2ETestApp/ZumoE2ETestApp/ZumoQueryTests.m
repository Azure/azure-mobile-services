//
//  ZumoQueryTests.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/9/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoQueryTests.h"
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>
#import "ZumoTestGlobals.h"
#import "ZumoTest.h"

@implementation ZumoQueryTests

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    
    [result addObject:[self createQueryTestWithName:@"Get null from string1" andPredicate:[NSPredicate predicateWithFormat:@"string1=nil"] andTop:nil andSkip:nil]];
    [result addObject:[self createQueryTestWithName:@"Get true from bool1" andPredicate:[NSPredicate predicateWithFormat:@"bool1=YES"] andTop:nil andSkip:nil]];
    [result addObject:[self createQueryTestWithName:@"Get numbers greater than 0" andPredicate:[NSPredicate predicateWithFormat:@"number > 0"] andTop:nil andSkip:nil]];
    [result addObject:[self createQueryTestWithName:@"Get all Date(2012,12,12) from date1" andPredicate:[NSPredicate predicateWithFormat:@"(date1 >= %@) AND (date1 < %@)", [ZumoTestGlobals createDateWithYear:2012 month:12 day:12], [ZumoTestGlobals createDateWithYear:2012 month:12 day:13]] andTop:nil andSkip:nil]];
    
    [result addObject:[self createQueryTestWithName:@"Get all using large 'fetchLimit' ($top)" andPredicate:nil andTop:[NSNumber numberWithInt:100] andSkip:nil]];
    NSString *stringAdded = [[ZumoTestGlobals propertyBag] objectForKey:ZumoKeyStringValue];
    [result addObject:[self createQueryTestWithName:@"Get top 10 where string1 = large string" andPredicate:[NSPredicate predicateWithFormat:@"string1='%@'", stringAdded] andTop:[NSNumber numberWithInt:10] andSkip:nil]];
    [result addObject:[self createQueryTestWithName:@"Get none using large 'fetchOffset' ($skip)" andPredicate:nil andTop:nil andSkip:[NSNumber numberWithInt:1000000]]];
    
    [result addObject:[self createOrderByTestWithName:@"Orderby, date ascending" andOrderField:@"date1" ascending:YES]];
    [result addObject:[self createOrderByTestWithName:@"Orderby, date descending" andOrderField:@"date1" ascending:NO]];
    
    [result addObject:[self createOrderByTestWithName:@"Orderby, string ascending" andOrderField:@"string1" ascending:YES]];
    [result addObject:[self createOrderByTestWithName:@"Orderby, string descending" andOrderField:@"string1" ascending:NO]];
    
    for (int i = -1; i <= 0; i++) {
        ZumoTest *negativeLookupTest = [[ZumoTest alloc] init];
        [negativeLookupTest setTestName:[NSString stringWithFormat:@"(Neg) MSTable readWithId:%d", i]];
        __weak ZumoTest *weakNegTest = negativeLookupTest;
        [negativeLookupTest setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            MSTable *table = [client getTable:@"TodoItem"];
            [table readWithId:[NSNumber numberWithInt:i] completion:^(NSDictionary *item, NSError *err) {
                BOOL passed = NO;
                if (err) {
                    if (err.code != MSErrorMessageErrorCode) {
                        [weakNegTest addLog:[NSString stringWithFormat:@"Invalid error code: %d", err.code]];
                    } else {
                        NSHTTPURLResponse *httpResponse = [[err userInfo] objectForKey:MSErrorResponseKey];
                        int statusCode = [httpResponse statusCode];
                        if (statusCode == 404) {
                            [weakNegTest addLog:@"Got expected error"];
                            passed = YES;
                        } else {
                            [weakNegTest addLog:[NSString stringWithFormat:@"Invalid response status code: %d", statusCode]];
                            passed = NO;
                        }
                    }
                } else {
                    [weakNegTest addLog:[NSString stringWithFormat:@"Expected error for lookup with id:%d, but got data back: %@", i, item]];
                }
                
                [weakNegTest setTestStatus:(passed ? TSPassed : TSFailed)];
                completion(passed);
            }];
        }];
        
        [result addObject:negativeLookupTest];
    }
    
    [result addObject:[self createNegativeTestWithName:@"(Neg) Very large fetchOffset" andQuerySettings:^(MSQuery *query) {
        query.fetchLimit = 1000000;
    } andQueryValidation:^(ZumoTest *test, NSError *err) {
        if (err.code != MSErrorMessageErrorCode) {
            [test addLog:[NSString stringWithFormat:@"Invalid error code: %d", err.code]];
            return NO;
        } else {
            NSHTTPURLResponse *resp = [[err userInfo] objectForKey:MSErrorResponseKey];
            if ([resp statusCode] == 400) {
                return YES;
            } else {
                [test addLog:[NSString stringWithFormat:@"Incorrect response status code: %d", [resp statusCode]]];
                return NO;
            }
        }
    }]];
    
    return result;
}

typedef void (^ActionQuery)(MSQuery *query);
typedef BOOL (^QueryValidation)(ZumoTest *test, NSError *error);

+ (ZumoTest *)createNegativeTestWithName:(NSString *)name andQuerySettings:(ActionQuery)settings andQueryValidation:(QueryValidation)queryValidation {
    ZumoTest *result = [[ZumoTest alloc] init];
    [result setTestName:name];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client getTable:@"TodoItem"];
        MSQuery *query = [table query];
        settings(query);
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            if (error) {
                if (queryValidation(weakRef, error)) {
                    [weakRef addLog:[NSString stringWithFormat:@"Got expected error: %@", error]];
                    [weakRef setTestStatus:TSPassed];
                    completion(YES);
                } else {
                    [weakRef addLog:[NSString stringWithFormat:@"Error wasn't the expected one: %@", error]];
                    [weakRef setTestStatus:TSFailed];
                    completion(NO);
                }
            } else {
                [weakRef addLog:@"Query should fail, but succeeded"];
                [weakRef setTestStatus:TSFailed];
                completion(NO);
            }
        }];
    }];
    
    return result;
}

+ (ZumoTest *)createOrderByTestWithName:(NSString *)name andOrderField:(NSString *)field ascending:(BOOL)asc {
    ZumoTest *result = [[ZumoTest alloc] init];
    [result setTestName:name];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client getTable:@"TodoItem"];
        NSPredicate *notNullPredicate = [NSPredicate predicateWithFormat:[NSString stringWithFormat:@"%@ != nil", field]];
        [table readWhere:notNullPredicate completion:^(NSArray *allItems, NSInteger totalCount, NSError *readAllError) {
            if (readAllError) {
                [weakRef addLog:[NSString stringWithFormat:@"Error retrieving all items: %@", readAllError]];
                [weakRef setTestStatus:TSFailed];
                completion(NO);
            } else {
                MSQuery *query = [table query];
                if (asc) {
                    [query orderByAscending:field];
                } else {
                    [query orderByDescending:field];
                }
                
                [query readWithCompletion:^(NSArray *queriedItems, NSInteger totalCount, NSError *orderedReadError) {
                    if (orderedReadError) {
                        [weakRef addLog:[NSString stringWithFormat:@"Error calling read with orderby: %@", orderedReadError]];
                        [weakRef setTestStatus:TSFailed];
                        completion(NO);
                    } else {
                        NSComparisonResult (^comparator)(id obj1, id obj2) = ^(id obj1, id obj2) {
                            id fieldValue1 = [obj1 objectForKey:field];
                            id fieldValue2 = [obj2 objectForKey:field];
                            if ([field isEqualToString:@"date1"]) {
                                NSDate *date1 = fieldValue1;
                                NSDate *date2 = fieldValue2;
                                return asc ? [date1 compare:date2] : [date2 compare:date1];
                            } else if ([field isEqualToString:@"string1"]) {
                                NSString *str1 = fieldValue1;
                                NSString *str2 = fieldValue2;
                                return asc ? [str1 compare:str2] : [str2 compare:str1];
                            } else {
                                NSLog(@"NOT SUPPORTED YET!");
                                return NSOrderedSame;
                            }
                        };
                        queriedItems = [queriedItems filteredArrayUsingPredicate:notNullPredicate];
                        NSArray *expectedResults = [allItems sortedArrayUsingComparator:comparator];
                        
                        BOOL allEqual = YES;
                        if ([expectedResults count] != [queriedItems count]) {
                            [weakRef addLog:[NSString stringWithFormat:@"Different number of elements: %d != %d", [expectedResults count], [queriedItems count]]];
                            allEqual = NO;
                        } else {
                            int i;
                            for (i = 0; i < [expectedResults count] - 1; i++) {
                                NSComparisonResult expectedOrder = comparator([expectedResults objectAtIndex:i], [expectedResults objectAtIndex:(i + 1)]);
                                NSComparisonResult actualOrder = comparator([queriedItems objectAtIndex:i], [queriedItems objectAtIndex:(i + 1)]);
                                if (expectedOrder != actualOrder) {
                                    [weakRef addLog:[NSString stringWithFormat:@"Ordering between items %d and %d are different: expected %d, got %d", i, i + 1, expectedOrder, actualOrder]];
                                    allEqual = NO;
                                }
                            }
                        }
                        
                        if (allEqual) {
                            [weakRef addLog:@"Test passed"];
                            [weakRef setTestStatus:TSPassed];
                            completion(YES);
                        } else {
                            [weakRef addLog:[NSString stringWithFormat:@"Expected result different than actual: %@ != %@", expectedResults, queriedItems]];
                            [weakRef setTestStatus:TSFailed];
                            completion(NO);
                        }
                    }
                }];
            }
        }];
    }];
    
    return result;
}

+ (ZumoTest *)createQueryTestWithName:(NSString *)name andPredicate:(NSPredicate *)predicate andTop:(NSNumber *)top andSkip:(NSNumber *)skip {
    ZumoTest *result = [[ZumoTest alloc] init];
    [result setTestName:name];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client getTable:@"TodoItem"];
        [table readWithCompletion:^(NSArray *allItems, NSInteger totalCount, NSError *readAllError) {
            if (readAllError) {
                [weakRef addLog:[NSString stringWithFormat:@"Error retrieving all items: %@", readAllError]];
                [weakRef setTestStatus:TSFailed];
                completion(NO);
            } else {
                if (!top && !skip) {
                    // use simple readWithPredicate
                    [table readWhere:predicate completion:^(NSArray *filteredItems, NSInteger totalCount2, NSError *readWhereError) {
                        if (readWhereError) {
                            [weakRef addLog:[NSString stringWithFormat:@"Error calling readWhere: %@", readWhereError]];
                            [weakRef setTestStatus:TSFailed];
                            completion(NO);
                        } else {
                            NSString *expected = [[allItems filteredArrayUsingPredicate:predicate] description];
                            NSString *actual = [filteredItems description];
                            if ([expected isEqualToString:actual]) {
                                [weakRef addLog:@"Test passed"];
                                [weakRef setTestStatus:TSPassed];
                                completion(YES);
                            } else {
                                [weakRef addLog:[NSString stringWithFormat:@"Expected result different than actual: %@ != %@", expected, actual]];
                                [weakRef setTestStatus:TSFailed];
                                completion(NO);
                            }
                        }
                    }];
                } else {
                    MSQuery *query = predicate ? [table queryWhere:predicate] : [table query];
                    if (top) {
                        [query setFetchLimit:[top integerValue]];
                    }
                    
                    if (skip) {
                        [query setFetchOffset:[skip integerValue]];
                    }
                    
                    [query readWithCompletion:^(NSArray *queriedItems, NSInteger totalCount3, NSError *queryReadError) {
                        if (queryReadError) {
                            [weakRef addLog:[NSString stringWithFormat:@"Error calling MSQuery readWithCompletion: %@", queryReadError]];
                            [weakRef setTestStatus:TSFailed];
                            completion(NO);
                        } else {
                            NSArray *filteredArray;
                            if (predicate) {
                                filteredArray = [NSMutableArray arrayWithArray:[allItems filteredArrayUsingPredicate:predicate]];
                            } else {
                                filteredArray = [NSMutableArray arrayWithArray:allItems];
                            }
                            NSMutableArray *expectedArray = [[NSMutableArray alloc] init];
                            int i;
                            int start = skip ? [skip intValue] : 0;
                            int count = top ? [top intValue] : [filteredArray count];
                            for (i = 0; i < count; i++) {
                                int index = i + start;
                                if (index < [filteredArray count]) {
                                    [expectedArray addObject:[filteredArray objectAtIndex:index]];
                                } else {
                                    break;
                                }
                            }
                            NSString *expected = [expectedArray description];
                            NSString *actual = [queriedItems description];
                            if ([expected isEqualToString:actual]) {
                                [weakRef addLog:@"Test passed"];
                                [weakRef setTestStatus:TSPassed];
                                completion(YES);
                            } else {
                                [weakRef addLog:[NSString stringWithFormat:@"Expected result different than actual: %@ != %@", expected, actual]];
                                [weakRef setTestStatus:TSFailed];
                                completion(NO);
                            }
                        }
                    }];
                }
            }
        }];
    }];
    
    return result;
}

+ (NSString *)helpText {
    NSArray *lines = [NSArray arrayWithObjects:
                      @"1. Create an application on Windows azure portal.",
                      @"2. Create TodoItem table in portal.",
                      @"2.1. Even if table already exists, remove and recreate"
                      @"3. Add Valid Application URL and Application Key.",
                      @"4. Run the 'Insert and Verify' tests.",
                      @"5. Run the 'Query Tests' tests.",
                      @"6. Make sure all the scenarios pass.", nil];
    return [lines componentsJoinedByString:@"\n"];
}

@end
