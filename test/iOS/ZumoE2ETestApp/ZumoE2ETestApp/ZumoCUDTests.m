//
//  ZumoCUDTests.m
//  ZumoE2ETestApp
//
//  Created by Carlos Figueira on 12/9/12.
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoCUDTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>

@implementation ZumoCUDTests

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    [result addObject:[self createDeleteTestWithName:@"Delete with id" andType:DeleteUsingId]];
    [result addObject:[self createDeleteTestWithName:@"(Neg) Delete with non-existent id" andType:NegDeleteUsingInvalidId]];
    [result addObject:[self createDeleteTestWithName:@"Delete with object" andType:DeleteUsingObject]];
    [result addObject:[self createDeleteTestWithName:@"(Neg) Delete with object and non-existent id" andType:NegDeleteObjectInvalidId]];
    [result addObject:[self createDeleteTestWithName:@"(Neg) Delete with object without 'id' field" andType:NegDeleteObjectNoId]];
    
    [result addObject:[self createUpdateTestWithName:@"Update item" andType:UpdateUsingObject]];
    [result addObject:[self createUpdateTestWithName:@"(Neg) Update with non-existing id" andType:NegUpdateObjectInvalidId]];
    [result addObject:[self createUpdateTestWithName:@"(Neg) Update with no id" andType:NegUpdateObjectNoId]];
    
    return result;
}

typedef enum { UpdateUsingObject, NegUpdateObjectInvalidId, NegUpdateObjectNoId } UpdateTestType;

+ (ZumoTest *)createUpdateTestWithName:(NSString *)name andType:(UpdateTestType)type {
    ZumoTest *result = [ZumoTest createTestWithName:name andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client getTable:@"TodoItem"];
        [table insert:@{@"name":@"John Doe",@"age":[NSNumber numberWithInt:33]} completion:^(NSDictionary *inserted, NSError *insertError) {
            if (insertError) {
                [test addLog:[NSString stringWithFormat:@"Error inserting data: %@", insertError]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                NSNumber *itemId = [inserted objectForKey:@"id"];
                [test addLog:[NSString stringWithFormat:@"Inserted element %d to be deleted", [itemId intValue]]];
                [table readWithId:itemId completion:^(NSDictionary *roundTripped, NSError *rtError) {
                    if (rtError) {
                        [test addLog:[NSString stringWithFormat:@"Error retrieving inserted item: %@", rtError]];
                        [test setTestStatus:TSFailed];
                        completion(NO);
                    } else {
                        BOOL isPositiveTest = type == UpdateUsingObject;
                        NSNumber *updatedValue = [NSNumber numberWithInt:35];
                        NSMutableDictionary *itemToUpdate = [[NSMutableDictionary alloc] initWithDictionary:inserted copyItems:YES];
                        if (type == NegUpdateObjectNoId) {
                            [itemToUpdate removeObjectForKey:@"id"];
                        } else if (type == NegUpdateObjectInvalidId) {
                            [itemToUpdate setValue:[NSNumber numberWithInt:1000000000] forKey:@"id"];
                        } else {
                            [itemToUpdate setValue:updatedValue forKey:@"age"];
                        }
                        
                        [table update:itemToUpdate completion:^(NSDictionary *updatedItem, NSError *updateError) {
                            BOOL passed = YES;
                            if (isPositiveTest) {
                                if (updateError) {
                                    passed = NO;
                                    [test addLog:[NSString stringWithFormat:@"Error updating item: %@", updateError]];
                                } else {
                                    if (![updatedValue isEqualToNumber:[updatedItem objectForKey:@"age"]]) {
                                        passed = NO;
                                        [test addLog:[NSString stringWithFormat:@"Incorrect value for updated object: %@", updatedItem]];
                                    }
                                }
                            } else {
                                if (!updateError) {
                                    passed = NO;
                                    [test addLog:[NSString stringWithFormat:@"Expected error, but update succeeded for item: %@", itemToUpdate]];
                                } else if (type == NegUpdateObjectNoId) {
                                    if (updateError.code != MSMissingItemIdWithRequest) {
                                        [test addLog:[NSString stringWithFormat:@"Unexpected error code: %d", updateError.code]];
                                        passed = NO;
                                    }
                                } else if (updateError.code != MSErrorMessageErrorCode) {
                                    [test addLog:[NSString stringWithFormat:@"Unexpected error code: %d", updateError.code]];
                                    passed = NO;
                                } else {
                                    NSHTTPURLResponse *resp = [[updateError userInfo] objectForKey:MSErrorResponseKey];
                                    if (resp.statusCode != 404) {
                                        [test addLog:[NSString stringWithFormat:@"Invalid response status code, expected 404, found %d", resp.statusCode]];
                                        passed = NO;
                                    }
                                }
                            }
                            
                            if (isPositiveTest && passed) {
                                [self validateUpdateForTest:test andTable:table andId:itemId andExpectedValue:updatedValue withCompletion:completion];
                            } else {
                                [test setTestStatus:(passed ? TSPassed : TSFailed)];
                                completion(passed);
                            }
                        }];
                    }
                }];
            }
        }];
    }];
    
    return result;
}

+ (void)validateUpdateForTest:(ZumoTest *)test andTable:(MSTable *)table andId:(NSNumber *)itemId andExpectedValue:(NSNumber *)expectedValue withCompletion:(ZumoTestCompletion)completion {
    [table readWithId:itemId completion:^(NSDictionary *item, NSError *err) {
        BOOL passed = YES;
        if (err) {
            [test addLog:[NSString stringWithFormat:@"Error retrieving updated item: %@", err]];
            passed = NO;
        } else {
            if (![expectedValue isEqualToNumber:[item objectForKey:@"age"]]) {
                [test addLog:[NSString stringWithFormat:@"Update not successful, expected %@, got %@", expectedValue, item]];
                passed = NO;
            } else {
                [test addLog:@"Item updated successfully"];
            }
        }
        
        [test setTestStatus:(passed ? TSPassed : TSFailed)];
        completion(passed);
    }];
}

typedef enum { DeleteUsingId, DeleteUsingObject, NegDeleteUsingInvalidId, NegDeleteObjectInvalidId, NegDeleteObjectNoId } DeleteTestType;

+ (ZumoTest *)createDeleteTestWithName:(NSString *)name andType:(DeleteTestType)type {
    ZumoTest *result = [ZumoTest createTestWithName:name andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client getTable:@"TodoItem"];
        [table insert:@{@"name":@"John Doe",@"age":[NSNumber numberWithInt:33]} completion:^(NSDictionary *inserted, NSError *insertError) {
            if (insertError) {
                [test addLog:[NSString stringWithFormat:@"Error inserting data: %@", insertError]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                NSNumber *itemId = [inserted objectForKey:@"id"];
                [test addLog:[NSString stringWithFormat:@"Inserted element %d to be deleted", [itemId intValue]]];
                [table readWithId:itemId completion:^(NSDictionary *roundTripped, NSError *rtError) {
                    if (rtError) {
                        [test addLog:[NSString stringWithFormat:@"Error retrieving inserted item: %@", rtError]];
                        [test setTestStatus:TSFailed];
                        completion(NO);
                    } else {
                        BOOL isPositiveTest = type == DeleteUsingId || type == DeleteUsingObject;
                        if (type == DeleteUsingId || type == NegDeleteUsingInvalidId) {
                            NSNumber *idToDelete = isPositiveTest ? itemId : [NSNumber numberWithInt:1000000000];
                            [table deleteWithId:idToDelete completion:^(NSNumber *deletedItemId, NSError *deleteError) {
                                BOOL passed = YES;
                                if (isPositiveTest) {
                                    if (deleteError) {
                                        passed = NO;
                                        [test addLog:[NSString stringWithFormat:@"Error deleting item: %@", deleteError]];
                                    } else if (![itemId isEqualToNumber:deletedItemId]) {
                                        [test addLog:[NSString stringWithFormat:@"Invalid returned value after deleting: %@", deletedItemId]];
                                        passed = NO;
                                    }
                                } else {
                                    if (!deleteError) {
                                        passed = NO;
                                        [test addLog:[NSString stringWithFormat:@"Expected error, but delete succeeded for item: %@", deletedItemId]];
                                    } else if (deleteError.code != MSErrorMessageErrorCode) {
                                        [test addLog:[NSString stringWithFormat:@"Unexpected error code: %d", deleteError.code]];
                                        passed = NO;
                                    } else {
                                        NSHTTPURLResponse *resp = [[deleteError userInfo] objectForKey:MSErrorResponseKey];
                                        if (resp.statusCode != 404) {
                                            [test addLog:[NSString stringWithFormat:@"Invalid response status code, expected 404, found %d", resp.statusCode]];
                                            passed = NO;
                                        }
                                    }
                                }
                                
                                [test setTestStatus:(passed ? TSPassed : TSFailed)];
                                completion(passed);
                            }];
                        } else {
                            NSMutableDictionary *itemToDelete = [[NSMutableDictionary alloc] initWithDictionary:inserted copyItems:YES];
                            if (type == NegDeleteObjectNoId) {
                                [itemToDelete removeObjectForKey:@"id"];
                            } else if (type == NegDeleteObjectInvalidId) {
                                [itemToDelete setValue:[NSNumber numberWithInt:1000000000] forKey:@"id"];
                            }
                            
                            [table delete:itemToDelete completion:^(NSNumber *deletedItemId, NSError *deleteError) {
                                BOOL passed = YES;
                                if (isPositiveTest) {
                                    if (deleteError) {
                                        passed = NO;
                                        [test addLog:[NSString stringWithFormat:@"Error deleting item: %@", deleteError]];
                                    } else if (![itemId isEqualToNumber:deletedItemId]) {
                                        [test addLog:[NSString stringWithFormat:@"Invalid returned value after deleting: %@", deletedItemId]];
                                        passed = NO;
                                    }
                                } else {
                                    if (!deleteError) {
                                        passed = NO;
                                        [test addLog:[NSString stringWithFormat:@"Expected error, but delete succeeded for item: %@", deletedItemId]];
                                    } else if (type == NegDeleteObjectNoId) {
                                        if (deleteError.code != MSMissingItemIdWithRequest) {
                                            [test addLog:[NSString stringWithFormat:@"Unexpected error code: %d", deleteError.code]];
                                            passed = NO;
                                        }
                                    } else if (deleteError.code != MSErrorMessageErrorCode) {
                                        [test addLog:[NSString stringWithFormat:@"Unexpected error code: %d", deleteError.code]];
                                        passed = NO;
                                    } else {
                                        NSHTTPURLResponse *resp = [[deleteError userInfo] objectForKey:MSErrorResponseKey];
                                        if (resp.statusCode != 404) {
                                            [test addLog:[NSString stringWithFormat:@"Invalid response status code, expected 404, found %d", resp.statusCode]];
                                            passed = NO;
                                        }
                                    }
                                }
                                
                                if (isPositiveTest && passed) {
                                    [self validateDeletionForTest:test andTable:table andId:itemId withCompletion:completion];
                                } else {
                                    [test setTestStatus:(passed ? TSPassed : TSFailed)];
                                    completion(passed);
                                }
                            }];
                        }
                    }
                }];
            }
        }];
    }];
    
    return result;
}

+ (void)validateDeletionForTest:(ZumoTest *)test andTable:(MSTable *)table andId:(NSNumber *)itemId withCompletion:(ZumoTestCompletion)completion {
    [table readWithId:itemId completion:^(NSDictionary *item, NSError *err) {
        if (!err) {
            [test addLog:[NSString stringWithFormat:@"Delete failed, item %@ still exists: %@", itemId, item]];
        } else {
            [test addLog:@"Item deleted successfully"];
        }
        
        [test setTestStatus:(err ? TSPassed : TSFailed)];
        completion(err ? YES : NO);
    }];
}

+ (NSString *)helpText {
    NSArray *lines = [NSArray arrayWithObjects:
                      @"1. Create an application on Windows azure portal.",
                      @"2. Create TodoItem table in portal.",
                      @"3. Add Valid Application URL and Application Key.",
                      @"4. Run the 'Create/Update/Delete' tests.",
                      @"5. Make sure all the scenarios pass.", nil];
    return [lines componentsJoinedByString:@"\n"];
}

@end
