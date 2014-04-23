// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSBookmarkOperation.h"

@implementation MSBookmarkOperation

@synthesize guid = guid_;
- (NSString *) guid {
    if (!guid_) {
        CFUUIDRef newUUID = CFUUIDCreate(kCFAllocatorDefault);
        guid_ = (__bridge_transfer NSString *)CFUUIDCreateString(kCFAllocatorDefault, newUUID);
        CFRelease(newUUID);
    }
    
    return guid_;
}
@end
