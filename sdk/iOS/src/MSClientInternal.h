// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSClient.h"
#import "MSSerializer.h"

@interface MSClient ()

// The serailizer to use with the client
@property (nonatomic, strong, readonly)         id<MSSerializer> serializer;
// The installation id used to track unique users of the sdk
@property (nonatomic, strong, readonly)         NSString* installId;
@end
