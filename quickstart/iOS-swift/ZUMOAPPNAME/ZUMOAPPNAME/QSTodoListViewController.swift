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

import UIKit

class QSTodoListViewController : UITableViewController {
    
    @IBOutlet weak var itemText : UITextField
    @IBOutlet weak var activityIndicator : UIActivityIndicatorView

    var todoService: QSTodoService = QSTodoService.defaultService
    var useRefreshControl: Bool = false
    
    // #pragma mark * UIView methods
    override func viewDidLoad() {
        super.viewDidLoad()
        
        // Set the busy method
        var indicator = activityIndicator

        todoService.busyUpdate = {(busy:Bool) in
            if (busy) {
                indicator.startAnimating()
            } else {
                indicator.stopAnimating()
            }
        }
        
        // add the refresh control to the table (iOS6+ only)
        addRefreshControl()
        
        // load the data
        refresh()
    }
    
    func refresh() {
        // only activate the refresh control if the feature is available
        if (useRefreshControl == true) {
            refreshControl.beginRefreshing()
        }
        
        todoService.refreshDataOnSuccess({() in
            if (self.useRefreshControl == true) {
                self.refreshControl.endRefreshing()
            }
            self.tableView.reloadData()
        })
    }

    // #pragma mark * UITableView methods
    override func tableView(tableView: UITableView, commitEditingStyle editingStyle: UITableViewCellEditingStyle, forRowAtIndexPath indexPath: NSIndexPath) {
        // Find item that was commited for editing (completed)
        let item = todoService.items[indexPath.row] as NSDictionary
        
        // Change the appearance to look greyed out until we remove the item
        let label = tableView.cellForRowAtIndexPath(indexPath).viewWithTag(1) as UILabel
        label.textColor = UIColor.grayColor()
        
        // Ask the todoService to set the item's complete value to YES, and remove the row if successful
        todoService.completeItem(item, completion:{index in
            let indexPath = NSIndexPath(forRow:index, inSection:0)
            tableView.deleteRowsAtIndexPaths([indexPath] , withRowAnimation:UITableViewRowAnimation.Top)
        })
    }

    override func tableView(tableView: UITableView, editingStyleForRowAtIndexPath indexPath: NSIndexPath) -> UITableViewCellEditingStyle {
        // Find the item that is about to be edited
        let item = todoService.items[indexPath.row] as NSDictionary
        
        // If the item is complete, then this is just pending upload. Editing is not allowed
        if (item["complete"].boolValue == true) {
            return UITableViewCellEditingStyle.None;
        }
        
        // Otherwise, allow the delete button to appear
        return UITableViewCellEditingStyle.Delete;
    }
    
    override func tableView(tableView: UITableView, titleForDeleteConfirmationButtonForRowAtIndexPath indexPath: NSIndexPath) -> String! {
        // Customize the Delete button to say "complete"
        return "complete"
    }

    override func tableView(tableView: UITableView, cellForRowAtIndexPath indexPath: NSIndexPath) -> UITableViewCell {
        let CellIdentifier = "Cell"
        var cell = tableView.dequeueReusableCellWithIdentifier(CellIdentifier) as UITableViewCell
        if (cell == nil) {
            cell = UITableViewCell(style: UITableViewCellStyle.Default, reuseIdentifier:CellIdentifier)
        }
        
        // Set the label on the cell and make sure the label color is black (in case this cell
        // has been reused and was previously greyed out
        let label = cell.viewWithTag(1) as UILabel
        label.textColor = UIColor.blackColor()
        let item = todoService.items[indexPath.row] as NSDictionary
        label.text = item["text"] as String
        
        return cell;
    }
    
    override func numberOfSectionsInTableView(tableView: UITableView) -> NSInteger {
        // Always a single section
        return 1;
    }
    
    override func tableView(tableView: UITableView, numberOfRowsInSection selection: NSInteger) -> NSInteger {
        // Return the number of items in the todoService items array
        return todoService.items.count
    }

    
    // #pragma mark * UITextFieldDelegate methods
    
    func textFieldShouldReturn(textField: UITextField) -> Bool {
        textField.resignFirstResponder()
        return true
    }

    
    // #pragma mark * UI Actions
    
    @IBAction func onAdd(sender: AnyObject) {
        if (itemText.text.bridgeToObjectiveC().length  == 0) {
            return
        }
        
        let item: Dictionary = ["text" : itemText.text as String, "complete" : false ]
        let view = tableView;
        todoService.addItem(item, completion:{index in
            let indexPath = NSIndexPath(forRow:index, inSection:0)
            view.insertRowsAtIndexPaths([indexPath], withRowAnimation:UITableViewRowAnimation.Top)
        })
        itemText.text = "";
    }

    // #pragma mark * iOS Specific Code
    
    // This method will add the UIRefreshControl to the table view if
    // it is available, ie, we are running on iOS 6+
    
    func addRefreshControl() {
        var refreshControlClass:AnyClass = NSClassFromString("UIRefreshControl")
        if (refreshControlClass != nil) {
            // the refresh control is available, let's add it
            refreshControl = UIRefreshControl()
            refreshControl.addTarget(self, action: "onRefresh:", forControlEvents: UIControlEvents.ValueChanged)
            useRefreshControl = true
        }
    }
    
    func onRefresh(sender: AnyObject) {
        refresh()
    }
}