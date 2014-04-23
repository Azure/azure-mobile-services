// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSOperationQueue.h"

#import "MSTable.h"
#import "MSTableOperationInternal.h"

@interface MSOperationQueue()
@property (nonatomic, strong) NSMutableArray *queue;
@end

@implementation MSOperationQueue

@synthesize queue = queue_;
- (NSArray *) queue
{
    if (queue_ == nil) {
        queue_ = [NSMutableArray new];
    }
    
    return queue_;
}

-(NSUInteger) count
{
    // TODO: Filter out bookmarks
    
    return self.queue.count;
}

-(NSArray *) getOperationsForTable:(NSString *) table item:(NSString *)item
{
    //return [self.queue filteredArrayUsingPredicate:[NSPredicate predicateWithFormat:@"(tableName == %@) AND (itemId == %@)", table, item]];

    //find table-item pairs
    NSMutableArray *results = [NSMutableArray new];
    for (id op in self.queue) {
        if ([op isKindOfClass:[MSTableOperation class]]) {
            MSTableOperation *tableOp = (MSTableOperation *)op;
            
            if ([tableOp.tableName isEqualToString:table] && (item == nil || [tableOp.itemId isEqualToString:item])) {
                [results addObject:tableOp];
            }
        }
    }
    return results;
}

-(void) addOperation:(id)operation
{
    [self.queue addObject:operation];
}

-(void) removeOperation:(id)operation
{
    [self.queue removeObject:operation];
}

- (id) peek
{
    return [self.queue firstObject];    
}

- (NSArray *) operations
{
    return self.queue;
}

@end
