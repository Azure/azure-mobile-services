// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

@interface MSStoredRegistration : NSObject

@property (copy, nonatomic) NSString* registrationName;
@property (copy, nonatomic) NSString* registrationId;

- (MSStoredRegistration*) initWithName:(NSString*)registrationName registrationId: (NSString*)registrationId;
@end
