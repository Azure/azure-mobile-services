//
//  MSManagedObjectObserver.h
//  WindowsAzureMobileServices
//
//  Created by Phillip Van Nortwick on 2/5/15.
//  Copyright (c) 2015 Windows Azure. All rights reserved.
//

#import <Foundation/Foundation.h>

@class MSClient;

@interface MSManagedObjectObserver : NSObject

- (instancetype) initWithClient:(MSClient *)client;

@end
