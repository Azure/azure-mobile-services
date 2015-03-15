//
//  MSManagedObjectObserver.h
//  WindowsAzureMobileServices
//
//  Created by Phillip Van Nortwick on 2/5/15.
//  Copyright (c) 2015 Windows Azure. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "MSTableOperation.h"

typedef void (^MSManagedObjectObserverCompletionBlock)(MSTableOperationTypes operationType, NSDictionary *item, NSError *error);

@class MSClient;

@interface MSManagedObjectObserver : NSObject

- (instancetype) initWithClient:(MSClient *)client;

/// Block to be called on each operation that will is inserted into MS_TableOperations
///
/// If not implemented or not dealing with errors and errors occur during these operations
/// the local store and associated messages to the mobile services API will be left
/// in a bad state.
@property (nonatomic, copy) MSManagedObjectObserverCompletionBlock observerActionCompleted;

@end
