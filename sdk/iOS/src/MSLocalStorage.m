// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSLocalStorage.h"

NSString *const MSNativeRegistrationName = @"$Default";

static NSString *const storageVersion = @"v1.0.0";

@interface MSLocalStorage ()
@property (nonatomic) NSMutableDictionary *registrations;
@property (copy, nonatomic) NSString *versionKey;
@property (copy, nonatomic) NSString *deviceTokenKey;
@property (copy, nonatomic) NSString *registrationsKey;
@property (nonatomic, readwrite) BOOL isRefreshNeeded;
@property (copy, nonatomic, readwrite) NSString *deviceToken;
@end

@implementation MSLocalStorage

- (MSLocalStorage *)initWithMobileServiceHost:(NSString *)mobileServiceHost
{
    self = [super init];
    
    if (self) {
        self.versionKey = [NSString stringWithFormat:@"%@-version", mobileServiceHost];
        self.deviceTokenKey = [NSString stringWithFormat:@"%@-deviceToken", mobileServiceHost];
        self.registrationsKey = [NSString stringWithFormat:@"%@-registrations", mobileServiceHost];
        self.registrations = [NSMutableDictionary dictionary];
        
        [self readContent];
    }
    
    return self;
}

- (NSString *)getRegistrationIdWithName:(NSString *)registrationName
{
    return [self.registrations objectForKey:registrationName];
}

- (NSArray *)getRegistrationIds
{
    return [self.registrations allValues];
}

- (void)updateRegistrationWithName:(NSString *)registrationName
                    registrationId:(NSString *)registrationId
                       deviceToken:(NSString *)deviceToken
{
    NSString *name = registrationName;
    if (!name) {
        name = MSNativeRegistrationName;
    }
    
    [self.registrations setObject:registrationId forKey:name];
    self.deviceToken = deviceToken;
    [self commitDefaults];
}

- (void)updateRegistrations:(NSArray *)registrations
                deviceToken:(NSString *)deviceToken
{
    self.registrations = [[NSMutableDictionary alloc] init];
    
    for (int i = 0; i < [registrations count]; i++) {
        NSString *name = registrations[i][@"templateName"];
        if (!name) {
            name = MSNativeRegistrationName;
        }
        
        /// All registrations passed to this method will have registrationId as they
        /// come directly from notification hub where registrationId is the key field.
        [self.registrations setObject:registrations[i][@"registrationId"] forKey:name];
    }
    
    self.deviceToken  = deviceToken;
    
    [self commitDefaults];
    
    self.isRefreshNeeded = NO;
}

- (void)deleteRegistrationWithName:(NSString *)registrationName
{
    [self.registrations removeObjectForKey:registrationName];
    [self commitDefaults];
}

- (void)deleteAllRegistrations
{
    [self.registrations removeAllObjects];
    [self commitDefaults];
}

- (void)readContent
{
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    
    self.deviceToken = [defaults stringForKey:self.deviceTokenKey];
    
    NSString *version = [defaults stringForKey:self.versionKey];
    self.isRefreshNeeded = version == nil || ![version isEqualToString:storageVersion];
    if(self.isRefreshNeeded) {
        return;
    }
    
    NSDictionary *registrations = [defaults dictionaryForKey:self.registrationsKey];
    if (registrations) {
        self.registrations = [registrations mutableCopy];
    } else {
        self.isRefreshNeeded = YES;
    }
}

- (void)commitDefaults
{
    [self commitDefaultsWithStorageVersion:storageVersion];
}

- (void)commitDefaultsWithStorageVersion:(NSString *)storageVersion
{
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    
    [defaults setObject:self.deviceToken forKey:self.deviceTokenKey];
    [defaults setObject:storageVersion forKey:self.versionKey];
    [defaults setObject:self.registrations forKey:self.registrationsKey];
    
    [defaults synchronize];
}

@end