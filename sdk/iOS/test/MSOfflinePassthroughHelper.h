//
//  MSOfflinePassthroughHelper.h
//  WindowsAzureMobileServices
//
//  Created by Phillip Van Nortwick on 2/2/14.
//  Copyright (c) 2014 Windows Azure. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "MSSyncContext.h"

@interface MSOfflinePassthroughHelper : NSObject <MSSyncContextDataSource, MSSyncContextDelegate>

@property (nonatomic) BOOL returnErrors;
@property (nonatomic) NSInteger upsertCalls;
@property (nonatomic, strong) NSMutableDictionary *data;
@end
