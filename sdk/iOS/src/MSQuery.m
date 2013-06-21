// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSQuery.h"
#import "MSPredicateTranslator.h"
#import "MSURLBuilder.h"


#pragma mark * MSQuery Implementation


@implementation MSQuery

@synthesize table = table_;
@synthesize fetchLimit = fetchLimit_;
@synthesize fetchOffset = fetchOffset_;
@synthesize includeTotalCount = includeTotalCount_;
@synthesize parameters = parameters_;
@synthesize selectFields = selectFields_;
@synthesize predicate = predicate_;
@synthesize orderBy = orderBy_;


#pragma mark * Public Initializer Methods


-(id) initWithTable:(MSTable *)table;
{
    return [self initWithTable:table predicate:nil];
}

-(id) initWithTable:(MSTable *)table predicate:(NSPredicate *)predicate
{
    self = [super init];
    if(self)
    {
        table_ = table;
        predicate_ = predicate;
        fetchLimit_ = -1;
        fetchOffset_ = -1;
    }
    return self;
}


#pragma mark * Public OrderBy Methods


-(void) orderByAscending:(NSString *)field
{
    [self orderBy:field isAscending:YES];
}

-(void) orderByDescending:(NSString *)field
{
    [self orderBy:field isAscending:NO];
}


#pragma mark * Public Read Methods


-(void) readWithCompletion:(MSReadQueryBlock)completion;
{
    // Get the query string
    NSError *error = nil;
    NSString *queryString = [self queryStringOrError:&error];
    
    if (!queryString) {
        // Query string is invalid, so call error handler
        if (completion) {
            completion(nil, -1, error);
        }
    }
    else {
        // Call read with the query string
        [self.table readWithQueryString:queryString
                              completion:completion];
    }
}


#pragma mark * Private Methods


-(void) orderBy:(NSString *)field isAscending:(BOOL)isAscending
{
    NSAssert(field != nil, @"'field' can not be nil.");
    
    NSMutableArray *currentOrderBy = [self.orderBy mutableCopy];
    if (currentOrderBy == nil) {
        currentOrderBy = [NSMutableArray array];
    }
    
    NSSortDescriptor *sort = [NSSortDescriptor sortDescriptorWithKey:field
                                                           ascending:isAscending];

    [currentOrderBy addObject:sort];
    self.orderBy = currentOrderBy;
}


-(NSString *) queryStringOrError:(NSError **)error
{
    return [MSURLBuilder queryStringFromQuery:self orError:error];
}


#pragma mark * Overridden Methods


-(NSString *) description
{
    return [self queryStringOrError:nil];
}

@end
