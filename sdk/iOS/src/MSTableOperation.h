// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSUser.h"

/// The *MSTableOperation* object represents a pending operation that was created by an earlier
/// call using the *MSSyncTable* object. This is a wrapper to facilitae sending the operation
/// to the server, handling any errors, and getting the appropriate local versions updated on
/// completion
@interface MSTableOperation : NSObject

typedef NS_OPTIONS(NSUInteger, MSTableOperationTypes) {
    MSTableOperationInsert = 0,
    MSTableOperationUpdate,
    MSTableOperationDelete
};

#pragma mark * Public Readonly Properties

///@name Properties
///@{

/// The asociated table action that should be take on this table item, for example
/// insert or update.
@property (nonatomic, readonly) MSTableOperationTypes type;

/// The name of the table associated with the item
@property (nonatomic, copy, readonly) NSString *tableName;

/// The Id of the item the operation should run on.
@property (nonatomic, copy, readonly) NSString *itemId;

/// @}

///@name Sending an operation to the Mobile Service
///@{

/// Perform's the associated PushOperationType (table insert, etc) for the associated
/// table item. The callback will be passed the result (an item on insert/update, and the string
/// id on a delete) or the error from the mobile service.
-(void) executeWithCompletion:(void(^)(NSDictionary *item, NSError *error))completion;

/// @}

///@name Initializing the MSTable Object
///@{

/// Initializes an *MSTableOperation* instance for the given type, table, and item.
-(id) initWithTable:(NSString *)tableName
               type:(MSTableOperationTypes)type
             itemId:(NSString *)itemId;

/// Initializes an *MSPushOperation* instance for the given type, table, and item.
+(MSTableOperation *) pushOperationForTable:(NSString *)tableName
                                      type:(MSTableOperationTypes)type
                                      item:(NSString *)itemId;

/// @}

@end
