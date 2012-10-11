// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import "MSQuery.h"
#import "MSPredicateTranslator.h"


#pragma mark * Query String Constants


NSString *const topParameter = @"$top";
NSString *const skipParameter = @"$skip";
NSString *const selectParameter = @"$select";
NSString *const orderByParameter = @"$orderby";
NSString *const orderByAscendingFormat = @"%@ asc";
NSString *const orderByDescendingFormat = @"%@ desc";
NSString *const filterParameter = @"$filter";
NSString *const inlineCountParameter = @"$inlinecount";
NSString *const inlineCountAllPage = @"allpages";
NSString *const inlineCountNone = @"none";


#pragma mark * MSQuery Private Interface


@interface MSQuery ()

// Private instance properties
@property (nonatomic, strong, readwrite)            NSPredicate *predicate;
@property (nonatomic, strong, readwrite)            NSMutableArray *orderBy;

@end


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
    return [self initWithTable:table withPredicate:nil];
}

-(id) initWithTable:(MSTable *)table withPredicate:(NSPredicate *)predicate
{
    self = [super init];
    if(self)
    {
        table_ = table;
        predicate_ = predicate;
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
    
    // If this is the first field being ordered, we'll need to create the array
    if (self.orderBy == nil) {
        self.orderBy = [NSMutableArray array];
    }
    
    NSSortDescriptor *sort = [NSSortDescriptor sortDescriptorWithKey:field ascending:isAscending];

    [self.orderBy addObject:sort];
}


-(NSString *) queryStringOrError:(NSError **)error
{
    NSMutableString *queryString = nil;
    NSString *filterValue = nil;

    if (self.predicate) {
        // Translate the predicate into the filter first since it might error
        filterValue = [MSPredicateTranslator
                       queryFilterFromPredicate:self.predicate
                       orError:error];
    }
    
    if (filterValue || !self.predicate) {
        
        // Create a dictionary to hold all of the query parameters
        NSMutableDictionary *queryParameters = [NSMutableDictionary dictionary];
        
        // Add the $filter parameter
        if (self.predicate) {
            [queryParameters setValue:filterValue forKey:filterParameter];
        }

        // Add the $top parameter
        if (self.fetchLimit > 0) {
            NSString *topValue = [NSString stringWithFormat:@"%u",
                                  self.fetchLimit];
            [queryParameters setValue:topValue forKey:topParameter];
        }
        
        // Add the $skip parameter
        if (self.fetchOffset > 0) {
            NSString *skipValue = [NSString stringWithFormat:@"%u",
                                   self.fetchOffset];
            [queryParameters setValue:skipValue forKey:skipParameter];
        }
        
        // Add the $select parameter
        if (self.selectFields) {
            NSString *selectFieldsValue = [self.selectFields
                                           componentsJoinedByString:@","];
            [queryParameters setValue:selectFieldsValue forKey:selectParameter];
        }
        
        // Add the $orderBy parameter
        if (self.orderBy) {
            NSMutableString *orderByString = [NSMutableString string];
            for (NSSortDescriptor* sort in self.orderBy){
                if (orderByString.length > 0) {
                    [orderByString appendString:@","];
                }
                NSString *format = (sort.ascending) ?
                    orderByAscendingFormat :
                    orderByDescendingFormat;
                [orderByString appendFormat:format, sort.key];
            }
            [queryParameters setValue:orderByString forKey:orderByParameter];
        }

        
        // Add the $inlineCount parameter
        NSString *includeTotalCountValue = self.includeTotalCount ?
            inlineCountAllPage :
            inlineCountNone;
        [queryParameters setValue:includeTotalCountValue
                           forKey:inlineCountParameter];
        
        // Add the user parameters
        if (self.parameters) {
            [queryParameters addEntriesFromDictionary:self.parameters];
        }
        
        // Iterate through the parameters to build the query string as key=value
        // pairs seperated by '&'. Don't bother to precent escape the string here
        // as that will be done by when the full URI is generated
        queryString = [NSMutableString string];
        for (NSString* key in [queryParameters allKeys]){
            if (queryString.length > 0) {
                [queryString appendString:@"&"];
            }
            
            [queryString appendFormat:@"%@=%@", key,
             [queryParameters objectForKey:key]];
        }
    }
    
    return queryString;
}


#pragma mark * Overridden Methods


-(NSString *) description
{
    return [self queryStringOrError:nil];
}

@end
