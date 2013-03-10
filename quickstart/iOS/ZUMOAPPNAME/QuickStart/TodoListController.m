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

#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>
#import "TodoListController.h"
#import "TodoService.h"


#pragma mark * Private Interface


@interface TodoListController ()

// Private properties
@property (strong, nonatomic) TodoService *todoService;

@end


#pragma mark * Implementation


@implementation TodoListController

@synthesize todoService;
@synthesize itemText;
@synthesize activityIndicator;


#pragma mark * UIView methods


- (void)viewDidLoad
{
    [super viewDidLoad];
    
    // Create the todoService - this creates the Mobile Service client inside the wrapped service
    self.todoService = [[TodoService alloc]init];

    // Set the busy method 
    UIActivityIndicatorView *indicator = self.activityIndicator;
    self.todoService.busyUpdate = ^(BOOL busy) {
        if (busy) {
            [indicator startAnimating];
        } else {
            [indicator stopAnimating];
        }
    };
    
    [self.todoService refreshDataOnSuccess:^{
        [self.tableView reloadData];
    }];
}


#pragma mark * UITableView methods


- (void) tableView:(UITableView *)tableView
                commitEditingStyle:(UITableViewCellEditingStyle)editingStyle
                 forRowAtIndexPath:(NSIndexPath *)indexPath
{
    // Find item that was commited for editing (completed)
    NSDictionary *item = [self.todoService.items objectAtIndex:indexPath.row];
    
    // Change the appearance to look greyed out until we remove the item
    UILabel *label = (UILabel *)[[tableView cellForRowAtIndexPath:indexPath] viewWithTag:1];
    label.textColor = [UIColor grayColor];
    
    // Ask the todoService to set the item's complete value to YES, and remove the row if successful
    [self.todoService completeItem:item completion:^(NSUInteger index) {
        
        // Remove the row from the UITableView
        NSIndexPath *indexPath = [NSIndexPath indexPathForRow:index inSection:0];
        [self.tableView deleteRowsAtIndexPaths:@[ indexPath ]
                              withRowAnimation:UITableViewRowAnimationTop];
    }];
}

-(UITableViewCellEditingStyle)tableView:(UITableView *)tableView editingStyleForRowAtIndexPath:(NSIndexPath *)indexPath
{
    // Find the item that is about to be edited
    NSDictionary *item = [self.todoService.items objectAtIndex:indexPath.row];
    
    // If the item is complete, then this is just pending upload. Editing is not allowed
    if ([[item objectForKey:@"complete"] boolValue]) {
        return UITableViewCellEditingStyleNone;
    }
    
    // Otherwise, allow the delete button to appear
    return UITableViewCellEditingStyleDelete;
}

-(NSString *)tableView:(UITableView *)tableView titleForDeleteConfirmationButtonForRowAtIndexPath:(NSIndexPath *)indexPath
{
    // Customize the Delete button to say "complete"
    return @"complete";
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *CellIdentifier = @"Cell";
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    if (cell == nil) {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:CellIdentifier];
    }
    
    // Set the label on the cell and make sure the label color is black (in case this cell
    // has been reused and was previously greyed out
    UILabel *label = (UILabel *)[cell viewWithTag:1];
    label.textColor = [UIColor blackColor];
    NSDictionary *item = [self.todoService.items objectAtIndex:indexPath.row];
    label.text = [item objectForKey:@"text"];
    
    return cell;
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    // Always a single section
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
    // Return the number of items in the todoService items array
    return [self.todoService.items count];
}


#pragma mark * UITextFieldDelegate methods


-(BOOL) textFieldShouldReturn:(UITextField *)textField
{
    [textField resignFirstResponder];
    return YES;
}


#pragma mark * UI Actions



- (IBAction)onAdd:(id)sender
{
    if (itemText.text.length  == 0) {
        return;
    }
    
    NSDictionary *item = @{ @"text" : itemText.text, @"complete" : @(NO) };
    [self.todoService addItem:item completion:^(NSUInteger index){
        NSIndexPath *indexPath = [NSIndexPath indexPathForRow:index inSection:0];
        [self.tableView insertRowsAtIndexPaths:@[ indexPath ]
                              withRowAnimation:UITableViewRowAnimationTop];
    }];
    
    itemText.text = @"";
}

@end
