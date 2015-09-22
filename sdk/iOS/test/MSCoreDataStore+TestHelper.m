// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSCoreDataStore+TestHelper.h"
#import "TodoItem.h"

@implementation MSCoreDataStore (TestHelper)

+ (NSManagedObjectContext *)inMemoryManagedObjectContext
{
	// Had to use T
    NSBundle *bundle = [NSBundle bundleForClass:[TodoItem class]];
    NSURL *url = [bundle URLForResource:@"CoreDataTestModel" withExtension:@"momd"];
    NSManagedObjectModel *model = [[NSManagedObjectModel alloc] initWithContentsOfURL:url];
    
    NSPersistentStoreCoordinator *coordinator = [[NSPersistentStoreCoordinator alloc] initWithManagedObjectModel:model];
    [coordinator addPersistentStoreWithType:NSInMemoryStoreType configuration:nil URL:nil options:nil error:0];
    
    NSManagedObjectContext *context = [[NSManagedObjectContext alloc] initWithConcurrencyType:NSPrivateQueueConcurrencyType];
    context.persistentStoreCoordinator = coordinator;
    
    return context;
}

@end
