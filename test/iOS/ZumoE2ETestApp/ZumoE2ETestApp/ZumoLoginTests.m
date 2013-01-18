//
//  ZumoLoginTests.m
//  ZumoE2ETestApp
//
//  Copyright (c) 2012 Microsoft. All rights reserved.
//

#import "ZumoLoginTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>

@implementation ZumoLoginTests

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    [result addObject:[self createClearAuthCookiesTest]];
    [result addObject:[self createLogoutTest]];
    [result addObject:[self createCRUDTestForProvider:nil forTable:@"iosApplication" ofType:ZumoTableApplication andAuthenticated:NO]];
    [result addObject:[self createCRUDTestForProvider:nil forTable:@"iosAuthenticated" ofType:ZumoTableAuthenticated andAuthenticated:NO]];
    [result addObject:[self createCRUDTestForProvider:nil forTable:@"iosAdmin" ofType:ZumoTableAdminScripts andAuthenticated:NO]];
    
    NSArray *providers = [NSArray arrayWithObjects:@"facebook", @"google", @"twitter", @"microsoftaccount", nil];
    NSString *provider;
    
    for (int useSimplifiedLogin = 0; useSimplifiedLogin <= 1; useSimplifiedLogin++) {
        for (provider in providers) {
            BOOL useSimplified = useSimplifiedLogin == 1;
            [result addObject:[self createLogoutTest]];
            [result addObject:[self createLoginTestForProvider:provider usingSimplifiedMode:useSimplified]];
            [result addObject:[self createCRUDTestForProvider:provider forTable:@"iosApplication" ofType:ZumoTableApplication andAuthenticated:YES]];
            [result addObject:[self createCRUDTestForProvider:provider forTable:@"iosAuthenticated" ofType:ZumoTableAuthenticated andAuthenticated:YES]];
            [result addObject:[self createCRUDTestForProvider:provider forTable:@"iosAdmin" ofType:ZumoTableAdminScripts andAuthenticated:YES]];
        }
    }
    
    return result;
}

typedef enum { ZumoTableUnauthenticated, ZumoTableApplication, ZumoTableAuthenticated, ZumoTableAdminScripts } ZumoTableType;

+ (ZumoTest *)createCRUDTestForProvider:(NSString *)providerName forTable:(NSString *)tableName ofType:(ZumoTableType)tableType andAuthenticated:(BOOL)isAuthenticated {
    NSString *tableTypeName;
    switch (tableType) {
        case ZumoTableAdminScripts:
            tableTypeName = @"admin";
            break;
            
        case ZumoTableApplication:
            tableTypeName = @"application";
            break;
            
        case ZumoTableAuthenticated:
            tableTypeName = @"authenticated users";
            break;
            
        default:
            tableTypeName = @"public";
            break;
    }
    
    if (!providerName) {
        providerName = @"no";
    }
    
    NSString *testName = [NSString stringWithFormat:@"CRUD, %@ auth, table with %@ permissions", providerName, tableTypeName];
    BOOL crudShouldWork = tableType == ZumoTableUnauthenticated || tableType == ZumoTableApplication || (tableType == ZumoTableAuthenticated && isAuthenticated);
    ZumoTest *result = [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client getTable:tableName];
        [table insert:@{@"foo":@"bar"} completion:^(NSDictionary *inserted, NSError *insertError) {
            if (![self validateCRUDResultForTest:test andOperation:@"Insert" andError:insertError andExpected:crudShouldWork]) {
                completion(NO);
                return;
            }
            
            NSDictionary *toUpdate = crudShouldWork ? inserted : @{@"foo":@"bar",@"id":[NSNumber numberWithInt:1]};
            [table update:toUpdate completion:^(NSDictionary *updated, NSError *updateError) {
                if (![self validateCRUDResultForTest:test andOperation:@"Update" andError:updateError andExpected:crudShouldWork]) {
                    completion(NO);
                    return;
                }
                
                NSNumber *itemId = crudShouldWork ? [inserted objectForKey:@"id"] : [NSNumber numberWithInt:1];
                [table readWithId:itemId completion:^(NSDictionary *read, NSError *readError) {
                    if (![self validateCRUDResultForTest:test andOperation:@"Read" andError:readError andExpected:crudShouldWork]) {
                        completion(NO);
                        return;
                    }
                    
                    [table deleteWithId:itemId completion:^(NSNumber *deletedId, NSError *deleteError) {
                        if (![self validateCRUDResultForTest:test andOperation:@"Delete" andError:deleteError andExpected:crudShouldWork]) {
                            completion(NO);
                        } else {
                            [test setTestStatus:TSPassed];
                            [test addLog:@"Validation succeeded for all operations"];
                            completion(YES);
                        }
                    }];
                }];
            }];
        }];
    }];
    return result;
}

+ (BOOL)validateCRUDResultForTest:(ZumoTest *)test andOperation:(NSString *)operation andError:(NSError *)error andExpected:(BOOL)shouldSucceed {
    if (shouldSucceed == (error == nil)) {
        if (error) {
            NSHTTPURLResponse *resp = [[error userInfo] objectForKey:MSErrorResponseKey];
            if (resp.statusCode == 401) {
                [test addLog:[NSString stringWithFormat:@"Got expected response code for operation %@: %d", operation, resp.statusCode]];
                return YES;
            } else {
                [test addLog:[NSString stringWithFormat:@"Got invalid response code for operation %@: %d", operation, resp.statusCode]];
                return NO;
            }
        } else {
            return YES;
        }
    } else {
        [test addLog:[NSString stringWithFormat:@"Should%@ succeed for %@, but error = %@", (shouldSucceed ? @"" : @" not"), operation, error]];
        [test setTestStatus:TSFailed];
        return NO;
    }
}

+ (ZumoTest *)createLoginTestForProvider:(NSString *)provider usingSimplifiedMode:(BOOL)useSimplified {
    ZumoTest *result = [ZumoTest createTestWithName:[NSString stringWithFormat:@"%@Login for %@", useSimplified ? @"Simplified " : @"", provider] andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        BOOL shouldDismissControllerInBlock = !useSimplified;
        MSClientLoginBlock loginBlock = ^(MSUser *user, NSError *error) {
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error logging in: %@", error]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                [test addLog:[NSString stringWithFormat:@"Logged in as %@", [user userId]]];
                [test setTestStatus:TSPassed];
                completion(YES);
            }
            
            if (shouldDismissControllerInBlock) {
                [viewController dismissViewControllerAnimated:YES completion:nil];
            }
        };
        
        if (useSimplified) {
            [client loginWithProvider:provider onController:viewController animated:YES completion:loginBlock];
        } else {
            UIViewController *loginController = [client loginViewControllerWithProvider:provider completion:loginBlock];
            [viewController presentViewController:loginController animated:YES completion:nil];
        }
    }];
    
    return result;
}

+ (ZumoTest *)createClearAuthCookiesTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Clear login cookies" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        NSHTTPCookieStorage *cookieStorage = [NSHTTPCookieStorage sharedHTTPCookieStorage];
        NSPredicate *isAuthCookie = [NSPredicate predicateWithFormat:@"domain ENDSWITH '.facebook.com' or domain ENDSWITH '.google.com' or domain ENDSWITH '.live.com' or domain ENDSWITH '.twitter.com'"];
        NSArray *cookiesToRemove = [[cookieStorage cookies] filteredArrayUsingPredicate:isAuthCookie];
        for (NSHTTPCookie *cookie in cookiesToRemove) {
            NSLog(@"Removed cookie from %@", [cookie domain]);
            [cookieStorage deleteCookie:cookie];
        }

        [test addLog:@"Removed authentication-related cookies from this app."];
        completion(YES);
    }];

    return result;
}

+ (ZumoTest *)createLogoutTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Logout" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        [client logout];
        [test addLog:@"Logged out"];
        MSUser *loggedInUser = [client currentUser];
        if (loggedInUser == nil) {
            [test setTestStatus:TSPassed];
            completion(YES);
        } else {
            [test addLog:[NSString stringWithFormat:@"Error, user for client is not null: %@", loggedInUser]];
            [test setTestStatus:TSFailed];
            completion(NO);
        }
    }];
    
    return result;
}

+ (NSString *)helpText {
    NSArray *lines = [NSArray arrayWithObjects:
                      @"1. Create an application on Windows azure portal.",
                      @"2. Create three tables in the application:",
                      @"2.1. iosApplication (set permissions to 'Application Key'):",
                      @"2.2. iosAuthenticated (set permissions to 'Authenticated Users'):",
                      @"2.3. iosAdmin (set permissions to 'Admin and Scripts'):",
                      @"3. Create applications in all supported identity providers",
                      @"4. Configure the identity tab of the Zumo app to point to the providers",
                      @"5. Run the 'Login' tests, entering valid credentials when prompted.",
                      @"6. Make sure all the scenarios pass.", nil];
    return [lines componentsJoinedByString:@"\n"];
}

@end
