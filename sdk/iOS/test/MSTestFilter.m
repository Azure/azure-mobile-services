// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import "MSTestFilter.h"

@implementation MSTestFilter

@synthesize requestToUse = requestToUse_;
@synthesize onInspectRequest = onInspectRequest_;
@synthesize responseToUse = responseToUse_;
@synthesize dataToUse = dataToUse_;
@synthesize errorToUse = errorToUse_;
@synthesize ignoreNextFilter = ignoreNextFilter_;

-(void) handleRequest:(NSURLRequest *)request
               onNext:(MSFilterNextBlock)onNext
           onResponse:(MSFilterResponseBlock)onResponse
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
            onResponse(nil, nil, self.errorToUse);
        }
        else {
            
            // Otherwise we'll assume a mock response/data are available
            onResponse(self.responseToUse, self.dataToUse, nil);
        }
    }
    else {
        
        // We'll call into the next filter, but we want to be able to
        // replace the response/data or error when the server replies so
        // we'll wrap the onResponse and onError callbacks with our own
        MSFilterResponseBlock localOnResponse =
        [^(NSHTTPURLResponse *response, NSData *data, NSError *error){
            
            if(self.errorToUse) {
                onResponse(nil, nil, self.errorToUse);
            }
            else {
            
                if (self.responseToUse) {
                    response = self.responseToUse;
                }
                
                if (self.dataToUse) {
                    data = self.dataToUse;
                }
                
                onResponse(response, data, error);
            }
            
        } copy];

        // Call the next filter
        onNext(request, localOnResponse);
    }
}

@end
