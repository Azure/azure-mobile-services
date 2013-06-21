// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>

@interface ZumoTestHelpViewController : UIViewController
{
    IBOutlet UILabel *titleLabel;
    IBOutlet UITextView *helpText;
    NSString *strTitle;
    NSString *strHelp;
}

- (IBAction)close:(id)sender;
- (void)setTitle:(NSString *)theTitle andHelpText:(NSString *)theHelpText;

@end
