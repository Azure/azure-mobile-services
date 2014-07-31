// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>

@class MSClient;
@class MSUser;


#pragma mark * Block Type Definitions

/// Callback logging in an end user. If there was an error or the login was
/// cancelled, *error* will be non-nil.
typedef void (^MSClientLoginBlock)(MSUser *user, NSError *error);

#pragma  mark * MSLoginController Public Interface


/// The *MSLoginController* class provides a UIViewController that can be
/// presented to allow an end user to authenticate with a Microsoft Azure Mobile
/// Service.
@interface MSLoginController : UIViewController

#pragma  mark * Public Initializer Methods

///@name Initializing the MSLoginController object
///@{

/// Initializes an *MSLoginController* instance with the given client, login
/// provider and completion block.
-(id)initWithClient:(MSClient *)client
           provider:(NSString *)provider
         completion:(MSClientLoginBlock)completion;

///@}


#pragma mark * Public Readonly Properties

///@name Properties
///@{

/// The client associated with this *MSLoginController*.
@property (nonatomic, strong, readonly)     MSClient *client;

/// The login provider associated with this *MSLoginController*.
@property (nonatomic, copy,   readonly)     NSString *provider;

/// The *UIActivityIndicatorView* on the *UIToolbar* associated with the
/// *MSLoginController*. If the toolbar is visible, the actvivity indicator
/// will become active whenever the end user is navigating to a new URL during
/// the login flow.
@property (nonatomic, strong, readonly)     UIActivityIndicatorView *activityIndicator;

/// The *UIToolbar* associated with the *MSLoginController*. The toolbar includes
/// a cancel button and an activity indicator. The visibility and placement of
/// the toolbar can be configured using the *showToolbar* and *toolbarPosition*
/// properties respectively.
@property (nonatomic, strong, readonly)     UIToolbar *toolbar;

///@}


#pragma mark * Public Readwrite Properties

///@name Customizing the Toolbar
///@{

/// Indicates if the *toolbar* show be displayed. By default, *showToolbar* is
/// YES.
@property (nonatomic, readwrite)            BOOL showToolbar;

/// Indicates if the *toolbar* should be positioned at the top or bottom of
/// the login view.  By default, the *toolbarPosition* is *UIToolbarPositionBottom*.
@property (nonatomic, readwrite)            UIToolbarPosition toolbarPosition;

///@}


@end
