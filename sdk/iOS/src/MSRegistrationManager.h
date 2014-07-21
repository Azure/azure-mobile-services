// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"

/// MSRegistrationManager orchestrates the steps of registering and unregistering.
/// Its goal is to ensure that registrations are never duplicated and that
/// existing registrations for the current device can always be removed.
@interface MSRegistrationManager : NSObject

/// Initialize with the specified MSClient.
- (MSRegistrationManager *)initWithClient:(MSClient *)client;

/// Create or update the provided registration.
- (void)upsertRegistration:(NSMutableDictionary *)registration
                completion:(MSCompletionBlock)completion;

/// Delete the registration with the specified name.
- (void)deleteRegistrationWithName:(NSString *)registrationName
                        completion:(MSCompletionBlock)completion;

/// Delete all registrations for the specified deviceToken.
- (void)deleteAllWithDeviceToken:(NSString *)deviceToken
                      completion:(MSCompletionBlock)completion;

@end
