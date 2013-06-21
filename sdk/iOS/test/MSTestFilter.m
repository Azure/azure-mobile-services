// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSTestFilter.h"

@implementation MSTestFilter

@synthesize requestToUse = requestToUse_;
@synthesize onInspectRequest = onInspectRequest_;
@synthesize responseToUse = responseToUse_;
@synthesize dataToUse = dataToUse_;
@synthesize errorToUse = errorToUse_;
@synthesize ignoreNextFilter = ignoreNextFilter_;

-(void) handleRequest:(NSURLRequest *)request
               next:(MSFilterNextBlock)next
           response:(MSFilterResponseBlock)response
{
    
    // Replace the request if we have one to replace it with
    if (self.onInspectRequest) {
        request = self.onInspectRequest(request);
    }
    else if (self.requestToUse) {
        request = self.requestToUse;
    }
    
    if (self.ignoreNextFilter) {
        
        // |ignoreNextFilter| is set so just return a response/data or
        // error right away.
        if (self.errorToUse) {
            
            // A mock error was provided, so we'll call onError
            // with it
            response(nil, nil, self.errorToUse);
        }
        else {
            
            // Otherwise we'll assume a mock response/data are available
            response(self.responseToUse, self.dataToUse, nil);
        }
    }
    else {
        
        // We'll call into the next filter, but we want to be able to
        // replace the response/data or error when the server replies so
        // we'll wrap the onResponse and onError callbacks with our own
        MSFilterResponseBlock localOnResponse =
        [^(NSHTTPURLResponse *res, NSData *data, NSError *error){
            
            if(self.errorToUse) {
                response(nil, nil, self.errorToUse);
            }
            else {
            
                if (self.responseToUse) {
                    res = self.responseToUse;
                }
                
                if (self.dataToUse) {
                    data = self.dataToUse;
                }
                
                response(res, data, error);
            }
            
        } copy];

        // Call the next filter
        next(request, localOnResponse);
    }
}

@end
