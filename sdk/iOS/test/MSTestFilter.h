// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSFilter.h"

typedef NSURLRequest *(^MSInspectRequestBlock)(NSURLRequest *request);

@interface MSTestFilter : NSObject <MSFilter>

@property (nonatomic, strong)   NSURLRequest *requestToUse;
@property (nonatomic, copy)     MSInspectRequestBlock onInspectRequest;
@property (nonatomic, strong)   NSHTTPURLResponse *responseToUse;
@property (nonatomic, strong)   NSData *dataToUse;
@property (nonatomic, strong)   NSError *errorToUse;
@property (nonatomic)           BOOL ignoreNextFilter;

@end
