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
import UIKit

class ToDoTableViewController: UITableViewController, ToDoItemDelegate {
    
    var records = [NSDictionary]()
    var table : MSTable?
    
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view, typically from a nib.
        
        let client = MSClient(applicationURLString: "ZUMOAPPURL", applicationKey: "ZUMOAPPKEY")
                
        self.table = client.tableWithName("TodoItem")!
        self.refreshControl.addTarget(self, action: "onRefresh:", forControlEvents: UIControlEvents.ValueChanged)
        
        self.refreshControl.beginRefreshing()
        self.onRefresh(self.refreshControl)
    }
    
    func onRefresh(sender: UIRefreshControl!) {
        let predicate = NSPredicate(format: "complete == NO")
        
        UIApplication.sharedApplication().networkActivityIndicatorVisible = true
        self.table!.readWithPredicate(predicate) {
            result, error  in
            
            UIApplication.sharedApplication().networkActivityIndicatorVisible = false
            if error {
                println("Error: " + error.description)
                return
            }
            
            self.records = result.items as [NSDictionary]
            println("Information: retrieved %d records", result.items.count)
            
            self.tableView.reloadData()
            self.refreshControl.endRefreshing()
        }
    }
    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
    }
    
    // Table
    
    override func tableView(tableView: UITableView!, canEditRowAtIndexPath indexPath: NSIndexPath!) -> Bool
    {
        return true
    }
    
    override func tableView(tableView: UITableView!, editingStyleForRowAtIndexPath indexPath: NSIndexPath!) -> UITableViewCellEditingStyle
    {
        return UITableViewCellEditingStyle.Delete
    }
    
    override func tableView(tableView: UITableView!, titleForDeleteConfirmationButtonForRowAtIndexPath indexPath: NSIndexPath!) -> String!
    {
        return "Complete"
    }
    
    override func tableView(tableView: UITableView!, commitEditingStyle editingStyle: UITableViewCellEditingStyle, forRowAtIndexPath indexPath: NSIndexPath!)
    {
        let record = self.records[indexPath.row]
        let completedItem = record.mutableCopy() as NSMutableDictionary
        completedItem["complete"] = true
        
        UIApplication.sharedApplication().networkActivityIndicatorVisible = true
        self.table!.update(completedItem) {
            (result, error) in
            
            UIApplication.sharedApplication().networkActivityIndicatorVisible = false
            if (error) {
                println("Error: " + error.description)
                return
            }
            
            self.records.removeAtIndex(indexPath.row)
            self.tableView.deleteRowsAtIndexPaths([indexPath], withRowAnimation: UITableViewRowAnimation.Automatic)
        }
    }
    
    override func tableView(tableView: UITableView!, numberOfRowsInSection section: Int) -> Int
    {
        return self.records.count
    }
    
    override func tableView(tableView: UITableView!, cellForRowAtIndexPath indexPath: NSIndexPath!) -> UITableViewCell! {
        let CellIdentifier = "Cell"
        
        let cell = tableView.dequeueReusableCellWithIdentifier(CellIdentifier, forIndexPath: indexPath) as UITableViewCell
        let item = self.records[indexPath.row]
        
        cell.textLabel.text = item["text"] as String
        cell.textLabel.textColor = UIColor.blackColor()
        
        return cell
    }
    
    // Navigation
    
    @IBAction func addItem(sender : AnyObject) {
        self.performSegueWithIdentifier("addItem", sender: self)
    }
    
    override func prepareForSegue(segue: UIStoryboardSegue!, sender: AnyObject!)
    {
        if segue.identifier == "addItem" {
            let todoController = segue.destinationViewController as ToDoItemViewController
            todoController.delegate = self
        }
    }
    
    
    // ToDoItemDelegate
    
    func didSaveItem(text: String)
    {
        if text.isEmpty {
            return
        }
        
        let itemToInsert = ["text": text, "complete": false]
        
        UIApplication.sharedApplication().networkActivityIndicatorVisible = true
        self.table!.insert(itemToInsert) {
            (item, error) in
            UIApplication.sharedApplication().networkActivityIndicatorVisible = false
            if error {
                println("Error: " + error.description)
            } else {
                self.records.append(item)
                self.tableView.reloadData()
            }
        }
    }
}
