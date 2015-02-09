//
//  MSManagedObjectObserver.m
//  WindowsAzureMobileServices
//
//  Created by Phillip Van Nortwick on 2/5/15.
//  Copyright (c) 2015 Windows Azure. All rights reserved.
//

#import "MSManagedObjectObserver.h"
#import <CoreData/CoreData.h>

@interface MSManagedObjectObserver()
@property (nonatomic, weak) MSClient *client;
@end

@implementation MSManagedObjectObserver

- (id) initWithClient:(MSClient *)client
{
    return [self initWithClient:client contextsToObserver:nil];
}

- (id) initWithClient:(MSClient *)client contextsToObserver:(NSArray *)contexts
{
    self = [super init];
    if (self) {
        _client = client;
    }
    return self;
}

- (void) registerAsObserver
{
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(handleDidSaveNotification:)
                                                 name:NSManagedObjectContextDidSaveNotification
                                               object:nil];

}

@end
