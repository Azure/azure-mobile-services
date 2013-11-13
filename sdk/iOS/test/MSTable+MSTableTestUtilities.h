// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>

@interface MSTable (MSTableTestUtilities)
-(void) deleteAllItemsWithCompletion:(void (^)(NSError *error))completion;
-(void) insertItems:(NSArray *)items completion:(void (^)(NSError *error))completion;

+(NSArray *) testValidStringIds;
+(NSArray *) testEmptyStringIdsIncludingNull:(bool)includeNull;
+(NSArray *) testInvalidStringIds;
+(NSArray *) testValidIntIds;
+(NSArray *) testInvalidIntIds;
+(NSArray *) testNonStringNonIntValidJsonIds;
+(NSArray *) testNonStringNonIntIds;
@end
