// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"

#pragma  mark * MSClient Public Interface

@interface MSPush : NSObject

#pragma  mark * Public Initializer Methods

@property (nonatomic, strong, readonly) MSClient *client;

///@name Initializing the MSPush Object
///@{

/// Initialize the *MSPush* instance with given *MSClient*
- (MSPush *)initWithClient:(MSClient *)client;

/// @}

#pragma  mark * Public Native Registration Methods

/// @name Working with Registrations
/// @{

/// Register for notifications with given a deviceToken.
-(void)registerDeviceToken:(NSData *)deviceToken completion:(MSCompletionBlock)completion;

/// Register for notifications with given deviceToken and a template.
-(void)registerDeviceToken:(NSData *)deviceToken template:(NSDictionary *)template completion:(MSCompletionBlock)completion;

/// Unregister device from all notifications.
-(void)unregisterWithCompletion:(MSCompletionBlock)completion;

/// @}

@end
