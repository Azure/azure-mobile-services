// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

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

static NSString *queryTestsTableName = @"intIdMovies";
static NSString *stringIdQueryTestsTableName = @"stringIdMovies";

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    
    [result addObject:[self createPopulateTest]];
    [result addObject:[self createPopulateStringIdTableTest]];

    [self addQueryTestToGroup:result name:@"GreaterThan and LessThan - Movies from the 90s" predicate:[NSPredicate predicateWithFormat:@"(Year > 1989) and (Year < 2000)"]];

    // Numeric fields
    [self addQueryTestToGroup:result name:@"GreaterThan and LessThan - Movies from the 90s" predicate:[NSPredicate predicateWithFormat:@"(Year > 1989) and (Year < 2000)"]];
    [self addQueryTestToGroup:result name:@"GreaterEqual and LessEqual - Movies from the 90s" predicate:[NSPredicate predicateWithFormat:@"(Year >= 1990) and (Year <= 1999)"]];
    [self addQueryTestToGroup:result name:@"Compound statement - OR of ANDs - Movies from the 30s and 50s" predicate:[NSPredicate predicateWithFormat:@"((Year >= 1930) && (Year < 1940)) || ((Year >= 1950) && (Year < 1960))"]];
    [self addQueryTestToGroup:result name:@"Division, equal and different - Movies from the year 2000 with rating other than R" predicate:[NSPredicate predicateWithFormat:@"((Year / 1000.0) = 2) and (MPAARating != 'R')"]];
    [self addQueryTestToGroup:result name:@"Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours" predicate:[NSPredicate predicateWithFormat:@"((Year - 1900) >= 80) and (Year + 10 < 2000) and (Duration < 120)"]];
    
    // String functions
    [self addQueryTestToGroup:result name:@"StartsWith - Movies which starts with 'The'" predicate:[NSPredicate predicateWithFormat:@"Title BEGINSWITH %@", @"The"] top:@100 skip:nil];
    [self addQueryTestToGroup:result name:@"StartsWith, case insensitive - Movies which start with 'the'" predicate:[NSPredicate predicateWithFormat:@"Title BEGINSWITH[c] %@", @"the"] top:@100 skip:nil];
    [self addQueryTestToGroup:result name:@"EndsWith, case insensitive - Movies which end with 'r'" predicate:[NSPredicate predicateWithFormat:@"Title ENDSWITH[c] 'r'"]];
    [self addQueryTestToGroup:result name:@"Contains - Movies which contain the word 'one', case insensitive" predicate:[NSPredicate predicateWithFormat:@"Title CONTAINS[c] %@", @"one"]];
    [self addQueryTestToGroup:result name:@"Contains (non-ASCII) - Movies containing the 'é' character" predicate:[NSPredicate predicateWithFormat:@"Title CONTAINS[c] 'é'"]];
    
    // String fields
    [self addQueryTestToGroup:result name:@"Equals - Movies since 1980 with rating PG-13" predicate:[NSPredicate predicateWithFormat:@"MPAARating = 'PG-13' and Year >= 1980"] top:@100 skip:nil];
    [self addQueryTestToGroup:result name:@"Comparison to nil - Movies since 1980 without a MPAA rating" predicate:[NSPredicate predicateWithFormat:@"MPAARating = %@ and Year >= 1980", nil]];
    [self addQueryTestToGroup:result name:@"Comparison to nil (not NULL) - Movies before 1970 with a MPAA rating" predicate:[NSPredicate predicateWithFormat:@"MPAARating <> %@ and Year < 1970", nil]];

    // Numeric functions
    [self addQueryTestToGroup:result name:@"Floor - Movies which last more than 3 hours" predicate:[NSPredicate predicateWithFormat:@"floor(Duration / 60.0) >= 3"]];
    [self addQueryTestToGroup:result name:@"Ceiling - Best picture winners which last at most 2 hours" predicate:[NSPredicate predicateWithFormat:@"BestPictureWinner = TRUE and ceiling(Duration / 60.0) = 2"]];
    
    // Constant predicates
    [self addQueryTestToGroup:result name:@"TRUEPREDICATE - First 10 movies" predicate:[NSPredicate predicateWithFormat:@"TRUEPREDICATE"] top:@10 skip:nil];
    [self addQueryTestToGroup:result name:@"FALSEPREDICATE - No movies" predicate:[NSPredicate predicateWithFormat:@"FALSEPREDICATE"]];

    // Date fields
    [self addQueryTestToGroup:result name:@"Date: Greater than, less than - Movies with release date in the 70s" predicate:[NSPredicate predicateWithFormat:@"ReleaseDate > %@ and ReleaseDate < %@", [ZumoTestGlobals createDateWithYear:1969 month:12 day:31], [ZumoTestGlobals createDateWithYear:1980 month:1 day:1]]];
    [self addQueryTestToGroup:result name:@"Date: Greater or equal, less or equal - Movies with release date in the 80s" predicate:[NSPredicate predicateWithFormat:@"ReleaseDate >= %@ and ReleaseDate <= %@", [ZumoTestGlobals createDateWithYear:1980 month:1 day:1], [ZumoTestGlobals createDateWithYear:1989 month:12 day:31]]];
    [self addQueryTestToGroup:result name:@"Date: Equal - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)" predicate:[NSPredicate predicateWithFormat:@"ReleaseDate = %@", [ZumoTestGlobals createDateWithYear:1994 month:10 day:14]]];

    // Bool fields
    [self addQueryTestToGroup:result name:@"Bool: equal to TRUE - Best picture winners before 1950" predicate:[NSPredicate predicateWithFormat:@"BestPictureWinner = TRUE and Year < 1950"]];
    [self addQueryTestToGroup:result name:@"Bool: equal to FALSE - Best picture winners after 2000" predicate:[NSPredicate predicateWithFormat:@"not(BestPictureWinner = FALSE) and Year >= 2000"]];
    [self addQueryTestToGroup:result name:@"Bool: not equal to FALSE - Best picture winners after 2000" predicate:[NSPredicate predicateWithFormat:@"BestPictureWinner != FALSE and Year >= 2000"]];
    
    // Predicate with substitution variables
    [self addQueryTestToGroup:result name:@"IN - Movies from the even years in the 2000s with rating PG, PG-13 or R" predicate:[NSPredicate predicateWithFormat:@"Year IN %@ and MPAARating IN %@", @[@2000, @2002, @2004, @2006, @2008], @[@"R", @"PG", @"PG-13"]] top:@100 skip:nil];
    [self addQueryTestToGroup:result name:@"BETWEEN - Movies from the 1960s" predicate:[NSPredicate predicateWithFormat:@"Year BETWEEN %@", @[@1960, @1970]]];
    [self addQueryTestToGroup:result name:@"%K, %d substitution - Movies from 2000 rated PG-13" predicate:[NSPredicate predicateWithFormat:@"%K >= %d and %K = %@", @"Year", @2000, @"MPAARating", @"PG-13"]];

    // Top and skip
    [self addQueryTestToGroup:result name:@"Get all using large $top - fetchLimit = 500" predicate:nil top:@500 skip:nil];
    [self addQueryTestToGroup:result name:@"Skip all using large $skip - fetchOffset = 500" predicate:nil top:nil skip:@500];
    [self addQueryTestToGroup:result name:@"Skip, take and includeTotalCount - Movies 11-20, ordered by title" predicate:nil top:@10 skip:@10 orderBy:@[[OrderByClause ascending:@"Title"]] includeTotalCount:YES selectFields:nil];
    [self addQueryTestToGroup:result name:@"Skip, take and includeTotalCount with predicate - Movies 11-20 which won the best picture award, ordered by release date" predicate:[NSPredicate predicateWithFormat:@"BestPictureWinner = TRUE"] top:@10 skip:@10 orderBy:[NSArray arrayWithObject:[OrderByClause descending:@"Year"]] includeTotalCount:YES selectFields:nil];
    
    // Order by
    [self addQueryTestToGroup:result name:@"Order by date and string - 50 movies, ordered by release date, then title" predicate:nil top:@50 skip:nil orderBy:@[[OrderByClause descending:@"ReleaseDate"], [OrderByClause ascending:@"Title"]] includeTotalCount:NO selectFields:nil];
    [self addQueryTestToGroup:result name:@"Order by number - 30 shorter movies since 1970" predicate:[NSPredicate predicateWithFormat:@"Year >= 1970"] top:@30 skip:nil orderBy:[NSArray arrayWithObjects:[OrderByClause ascending:@"Duration"], [OrderByClause ascending:@"Title"], nil] includeTotalCount:YES selectFields:nil];
    
    // Select
    [self addQueryTestToGroup:result name:@"Select single field - Title of movies since 2000" predicate:[NSPredicate predicateWithFormat:@"Year >= 2000"] top:@200 skip:nil orderBy:nil includeTotalCount:NO selectFields:@[@"Title"]];
    [self addQueryTestToGroup:result name:@"Select multiple fields - Title, BestPictureWinner, Duration, ordered by release date, movies from the 1990" predicate:[NSPredicate predicateWithFormat:@"Year >= 1990 and Year < 2000"] top:@300 skip:nil orderBy:@[[OrderByClause ascending:@"Title"]] includeTotalCount:NO selectFields:@[@"Title", @"BestPictureWinner", @"Duration"]];
    
    for (int i = -1; i <= 0; i++) {
        ZumoTest *negativeLookupTest = [ZumoTest createTestWithName:[NSString stringWithFormat:@"(Neg) MSTable readWithId:%d", i] andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            MSTable *table = [client tableWithName:queryTestsTableName];
            [table readWithId:[NSNumber numberWithInt:i] completion:^(NSDictionary *item, NSError *err) {
                BOOL passed = NO;
                if (err) {
                    if (i == 0) {
                        if (err.code != MSInvalidItemIdWithRequest) {
                            [test addLog:[NSString stringWithFormat:@"Invalid error code: %d", err.code]];
                        } else {
                            [test addLog:@"Got expected error"];
                            NSHTTPURLResponse *response = [[err userInfo] objectForKey:MSErrorResponseKey];
                            if (response) {
                                [test addLog:[NSString stringWithFormat:@"Error, response should be nil (request not sent), but its status code is %d", [response statusCode]]];
                                passed = NO;
                            } else {
                                [test addLog:@"Success, request was not sent to the server"];
                                passed = YES;
                            }
                        }
                    } else {
                        if (err.code != MSErrorMessageErrorCode) {
                            [test addLog:[NSString stringWithFormat:@"Invalid error code: %d", err.code]];
                        } else {
                            NSHTTPURLResponse *httpResponse = [[err userInfo] objectForKey:MSErrorResponseKey];
                            int statusCode = [httpResponse statusCode];
                            if (statusCode == 404) {
                                [test addLog:@"Got expected error"];
                                passed = YES;
                            } else {
                                [test addLog:[NSString stringWithFormat:@"Invalid response status code: %d", statusCode]];
                                passed = NO;
                            }
                        }
                    }
                } else {
                    [test addLog:[NSString stringWithFormat:@"Expected error for lookup with id:%d, but got data back: %@", i, item]];
                }
                
                [test setTestStatus:(passed ? TSPassed : TSFailed)];
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
        ZumoTest *negTest = [ZumoTest createTestWithName:[NSString stringWithFormat:@"(Neg) Unsupported predicate: %@", unsupportedPredicate] andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            MSTable *table = [client tableWithName:queryTestsTableName];
            NSPredicate *predicate;
            if ([unsupportedPredicate isEqualToString:@"predicate from block"]) {
                predicate = [NSPredicate predicateWithBlock:^BOOL(id evaluatedObject, NSDictionary *bindings) {
                    return [[(NSDictionary *)evaluatedObject objectForKey:@"BestPictureWinner"] boolValue];
                }];
            } else {
                predicate = [NSPredicate predicateWithFormat:unsupportedPredicate];
            }
            
            [table readWithPredicate:predicate completion:^(NSArray *items, NSInteger totalCount, NSError *error) {
                BOOL passed = NO;
                if (!error) {
                    [test addLog:[NSString stringWithFormat:@"Expected error, got result: %@", items]];
                } else {
                    if ([error code] == MSPredicateNotSupported) {
                        [test addLog:[NSString stringWithFormat:@"Got expected error: %@", error]];
                        passed = YES;
                    } else {
                        [test addLog:[NSString stringWithFormat:@"Wrong error received: %@", error]];
                    }
                }
                
                [test setTestStatus:(passed ? TSPassed : TSFailed)];
                completion(passed);
            }];
        }];
        [result addObject:negTest];
    }
    
    return result;
}

+ (ZumoTest *)createPopulateTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Populate table, if necessary, for query tests" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        NSArray *movies = [ZumoQueryTestData getMovies];
        NSDictionary *item = @{@"movies" : movies};
        MSTable *table = [client tableWithName:queryTestsTableName];
        [table insert:item completion:^(NSDictionary *item, NSError *error) {
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error populating table: %@", error]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                [test addLog:@"Table is populated and ready for query tests"];
                [test setTestStatus:TSPassed];
                completion(YES);
            }
        }];
    }];
    
    return result;
}

+ (ZumoTest *)createPopulateStringIdTableTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Populate table with string ids, if necessary, for query tests" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        NSArray *movies = [ZumoQueryTestData getMovies];
        NSMutableArray *stringIdMovies = [[NSMutableArray alloc] init];
        for (int i = 0; i < [movies count]; i++) {
            NSMutableDictionary *strIdMovie = [NSMutableDictionary dictionaryWithDictionary:[movies objectAtIndex:i]];
            NSString *movieId = [NSString stringWithFormat:@"Movie %03d", i];
            [strIdMovie setValue:movieId forKey:@"id"];
            [stringIdMovies addObject:strIdMovie];
        }
        NSDictionary *item = @{@"movies" : stringIdMovies};
        MSTable *table = [client tableWithName:stringIdQueryTestsTableName];
        [table insert:item completion:^(NSDictionary *item, NSError *error) {
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error populating table: %@", error]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                [test addLog:@"Table is populated and ready for query tests"];
                [test setTestStatus:TSPassed];
                completion(YES);
            }
        }];
    }];
    
    return result;
}

typedef void (^ActionQuery)(MSQuery *query);
typedef BOOL (^QueryValidation)(ZumoTest *test, NSError *error);

+ (ZumoTest *)createNegativeTestWithName:(NSString *)name andQuerySettings:(ActionQuery)settings andQueryValidation:(QueryValidation)queryValidation {
    ZumoTest *result = [ZumoTest createTestWithName:name andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:queryTestsTableName];
        MSQuery *query = [table query];
        settings(query);
        [query readWithCompletion:^(NSArray *items, NSInteger totalCount, NSError *error) {
            if (error) {
                if (queryValidation(test, error)) {
                    [test addLog:[NSString stringWithFormat:@"Got expected error: %@", error]];
                    [test setTestStatus:TSPassed];
                    completion(YES);
                } else {
                    [test addLog:[NSString stringWithFormat:@"Error wasn't the expected one: %@", error]];
                    [test setTestStatus:TSFailed];
                    completion(NO);
                }
            } else {
                [test addLog:@"Query should fail, but succeeded"];
                [test setTestStatus:TSFailed];
                completion(NO);
            }
        }];
    }];
    
    return result;
}

+ (void)addQueryTestToGroup:(NSMutableArray *)testGroup name:(NSString *)name predicate:(NSPredicate *)predicate {
    [self addQueryTestToGroup:testGroup name:name predicate:predicate top:nil skip:nil orderBy:nil includeTotalCount:NO selectFields:nil];
}

+ (void)addQueryTestToGroup:(NSMutableArray *)testGroup name:(NSString *)name predicate:(NSPredicate *)predicate top:(NSNumber *)top skip:(NSNumber *)skip {
    [self addQueryTestToGroup:testGroup name:name predicate:predicate top:top skip:skip orderBy:nil includeTotalCount:NO selectFields:nil];
}

+ (void)addQueryTestToGroup:(NSMutableArray *)testGroup name:(NSString *)name predicate:(NSPredicate *)predicate top:(NSNumber *)top skip:(NSNumber *)skip orderBy:(NSArray *)orderByClauses includeTotalCount:(BOOL)includeTotalCount selectFields:(NSArray *)selectFields {
    [testGroup addObject:[self createQueryTestWithName:name andPredicate:predicate andTop:top andSkip:skip andOrderBy:orderByClauses andIncludeTotalCount:includeTotalCount andSelectFields:selectFields useStringIdTable:NO]];
    [testGroup addObject:[self createQueryTestWithName:name andPredicate:predicate andTop:top andSkip:skip andOrderBy:orderByClauses andIncludeTotalCount:includeTotalCount andSelectFields:selectFields useStringIdTable:YES]];
}

+ (ZumoTest *)createQueryTestWithName:(NSString *)name andPredicate:(NSPredicate *)predicate andTop:(NSNumber *)top andSkip:(NSNumber *)skip andOrderBy:(NSArray *)orderByClauses andIncludeTotalCount:(BOOL)includeTotalCount andSelectFields:(NSArray *)selectFields useStringIdTable:(BOOL)useStringIdTable {
    NSString *testName = [NSString stringWithFormat:@"[%@ id] %@", useStringIdTable ? @"string" : @"int", name];
    ZumoTest *result = [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {

        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:queryTestsTableName];
        NSArray *allItems = [ZumoQueryTestData getMovies];
        if (!top && !skip && !orderByClauses && !includeTotalCount && !selectFields) {
            // use simple readWithPredicate
            [table readWithPredicate:predicate completion:^(NSArray *queriedItems, NSInteger totalCount2, NSError *readWhereError) {
                if (readWhereError) {
                    [test addLog:[NSString stringWithFormat:@"Error calling readWhere: %@", readWhereError]];
                    [test setTestStatus:TSFailed];
                    completion(NO);
                } else {
                    NSArray *filteredArray = [allItems filteredArrayUsingPredicate:predicate];
                    BOOL passed = [self compareExpectedArray:filteredArray withActual:queriedItems forTest:test];

                    int queriedCount = [queriedItems count];
                    int maxTrace = queriedCount > 5 ? 5 : queriedCount;
                    NSArray *toTrace = [queriedItems subarrayWithRange:NSMakeRange(0, maxTrace)];
                    NSString *continuation = queriedCount > 5 ? @" (and more items)" : @"";
                    [test addLog:[NSString stringWithFormat:@"Queried items: %@%@", toTrace, continuation]];
                    
                    if (passed) {
                        [test addLog:@"Test passed"];
                        [test setTestStatus:TSPassed];
                        completion(YES);
                    } else {
                        [test setTestStatus:TSFailed];
                        completion(NO);
                    }
                }
            }];
        } else {
            MSQuery *query = predicate ? [table queryWithPredicate:predicate] : [table query];
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
            
            if (selectFields) {
                [query setSelectFields:selectFields];
            }
                    
            [query readWithCompletion:^(NSArray *queriedItems, NSInteger totalCount, NSError *queryReadError) {
                if (queryReadError) {
                    [test addLog:[NSString stringWithFormat:@"Error calling MSQuery readWithCompletion: %@", queryReadError]];
                    [test setTestStatus:TSFailed];
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
                        [test addLog:[NSString stringWithFormat:@"Error in 'totalCount'. Expected: %d, actual: %d", expectedTotalItems, totalCount]];
                    } else {
                        if (orderByClauses) {
                            if ([orderByClauses count] == 1 && [[orderByClauses[0] fieldName] isEqualToString:@"Title"] && [orderByClauses[0] isAscending]) {
                                // Special case, need to ignore accents
                                filteredArray = [filteredArray sortedArrayUsingComparator:^NSComparisonResult(id obj1, id obj2) {
                                    NSString *title1 = obj1[@"Title"];
                                    NSString *title2 = obj2[@"Title"];
                                    return [title1 compare:title2 options:NSDiacriticInsensitiveSearch];
                                }];
                            } else {
                                NSMutableArray *sortDescriptors = [[NSMutableArray alloc] init];
                                for (OrderByClause *clause in orderByClauses) {
                                    [sortDescriptors addObject:[[NSSortDescriptor alloc] initWithKey:[clause fieldName] ascending:[clause isAscending]]];
                                }
                            
                                filteredArray = [filteredArray sortedArrayUsingDescriptors:sortDescriptors];
                            }
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
                        
                        if (selectFields) {
                            NSMutableArray *projectedArray = [[NSMutableArray alloc] init];
                            for (int i = 0; i < [filteredArray count]; i++) {
                                NSDictionary *item = filteredArray[i];
                                item = [item dictionaryWithValuesForKeys:selectFields];
                                [projectedArray addObject:item];
                            }
                            
                            filteredArray = projectedArray;
                        }

                        passed = [self compareExpectedArray:filteredArray withActual:queriedItems forTest:test];
                    }
                    
                    if (passed) {
                        [test addLog:@"Received expected result"];
                        [test setTestStatus:TSPassed];
                    } else {
                        [test setTestStatus:TSFailed];
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
            NSDictionary *expectedItem = [expectedItems objectAtIndex:i];
            NSDictionary *actualItem = [actualItems objectAtIndex:i];
            BOOL allValuesEqual = YES;
            for (NSString *key in [expectedItem keyEnumerator]) {
                if ([key isEqualToString:@"id"]) continue; // don't care about id
                id expectedValue = [expectedItem objectForKey:key];
                id actualValue = [actualItem objectForKey:key];
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

+ (NSString *)groupDescription {
    return @"Tests for validating different query capabilities of the client SDK.";
}

@end
