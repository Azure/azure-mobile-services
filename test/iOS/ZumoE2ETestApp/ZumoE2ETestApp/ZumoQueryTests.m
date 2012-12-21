//
//  ZumoQueryTests.m
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoQueryTests.h"
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>
#import "ZumoTestGlobals.h"
#import "ZumoTest.h"
#import "ZumoQueryTestData.h"

@interface OrderByClause : NSObject

@property (nonatomic, copy) NSString *fieldName;
@property (nonatomic) BOOL isAscending;

+(id)ascending:(NSString *)fieldName;
+(id)descending:(NSString *)fieldName;

@end

@implementation OrderByClause

@synthesize fieldName, isAscending;

+(id)ascending:(NSString *)field {
    OrderByClause *result = [[OrderByClause alloc] init];
    [result setFieldName:field];
    [result setIsAscending:YES];
    return result;
}

+(id)descending:(NSString *)field {
    OrderByClause *result = [[OrderByClause alloc] init];
    [result setFieldName:field];
    [result setIsAscending:NO];
    return result;
}

@end

@implementation ZumoQueryTests

static NSString *queryTestsTableName = @"iosMovies";

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    
    [result addObject:[self createPopulateTest]];

    // Numeric fields
    [result addObject:[self createQueryTestWithName:@"GreaterThan and LessThan - Movies from the 90s" andPredicate:[NSPredicate predicateWithFormat:@"(Year > 1989) and (Year < 2000)"]]];
    [result addObject:[self createQueryTestWithName:@"GreaterEqual and LessEqual - Movies from the 90s" andPredicate:[NSPredicate predicateWithFormat:@"(Year >= 1990) and (Year <= 1999)"]]];
    [result addObject:[self createQueryTestWithName:@"Compound statement - OR of ANDs - Movies from the 30s and 50s" andPredicate:[NSPredicate predicateWithFormat:@"((Year >= 1930) && (Year < 1940)) || ((Year >= 1950) && (Year < 1960))"]]];
    [result addObject:[self createQueryTestWithName:@"Division, equal and different - Movies from the year 2000 with rating other than R" andPredicate:[NSPredicate predicateWithFormat:@"((Year / 1000.0) = 2) and (Rating != 'R')"]]];
    [result addObject:[self createQueryTestWithName:@"Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours" andPredicate:[NSPredicate predicateWithFormat:@"((Year - 1900) >= 80) and (Year + 10 < 2000) and (Duration < 120)"]]];
    
    // String functions
    [result addObject:[self createQueryTestWithName:@"StartsWith - Movies which starts with 'The'" andPredicate:[NSPredicate predicateWithFormat:@"Title BEGINSWITH %@", @"The"] andTop:@100 andSkip:nil]];
    [result addObject:[self createQueryTestWithName:@"StartsWith, case insensitive - Movies which start with 'the'" andPredicate:[NSPredicate predicateWithFormat:@"Title BEGINSWITH[c] %@", @"the"] andTop:@100 andSkip:nil]];
    [result addObject:[self createQueryTestWithName:@"EndsWith, case insensitive - Movies which end with 'r'" andPredicate:[NSPredicate predicateWithFormat:@"Title ENDSWITH[c] 'r'"]]];
    [result addObject:[self createQueryTestWithName:@"Contains - Movies which contain the word 'one', case insensitive" andPredicate:[NSPredicate predicateWithFormat:@"Title CONTAINS[c] %@", @"one"]]];
    
    // Numeric functions
    [result addObject:[self createQueryTestWithName:@"Floor - Movies which last more than 3 hours" andPredicate:[NSPredicate predicateWithFormat:@"floor(Duration / 60.0) >= 3"]]];
    [result addObject:[self createQueryTestWithName:@"Ceiling - Best picture winners which last at most 2 hours" andPredicate:[NSPredicate predicateWithFormat:@"BestPictureWinner = TRUE and ceiling(Duration / 60.0) = 2"]]];
    
    // Date fields
    [result addObject:[self createQueryTestWithName:@"Date: Greater than, less than - Movies with release date in the 70s" andPredicate:[NSPredicate predicateWithFormat:@"ReleaseDate > %@ and ReleaseDate < %@", [ZumoTestGlobals createDateWithYear:1969 month:12 day:31], [ZumoTestGlobals createDateWithYear:1980 month:1 day:1]]]];
    [result addObject:[self createQueryTestWithName:@"Date: Greater or equal, less or equal - Movies with release date in the 80s" andPredicate:[NSPredicate predicateWithFormat:@"ReleaseDate >= %@ and ReleaseDate <= %@", [ZumoTestGlobals createDateWithYear:1980 month:1 day:1], [ZumoTestGlobals createDateWithYear:1989 month:12 day:31]]]];
    [result addObject:[self createQueryTestWithName:@"Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)" andPredicate:[NSPredicate predicateWithFormat:@"ReleaseDate = %@", [ZumoTestGlobals createDateWithYear:1994 month:10 day:14]]]];
    
    // Bool fields
    [result addObject:[self createQueryTestWithName:@"Bool: equal to TRUE - Best picture winners before 1950" andPredicate:[NSPredicate predicateWithFormat:@"BestPictureWinner = TRUE and Year < 1950"]]];
    [result addObject:[self createQueryTestWithName:@"Bool: equal to FALSE - Best picture winners after 2000" andPredicate:[NSPredicate predicateWithFormat:@"not(BestPictureWinner = FALSE) and Year >= 2000"]]];
    [result addObject:[self createQueryTestWithName:@"Bool: not equal to FALSE - Best picture winners after 2000" andPredicate:[NSPredicate predicateWithFormat:@"BestPictureWinner != FALSE and Year >= 2000"]]];
    
    // Predicate with substitution variables
    [result addObject:[self createQueryTestWithName:@"IN - Movies from the even years in the 2000s with rating PG, PG-13 or R" andPredicate:[NSPredicate predicateWithFormat:@"Year IN %@ and Rating IN %@", @[@2000, @2002, @2004, @2006, @2008], @[@"R", @"PG", @"PG-13"]] andTop:@100 andSkip:nil]];
    [result addObject:[self createQueryTestWithName:@"%K, %d substitution - Movies from 2000 rated PG-13" andPredicate:[NSPredicate predicateWithFormat:@"%K >= %d and %K = %@", @"Year", @2000, @"Rating", @"PG-13"]]];

    // Top and skip
    [result addObject:[self createQueryTestWithName:@"Get all using large $top - fetchLimit = 500" andPredicate:nil andTop:@500 andSkip:nil]];
    [result addObject:[self createQueryTestWithName:@"Skip all using large $skip - fetchOffset = 500" andPredicate:nil andTop:nil andSkip:@500]];
    [result addObject:[self createQueryTestWithName:@"Skip, take and includeTotalCount - Movies 11-20, ordered by title" andPredicate:nil andTop:@10 andSkip:@10 andOrderBy:[NSArray arrayWithObject:[OrderByClause ascending:@"Title"]] andIncludeTotalCount:YES]];
    [result addObject:[self createQueryTestWithName:@"Skip, take and includeTotalCount with predicate - Movies 11-20 which won the best picture award, ordered by release date" andPredicate:[NSPredicate predicateWithFormat:@"BestPictureWinner = TRUE"] andTop:@10 andSkip:@10 andOrderBy:[NSArray arrayWithObject:[OrderByClause descending:@"Year"]] andIncludeTotalCount:YES]];
    
    // Order by
    [result addObject:[self createQueryTestWithName:@"Order by date and string - 50 movies, ordered by release date, then title" andPredicate:nil andTop:@50 andSkip:nil andOrderBy:[NSArray arrayWithObjects:[OrderByClause descending:@"ReleaseDate"], [OrderByClause ascending:@"Title"], nil]]];
    [result addObject:[self createQueryTestWithName:@"Order by number - 30 shorter movies since 1970" andPredicate:[NSPredicate predicateWithFormat:@"Year >= 1970"] andTop:@30 andSkip:nil andOrderBy:[NSArray arrayWithObjects:[OrderByClause ascending:@"Duration"], [OrderByClause ascending:@"Title"], nil] andIncludeTotalCount:YES]];
    
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
    
    NSArray *unsupportedPredicates = [NSArray arrayWithObjects:
                                      @"average(Duration) > 120",
                                      @"predicate from block",
                                      nil];
    for (NSString *unsupportedPredicate in unsupportedPredicates) {
        ZumoTest *negTest = [ZumoTest createTestWithName:[NSString stringWithFormat:@"(Neg) Unsupported predicate: %@", unsupportedPredicate] andExecution:nil];
        __weak ZumoTest *weakNegTest = negTest;
        [negTest setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            MSTable *table = [client getTable:queryTestsTableName];
            NSPredicate *predicate;
            if ([unsupportedPredicate isEqualToString:@"predicate from block"]) {
                predicate = [NSPredicate predicateWithBlock:^BOOL(id evaluatedObject, NSDictionary *bindings) {
                    return [[(NSDictionary *)evaluatedObject objectForKey:@"BestPictureWinner"] boolValue];
                }];
            } else {
                predicate = [NSPredicate predicateWithFormat:unsupportedPredicate];
            }
            
            [table readWhere:predicate completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
                BOOL passed = NO;
                if (!error) {
                    [weakNegTest addLog:[NSString stringWithFormat:@"Expected error, got result: %@", items]];
                } else {
                    if ([error code] == MSPredicateNotSupported) {
                        [weakNegTest addLog:[NSString stringWithFormat:@"Got expected error: %@", error]];
                        passed = YES;
                    } else {
                        [weakNegTest addLog:[NSString stringWithFormat:@"Wrong error received: %@", error]];
                    }
                }
                
                [weakNegTest setTestStatus:(passed ? TSPassed : TSFailed)];
                completion(passed);
            }];
        }];
        [result addObject:negTest];
    }
    
    return result;
}

+ (ZumoTest *)createPopulateTest {
    ZumoTest *result = [[ZumoTest alloc] init];
    [result setTestName:@"Populate table, if necessary, for query tests"];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        NSArray *movies = [ZumoQueryTestData getMovies];
        NSDictionary *item = @{@"movies" : movies};
        MSTable *table = [client getTable:queryTestsTableName];
        [table insert:item completion:^(NSDictionary *item, NSError *error) {
            if (error) {
                [weakRef addLog:[NSString stringWithFormat:@"Error populating table: %@", error]];
                [weakRef setTestStatus:TSFailed];
                completion(NO);
            } else {
                [weakRef addLog:@"Table is populated and ready for query tests"];
                [weakRef setTestStatus:TSPassed];
                completion(YES);
            }
        }];
    }];
    
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
        MSTable *table = [client getTable:queryTestsTableName];
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

+ (ZumoTest *)createQueryTestWithName:(NSString *)name andPredicate:(NSPredicate *)predicate {
    return [self createQueryTestWithName:name andPredicate:predicate andTop:nil andSkip:nil andOrderBy:nil andIncludeTotalCount:NO];
}

+ (ZumoTest *)createQueryTestWithName:(NSString *)name andPredicate:(NSPredicate *)predicate andTop:(NSNumber *)top andSkip:(NSNumber *)skip {
    return [self createQueryTestWithName:name andPredicate:predicate andTop:top andSkip:skip andOrderBy:nil andIncludeTotalCount:NO];
}

+ (ZumoTest *)createQueryTestWithName:(NSString *)name andPredicate:(NSPredicate *)predicate andTop:(NSNumber *)top andSkip:(NSNumber *)skip andOrderBy:(NSArray *)orderByClauses {
    return [self createQueryTestWithName:name andPredicate:predicate andTop:top andSkip:skip andOrderBy:orderByClauses andIncludeTotalCount:NO];
}

+ (ZumoTest *)createQueryTestWithName:(NSString *)name andPredicate:(NSPredicate *)predicate andTop:(NSNumber *)top andSkip:(NSNumber *)skip andOrderBy:(NSArray *)orderByClauses andIncludeTotalCount:(BOOL)includeTotalCount {
    ZumoTest *result = [[ZumoTest alloc] init];
    [result setTestName:name];
    __weak ZumoTest *weakRef = result;
    [result setExecution:^(UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client getTable:queryTestsTableName];
        NSArray *allItems = [ZumoQueryTestData getMovies];
        if (!top && !skip && !orderByClauses && !includeTotalCount) {
            // use simple readWithPredicate
            [table readWhere:predicate completion:^(NSArray *queriedItems, NSInteger totalCount2, NSError *readWhereError) {
                if (readWhereError) {
                    [weakRef addLog:[NSString stringWithFormat:@"Error calling readWhere: %@", readWhereError]];
                    [weakRef setTestStatus:TSFailed];
                    completion(NO);
                } else {
                    NSArray *filteredArray = [allItems filteredArrayUsingPredicate:predicate];
                    BOOL passed = [self compareExpectedArray:filteredArray withActual:queriedItems forTest:weakRef];
                    if (passed) {
                        [weakRef addLog:@"Test passed"];
                        [weakRef setTestStatus:TSPassed];
                        completion(YES);
                    } else {
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
            
            if (orderByClauses) {
                for (OrderByClause *clause in orderByClauses) {
                    if ([clause isAscending]) {
                        [query orderByAscending:[clause fieldName]];
                    } else {
                        [query orderByDescending:[clause fieldName]];
                    }
                }
            }
            
            if (includeTotalCount) {
                [query setIncludeTotalCount:YES];
            }
                    
            [query readWithCompletion:^(NSArray *queriedItems, NSInteger totalCount, NSError *queryReadError) {
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
                    
                    BOOL passed = NO;
                    int expectedTotalItems = [filteredArray count];
                    
                    if (includeTotalCount && totalCount != expectedTotalItems) {
                        [weakRef addLog:[NSString stringWithFormat:@"Error in 'totalCount'. Expected: %d, actual: %d", expectedTotalItems, totalCount]];
                    } else {
                        if (orderByClauses) {
                            NSMutableArray *sortDescriptors = [[NSMutableArray alloc] init];
                            for (OrderByClause *clause in orderByClauses) {
                                [sortDescriptors addObject:[[NSSortDescriptor alloc] initWithKey:[clause fieldName] ascending:[clause isAscending]]];
                            }
                            
                            filteredArray = [filteredArray sortedArrayUsingDescriptors:sortDescriptors];
                        }

                        if (top || skip) {
                            int rangeStart = skip ? [skip intValue] : 0;
                            if (rangeStart > expectedTotalItems) {
                                rangeStart = expectedTotalItems;
                            }
                        
                            int rangeLen = top ? [top intValue] : expectedTotalItems;
                            if ((rangeStart + rangeLen) > expectedTotalItems) {
                                rangeLen = expectedTotalItems - rangeStart;
                            }
                            
                            filteredArray = [filteredArray subarrayWithRange:NSMakeRange(rangeStart, rangeLen)];
                        }

                        passed = [self compareExpectedArray:filteredArray withActual:queriedItems forTest:weakRef];
                    }
                    
                    if (passed) {
                        [weakRef addLog:@"Received expected result"];
                        [weakRef setTestStatus:TSPassed];
                    } else {
                        [weakRef setTestStatus:TSFailed];
                    }
                    
                    completion(passed);
                }
            }];
        }
    }];
    
    return result;
}

+ (BOOL)compareExpectedArray:(NSArray *)expectedItems withActual:(NSArray *)actualItems forTest:(__weak ZumoTest *)test {
    BOOL result = NO;
    int actualCount = [actualItems count];
    int expectedCount = [expectedItems count];
    if (actualCount != expectedCount) {
        [test addLog:[NSString stringWithFormat:@"Expected %d items, but got %d", expectedCount, actualCount]];
        [test addLog:[NSString stringWithFormat:@"Expected items: %@", expectedItems]];
        [test addLog:[NSString stringWithFormat:@"Actual items: %@", actualItems]];
    } else {
        BOOL allItemsEqual = YES;
        for (int i = 0; i < actualCount; i++) {
            NSDictionary *expectedItem = expectedItems[i];
            NSDictionary *actualItem = actualItems[i];
            BOOL allValuesEqual = YES;
            for (NSString *key in [expectedItem keyEnumerator]) {
                if ([key isEqualToString:@"id"]) continue; // don't care about id
                id expectedValue = expectedItem[key];
                id actualValue = actualItem[key];
                if (![expectedValue isEqual:actualValue]) {
                    allValuesEqual = NO;
                    [test addLog:[NSString stringWithFormat:@"Error comparing field %@ of item %d: expected - %@, actual - %@", key, i, expectedValue, actualValue]];
                    break;
                }
            }
            
            if (!allValuesEqual) {
                allItemsEqual = NO;
            }
        }
        
        if (allItemsEqual) {
            result = YES;
        }
    }
    
    return result;
}

+ (NSString *)helpText {
    NSArray *lines = [NSArray arrayWithObjects:
                      @"1. Create an application on Windows azure portal.",
                      @"2. Create a table called 'iOSMovies'",
                      @"3. Set the following code to be the 'insert' script for the table:",
                      @"   function insert(item, user, request) {",
                      @"       item.id = 1;",
                      @"       var table = tables.getTable('iosmovies');",
                      @"       table.take(1).read({",
                      @"       success: function(items) {",
                      @"           if (items.length > 0) {",
                      @"               // table already populated",
                      @"               request.respond(201, {id: 1, status: 'Already populated'});",
                      @"           } else {",
                      @"               // Need to populate the table",
                      @"               populateTable(table, request, item.movies);",
                      @"           }",
                      @"       }",
                      @"       });",
                      @"   }",
                      @"   ",
                      @"   function populateTable(table, request, films) {",
                      @"       var index = 0;",
                      @"       films.forEach(fixReleaseDate);"
                      @"       var insertNext = function() {",
                      @"           if (index >= films.length) {",
                      @"               request.respond(201, {id : 1, status : 'Table populated successfully'});",
                      @"           } else {",
                      @"               var toInsert = films[index];",
                      @"               table.insert(toInsert, {",
                      @"               success: function() {",
                      @"                   index++;",
                      @"                   if ((index % 20) === 0) {",
                      @"                       console.log('Inserted %d items', index);",
                      @"                   }",
                      @"                   insertNext();",
                      @"               }",
                      @"               });",
                      @"           }",
                      @"       };",
                      @"       ",
                      @"       insertNext();",
                      @"   }",
                      @"   ",
                      @"   function fixReleaseDate(movie) {",
                      @"       var releaseDate = movie.ReleaseDate;",
                      @"       if (typeof releaseDate === 'string') {",
                      @"           movie.ReleaseDate = new Date(releaseDate);",
                      @"       }",
                      @"   }",
                      @"4. Click the 'Query Tests' button.",
                      @"5. Make sure all the scenarios pass.", nil];
    return [lines componentsJoinedByString:@"\n"];
}

@end
