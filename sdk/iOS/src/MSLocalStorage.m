// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSLocalStorage.h"

@implementation MSLocalStorage

@synthesize isRefreshNeeded, deviceToken;

NSString* _path;
NSMutableDictionary* _registrations;
NSString* _versionKey;
NSString* _deviceTokenKey;
NSString* _registrationsKey;

NSString* const storageVersion = @"v1.0.0";

- (MSLocalStorage*) initWithNotificationHubPath: (NSString*) mobileServiceUrl
{
    self = [super init];
    
    if(self){
        _path = mobileServiceUrl;
        
        _versionKey = [NSString stringWithFormat:@"%@-version", mobileServiceUrl];
        _deviceTokenKey = [NSString stringWithFormat:@"%@-deviceToken", mobileServiceUrl];
        _registrationsKey =[NSString stringWithFormat:@"%@-registrations", mobileServiceUrl];
        _registrations = [NSMutableDictionary dictionary];
        
        [self readContent];
    }
    
    return self;
}


- (MSStoredRegistration*) getMSStoredRegistrationWithRegistrationName:(NSString*) registrationName
{
    NSString* registrationId = [_registrations objectForKey:registrationName];
    if (registrationId)
    {
        return [[MSStoredRegistration alloc] initWithName:registrationName registrationId:registrationId];
    }
    else
    {
        return nil;
    }
}

- (void) updateWithRegistrationId: (NSString*) registrationId registrationName:(NSString *)registrationName deviceToken:(NSString *)deviceTokenIn
{
    for (NSString* key in [_registrations allKeys])
    {
        NSString* regId = _registrations[key];
        if([regId isEqualToString: registrationId])
        {
            if (![key isEqualToString:registrationName])
            {
                [self deleteWithRegistrationName:key];
            }
        }
    }
    
    [self updateWithRegistrationName:registrationName registrationId:registrationId deviceToken:deviceTokenIn];
}

- (void) updateWithRegistrationName: (NSString*) registrationName registrationId:(NSString *)registrationId deviceToken:(NSString *)deviceTokenIn
{
    [_registrations setObject:registrationId forKey:registrationName];
    self.deviceToken = deviceTokenIn;
    [self flush];
}

- (void) deleteWithRegistrationName: (NSString*) registrationName
{
    [_registrations removeObjectForKey:registrationName];
    [self flush];
}

- (void) deleteAllRegistrations
{
    [_registrations removeAllObjects];
    [self flush];
}

- (void) readContent
{
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    
    self.deviceToken = [defaults stringForKey:_deviceTokenKey];
    
    NSString* version = [defaults stringForKey:_versionKey];
    self.isRefreshNeeded = version == nil || ![version isEqualToString:storageVersion];
    if(self.isRefreshNeeded)
    {
        return;
    }
    
    NSDictionary* registrations = [defaults dictionaryForKey:_registrationsKey];
    if (registrations)
    {
        _registrations = [registrations mutableCopy];
    }
    else
    {
        self.isRefreshNeeded = YES;
    }
}

- (void) flush
{
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    
    [defaults setObject:self.deviceToken forKey:_deviceTokenKey];
    [defaults setObject:storageVersion forKey:_versionKey];
    [defaults setObject:_registrations forKey:_registrationsKey];
    
    [defaults synchronize];
}

- (void) refreshFinishedWithDeviceToken:(NSString*)newDeviceToken
{
    self.isRefreshNeeded = NO;
    
    if(![self.deviceToken isEqualToString:newDeviceToken])
    {
        self.deviceToken = newDeviceToken;
        [self flush];
    }
}

@end

