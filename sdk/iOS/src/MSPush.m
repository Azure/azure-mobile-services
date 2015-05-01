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
    if (!deviceToken) {
        if (completion) {
            completion([self errorForMissingParameterWithParameterName:@"deviceToken"]);
        }
        return;
    }
    
    NSMutableDictionary *registration = [self createBaseRegistration:deviceToken
                                                                tags:tags];
    [self.registrationManager upsertRegistration:registration
                                      completion:completion];
}

- (void) unregisterNativeWithCompletion:(MSCompletionBlock)completion
{
    [self.registrationManager deleteRegistrationWithName:MSNativeRegistrationName
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
    if (!deviceToken) {
        if (completion) {
            completion([self errorForMissingParameterWithParameterName:@"deviceToken"]);
        }
        return;
    }
    
    if (!name) {
        if (completion) {
            completion([self errorForMissingParameterWithParameterName:@"name"]);
        }
        return;
    }
    
    if (!bodyTemplate) {
        if (completion) {
            completion([self errorForMissingParameterWithParameterName:@"bodyTemplate"]);
        }
        return;
    }
    
    NSMutableDictionary *registration = [self createBaseRegistration:deviceToken
                                                                tags:tags];
    [registration setValue:name forKey:@"templateName"];
    [registration setValue:bodyTemplate forKey:@"templateBody"];
    [registration setValue:expiryTemplate forKey:@"expiry"];
    
    [self.registrationManager upsertRegistration:registration
                                      completion:completion];
}

- (void) unregisterTemplateWithName:(NSString *)name completion:(MSCompletionBlock)completion
{
    if (!name) {
        if (completion) {
            completion([self errorForMissingParameterWithParameterName:@"name"]);
        }
        return;
    }
    
    [self.registrationManager deleteRegistrationWithName:name
                              completion:completion];
}

#pragma  mark * Public Unregister All Registration Methods

- (void) unregisterAllWithDeviceToken:(NSData *)deviceToken completion:(MSCompletionBlock)completion
{
    if (!deviceToken) {
        if (completion) {
            completion([self errorForMissingParameterWithParameterName:@"deviceToken"]);
        }
        return;
    }
    
    [self.registrationManager deleteAllWithDeviceToken:[self convertDeviceToken:deviceToken]
                                                      completion:completion];
}

#pragma  mark * Private Methods

- (NSString *)convertDeviceToken:(NSData *)deviceTokenData
{
    NSCharacterSet *hexFormattingCharacters = [NSCharacterSet characterSetWithCharactersInString:@"<>"];
    NSString* newDeviceToken = [[[[deviceTokenData description]
                                 stringByTrimmingCharactersInSet:hexFormattingCharacters]
                                stringByReplacingOccurrencesOfString:@" " withString:@""]
                                uppercaseString];
    return newDeviceToken;
}

- (NSMutableDictionary *)createBaseRegistration:(NSData *)deviceToken
                                           tags:(NSArray *)tags
{
    NSMutableDictionary *registration = [[NSMutableDictionary alloc] init];
    [registration setValue:[self convertDeviceToken:deviceToken] forKey:@"deviceId"];
    if (tags) {
        [registration setValue:tags forKey:@"tags"];
    }
    [registration setValue:@"apns" forKey:@"platform"];
    return registration;
}

-(NSError *) errorForMissingParameterWithParameterName:(NSString *)parameterName
{
    NSString *descriptionKey = @"'%@' is a required parameter.";
    NSString *descriptionFormat = NSLocalizedString(descriptionKey, nil);
    NSString *description = [NSString stringWithFormat:descriptionFormat, parameterName];
    NSDictionary *userInfo = @{ NSLocalizedDescriptionKey :description };
    
    return [NSError errorWithDomain:MSErrorDomain
                               code:MSPushRequiredParameter
                           userInfo:userInfo];
}

@end