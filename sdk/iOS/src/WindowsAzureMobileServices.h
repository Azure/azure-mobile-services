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
#import "MSSyncContextReadResult.h"

#if TARGET_OS_IPHONE
#import "MSLoginController.h"
#endif

#define WindowsAzureMobileServicesSdkMajorVersion 3
#define WindowsAzureMobileServicesSdkMinorVersion 0
#define WindowsAzureMobileServicesSdkBuildVersion 0

#endif
