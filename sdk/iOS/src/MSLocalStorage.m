// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSLocalStorage.h"

@interface MSLocalStorage ()
@property (nonatomic) NSMutableDictionary *registrations;
@property (copy, nonatomic) NSString *versionKey;
@property (copy, nonatomic) NSString *deviceTokenKey;
@property (copy, nonatomic) NSString *registrationsKey;
@end

@implementation MSLocalStorage

NSString * const storageVersion = @"v1.0.0";

- (MSLocalStorage *) initWithNotificationHubPath:(NSString *) mobileServiceUrl
{
    self = [super init];
    
    if (self) {
        self.versionKey = [NSString stringWithFormat:@"%@-version", mobileServiceUrl];
        self.deviceTokenKey = [NSString stringWithFormat:@"%@-deviceToken", mobileServiceUrl];
        self.registrationsKey = [NSString stringWithFormat:@"%@-registrations", mobileServiceUrl];
        self.registrations = [NSMutableDictionary dictionary];
        
        [self readContent];
    }
    
    return self;
}


- (MSStoredRegistration *) getMSStoredRegistrationWithRegistrationName:(NSString *)registrationName
{
    NSString * registrationId = [self.registrations objectForKey:registrationName];
    return [[MSStoredRegistration alloc] initWithName:registrationName registrationId:registrationId];
}


// Can we chop this?
- (void) updateWithRegistrationId:(NSString *)registrationId
                 registrationName:(NSString *)registrationName
                      deviceToken:(NSString *)deviceToken
{
    for (NSString *key in [self.registrations allKeys])
    {
        NSString *regId = self.registrations[key];
        if([regId isEqualToString: registrationId])
        {
            if (![key isEqualToString:registrationName])
            {
                [self deleteWithRegistrationName:key];
            }
        }
    }
    
    [self updateWithRegistrationName:registrationName
                      registrationId:registrationId
                         deviceToken:deviceToken];
}

- (void) updateWithRegistrationName:(NSString *)registrationName
                     registrationId:(NSString *)registrationId
                        deviceToken:(NSString *)deviceToken
{
    [self.registrations setObject:registrationId forKey:registrationName];
    self.deviceToken = deviceToken;
    [self flush];
}

- (void) deleteWithRegistrationName:(NSString *)registrationName
{
    [self.registrations removeObjectForKey:registrationName];
    [self flush];
}

- (void) deleteAllRegistrations
{
    [self.registrations removeAllObjects];
    [self flush];
}

- (void) readContent
{
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    
    self.deviceToken = [defaults stringForKey:self.deviceTokenKey];
    
    NSString *version = [defaults stringForKey:self.versionKey];
    self.isRefreshNeeded = version == nil || ![version isEqualToString:storageVersion];
    if(self.isRefreshNeeded)
    {
        return;
    }
    
    NSDictionary *registrations = [defaults dictionaryForKey:self.registrationsKey];
    if (registrations)
    {
        self.registrations = [registrations mutableCopy];
    }
    else
    {
        self.isRefreshNeeded = YES;
    }
}

- (void) flush
{
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    
    [defaults setObject:self.deviceToken forKey:self.deviceTokenKey];
    [defaults setObject:storageVersion forKey:self.versionKey];
    [defaults setObject:self.registrations forKey:self.registrationsKey];
    
    [defaults synchronize];
}

- (void) refreshFinishedWithDeviceToken:(NSString *)newDeviceToken
{
    self.isRefreshNeeded = NO;
    
    if(![self.deviceToken isEqualToString:newDeviceToken])
    {
        self.deviceToken = newDeviceToken;
        [self flush];
    }
}

@end