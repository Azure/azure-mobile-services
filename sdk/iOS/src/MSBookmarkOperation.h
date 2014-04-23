// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSOperationQueue.h"

// Internal class used as a placholder to track where a push started
@interface MSBookmarkOperation : NSObject

#pragma mark * Public Readonly Properties

// unique id to identify the push operation
@property (nonatomic, strong, readonly) NSString *guid;

@end
