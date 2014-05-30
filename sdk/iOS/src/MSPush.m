// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSPush.h"
#import "MSRegistrationManager.h"

@interface MSPush ()
@property (nonatomic, strong, readonly) MSRegistrationManager *registrationManager;
@end

@implementation MSPush
- (MSPush *)initWithClient:(MSClient *)client
{
    self = [super init];
    
    if (self) {
        _registrationManager = [[MSRegistrationManager alloc] init:client];
    }
    
    return self;
}

- (void)registerNativeWithDeviceToken:(NSData*)deviceToken
                                 tags:(NSArray*)tags
                           completion:(MSCompletionBlock)completion
{
    NSMutableDictionary *registration = [self createBaseRegistration:deviceToken
                                                                tags:tags
                                                                name:@"$Default"];
    [self.registrationManager upsertRegistration:registration
                                      completion:completion];
}

- (void)registerTemplateWithDeviceToken:(NSData*)deviceToken
                                    name:(NSString*)name
                        jsonBodyTemplate:(NSString*)bodyTemplate
                          expiryTemplate:(NSString*)expiryTemplate
                                   tags:(NSArray*)tags
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

- (void) unregisterNativeWithCompletion:(MSCompletionBlock)completion
{
    [self.registrationManager unregister:@"$Default"
                                   retry:YES
                              completion:completion];
}

- (void) unregisterTemplateWithName:(NSString*)name completion:(MSCompletionBlock)completion
{
    [self.registrationManager unregister:name
                                   retry:YES
                              completion:completion];
}

- (void) unregisterAllWithDeviceToken:(NSData*)deviceToken completion:(MSCompletionBlock)completion
{
    [self.registrationManager unregisterAllWithDeviceToken:[self convertDeviceToken:deviceToken]
                                                      completion:completion];
}

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
