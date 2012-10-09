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

#import "MSLoginViewController.h"

@interface MSLoginViewController ()

// Private instance properties
@property (nonatomic, strong, readwrite) NSURL* startUrl;
@property (nonatomic, strong, readwrite) NSURL* endUrl;
@property (nonatomic, strong, readwrite) MSEndUrlNavigatedTo onSuccess;
@property (nonatomic, strong, readwrite) MSNavigationCancelled onCancel;
@property (nonatomic, strong, readwrite) MSErrorBlock onError;

- (IBAction)cancel:(id)sender;

@end

@implementation MSLoginViewController

@synthesize startUrl = _startUrl;
@synthesize endUrl = _endUrl;
@synthesize onError = _onError;
@synthesize onCancel = _onCancel;
@synthesize onSuccess = _onSuccess;

UIWebView *hostedWebView;
NSError *lastError;

- (id) initWithStartUrl:(NSURL *)startUrl
                 endUrl:(NSURL *)endUrl
              onSuccess:(MSEndUrlNavigatedTo)onSuccess
               onCancel:(MSNavigationCancelled)onCancel
                onError:(MSErrorBlock)onError {
    _startUrl = startUrl;
    _endUrl = endUrl;
    _onSuccess = onSuccess;
    _onCancel = onCancel;
    _onError = onError;
    
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];

    hostedWebView.delegate = self;
    
    NSURLRequest *request = [NSURLRequest requestWithURL:self.startUrl];
    [hostedWebView loadRequest:request];
}

- (void) loadView {
    // TODO - make the view expand to fit
    // TODO - make cancel button visibleb
    UIView *view = [[UIView alloc] initWithFrame:CGRectMake(0,0,320,480)];
    
    hostedWebView = [[UIWebView alloc] initWithFrame:CGRectMake(0,0, 320, 416)];
    [view addSubview:hostedWebView];
    
    UIToolbar *toolbar = [[UIToolbar alloc] initWithFrame:CGRectMake(0, 416, 320, 480)];
    
    UIBarButtonItem *button = [[UIBarButtonItem alloc] initWithTitle:@"cancel"
                                                               style:UIBarButtonItemStyleBordered
                                                              target:self
                                                              action:@selector(cancel:)];
    [toolbar setItems:@[button] animated:YES];
    
    [view addSubview:toolbar];
    
    self.view = view;
}

- (void) viewDidAppear:(BOOL)animated {
    if (lastError) {
        if (self.onError)
            self.onError(lastError);
        lastError = nil;
    }
}

- (IBAction)cancel:(id)sender {
    self.onCancel();
}

#pragma mark * UIWebViewDelegate methods

- (BOOL) webView:(UIWebView *)webView shouldStartLoadWithRequest:(NSURLRequest *)request navigationType:(UIWebViewNavigationType)navigationType {    
    if ([request.URL.absoluteString rangeOfString:self.endUrl.absoluteString].location == 0) {
        if (self.onSuccess)
            self.onSuccess(request.URL);
        return NO;
    }
        
    return YES;
}

- (void) webView:(UIWebView *)webView didFailLoadWithError:(NSError *)error {
    if (self.onError)
        self.onError(error);
}

- (void) webViewDidFinishLoad:(UIWebView *)webView {
    NSString* body = [webView stringByEvaluatingJavaScriptFromString:@"document.body.innerText"];
    NSError *error;
    id json = [NSJSONSerialization JSONObjectWithData:[body dataUsingEncoding:NSUTF8StringEncoding] options:0 error:&error];
    int statusCode = [[json objectForKey:@"code"] intValue];
    if (!error && statusCode >= 400) {
        // The last error is set here and will cause onError callback to be called only in viewDidAppear
        // to avoid overallaping UI animations.
        lastError = [NSError errorWithDomain:MSErrorDomain code:MSLoginFailed userInfo:json];
    }
}

@end