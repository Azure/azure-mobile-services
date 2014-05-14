//
//  MSSyncContextReadResult.h
//  WindowsAzureMobileServices
//
//  Created by Phillip Van Nortwick on 4/29/14.
//  Copyright (c) 2014 Windows Azure. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface MSSyncContextReadResult : NSObject

@property (nonatomic, readonly) NSInteger totalCount;
@property (nonatomic, readonly, strong) NSArray *items;

- initWithCount:(NSInteger)count items:(NSArray *)items;

@end
