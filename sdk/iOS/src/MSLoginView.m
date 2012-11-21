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

#import "MSLoginView.h"
#import "MSClientConnection.h"


#pragma mark * MSLoginViewErrorDomain


NSString *const MSLoginViewErrorDomain = @"com.Microsoft.WindowsAzureMobileServices.LoginViewErrorDomain";


#pragma mark * UserInfo Request and Response Keys


NSString *const MSLoginViewErrorResponseData = @"com.Microsoft.WindowsAzureMobileServices.LoginViewErrorResponseData";


#pragma mark * MSLoginController Private Interface


@interface MSLoginView() <UIWebViewDelegate>

// Private instance properties
@property (nonatomic, strong, readwrite) UIWebView *webView;
@property (nonatomic, strong, readwrite) NSURL* currentURL;
@property (nonatomic, strong, readwrite) NSString* endURLString;
@property (nonatomic, copy, readwrite)   MSLoginViewBlock completion;

@end


#pragma mark * MSLoginController Implementation


@implementation MSLoginView

@synthesize client = client_;
@synthesize startURL = startURL_;
@synthesize endURL = endURL_;
@synthesize toolbar = toolbar_;
@synthesize showToolbar = showToolbar_;
@synthesize toolbarPosition = toolbarPosition_;
@synthesize activityIndicator = activityIndicator_;
@synthesize webView = webView_;
@synthesize currentURL = currentURL_;
@synthesize endURLString = endURLString_;
@synthesize completion = completion_;


#pragma mark * Public Initializer and Dealloc Methods


-(id) initWithFrame:(CGRect)frame
             client:(MSClient *)client
           startURL:(NSURL *)startURL
             endURL:(NSURL *)endURL
         completion:(MSLoginViewBlock)completion;
{
    self = [super initWithFrame:frame];
    if (self) {
        
        // Capture all of the initializer values as properties
        client_ = client;
        startURL_ = startURL;
        endURL_ = endURL;
        endURLString_ = endURL_.absoluteString;        
        completion_ = [completion copy];
        
        // Create the activity indicator and toolbar
        activityIndicator_ = [[UIActivityIndicatorView alloc]
                              initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhite];
        toolbar_ = [self createToolbar:activityIndicator_];
        [self addSubview:toolbar_];
        
        // Set the toolbar defaults
        showToolbar_ = YES;
        toolbarPosition_ = UIToolbarPositionBottom;
        
        // Create the webview
        webView_ = [[UIWebView alloc] init];
        webView_.delegate = self;
        [self addSubview:webView_];
        
        // Call setViewFrames to update the subview frames
        [self setViewFrames];
        
        // Start the first request
        NSURLRequest *firstRequest = [NSURLRequest requestWithURL:startURL];
        [webView_ loadRequest:firstRequest];
    }
    return self;
}

- (void) dealloc
{
    webView_.delegate = nil;
}


#pragma mark * Public ShowToolbar Property Accessor Methods


-(void) setShowToolbar:(BOOL)showToolbar
{
    if (showToolbar != showToolbar_) {
        showToolbar_ = showToolbar;
        [self setViewFrames];
    }
}


#pragma mark * Public ToolbarPosition Property Accessor Methods


-(void) setToolbarPosition:(UIToolbarPosition)toolbarPosition
{
    if (toolbarPosition != toolbarPosition_) {
        toolbarPosition_ = toolbarPosition;
        [self setViewFrames];
    }
}


#pragma mark * UIWebViewDelegate Private Implementation


-(BOOL) webView:(UIWebView *)webView
            shouldStartLoadWithRequest:(NSURLRequest *)request
            navigationType:(UIWebViewNavigationType)navigationType;
{
    // The UIWebView by default will make requests itself.  This doesn't work
    // for the MSLoginView scenarios  because it needs access to the
    // response, which UIWebview doesn't allow. Therefore, we will
    // make all requests using an MSClientConnection and then have the
    // WebView simply load the response data that we get. In this method, we
    // have to distinguish between when the user clicked on a link and when
    // we explicilty asked the WebView to load the response data
    
    BOOL shouldLoad = NO;
    
    NSURL *requestURL = request.URL;
    NSString *requestURLString = request.URL.absoluteString;
    
    // Check if we've reached the end URL and we're done
    if ([requestURLString rangeOfString:self.endURLString].location == 0) {
        [self callCompletion:requestURL orError:nil];
    }
    else {
        
        // Check if this request has already been made using the
        // MSClientConnection so that we can go ahead and load the response
        // in the webView
        if ([requestURL isEqual:self.currentURL] ||
             navigationType == UIWebViewNavigationTypeOther) {
            shouldLoad = YES;
        }
        else {
            
            // A user clicked on a link/form from the current page, so we
            // need to use an MSClientConnection to make the request
            [self makeRequest:request];
        }
    }
    
    return shouldLoad;
}

-(void) webView:(UIWebView *)webView didFailLoadWithError:(NSError *)error
{
    [self callCompletion:nil orError:error];
}

-(void) webViewDidStartLoad:(UIWebView *)webView
{
    [self.activityIndicator startAnimating];
}

-(void) webViewDidFinishLoad:(UIWebView *)webView
{
    [self.activityIndicator stopAnimating];
}


#pragma mark * Private Methods


-(UIToolbar *) createToolbar:(UIActivityIndicatorView *)activityIndicatorView
{
    // Create the toolbar
    UIToolbar *toolbar = [[UIToolbar alloc] init];
    toolbar.barStyle = UIBarStyleDefault;
    
    // Create the three toolbar items
    UIBarButtonItem *activity = [[UIBarButtonItem alloc]
                                 initWithCustomView:activityIndicatorView];
    UIBarButtonItem *cancel = [[UIBarButtonItem alloc]
                        initWithBarButtonSystemItem:UIBarButtonSystemItemCancel
                        target:self
                        action:@selector(cancel:)];
    UIBarButtonItem *space = [[UIBarButtonItem alloc]
                 initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace
                 target:self
                 action:nil];
    
    // Add the items to the toolbar
    [toolbar setItems:[NSArray arrayWithObjects:cancel, space, activity, nil]];
    
    return toolbar;
}

- (IBAction)cancel:(id)sender
{
    [self.activityIndicator stopAnimating];
    
    NSError *error =[self errorForLoginViewCanceled];
    [self callCompletion:nil orError:error];
}

-(void) setViewFrames
{
    CGRect rootFrame = self.frame;
    CGFloat toolBarHeight = 44;
    CGFloat webViewHeightOffset = 0;
    CGFloat webViewYOffset = 0;
    
    // Set the toolbar's frame if it should be displayed
    if (self.showToolbar) {
        if (self.toolbarPosition == UIToolbarPositionTop) {
            self.toolbar.frame = CGRectMake(0,
                                            0,
                                            rootFrame.size.width,
                                            toolBarHeight);
            self.toolbar.autoresizingMask = UIViewAutoresizingFlexibleWidth |
            UIViewAutoresizingFlexibleBottomMargin;
            
            // Offset the webView to make room for the toolbar at the top
            webViewYOffset = toolBarHeight;
        }
        else {
            self.toolbar.frame = CGRectMake(0,
                                            rootFrame.size.height-toolBarHeight,
                                            rootFrame.size.width,
                                            toolBarHeight);
            self.toolbar.autoresizingMask = UIViewAutoresizingFlexibleWidth |
            UIViewAutoresizingFlexibleTopMargin;
            
            // Shorten the webview to make room for the toolbar at the bottom
            webViewHeightOffset = toolBarHeight;
        }
        
        
    }
    
    // Set the webview's frame
    self.webView.frame = CGRectMake(0,
                                    webViewYOffset,
                                    rootFrame.size.width,
                                    rootFrame.size.height - webViewHeightOffset);
    self.webView.autoresizingMask = UIViewAutoresizingFlexibleWidth |
    UIViewAutoresizingFlexibleHeight;
}

-(void) makeRequest:(NSURLRequest *)request
{
    // Create the callback
    MSResponseBlock responseCompletion =
    ^(NSHTTPURLResponse *response, NSData *data, NSError *error)
    {
        [self.activityIndicator stopAnimating];

        if (!error && response.statusCode >= 400) {
            error = [self errorForLoginViewFailedWithResponse:response
                                                      andData:data];
        }
        
        
        // If the connection had an error or we got an error status code,
        // then we call the completion block
        if (error) {
            [self callCompletion:nil orError:error];
        }
        else {
            
            // The request was successful, so ask the WebView to load the
            // response data
            self.currentURL = response.URL;
            [self.webView loadData:data
                          MIMEType:response.MIMEType
                  textEncodingName:response.textEncodingName
                           baseURL:self.currentURL];
        }
    };
    
    // Make the connection and start it
    MSClientConnection  *connection = [[MSClientConnection alloc]
                                       initWithRequest:request
                                       withClient:self.client
                                       completion:responseCompletion];
    
    [self.activityIndicator startAnimating];
    
    [connection startWithoutFilters];
}

-(void) callCompletion:(NSURL *)endURL orError:(NSError *)error
{
    // Call the completion and then set it to nil so that it does not
    // get called twice.
    if (self.completion) {
        self.completion(endURL, error);
        self.completion = nil;
    }
}


#pragma mark * Private NSError Generation Methods


-(NSError *) errorForLoginViewFailedWithResponse:(NSURLResponse *)response
                                         andData:(NSData *)data
{
    NSDictionary *userInfo = @{
        MSErrorResponseKey:response,
        MSLoginViewErrorResponseData : data
    };
    
    return [self errorWithDescriptionKey:@"The login operation failed."
                            andErrorCode:MSLoginViewFailed
                             andUserInfo:userInfo];
}

-(NSError *) errorForLoginViewCanceled
{
    return [self errorWithDescriptionKey:@"The login operation was canceled."
                            andErrorCode:MSLoginViewFailed
                             andUserInfo:nil];
}

-(NSError *) errorWithDescriptionKey:(NSString *)descriptionKey
                        andErrorCode:(NSInteger)errorCode
                         andUserInfo:(NSDictionary *)userInfo
{
    NSString *description = NSLocalizedString(descriptionKey, nil);
    
    if (!userInfo) {
        userInfo = [[NSMutableDictionary alloc] init];
    }
    else {
        userInfo = [userInfo mutableCopy];
    }
    
    [userInfo setValue:description forKey:NSLocalizedDescriptionKey];
    
    return [NSError errorWithDomain:MSErrorDomain
                               code:errorCode
                           userInfo:userInfo];
}

@end
