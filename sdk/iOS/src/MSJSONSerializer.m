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

#import "MSJSONSerializer.h"
#import "MSClient.h"


#pragma mark * Mobile Services Special Keys String Constants


NSString *const idKey = @"id";
NSString *const resultsKey =@"results";
NSString *const countKey =@"count";
NSString *const errorKey =@"error";
NSString *const descriptionKey =@"description";


#pragma mark * MSJSONSerializer Implementation


@implementation MSJSONSerializer

static MSJSONSerializer *staticJSONSerializerSingleton;


#pragma mark * Public Static Singleton Constructor


+(id <MSSerializer>) JSONSerializer
{
    if (staticJSONSerializerSingleton == nil) {
        staticJSONSerializerSingleton = [[MSJSONSerializer alloc] init];
    }
    
    return  staticJSONSerializerSingleton;
}


# pragma mark * MSSerializer Protocol Implementation


-(NSData *) dataFromItem:(id)item orError:(NSError **)error
{
    NSData *data = nil;
    
    // First, ensure there is an item...
    if (!item) {
        *error = [self errorForNilItem];
    }
    else {
        
        // ... then make sure the |NSJSONSerializer| can serialize it, otherwise
        // the |NSJSONSerializer| will throw an exception, which we don't
        // want--we'd rather return an error.
        if (![NSJSONSerialization isValidJSONObject:item]) {
            *error = [self errorForInvalidItem];
        }
        else {
            
            // If there is still an error serializing, |dataWithJSONObject|
            // will ensure that data the error is set and data is nil.
            data = [NSJSONSerialization dataWithJSONObject:item
                                               options:0
                                                 error:error];
        }
    }
    
    return data;
}

-(NSNumber *) itemIdFromItem:(NSDictionary *)item orError:(NSError **)error
{
    NSNumber *itemId = nil;
    
    // Ensure there is an item
    if (!item) {
        *error = [self errorForNilItem];
    }
    else {
        
        // Then get the value of the id key, which must be present or else
        // it is an error.
        itemId = [item objectForKey:idKey];
        if (!itemId) {
            *error = [self errorForMissingItemId];
        }
        else if(![itemId isKindOfClass:[NSNumber class]]) {
            
            // The id was there, but it wasn't a number--this is also an error.
            *error = [self errorForInvalidItemId];
            itemId = nil;
        }
    }

    return itemId;;
}

-(id) itemFromData:(NSData *)data
            withOriginalItem:(id)originalItem
            orError:(NSError **)error
{
    id item = nil;
    
    // Ensure there is data
    if (!data) {
        *error = [self errorForNilData];
    }
    else {
        
        // Try to deserialize the data; if it fails the error will be set
        // and item will be nil.
        item = [NSJSONSerialization JSONObjectWithData:data
                                               options:NSJSONReadingAllowFragments
                                                 error:error];

        if (item) {
            
            // The data should have been only a single item--that is, a
            // dictionary and not an array or string, etc.
            if (![item isKindOfClass:[NSDictionary class]]) {
                item = nil;
                *error = [self errorForExpectedItem];
            }
            else if (originalItem) {
                
                // If the originalitem was provided, update it with the values
                // from the new item.
                for (NSString *key in [item allKeys]) {
                    id value = [item objectForKey:key];
                    [originalItem setValue:value forKey:key];
                }
                
                // And return the original value instead
                item = originalItem;
            }
        }
    }
    
    return item;
}

-(NSInteger) totalCountAndItems:(NSArray **)items
                       fromData:(NSData *)data
                        orError:(NSError **)error
{
    NSInteger totalCount = -1;
    
    // Ensure there is data
    if (!data) {
        *error = [self errorForNilData];
    }
    else
    {
        id JSONObject = [NSJSONSerialization JSONObjectWithData:data
                        options:NSJSONReadingMutableContainers |
                                NSJSONReadingAllowFragments
                        error:error];
    
        if (JSONObject) {

            // The JSONObject could be either an array or a dictionary
            if ([JSONObject isKindOfClass:[NSArray class]]) {
                
                // The JSONObject was just an array, so it is just the items
                *items = JSONObject;
            }
            else if ([JSONObject isKindOfClass:[NSDictionary class]]) {
            
                // Since it was a dictionary, it has to have both the
                // count, which is a number...
                id count = [JSONObject objectForKey:countKey];
                if (![count isKindOfClass:[NSNumber class]]) {
                    *error = [self errorForMissingTotalCount];
                }
                else {
                    totalCount = [count integerValue];
                
                    // ...and it has to have the array of items.
                    *items = [JSONObject objectForKey:resultsKey];
                    if (![*items isKindOfClass:[NSArray class]]) {
                        *error = [self errorForMissingItems];
                        *items = nil;
                        totalCount = -1;
                    }
                }
            }
            else {
                // The JSONObject was neither a dictionary nor an array, so that
                // is also an error.
                *error = [self errorForMissingItems];
            }
        }
    }
    
    return totalCount;
}

-(NSError *) errorFromData:(NSData *)data
{
    NSError *error = nil;
    
    // If there is data, deserialize it
    if (data) {
        id JSONObject = [NSJSONSerialization JSONObjectWithData:data
                         options: NSJSONReadingMutableContainers |
                                  NSJSONReadingAllowFragments
                         error:&error];

        if (JSONObject) {
            
            // We'll see if we can find an error message in the data
            NSString *errorMessage = nil;
            
            if ([JSONObject isKindOfClass:[NSString class]]) {
                
                // Since the JSONObject was just a string, we'll assume it
                // is the error message.
                errorMessage = JSONObject;
            }
            else if ([JSONObject isKindOfClass:[NSDictionary class]]) {
                
                // Since we have a dictionary, we'll look for the 'error' or
                // 'description' keys.
                errorMessage = [JSONObject objectForKey:errorKey];
                if (![errorMessage isKindOfClass:[NSString class]]) {
                    
                    // The 'error' key didn't work, so we'll try 'description'
                    errorMessage = [JSONObject objectForKey:descriptionKey];
                    if (![errorMessage isKindOfClass:[NSString class]]) {
                        
                        // 'description' didn't work either
                        errorMessage = nil;
                    }
                }
            }
            
            // If we found an error message, make an error from it
            if (errorMessage) {
                error = [self errorWithMessage:errorMessage];
            }
        }
    }
    
    if (!error) {
        
        // If we couldn't find an error message, return a generic error
        error = [self errorWithoutMessage];
    }
    
    return error;
}


#pragma mark * NSError Generation Methods


-(NSError *) errorForNilItem
{
    return [self errorWithDescriptionKey:@"MSJSONSerializer-NilItem"
                            andErrorCode:MSExpectedItemWithRequest];
}

-(NSError *) errorForInvalidItem
{
    return [self errorWithDescriptionKey:@"MSJSONSerializer-InvalidItem"
                            andErrorCode:MSInvalidItemWithRequest];
}

-(NSError *) errorForMissingItemId
{
    return [self errorWithDescriptionKey:@"MSJSONSerializer-MissingItemId"
                            andErrorCode:MSMissingItemIdWithRequest];
}

-(NSError *) errorForInvalidItemId
{
    return [self errorWithDescriptionKey:@"MSJSONSerializer-InvalidItemId"
                            andErrorCode:MSInvalidItemIdWithRequest];
}

-(NSError *) errorForNilData
{
    return [self errorWithDescriptionKey:@"MSJSONSerializer-NilData"
                            andErrorCode:MSExpectedBodyWithResponse];
}

-(NSError *) errorForExpectedItem
{
    return [self errorWithDescriptionKey:@"MSJSONSerializer-ExpectedItem"
                            andErrorCode:MSExpectedItemWithResponse];
}

-(NSError *) errorForMissingTotalCount
{
    return [self errorWithDescriptionKey:@"MSJSONSerializer-MissingTotalCount"
                            andErrorCode:MSExpectedTotalCountWithResponse];
}

-(NSError *) errorForMissingItems
{
    return [self errorWithDescriptionKey:@"MSJSONSerializer-MissingItems"
                            andErrorCode:MSExpectedItemsWithResponse];
}

-(NSError *) errorWithoutMessage
{
    return [self errorWithDescriptionKey:@"MSJSONSerializer-ErrorWithoutMessage"
                            andErrorCode:MSErrorNoMessageErrorCode];
}

-(NSError *) errorWithMessage:(NSString *)errorMessage
{
    return [self errorWithDescription:errorMessage
                            andErrorCode:MSErrorMessageErrorCode];
}

-(NSError *) errorWithDescriptionKey:(NSString *)descriptionKey
                        andErrorCode:(NSInteger)errorCode
{
    NSString *description = NSLocalizedString(descriptionKey, nil);
    NSDictionary *userInfo = @{ NSLocalizedDescriptionKey :description };
    
    return [NSError errorWithDomain:MSErrorDomain
                               code:errorCode
                           userInfo:userInfo];
}

-(NSError *) errorWithDescription:(NSString *)description
                     andErrorCode:(NSInteger)errorCode
{
    NSDictionary *userInfo = @{ NSLocalizedDescriptionKey :description };
    
    return [NSError errorWithDomain:MSErrorDomain
                               code:errorCode
                           userInfo:userInfo];
}

@end
