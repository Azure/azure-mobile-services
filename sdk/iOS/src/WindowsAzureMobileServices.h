// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#ifndef WindowsAzureMobileServices_WindowsAzureMobileServices_h
#define WindowsAzureMobileServices_WindowsAzureMobileServices_h


#import "MSClient.h"
#import "MSTable.h"
#import "MSQuery.h"
#import "MSUser.h"
#import "MSFilter.h"
#import "MSError.h"
#import "MSTableOperation.h"
#import "MSSyncContext.h"
#import "MSSyncTable.h"
#import "MSTableOperationError.h"
#import "MSCoreDataStore.h"
#import "MSPush.h"
#import "MSDateOffset.h"
#import "MSQueryResult.h"
#import "MSSyncContextReadResult.h"
#import "MSQuery.h"
#import "MSOperationQueue.h"
#import "MSAPIConnection.h"
#import "MSAPIRequest.h"
#import "MSClientConnection.h"
#import "MSJSONSerializer.h"
#import "MSLogin.h"
#import "MSLoginSerializer.h"
#import "MSNaiveISODateFormatter.h"
#import "MSPredicateTranslator.h"
#import "MSPushRequest.h"
#import "MSQueuePullOperation.h"
#import "MSQueuePurgeOperation.h"
#import "MSSDKFeatures.h"
#import "MSTableConfigValue.h"
#import "MSTableConnection.h"
#import "MSTableRequest.h"
#import "MSURLBuilder.h"
#import "MSUserAgentBuilder.h"
#import "MSQueuePushOperation.h"

#if TARGET_OS_IPHONE
#import "MSLoginController.h"
#endif

#define WindowsAzureMobileServicesSdkMajorVersion 3
#define WindowsAzureMobileServicesSdkMinorVersion 0
#define WindowsAzureMobileServicesSdkBuildVersion 0

#endif
