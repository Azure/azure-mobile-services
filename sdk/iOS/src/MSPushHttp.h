// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClientConnection.h"
#import "MSPush.h"

typedef void (^MSCreateRegistrationIdBlock)(NSString *registrationId, NSError *error);
typedef void (^MSListRegistrationsBlock)(NSArray *registrations, NSError *error);

@interface MSPushHttp : NSObject

- (MSPushHttp*)initWithClient:(MSClient *)client;

- (void)createRegistrationId:(MSCreateRegistrationIdBlock)completion;

- (void)upsertRegistration:(NSDictionary *)registration
                completion:(MSCompletionBlock)completion;

- (void)listRegistrations:(NSString *)deviceToken
               completion:(MSListRegistrationsBlock)completion;

- (void)deleteRegistration:(NSString *)registrationId
                completion:(MSCompletionBlock)completion;

@end
