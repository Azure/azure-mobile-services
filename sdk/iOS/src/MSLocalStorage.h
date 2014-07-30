// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

extern NSString * const MSNativeRegistrationName;

@interface MSLocalStorage : NSObject

@property (copy, nonatomic, readonly) NSString *deviceToken;
@property (nonatomic, readonly) BOOL isRefreshNeeded;

/// Initialize *MSLocalStorage* using the host of the Mobile Service.
- (MSLocalStorage *)initWithMobileServiceHost:(NSString *)mobileServiceHost;

/// Get an NSArray of all registrationIds as NSString.
- (NSArray *)getRegistrationIds;

/// Get a registrationId for the specific registrationName.
- (NSString *)getRegistrationIdWithName:(NSString *)registrationName;

/// Replace all registrations and the deviceToken with those passed in.
- (void)updateRegistrations:(NSArray *)registrations
                deviceToken:(NSString *)deviceToken;

/// Upsert the registrationId of the individual registration by name and update
/// the deviceToken.
- (void)updateRegistrationWithName:(NSString *)registrationName
                    registrationId:(NSString *)registrationId
                       deviceToken:(NSString *)deviceToken;

/// Delete the specified registration name.
- (void)deleteRegistrationWithName:(NSString *)registrationName;

/// Delete all registrations.
- (void)deleteAllRegistrations;

@end