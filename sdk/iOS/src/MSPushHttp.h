// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClientConnection.h"
#import "MSPush.h"

typedef void (^MSCreateRegistrationIdBlock)(NSString *registrationId, NSError *error);
typedef void (^MSListRegistrationsBlock)(NSArray *registrations, NSError *error);

@interface MSPushHttp : NSObject

/// Initialize an MSPushHttp instance with the specified MSClient.
- (MSPushHttp *)initWithClient:(MSClient *)client;

/// Make the http call to create a registrationId.
- (void)createRegistrationId:(MSCreateRegistrationIdBlock)completion;

/// Make the http call to upsert a registration.
- (void)upsertRegistration:(NSDictionary *)registration
                completion:(MSCompletionBlock)completion;

/// Make the http call to retrieve registrations for the specified deviceToken.
- (void)registrationsForDeviceToken:(NSString *)deviceToken
               completion:(MSListRegistrationsBlock)completion;

/// Make the http call to delete the registration having specified registrationId.
- (void)deleteRegistrationById:(NSString *)registrationId
                completion:(MSCompletionBlock)completion;

@end
