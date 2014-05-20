// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSPush.h"

@interface MSPushHttp : NSObject

-(MSPushHttp*)init:(MSClient*)client;

-(void)createRegistrationId:(MSCreateRegistrationBlock)completion;

-(void)createRegistration:(NSDictionary *)registration
               completion:(MSCompletionBlock)completion;

-(void)listRegistrations:(NSString *)deviceToken
              completion:(MSListRegistrationsBlock)completion;

-(void)deleteRegistration:(NSString *)registrationId
               completion:(MSCompletionBlock)completion;

@end
