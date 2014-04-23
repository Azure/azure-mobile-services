// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSTableOperation.h"
#import "MSTableOperationInternal.h"
#import "MSClient.h"
#import "MSTable.h"
#import "MSJSONSerializer.h"

@implementation MSTableOperation

@synthesize type = type_;
@synthesize tableName = tableName_;
@synthesize itemId = itemId_;
@synthesize guid = guid_;

+(MSTableOperation *) pushOperationForTable:(NSString *)tableName
                                      type:(MSTableOperationTypes)type
                                      item:(NSString *)itemId;
{
    return [[MSTableOperation alloc] initWithTable:tableName type:type itemId:itemId];
}

-(id) initWithTable:(NSString *)tableName
               type:(MSTableOperationTypes)type
             itemId:(NSString *)itemId;
{
    self = [super init];
    if (self)
    {
        guid_ = [MSJSONSerializer generateGUID];
        type_ = type;
        tableName_ = [tableName copy];
        itemId_ = [itemId copy];
    }
    
    return self;
}

-(id) initWithItem:(NSDictionary *)item
{
    self = [super init];
    if (self) {
        guid_ = [item objectForKey:@"id"];
        
        NSData *data = [item objectForKey:@"properties"];
        MSJSONSerializer *serializer = [MSJSONSerializer new];
        
        NSDictionary *rawItem = [serializer itemFromData:data withOriginalItem:nil ensureDictionary:YES orError:nil];
        
        type_ = [[rawItem objectForKey:@"type"] integerValue];
        itemId_ = [rawItem objectForKey:@"itemId"];
        tableName_ = [rawItem objectForKey:@"table"];
    }
    return self;
}

-(id) initWithData:(NSData *)data
{
    return nil;    
}

-(NSDictionary *) serialize
{
    NSDictionary *properties;
    if (self.type == MSTableOperationDelete) {
        properties = @{ @"type": [NSNumber numberWithInteger:self.type],
                        @"item": self.item };
    } else {
        properties = @{ @"type": [NSNumber numberWithInteger:self.type] };
    }
    
    MSJSONSerializer *serializer = [MSJSONSerializer new];
    NSData *data = [serializer dataFromItem:properties idAllowed:YES ensureDictionary:NO removeSystemProperties:NO orError:nil];
    
    // TODO: ordering on table operation
    // Operation searches by:
    // -- table
    // -- item id
    // -- instant added
    
    return @{ @"id": self.guid, @"table": self.tableName, @"itemId": self.itemId, @"properties": data };
}

- (void) executeWithCompletion:(void(^)(NSDictionary *, NSError *))completion
{
    MSTable *table = [self.client tableWithName:self.tableName];
    table.systemProperties = MSSystemPropertyVersion;
    
    if (self.type == MSTableOperationInsert) {
        [table insert:self.item completion:completion];
    } else if (self.type == MSTableOperationUpdate) {
        [table update:self.item completion:completion];
    } else if (self.type == MSTableOperationDelete) {
        [table delete:self.item completion:completion];
    }
}


/// Logic for determining how to operations should be condensed into one single pending operation
/// For example: Insert + Update -> Insert
///              Update + Insert -> Error (don't allow user to do this)
+ (MSCondenseAction) condenseAction:(MSTableOperationTypes)newAction withExistingOperation:(MSTableOperation *)operation
{
    MSTableOperationTypes existingAction = operation.type;
    MSCondenseAction actionToTake = MSCondenseNotSupported;
    
    if (existingAction == MSTableOperationInsert) {
        switch (newAction) {
            case MSTableOperationUpdate:
                actionToTake = MSCondenseKeep;
                break;
            case MSTableOperationDelete:
                actionToTake = MSCondenseRemove;
                break;
            default:
                actionToTake = MSCondenseNotSupported;
                break;
        }
    }
    else if (existingAction == MSTableOperationUpdate) {
        switch (newAction) {
            case MSTableOperationDelete:
                actionToTake = MSCondenseToDelete;
                break;
            case MSTableOperationUpdate:
                actionToTake = MSCondenseKeep;
                break;
            default:
                actionToTake = MSCondenseNotSupported;
                break;
        }
    }
    
    // All actions after a MSPushOperationDelete are invalid
    
    if (operation.inProgress && actionToTake != MSCondenseNotSupported) {
        actionToTake = MSCondenseAddNew;
    }
    
    return actionToTake;
}

@end
