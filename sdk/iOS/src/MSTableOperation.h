// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSTableOperation.h"

/// The *MSTableOperation* object represents a pending operation that was created by an earlier
/// call using the *MSSyncTable* object. This is a wrapper to facilitae sending the operation
/// to the server, handling any errors, and getting the appropriate local versions updated on
/// completion
@interface MSTableOperation : NSObject

/// The types of operations possible to perform.
typedef NS_OPTIONS(NSUInteger, MSTableOperationTypes) {
    MSTableOperationInsert = 0,
    MSTableOperationUpdate,
    MSTableOperationDelete
};

#pragma mark * Public Readonly Properties

///@name Properties
///@{

/// The action that should be taken for this table item, for example
/// insert or update.
@property (nonatomic, readonly) MSTableOperationTypes type;

/// The name of the table associated with the item
@property (nonatomic, copy, readonly, nonnull) NSString *tableName;

/// The Id of the item the operation should run on.
@property (nonatomic, copy, readonly, nonnull) NSString *itemId;

/// The item that will be sent to the server when execute is called.
@property (nonatomic, strong, nullable) NSDictionary *item;

/// @}

///@name Sending an operation to the Mobile Service
///@{

/// Perform's the associated PushOperationType (insert, etc) for the table item.
/// The callback will be passed the result (an item on insert/update, and the string
/// id on a delete) or the error from the mobile service.
-(void) executeWithCompletion:(nullable void(^)(id __nonnull itemOrItemId, NSError *__nullable error))completion;

/// @}

/// @name Canceling a Push operation
/// @{
- (void) cancelPush;

/// @}

@end
