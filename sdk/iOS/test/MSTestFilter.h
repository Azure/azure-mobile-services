// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSFilter.h"

typedef NSURLRequest *(^MSInspectRequestBlock)(NSURLRequest *request);
typedef NSData *(^MSInspectResponseDataBlock)(NSURLRequest *request, NSData *data);

@interface MSTestFilter : NSObject <MSFilter>

+(MSTestFilter *)testFilterWithStatusCode:(NSInteger) statusCode;
+(MSTestFilter *)testFilterWithStatusCode:(NSInteger) statusCode data:(NSString *)data;

@property (nonatomic, strong)   NSURLRequest *requestToUse;
@property (nonatomic, copy)     MSInspectRequestBlock onInspectRequest;
@property (nonatomic, strong)   NSHTTPURLResponse *responseToUse;
@property (nonatomic, strong)   NSData *dataToUse;
@property (nonatomic, strong)   NSError *errorToUse;
@property (nonatomic)           BOOL ignoreNextFilter;
@property (nonatomic, copy)     MSInspectResponseDataBlock onInspectResponseData;

@end
