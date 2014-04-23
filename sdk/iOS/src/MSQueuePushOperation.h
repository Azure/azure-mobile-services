// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSBookmarkOperation.h"
#import "MSSyncContext.h"

/// Performs all actions associated with a push operation including, sending each operation to
/// the server, processing errors, and triggering the appropriate calls to the delegate, datasource,
/// and callbacks
@interface MSQueuePushOperation : NSOperation {
    BOOL executing_;
    BOOL finished_;
}

- (id) initWithPushOperation:(MSBookmarkOperation *)pushOperation
                 syncContext:(MSSyncContext *)syncContext
               dispatchQueue:(dispatch_queue_t)dispatchQueue
                  completion:(MSSyncBlock)completion;

@end