// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSTable.h"


#pragma mark * MSQuery Public Interface


/// The *MSQuery* class represents a query that can be configured and then
/// executed against a table of a Microsoft Azure Mobile Service. The query is
/// serialized as a query string in the URL of the request. A query can be
/// configured and then sent to the Microsoft Azure Mobile Service using the
/// *readOnSuccess:onError:* method. *MSQuery* instances can be modfied and
/// reused, but are not threadsafe.
@interface MSQuery : NSObject

#pragma mark * Public Initializer Methods

///@name Initializing the MSQuery object
///@{

/// Initializes a *MSQuery* instance with the given table.
-(id)initWithTable:(MSTable *)table;

/// Returns a new *MSQuery* instance with the given table and the given
/// predicate is used as the filter clause of the query.
-(id)initWithTable:(MSTable *)table predicate:(NSPredicate *)predicate;

///@}

///@name Modifying the Query
///@{

#pragma mark * Public ReadWrite Properties

/// The predicate used as the filter clause of the query.
@property (nonatomic) NSPredicate *predicate;

/// The maximum number of items to return from the query.
@property (nonatomic) NSInteger fetchLimit;

/// The offset from the initial item to use when returning items from a query.
/// Can be used with *fetchLimit* to implement paging.
@property (nonatomic) NSInteger fetchOffset;

/// The array of NSSortDescriptors used to order the query results
@property (nonatomic, copy) NSArray *orderBy;

/// Indicates if the Microsoft Azure Mobile Service should also include the total
/// count of items on the server (not just the count of items returned) with
/// the query results.
@property (nonatomic) BOOL includeTotalCount;

/// A dictionary of string key-value pairs that can include user-defined
/// parameters to use with the query.
@property (nonatomic, retain) NSDictionary *parameters;

/// The fields or keys of an item that should be included in the results. A
/// value of "*" means all fields should be included. "*" is the default value
/// if no select keys are specified.
@property (nonatomic, retain) NSArray *selectFields;

#pragma mark * Public OrderBy Methods
/// Indicates that the query results should be returned in ascending order
/// based on the given field. *orderByAscending:* and *orderByDescending:* can
/// each be called multiple times to further specify how the query results
/// should be ordered.
-(void)orderByAscending:(NSString *)field;

/// Indicates that the query results should be returned in descending order
/// based on the given field. *orderByAscending:* and *orderByDescending:* can
/// each be called multiple times to further specify how the query results
/// should be ordered.
-(void)orderByDescending:(NSString *)field;


///@}



#pragma mark * Public Read Methods
///@name Executing the query
///@{

/// Executes the query by sending a request to the Microsoft Azure Mobile Service.
-(void)readWithCompletion:(MSReadQueryBlock)completion;

///@}

///@name Properties
///@{

#pragma mark * Public QueryString Methods

/// Generates a query string for current state of the *MSQuery* instance or an
/// error if the query string could not be generated.
-(NSString *)queryStringOrError:(NSError **)error;


#pragma mark * Public Readonly Properties

/// The table associated with this query.
@property (nonatomic, strong, readonly)         MSTable *table;

///@}

@end
