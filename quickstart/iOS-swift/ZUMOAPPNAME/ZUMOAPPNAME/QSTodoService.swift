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

import Foundation

class QSTodoService : NSObject, MSFilter {
    
    var busyCount = 0
    var items = NSMutableArray()
    var client: MSClient! = nil
    var table: MSTable! = nil
    var busyUpdate: ((Bool)->Void)! = nil
    
    class var defaultService : QSTodoService {
        struct Singleton {
            static let instance = QSTodoService()
        }
        return Singleton.instance
    }
    
    convenience init() {
        let urlString = "ZUMOAPPURL"
        let applicationKey = "ZUMOAPPKEY"
        var client: MSClient = MSClient(applicationURLString:urlString, applicationKey:applicationKey)

        self.init(client: client)
    }

    init(client: MSClient) {
        super.init()
        
        self.client = client.clientWithFilter(self)
        self.table = self.client.tableWithName("TodoItem")
    }
    
    
    func refreshDataOnSuccess(completion:()->Void) {
        // Create a predicate that finds items where complete is false
        var predicate = NSPredicate(format:"complete == NO")
        
        // Query the TodoItem table and update the items property with the results from the service
        table.readWithPredicate(predicate, completion:{results, totalCount, error in
            self.logErrorIfNotNil(error)
            
            self.items = NSMutableArray(array:results)
            
            // Let the caller know that we finished
            completion();
        })

    }
    
    func addItem(item:NSDictionary, completion:(Int)->Void) {
        // Insert the item into the TodoItem table and add to the items array on completion
        table.insert(item, completion:{result, error in
            self.logErrorIfNotNil(error)
            
            var index = self.items.count
            self.items.insertObject(result, atIndex:index)
            
            // Let the caller know that we finished
            completion(index);
        })

    }
    
    func completeItem(item:NSDictionary, completion:(Int)->Void) {
        // Cast the public items property to the mutable type (it was created as mutable)
        var mutableItems = items as NSMutableArray
        
        // Set the item to be complete (we need a mutable copy)
        var mutable = item.mutableCopy() as NSMutableDictionary
        mutable.setObject(NSNumber.numberWithBool(true), forKey:"complete")
        
        // Replace the original in the items array
        var index = items.indexOfObjectIdenticalTo(item)
        mutableItems.replaceObjectAtIndex(index, withObject:mutable)
        
        // Update the item in the TodoItem table and remove from the items array on completion
        table.update(mutable, completion:{item, error in
            self.logErrorIfNotNil(error)
            var index = self.items.indexOfObjectIdenticalTo(mutable)
            if (index != Foundation.NSNotFound) {
                mutableItems.removeObjectAtIndex(index)
            }
            
            // Let the caller know that we have finished
            completion(index);
        })
    }

    func busy(busy: Bool) {
        // assumes always executes on UI thread
        if (busy) {
            if (busyCount == 0 && busyUpdate != nil) {
                busyUpdate(true)
            }
            busyCount++
        }
        else {
            if (busyCount == 1 && busyUpdate != nil) {
                busyUpdate(false);
            }
            busyCount--
        }
    }
    
    func logErrorIfNotNil(error:NSError?) {
        if (error) {
            NSLog("ERROR %@", error!)
        }
    }
    
    // #pragma mark * MSFilter methods
    
    func handleRequest(request: NSURLRequest, next: MSFilterNextBlock, response: MSFilterResponseBlock) {
        // A wrapped response block that decrements the busy counter
        let wrappedResponse: MSFilterResponseBlock = {innerResponse, data, error in
            self.busy(false)
            response(innerResponse, data, error);
        }
        
        // Increment the busy counter before sending the request
        busy(true)
        next(request, wrappedResponse)
    }
}
