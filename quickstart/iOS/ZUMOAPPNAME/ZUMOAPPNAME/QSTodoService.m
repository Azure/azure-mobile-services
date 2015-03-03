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

#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>
#import "QSTodoService.h"
#import "QSAppDelegate.h"

#pragma mark * Private interace


@interface QSTodoService()

@property (nonatomic, strong)   MSSyncTable *syncTable;

@end


#pragma mark * Implementation


@implementation QSTodoService


+ (QSTodoService *)defaultService
{
    // Create a singleton instance of QSTodoService
    static QSTodoService* service;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        service = [[QSTodoService alloc] init];
    });
    
    return service;
}

-(QSTodoService *)init
{
    self = [super init];
    
    if (self)
    {
        // Initialize the Mobile Service client with your URL and key   
        self.client = [MSClient clientWithApplicationURLString:@"ZUMOAPPURL"
                                                applicationKey:@"ZUMOAPPKEY"];
    
        QSAppDelegate *delegate = (QSAppDelegate *)[[UIApplication sharedApplication] delegate];
        NSManagedObjectContext *context = delegate.managedObjectContext;
        MSCoreDataStore *store = [[MSCoreDataStore alloc] initWithManagedObjectContext:context];
        
        self.client.syncContext = [[MSSyncContext alloc] initWithDelegate:nil dataSource:store callback:nil];
        
        // Create an MSSyncTable instance to allow us to work with the TodoItem table
        self.syncTable = [_client syncTableWithName:@"TodoItem"];
    }
    
    return self;
}

-(void)addItem:(NSDictionary *)item completion:(QSCompletionBlock)completion
{
    // Insert the item into the TodoItem table and add to the items array on completion
    [self.syncTable insert:item completion:^(NSDictionary *result, NSError *error)
    {
        [self logErrorIfNotNil:error];
    
        [self syncData: ^{
            // Let the caller know that we finished
            if (completion != nil) {
                dispatch_async(dispatch_get_main_queue(), completion);
            }
        }];
    }];
}

-(void)completeItem:(NSDictionary *)item completion:(QSCompletionBlock)completion
{
    // Set the item to be complete (we need a mutable copy)
    NSMutableDictionary *mutable = [item mutableCopy];
    [mutable setObject:@YES forKey:@"complete"];
    
    // Update the item in the TodoItem table and remove from the items array on completion
    [self.syncTable update:mutable completion:^(NSError *error)
    {
        [self logErrorIfNotNil:error];
        
        [self syncData: ^{
            // Let the caller know that we finished
            if (completion != nil) {
                dispatch_async(dispatch_get_main_queue(), completion);
            }
        }];
    }];
}

-(void)syncData:(QSCompletionBlock)completion
{
    // push all changes in the sync context, then pull new data
    [self.client.syncContext pushWithCompletion:^(NSError *error) {
        [self logErrorIfNotNil:error];
        [self pullData:completion];
    }];
}

-(void)pullData:(QSCompletionBlock)completion
{
    MSQuery *query = [self.syncTable query];
    
    // Pulls data from the remote server into the local table.
    // We're pulling all items and filtering in the view
    // query ID is used for incremental sync
    [self.syncTable pullWithQuery:query queryId:@"allTodoItems" completion:^(NSError *error) {
        [self logErrorIfNotNil:error];
        
        // Let the caller know that we have finished
        if (completion != nil) {
            dispatch_async(dispatch_get_main_queue(), completion);
        }
    }];
}

- (void)logErrorIfNotNil:(NSError *) error
{
    if (error)
    {
        NSLog(@"ERROR %@", error);
    }
}

@end
