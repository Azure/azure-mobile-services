// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSStoredRegistration.h"

@interface MSLocalStorage : NSObject

@property (copy, nonatomic) NSString *deviceToken;
@property (nonatomic) BOOL isRefreshNeeded;

- (MSLocalStorage *) initWithNotificationHubPath:(NSString *)mobileServiceUrl;
- (void) refreshFinishedWithDeviceToken:(NSString *)newDeviceToken;
- (MSStoredRegistration *) getMSStoredRegistrationWithRegistrationName:(NSString *)registrationName;
- (void) updateWithRegistrationName:(NSString *)registrationName
                     registrationId:(NSString *)registrationId
                        deviceToken:(NSString *)deviceToken;
- (void) updateWithRegistrationId:(NSString *)registrationId
                 registrationName:(NSString *)registrationName
                      deviceToken:(NSString *)deviceToken;
- (void) deleteWithRegistrationName:(NSString*)registrationName;
- (void) deleteAllRegistrations;
@end
