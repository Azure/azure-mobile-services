//
//  MSSyncTableReadResult.m
//  WindowsAzureMobileServices
//
//  Created by Phillip Van Nortwick on 4/29/14.
//  Copyright (c) 2014 Windows Azure. All rights reserved.
//

#import "MSSyncContextReadResult.h"

@implementation MSSyncContextReadResult

@synthesize totalCount = totalCount_;
@synthesize items = items_;

- (id)initWithCount:(NSInteger)count items:(NSArray *)items;
{
    self = [super init];
    if (self) {
        totalCount_ = count;
        items_ = items;
    }
    
    return self;
}

@end
