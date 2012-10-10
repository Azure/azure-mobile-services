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
@property (nonatomic, copy, readwrite) MSEndUrlNavigatedTo onSuccess;
@property (nonatomic, copy, readwrite) MSNavigationCancelled onCancel;
@property (nonatomic, copy, readwrite) MSErrorBlock onError;

- (IBAction)cancel:(id)sender;

@end

@implementation MSLoginViewController

@synthesize startUrl = startUrl_;
@synthesize endUrl = endUrl_;
@synthesize onError = onError_;
@synthesize onCancel = onCancel_;
@synthesize onSuccess = onSuccess_;

UIWebView *hostedWebView;

- (void) dealloc {
    if (hostedWebView) {
        hostedWebView.delegate = nil;
    }
}

- (id) initWithStartUrl:(NSURL *)startUrl
                 endUrl:(NSURL *)endUrl
              onSuccess:(MSEndUrlNavigatedTo)onSuccess
               onCancel:(MSNavigationCancelled)onCancel
                onError:(MSErrorBlock)onError {
    self = [super initWithNibName:nil bundle:nil];
    if (self) {
        startUrl_ = startUrl;
        endUrl_ = endUrl;
        self.onSuccess = onSuccess;
        self.onCancel = onCancel;
        self.onError = onError;
    }
    
    return self;
}

- (void) loadView {
    [super loadView];
    
    UIView *view = [[UIView alloc] initWithFrame:CGRectMake(0,0,0,0)];
    view.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
    hostedWebView = [[UIWebView alloc] initWithFrame:CGRectMake(0,0,0,0)];
    hostedWebView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
    hostedWebView.delegate = self;
    NSURLRequest *request = [NSURLRequest requestWithURL:self.startUrl];
    [hostedWebView loadRequest:request];
    [view addSubview:hostedWebView];
    UIBarButtonItem *button = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:(UIBarButtonSystemItemCancel)
                                                                            target:self
                                                                            action:@selector(cancel:)];
    self.navigationItem.leftBarButtonItem = button;
    self.view = view;
}

- (IBAction)cancel:(id)sender {
    self.onCancel();
}

#pragma mark * UIWebViewDelegate methods

- (BOOL) webView:(UIWebView *)webView shouldStartLoadWithRequest:(NSURLRequest *)request navigationType:(UIWebViewNavigationType)navigationType {    
    if ([request.URL.absoluteString rangeOfString:self.endUrl.absoluteString].location == 0) {
        if (self.onSuccess) {
            self.onSuccess(request.URL);
        }
    
        return NO;
    }
        
    return YES;
}

- (void) webView:(UIWebView *)webView didFailLoadWithError:(NSError *)error {
    if (self.onError) {
        self.onError(error);
    }
}

- (void) webViewDidFinishLoad:(UIWebView *)webView {
    if (self.onError) {
        NSString* body = [webView stringByEvaluatingJavaScriptFromString:@"document.body.innerText"];
        NSError *error;
        id json = [NSJSONSerialization JSONObjectWithData:[body dataUsingEncoding:NSUTF8StringEncoding] options:0 error:&error];
        int statusCode = [[json objectForKey:@"code"] intValue];
        if (!error && statusCode >= 400) {
            self.onError([NSError errorWithDomain:MSErrorDomain
                                             code:MSLoginFailed
                                         userInfo:@{
                        NSLocalizedDescriptionKey:NSLocalizedString(@"Authentication failed.", nil),
                                      @"response":body}]);
        }
    }
}

@end