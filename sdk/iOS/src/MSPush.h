// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"

#pragma  mark * MSClient Public Interface

@interface MSPush : NSObject

#pragma  mark * Public Initializer Methods

///@name Initializing the MSPush Object
///@{

/// Initialize the *MSPush* instance with given *MSClient*
- (MSPush *)initWithClient:(MSClient *)client;

/// @}

#pragma  mark * Public Native Registration Methods

/// @name Working with Native Registrations
/// @{

/// Register for notifications with given deviceToken and tags. deviceToken is required.
- (void)registerNativeWithDeviceToken:(NSData *)deviceToken
                                 tags:(NSArray *)tags
                           completion:(MSCompletionBlock)completion;

/// Unregister for native notififications.
- (void)unregisterNativeWithCompletion:(MSCompletionBlock)completion;

/// @}

#pragma  mark * Public Template Registration Methods

/// @name Working with Template Registrations
/// @{

/// Register for template notififications. deviceToken, name and bodyTemplate are required.
/// See http://go.microsoft.com/fwlink/?LinkId=401137 for furthur details.
- (void)registerTemplateWithDeviceToken:(NSData *)deviceToken
                                   name:(NSString *)name
                       jsonBodyTemplate:(NSString *)bodyTemplate
                         expiryTemplate:(NSString *)expiryTemplate
                                   tags:(NSArray *)tags
                             completion:(MSCompletionBlock)completion;

/// Unregister for tempalte notifications. name is required.
- (void)unregisterTemplateWithName:(NSString *)name
                        completion:(MSCompletionBlock)completion;

/// @}

#pragma  mark * Public Unregister All Registration Methods

/// @name Debug utilities--Unregistering All Registrations
/// @{

/// Unregister for all native and template registrations. deviceToken is required.
/// NOT RECOMMENDED FOR USE IN PRODUCTION APPLICATON.
- (void)unregisterAllWithDeviceToken:(NSData *)deviceToken
                          completion:(MSCompletionBlock)completion;

/// @}

@end
