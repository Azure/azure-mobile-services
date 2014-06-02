// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

extern NSString * const NativeRegistrationName;

@interface MSLocalStorage : NSObject

@property (copy, nonatomic, readonly) NSString *deviceToken;
@property (nonatomic, readonly) BOOL isRefreshNeeded;

- (MSLocalStorage *)initWithMobileServiceHost:(NSString *)mobileServiceHost;

- (NSArray *)getRegistrationIds;

- (NSString *)getRegistrationIdWithName:(NSString *)registrationName;

- (void)updateRegistrations:(NSArray *)registrations
                deviceToken:(NSString *)deviceToken;

- (void)updateRegistrationWithName:(NSString *)registrationName
                    registrationId:(NSString *)registrationId
                       deviceToken:(NSString *)deviceToken;

- (void)deleteRegistrationWithName:(NSString *)registrationName;

- (void)deleteAllRegistrations;

- (void)corruptDefaults;

@end
