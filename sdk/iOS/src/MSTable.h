// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"
#import "MSQueryResult.h"

@class MSQuery;

#pragma mark * Block Type Definitions

/// Callback for updates, inserts or readWithId requests. If there was an
/// error, the *error* will be non-nil.
typedef void (^MSItemBlock)(NSDictionary *item, NSError *error);

/// Callback for deletes. If there was an error, the *error* will be non-nil.
typedef void (^MSDeleteBlock)(id itemId, NSError *error);

/// Callback for reads. If there was an error, the *error* will be non-nil. If
/// there was not an error, then the result will always be non-nil
/// but but items may be empty if the query returned no results. If the query included a
/// request for the total count of items on the server (not just those returned
/// in *items* array), the *totalCount* in the result will have this value; otherwise
/// *totalCount* will be -1.
/// if the server returned a link to next page of results then
/// nextLink will be non-nil.
typedef void (^MSReadQueryBlock)(MSQueryResult *result,
                                 NSError *error);

typedef NS_OPTIONS(NSUInteger, MSSystemProperties) {
    MSSystemPropertyNone        = 0,
    MSSystemPropertyCreatedAt   = 1 << 0,
    MSSystemPropertyUpdatedAt   = 1 << 1,
    MSSystemPropertyVersion     = 1 << 2,
    MSSystemPropertyDeleted     = 1 << 3,
    MSSystemPropertyAll         = 0xFFFF
};

extern NSString *const MSSystemColumnId;
extern NSString *const MSSystemColumnCreatedAt;
extern NSString *const MSSystemColumnUpdatedAt;
extern NSString *const MSSystemColumnVersion;
extern NSString *const MSSystemColumnDeleted;

#pragma mark * MSTable Public Interface


/// The *MSTable* class represents a table of a Windows Azure Mobile Service.
/// The *MSTable* class represents a table of a Microsoft Azure Mobile Service.
/// Items can be inserted, updated, deleted and read from the table. The table
/// can also be queried to retrieve an array of items that meet the given query
/// conditions. All table operations result in a request to the Windows Azure
/// conditions. All table operations result in a request to the Microsoft Azure
/// Mobile Service to perform the given operation.
@interface MSTable : NSObject


#pragma mark * Public Readonly Properties

///@name Properties
///@{

/// The name of this table.
@property (nonatomic, copy, readonly)           NSString *name;

/// The client associated with this table.
@property (nonatomic, strong, readonly)         MSClient *client;

@property (nonatomic) MSSystemProperties systemProperties;
///@}

#pragma mark * Public Initializers

///@name Initializing the MSTable Object
///@{

/// Initializes an *MSTable* instance with the given name and client.
-(id)initWithName:(NSString *)tableName client:(MSClient *)client;

///@}

#pragma mark * Public Insert, Update and Delete Methods

///@name Modifying Items
///@{

/// Sends a request to the Microsoft Azure Mobile Service to insert the given
/// item into the table. The item must not have an id.
-(void)insert:(NSDictionary *)item completion:(MSItemBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to insert the given
/// item into the table. Addtional user-defined parameters are sent in the
/// request query string. The item must not have an id.
-(void)insert:(NSDictionary *)item
   parameters:(NSDictionary *)parameters
   completion:(MSItemBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to update the given
/// item in the table. The item must have an id.
-(void)update:(NSDictionary *)item completion:(MSItemBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to update the given
/// item in the table. Addtional user-defined parameters are sent in the
/// request query string. The item must have an id.
-(void)update:(NSDictionary *)item
   parameters:(NSDictionary *)parameters
   completion:(MSItemBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to delete the given
/// item from the table. The item must have an id.
-(void)delete:(NSDictionary *)item completion:(MSDeleteBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to delete the given
/// item from the table. Addtional user-defined parameters are sent in the
/// request query string. The item must have an id.
-(void)delete:(NSDictionary *)item
   parameters:(NSDictionary *)parameters
   completion:(MSDeleteBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to delete the item
/// with the given id in from table.
-(void)deleteWithId:(id)itemId completion:(MSDeleteBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to delete the item
/// with the given id in from table. Addtional user-defined parameters are
/// sent in the request query string.
-(void)deleteWithId:(id)itemId
         parameters:(NSDictionary *)parameters
         completion:(MSDeleteBlock)completion;

/// Sends a request to the Azure Mobile Service to undelete the item
/// with the given id in from table.
-(void)undelete:(NSDictionary *)item completion:(MSItemBlock)completion;

/// Sends a request to the Azure Mobile Service to undelete the item
/// with the given id in from table. Addtional user-defined parameters are
/// sent in the request query string.
-(void)undelete:(NSDictionary *)item
     parameters:(NSDictionary *)parameters
     completion:(MSItemBlock)completion;

///@}

#pragma mark * Public Read Methods

///@name Retreiving Items
///@{

/// Sends a request to the Microsoft Azure Mobile Service to return the item
/// with the given id from the table.
-(void)readWithId:(id)itemId completion:(MSItemBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to return the item
/// with the given id from the table. Addtional user-defined parameters are
/// sent in the request query string.
-(void)readWithId:(id)itemId
       parameters:(NSDictionary *)parameters
       completion:(MSItemBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to return all items
/// fromm the table that meet the conditions of the given query.
/// You can also use a URI in place of queryString to fetch results from a URI e.g.
/// result.nextLink gives you URI to next page of results for a query that you can pass here.
-(void)readWithQueryString:(NSString *)queryString
                completion:(MSReadQueryBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to return all items
/// from the table. The Microsoft Azure Mobile Service will apply a default
/// limit to the number of items returned.
-(void)readWithCompletion:(MSReadQueryBlock)completion;

/// Sends a request to the Microsoft Azure Mobile Service to return all items
/// from the table that meet the conditions of the given predicate.
-(void)readWithPredicate:(NSPredicate *) predicate
      completion:(MSReadQueryBlock)completion;

#pragma mark * Public Query Constructor Methods


/// Returns an *MSQuery* instance associated with the table that can be
/// configured and then executed to retrieve items from the table. An *MSQuery*
/// instance provides more flexibilty when querying a table than the table
/// read* methods.
-(MSQuery *)query;

/// Returns an *MSQuery* instance associated with the table that uses
/// the given predicate. An *MSQuery* instance provides more flexibilty when
/// querying a table than the table read* methods.
-(MSQuery *)queryWithPredicate:(NSPredicate *)predicate;

///@}

@end
