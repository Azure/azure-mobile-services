// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSMultiRequestTestFilter.h"

@implementation MSMultiRequestTestFilter

-(MSMultiRequestTestFilter *) init {
    self = [super init];
    if (self)
    {
        _testFilters = @[];
        _currentIndex = 0;
        _actualRequests = @[];
    }
    
    return self;
}

+(NSString *) testDataWithItemCount:(int)itemCount startId:(int)start
{
    NSMutableString *string = [NSMutableString stringWithString:@"["];
    for (int i = start; i < start + itemCount; i++)
    {
        [string appendFormat:@"{\"id\": \"%d\"},", i];
    }
    [string appendString:@"]"];
    return string;
}

+(MSMultiRequestTestFilter *) testFilterWithStatusCodes:(NSArray *)statusCodes data:(NSArray *)data appendEmptyRequest:(BOOL)appendEmptyRequest {
    MSMultiRequestTestFilter *multiFilter = [MSMultiRequestTestFilter new];
    NSMutableArray *filters = [NSMutableArray new];
    NSMutableArray *requests = [NSMutableArray new];
    
    if (appendEmptyRequest) {
        NSMutableArray *mutableStatusCodes = [statusCodes mutableCopy];
        [mutableStatusCodes addObject:@200];
        statusCodes = mutableStatusCodes;
        NSMutableArray *mutableData = [data mutableCopy];
        [mutableData addObject:@"[]"];
        data = mutableData;
    }
    
    for (int i=0;i < statusCodes.count; i++) {
        NSNumber *number = (NSNumber *)statusCodes[i];
        MSTestFilter *filter = [MSTestFilter testFilterWithStatusCode:number.integerValue data:data[i]];
        filter.ignoreNextFilter = YES;
        __block NSURLRequest *actualRequest = [NSURLRequest new];
        filter.onInspectRequest = ^(NSURLRequest *request) {
            actualRequest = request;
            NSLog(@"%@", request.URL.absoluteString);
            [requests addObject:actualRequest];
            return request;
        };
        [filters addObject:filter];
    }
    multiFilter.testFilters = filters;
    multiFilter.actualRequests = requests;
    return multiFilter;
}

-(void) handleRequest:(NSURLRequest *)request
                 next:(MSFilterNextBlock)next
             response:(MSFilterResponseBlock)response
{
    MSTestFilter *currentFilter = self.testFilters[self.currentIndex];
    self.currentIndex++;
    
    [currentFilter handleRequest:request next:next response:response];
}

@end