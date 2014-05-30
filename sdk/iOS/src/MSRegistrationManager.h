// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"

@interface MSRegistrationManager : NSObject
- (MSRegistrationManager *)init:(MSClient *)client;

- (void)upsertRegistration:(NSMutableDictionary *)registration
                completion:(MSCompletionBlock)completion;

- (void)unregister:(NSString *)registrationName
             retry:(BOOL)retry
        completion:(MSCompletionBlock)completion;

- (void)unregisterAllWithDeviceToken:(NSString *)deviceToken
                          completion:(MSCompletionBlock)completion;
@end
