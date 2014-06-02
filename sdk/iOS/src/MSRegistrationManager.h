// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"

@interface MSRegistrationManager : NSObject

- (MSRegistrationManager *)initWithClient:(MSClient *)client;

- (void)upsertRegistration:(NSMutableDictionary *)registration
                completion:(MSCompletionBlock)completion;

- (void)deleteRegistrationWithName:(NSString *)registrationName
                             retry:(BOOL)retry
                        completion:(MSCompletionBlock)completion;

- (void)deleteAllWithDeviceToken:(NSString *)deviceToken
                      completion:(MSCompletionBlock)completion;

@end
