// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSStoredRegistration.h"

@implementation MSStoredRegistration
@synthesize registrationName, registrationId;

- (MSStoredRegistration*) initWithName:(NSString*)regName registrationId: (NSString*)regId;
{
    self = [super init];
    
    if(self){
        self.registrationName = regName;
        self.registrationId = regId;
    }
    
    return self;
}

@end
