// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"

typedef void (^MSCreateRegistrationBlock)(NSString *registrationId, NSError *error);
typedef void (^MSListRegistrationsBlock)(NSArray *registrations, NSError *error);

@interface MSPush : NSObject

@end
