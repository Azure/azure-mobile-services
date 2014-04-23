// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSTableOperation.h"

/// A simple queue interface to abstract access from implementation. For now this may just
/// be an NSArray but long term this is liable to change
@interface MSOperationQueue : NSObject

-(NSArray *) getOperationsForTable:(NSString *) table item:(NSString *)item;

-(void) addOperation:(id)operation;
-(void) removeOperation:(id)operation;
-(id) peek;
-(NSUInteger) count;

@end
