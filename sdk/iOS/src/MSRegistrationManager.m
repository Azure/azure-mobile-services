// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSRegistrationManager.h"
#import "MSLocalStorage.h"
#import "MSPushHttp.h"

@interface MSRegistrationManager ()
@property (nonatomic, strong, readonly) MSLocalStorage *storage;
@property (nonatomic, strong, readonly) MSPushHttp *pushHttp;
@end

@implementation MSRegistrationManager

- (MSRegistrationManager *)initWithClient:(MSClient *)client
{
    self = [super init];
    
    if (self) {
        _storage = [[MSLocalStorage alloc] initWithMobileServiceHost:client.applicationURL.host];
        _pushHttp = [[MSPushHttp alloc] initWithClient:client];
    }
    
    return self;
}

- (void)upsertRegistration:(NSMutableDictionary *)registration
                completion:(MSCompletionBlock)completion
{
    if (self.storage.isRefreshNeeded)
    {
        [self refreshRegistrations:registration[@"deviceId"] completion:^(NSError *error) {
            if (error) {
                if (completion) {
                    completion(error);
                }
                return;
            }
            
            [self firstRegistration:registration completion:completion];
        }];
    } else {
        [self firstRegistration:registration completion:completion];
    }
}

- (void)refreshRegistrations:(NSString *)deviceToken
                  completion:(MSCompletionBlock)completion
{
    NSString *refreshDeviceToken;
    if (!self.storage.deviceToken || [self isWhitespace:self.storage.deviceToken]) {
        refreshDeviceToken = deviceToken;
    } else {
        refreshDeviceToken = self.storage.deviceToken;
    }
    
    if (!refreshDeviceToken) {
        if (completion) {
            completion([self errorForCorruptLocalStorage]);
        }
        return;
    }
    
    [self.pushHttp listRegistrations:refreshDeviceToken completion:^(NSArray *registrations, NSError *error) {
        if (!error) {
            [self.storage updateRegistrations:registrations deviceToken:refreshDeviceToken];
        }
        
        if (completion) {
            completion(error);
        }
    }];
}

- (void)firstRegistration:(NSMutableDictionary *)registration
               completion:(MSCompletionBlock)completion
{
    NSString *name = registration[@"templateName"];
    if (!name) {
        name = NativeRegistrationName;
    }
    
    NSString *cachedRegistrationId = [self.storage getRegistrationIdWithName:name];
    if (cachedRegistrationId) {
        [registration setValue:cachedRegistrationId forKey:@"registrationId"];
        [self upsertRegistrationCore:registration
                               retry:YES
                          completion:completion];
    } else {
        [self createRegistrationId:registration completion:^(NSError *error) {
            if (error) {
                if (completion) {
                    completion(error);
                }
                return;
            }
            
            [self upsertRegistrationCore:registration
                                   retry:YES
                              completion:completion];
        }];
    }
}

- (void)expiredRegistration:(NSMutableDictionary *)registration
                 completion:(MSCompletionBlock)completion
{
    [self createRegistrationId:registration completion:^(NSError *error) {
        if (error) {
            if (completion) {
                completion(error);
            }
            return;
        }
        
        [self upsertRegistrationCore:registration
                               retry:NO
                          completion:^(NSError *error) {
                              if (completion) {
                                  completion(error);
                              }
        }];
    }];
}

- (void)deleteRegistrationWithName:(NSString *)registrationName
                             retry:(BOOL)retry
                        completion:(MSCompletionBlock)completion
{
    NSString *cachedRegistrationId = [self.storage getRegistrationIdWithName:registrationName];
    if (!cachedRegistrationId) {
        if (retry)
        {
            [self refreshRegistrations:nil completion:^(NSError *error) {
                if (error) {
                    if (completion) {
                        completion(error);
                    }
                    return;
                }
                
                [self deleteRegistrationWithName:registrationName retry:NO completion:completion];
            }];
        } else if (completion) {
            completion(nil);
        }
        return;
    }
    
    // Corrupt local storage in case disconnect occurs in midst of operation
    [self.storage forceRefreshUntilSaved];
    [self.pushHttp deleteRegistration:cachedRegistrationId completion:^(NSError *error) {
        if (!error) {
            [self.storage deleteRegistrationWithName:registrationName];
        }

        if (completion) {
            completion(error);
        }
    }];
}

- (void)deleteAllWithDeviceToken:(NSString *)deviceToken
                      completion:(MSCompletionBlock)completion
{
    [self refreshRegistrations:deviceToken completion:^(NSError *error) {
        if (!error) {
            // Corrupt local storage in case disconnect occurs in midst of operation
            [self.storage forceRefreshUntilSaved];
            
            NSMutableArray *registrationIds = [[self.storage getRegistrationIds] mutableCopy];
            [self recursiveDelete:registrationIds completion:completion];
            return;
        }
        
        if (completion) {
            completion(error);
        }
    }];
}

- (void)recursiveDelete:(NSMutableArray *)registrationIds
             completion:(MSCompletionBlock)completion
{
    NSString *registrationId = registrationIds[registrationIds.count-1];
    [registrationIds removeLastObject];
    
    [self.pushHttp deleteRegistration:registrationId completion:^(NSError *error) {
        if (!error) {
            if (registrationIds.count > 0) {
                [self recursiveDelete:registrationIds completion:completion];
                return;
            }
            
            [self.storage deleteAllRegistrations];
        }

        if (completion) {
            completion(error);
        }
    }];
}

- (void)createRegistrationId:(NSMutableDictionary *)registration
                  completion:(MSCompletionBlock)completion
{
    [self.pushHttp createRegistrationId:^(NSString *registrationId, NSError *error) {
        if (!error) {
            [registration setValue:registrationId forKey:@"registrationId"];
        }
    
        if (completion) {
            completion(error);
        }
    }];
}

-(void)upsertRegistrationCore:(NSMutableDictionary *)registration
                        retry:(BOOL)retry
                   completion:(MSCompletionBlock)completion
{
    // Corrupt local storage in case disconnect occurs in midst of operation
    [self.storage forceRefreshUntilSaved];
    [self.pushHttp upsertRegistration:registration completion:^(NSError *error) {
        if (!error) {
            [self.storage updateRegistrationWithName:registration[@"templateName"]
                                      registrationId:registration[@"registrationId"]
                                         deviceToken:registration[@"deviceId"]];
        } else if (retry && [error.userInfo[MSErrorResponseKey] statusCode] == 410) {
            // evaluate if error has 410
            [self expiredRegistration:registration completion:completion];
            return;
        }
        
        if (completion) {
            completion(error);
        }
    }];
}

-(BOOL)isWhitespace:(NSString *)string{
    return [[string stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]] length] == 0;
}

-(NSError *) errorForCorruptLocalStorage
{
    NSDictionary *userInfo = @{ NSLocalizedDescriptionKey:@"Deleting registration name cannot be completed due to corrupt local storage. Recommend using registerNativeWithDeviceToken, registerTemplateWithDeviceToken or unregisterAllWithDeviceToken to rebuild local storage from service." };
    
    return [NSError errorWithDomain:MSErrorDomain
                               code:MSPushLocalStorageCorrupt
                           userInfo:userInfo];
}

@end