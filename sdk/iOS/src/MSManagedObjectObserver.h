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
@property (nonatomic, copy) MSManagedObjectObserverCompletionBlock observerActionCompleted;

@end
