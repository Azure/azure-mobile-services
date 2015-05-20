// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"
#import "MSOperationQueue.h"
#import "MSTable.h"

@class MSQueuePushOperation;
@class MSQueuePullOperation;
@class MSQueuePurgeOperation;

@interface MSSyncContext()

@property (nonatomic, weak)             MSClient *client;
@property (nonatomic, strong, readonly) MSOperationQueue *operationQueue;
@property (nonatomic, strong)           NSOperationQueue *callbackQueue;

@property (atomic) NSInteger operationSequence;


#pragma mark * SyncTable helpers


-(void) syncTable:(NSString *)table item:(NSDictionary *)item action:(MSTableOperationTypes)action completion:(MSSyncItemBlock)completion;

-(NSDictionary *) syncTable:(NSString *)table readWithId:(NSString *)itemId orError:(NSError **)error;

-(void) readWithQuery:(MSQuery *)query completion:(MSReadQueryBlock)completion;

-(NSOperation *) pullWithQuery:(MSQuery *)query queryId:(NSString *)queryId completion:(MSSyncBlock)completion;

-(NSOperation *) purgeWithQuery:(MSQuery *)query completion:(MSSyncBlock)completion;

-(NSOperation *) forcePurgeWithTable:(MSSyncTable *)syncTable completion:(MSSyncBlock)completion;


#pragma mark * Operation Helpers


-(NSError *) removeOperation:(MSTableOperation *)operation;

-(void) cancelOperation:(MSTableOperation *)operation updateItem:(NSDictionary *)item completion:(MSSyncBlock)completion;

-(void) cancelOperation:(MSTableOperation *)operation discardItemWithCompletion:(MSSyncBlock)completion;

@end