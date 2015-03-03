// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import <CoreData/CoreData.h>

@interface TodoItem : NSManagedObject

@property (nonatomic, retain) NSString * id;
@property (nonatomic, retain) NSString * ms_version;
@property (nonatomic, retain) NSNumber * sort;
@property (nonatomic, retain) NSString * text;

@end
