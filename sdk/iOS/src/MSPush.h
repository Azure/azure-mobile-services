// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"

@interface MSPush : NSObject
- (MSPush *)initWithClient:(MSClient *)client;

- (void)registerNativeWithDeviceToken:(NSData*)deviceToken
                                 tags:(NSArray*)tags
                           completion:(MSCompletionBlock)completion;

- (void)registerTemplateWithDeviceToken:(NSData*)deviceToken
                                   name:(NSString*)name
                       jsonBodyTemplate:(NSString*)bodyTemplate
                         expiryTemplate:(NSString*)expiryTemplate
                                   tags:(NSArray*)tags
                             completion:(MSCompletionBlock)completion;

- (void)unregisterNativeWithCompletion:(MSCompletionBlock)completion;

- (void)unregisterTemplateWithName:(NSString*)name
                        completion:(MSCompletionBlock)completion;

- (void)unregisterAllWithDeviceToken:(NSData*)deviceToken
                           completion:(MSCompletionBlock)completion;
@end
