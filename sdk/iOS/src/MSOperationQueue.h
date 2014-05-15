// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSTableOperation.h"
#import "MSClient.h"

/// A simple queue interface to abstract access from implementation. For now this may just
/// be an NSArray but long term this is liable to change
@interface MSOperationQueue : NSObject

/// Creates a MSOperationQueue
- (id) initWithClient:(MSClient *)client dataSource:(id<MSSyncContextDataSource>)dataSource;

/// Adds a table operation to the queue
-(void) addOperation:(MSTableOperation *)operation orError:(NSError **)error;

/// Removes a given operation from the queue
-(void) removeOperation:(MSTableOperation *)operation orError:(NSError **)error;

/// Gets a list of all operations in the queue for a given table (and optionally item)
-(NSArray *) getOperationsForTable:(NSString *) table item:(NSString *)item;

/// Returns the topmost operation
-(id) peek;

/// Get the next operation on the queue, skipping any before the given operation.
-(id) getOperationAfter:(NSInteger)operationId;

/// Returns a count of the total pending operations
-(NSUInteger) count;

/// Returns the next operation id available (based on all current operations)
-(NSInteger) getNextOperationId;

/// Marks an operation as being in use and no other process should update it
- (BOOL) lockOperation:(MSTableOperation *)operation;

/// Removes the lock on the operation
- (BOOL) unlockOperation:(MSTableOperation *)operation;

@end
