// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "BlockDefinitions.h"

@class MSClient;
@class MSQuery;

/// The *MSSyncTable* class represents a table of a Windows Azure Mobile Service.
/// Items can be inserted, updated, deleted and read from the table. The table
/// can also be queried to retrieve an array of items that meet the given query
/// conditions. All table operations result in a request to the local store
/// and an eventual request to the azure mobile service (see the *MSSyncContext* for more
/// details).
@interface MSSyncTable : NSObject

///@name Properties
///@{

/// The name of this table.
@property (nonatomic, copy, readonly, nonnull)           NSString *name;

/// The client associated with this table.
@property (nonatomic, strong, readonly, nonnull)         MSClient *client;

//@property (nonatomic) MSSystemProperties systemProperties;
///@}

///@name Initializing the MSTable Object
///@{

/// Initializes an *MSTable* instance with the given name and client.
-(nonnull instancetype)initWithName:(nonnull NSString *)tableName client:(nonnull MSClient *)client;

///@}

///@name Modifying Items
///@{

/// Sends a request to the MSSyncContext's data source to upsert the given
/// item into the local store. In addition queues a request to send the insert
/// to the mobile service.
-(void)insert:(nonnull NSDictionary *)item completion:(nullable MSSyncItemBlock)completion;

/// Sends a request to the MSSyncContext's data source to upsert the given
/// item into the local store. In addition queues a request to send the update
/// to the mobile service.
-(void)update:(nonnull NSDictionary *)item completion:(nullable MSSyncBlock)completion;

/// Sends a request to the MSSyncContext's data source to delete the given
/// item in the local store. In addition queues a request to send the delete
/// to the mobile service.
-(void)delete:(nonnull NSDictionary *)item completion:(nullable MSSyncBlock)completion;

///@}


#pragma mark * Public Read Methods

///@name Retreiving Local Items
///@{

/// Sends a request to the Windows Azure Mobile Service to return the item
/// with the given id from the table.
-(void)readWithId:(nonnull NSString *)itemId completion:(nullable MSItemBlock)completion;

/// Sends a request to the Windows Azure Mobile Service to return all items
/// from the table. The Windows Azure Mobile Service will apply a default
/// limit to the number of items returned.
-(void)readWithCompletion:(nullable MSReadQueryBlock)completion;

/// Sends a request to the Windows Azure Mobile Service to return all items
/// from the table that meet the conditions of the given predicate.
-(void)readWithPredicate:(nullable NSPredicate *)predicate
              completion:(nullable MSReadQueryBlock)completion;

#pragma mark * Public Query Constructor Methods

/// Returns an *MSQuery* instance associated with the table that can be
/// configured and then executed to retrieve items from the table. An *MSQuery*
/// instance provides more flexibilty when querying a table than the table
/// read* methods.
-(nonnull MSQuery *)query;

/// Returns an *MSQuery* instance associated with the table that uses
/// the given predicate. An *MSQuery* instance provides more flexibilty when
/// querying a table than the table read* methods.
-(nonnull MSQuery *)queryWithPredicate:(nullable NSPredicate *)predicate;

/// @}

/// @name Managing local storage
/// @{

/// Initiates a request to go to the server and get a set of records matching the specified
/// MSQeury object.
/// Before a pull is allowed to run, all pending requests on the specified table will be sent to
/// the server. If a pending request for this table fails, the pull will be cancelled
-(void)pullWithQuery:(nonnull MSQuery *)query queryId:(nullable NSString *)queryId completion:(nullable MSSyncBlock)completion;

/// Removes all records in the local cache that match the results of the specified query.
/// If query is nil, all records in the local table will be removed.
/// Before local data is removed, a check will be made for pending operations on this table. If
/// any are found the purge will be cancelled and an error returned.
-(void)purgeWithQuery:(nullable MSQuery *)query completion:(nullable MSSyncBlock)completion;

/// Purges all data, pending operations, operation errors, and metadata for the
/// MSSyncTable from the local cache.
-(void)forcePurgeWithCompletion:(nullable MSSyncBlock)completion;

/// @}

@end
