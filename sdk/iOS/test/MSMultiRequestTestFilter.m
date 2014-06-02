// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSMultiRequestTestFilter.h"

@implementation MSMultiRequestTestFilter

-(MSMultiRequestTestFilter *) init {
    self = [super init];
    if (self)
    {
        _testFilters = [NSArray array];
        _currentIndex = 0;
    }
    
    return self;
}


-(void) handleRequest:(NSURLRequest *)request
                 next:(MSFilterNextBlock)next
             response:(MSFilterResponseBlock)response
{
    MSTestFilterData *currentFilter = self.testFilters[self.currentIndex];
    self.currentIndex++;
    
    // Replace the request if we have one to replace it with
    if (currentFilter.onInspectRequest) {
        request = currentFilter.onInspectRequest(request);
    }
    else if (currentFilter.requestToUse) {
        request = currentFilter.requestToUse;
    }
    
    if (currentFilter.ignoreNextFilter) {
        
        // |ignoreNextFilter| is set so just return a response/data or
        // error right away.
        if (currentFilter.errorToUse) {
            
            // A mock error was provided, so we'll call onError
            // with it
            response(nil, nil, currentFilter.errorToUse);
        }
        else {
            
            // Otherwise we'll assume a mock response/data are available
            response(currentFilter.responseToUse, currentFilter.dataToUse, nil);
        }
    }
    else {
        
        // We'll call into the next filter, but we want to be able to
        // replace the response/data or error when the server replies so
        // we'll wrap the onResponse and onError callbacks with our own
        MSFilterResponseBlock localOnResponse =
        [^(NSHTTPURLResponse *res, NSData *data, NSError *error){
            
            if(currentFilter.errorToUse) {
                response(nil, nil, currentFilter.errorToUse);
            }
            else {
                
                if (currentFilter.responseToUse) {
                    res = currentFilter.responseToUse;
                }
                
                if (currentFilter.dataToUse) {
                    data = currentFilter.dataToUse;
                }
                
                response(res, data, error);
            }
            
        } copy];
        
        // Call the next filter
        next(request, localOnResponse);
    }
}

@end