// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

@interface MSLocalStorage : NSObject

@property (copy, nonatomic) NSString *deviceToken;
@property (nonatomic) BOOL isRefreshNeeded;

- (MSLocalStorage *)initWithMobileServiceHost:(NSString *)mobileServiceHost;
- (NSArray *)getRegistrationIds;
- (NSString *)getRegistrationId:(NSString *)registrationName;
- (void)updateRegistrations:(NSArray *)registrations
                deviceToken:(NSString *)deviceToken;
- (void)updateWithRegistrationName:(NSString *)registrationName
                    registrationId:(NSString *)registrationId
                       deviceToken:(NSString *)deviceToken;
- (void)deleteWithRegistrationName:(NSString*)registrationName;
- (void)deleteAllRegistrations;
@end
