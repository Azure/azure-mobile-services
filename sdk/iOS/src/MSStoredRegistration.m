// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSStoredRegistration.h"

@implementation MSStoredRegistration

- (MSStoredRegistration*) initWithName:(NSString*)registrationName
                        registrationId:(NSString*)registrationId;
{
    if (!registrationName || !registrationId) {
        return nil;
    }
    
    self = [super init];
    
    if (self) {
        self.registrationName = registrationName;
        self.registrationId = registrationId;
    }
    
    return self;
}

@end
