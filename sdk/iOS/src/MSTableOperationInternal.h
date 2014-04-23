// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSClient.h"
#import "MSTableOperation.h"
#import "MSSyncContext.h"

@interface MSTableOperation()
@property (nonatomic, weak)   MSClient *client;
@property (nonatomic, strong) NSDictionary *item;
@property (atomic)            BOOL inProgress;
@property (nonatomic, weak)   id<MSSyncContextDataSource> dataSource;
@property (nonatomic, weak)   id<MSSyncContextDelegate> delegate;
@property (nonatomic)         MSTableOperationTypes type;
@property (nonatomic, strong) NSString *guid;

- (NSDictionary *) serialize;

-(id) initWithItem:(NSDictionary *)item;

/// Operation helper for tables

typedef NS_OPTIONS(NSUInteger, MSCondenseAction) {
    MSCondenseRemove = 0,
    MSCondenseKeep,
    MSCondenseToDelete,
    MSCondenseNotSupported,
    MSCondenseAddNew
};

+ (MSCondenseAction) condenseAction:(MSTableOperationTypes)newAction withExistingOperation:(MSTableOperation *)operation;

@end

