// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"
#import "MSOperationQueue.h"

@interface MSSyncContext()

@property (nonatomic, weak)             MSClient *client;
@property (nonatomic, strong, readonly) MSOperationQueue *operationQueue;
@property (nonatomic, strong)           NSMutableArray *errors;

#pragma mark * SyncTable helpers

-(void) loadOperations;

-(void) syncTable:(NSString *)table item:(NSDictionary *)item action:(MSTableOperationTypes)action completion:(MSSyncItemBlock)completion;

-(NSDictionary *) syncTable:(NSString *)table readWithId:(NSString *)itemId orError:(NSError **)error;

-(void) readWithQuery:(MSQuery *)query completion:(MSSyncReadBlock)completion;

-(void) pullWithQuery:(MSQuery *)query completion:(MSSyncBlock)completion;

-(void) purgeWithQuery:(MSQuery *)query completion:(MSSyncBlock)completion;

#pragma mark * Operation Helpers

-(NSError *) removeOperation:(MSTableOperation *)operation;

@end