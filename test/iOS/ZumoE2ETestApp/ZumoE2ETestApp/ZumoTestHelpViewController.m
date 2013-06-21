// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTestHelpViewController.h"

@interface ZumoTestHelpViewController ()

@end

@implementation ZumoTestHelpViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];

    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    // Do any additional setup after loading the view from its nib.
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (void)setTitle:(NSString *)theTitle andHelpText:(NSString *)theHelpText {
    self->strTitle = theTitle;
    self->strHelp = theHelpText;
    [self performSelector:@selector(updateUI) withObject:self afterDelay:0.1];
}

- (void)updateUI {
    [helpText setText:strHelp];
    [titleLabel setText:strTitle];
}

- (void)close:(id)sender {
    [self dismissViewControllerAnimated:YES completion:nil];
}

@end
