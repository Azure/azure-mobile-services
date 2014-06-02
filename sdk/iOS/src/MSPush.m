// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSPush.h"
#import "MSRegistrationManager.h"
#import "MSLocalStorage.h"

@interface MSPush ()
@property (nonatomic, strong, readonly) MSRegistrationManager *registrationManager;
@end

#pragma mark * MSPush Implementation

@implementation MSPush

#pragma  mark * Public Initializer Methods

- (MSPush *)initWithClient:(MSClient *)client
{
    self = [super init];
    
    if (self) {
        _registrationManager = [[MSRegistrationManager alloc] initWithClient:client];
    }
    
    return self;
}

#pragma  mark * Public Native Registration Methods

- (void)registerNativeWithDeviceToken:(NSData *)deviceToken
                                 tags:(NSArray *)tags
                           completion:(MSCompletionBlock)completion
{
    NSMutableDictionary *registration = [self createBaseRegistration:deviceToken
                                                                tags:tags
                                                                name:NativeRegistrationName];
    [self.registrationManager upsertRegistration:registration
                                      completion:completion];
}

- (void) unregisterNativeWithCompletion:(MSCompletionBlock)completion
{
    [self.registrationManager deleteRegistrationWithName:NativeRegistrationName
                                   retry:YES
                              completion:completion];
}

#pragma  mark * Public Template Registration Methods

- (void)registerTemplateWithDeviceToken:(NSData *)deviceToken
                                   name:(NSString *)name
                       jsonBodyTemplate:(NSString *)bodyTemplate
                         expiryTemplate:(NSString *)expiryTemplate
                                   tags:(NSArray *)tags
                             completion:(MSCompletionBlock)completion
{
    NSMutableDictionary *registration = [self createBaseRegistration:deviceToken
                                                                tags:tags
                                                                name:name];
    [registration setValue:name forKey:@"templateName"];
    [registration setValue:bodyTemplate forKey:@"templateBody"];
    [registration setValue:expiryTemplate forKey:@"expiry"];
    
    [self.registrationManager upsertRegistration:registration
                                      completion:completion];
}

- (void) unregisterTemplateWithName:(NSString *)name completion:(MSCompletionBlock)completion
{
    [self.registrationManager deleteRegistrationWithName:name
                                   retry:YES
                              completion:completion];
}

#pragma  mark * Public Unregister All Registration Methods

- (void) unregisterAllWithDeviceToken:(NSData *)deviceToken completion:(MSCompletionBlock)completion
{
    [self.registrationManager deleteAllWithDeviceToken:[self convertDeviceToken:deviceToken]
                                                      completion:completion];
}

#pragma  mark * Private Methods

- (NSString *)convertDeviceToken:(NSData *)deviceTokenData
{
    NSString* newDeviceToken = [[[[[deviceTokenData description]
                                   stringByReplacingOccurrencesOfString:@"<"withString:@""]
                                  stringByReplacingOccurrencesOfString:@">" withString:@""]
                                 stringByReplacingOccurrencesOfString: @" " withString: @""] uppercaseString];
    return newDeviceToken;
}

- (NSMutableDictionary *)createBaseRegistration:(NSData *)deviceToken
                                           tags:(NSArray *)tags
                                           name:(NSString *)name
{
    NSMutableDictionary *registration = [[NSMutableDictionary alloc] init];
    [registration setValue:[self convertDeviceToken:deviceToken] forKey:@"deviceId"];
    if (tags) {
        [registration setValue:tags forKey:@"tags"];
    }
    [registration setValue:@"apns" forKey:@"platform"];
    [registration setValue:name forKey:@"name"];
    return registration;
}

@end