//
//  MSManagedObjectObserver.h
//  WindowsAzureMobileServices
//
//  Created by Phillip Van Nortwick on 2/5/15.
//  Copyright (c) 2015 Windows Azure. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "MSClient.h"

@interface MSManagedObjectObserver : NSObject

- (id) initWithClient:(MSClient *)client;

- (id) initWithClient:(MSClient *)client contextsToObserver:(NSArray *)contexts;

@end
