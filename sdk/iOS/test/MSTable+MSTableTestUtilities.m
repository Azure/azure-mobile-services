// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSTable+MSTableTestUtilities.h"

@implementation MSTable (MSTableFunctionalTestUtilities)

-(void) deleteAllItemsWithCompletion:(void (^)(NSError *error))completion;
{
    __block MSReadQueryBlock readCompletion;
    readCompletion = ^(NSArray *items, NSInteger totalCount, NSError *error) {
        if (error) {
            completion(error);
        }
        else {
            [self deleteItems:items completion:completion];
        }
    };
    
    [self readWithQueryString:@"$top=500" completion:readCompletion];
}

-(void) deleteItems:(NSArray *)items
         completion:(void (^)(NSError *))completion
{
    __block NSInteger lastItemIndex = -1;
    
    void (^nextDeleteBlock)(id, NSError *error);
    void (^__block __weak weakNextDeleteBlock)(id, NSError *error);
    weakNextDeleteBlock = nextDeleteBlock = ^(id itemId, NSError *error) {
        if (error) {
            completion(error);
        }
        else {
            if (lastItemIndex + 1  < items.count) {
                lastItemIndex++;
                id itemToDelete = [items objectAtIndex:lastItemIndex];
                [self delete:itemToDelete completion:weakNextDeleteBlock];
            }
            else {
                completion(nil);
            }
        }
    };
    
    nextDeleteBlock(0, nil);
}

-(void) insertItems:(NSArray *)items
         completion:(void (^)(NSError *))completion
{
    __block NSInteger lastItemIndex = -1;
    
    void (^ nextInsertBlock)(id, NSError *error);
    void (^__block __weak weakNextInsertBlock)(id, NSError *error);
    weakNextInsertBlock = nextInsertBlock = ^(id newItem, NSError *error) {
        if (error) {
            completion(error);
        }
        else {
            if (lastItemIndex + 1  < items.count) {
                lastItemIndex++;
                id itemToInsert = [items objectAtIndex:lastItemIndex];
                [self insert:itemToInsert completion:weakNextInsertBlock];
            }
            else {
                completion(nil);
            }
        }
    };
    
    nextInsertBlock(nil, nil);
}

+ (NSArray *) testValidStringIds
{
    return @[
             @"id",
             @"true",
             @"false",
             @"00000000-0000-0000-0000-000000000000",
             @"aa4da0b5-308c-4877-a5d2-03f274632636",
             @"69C8BE62-A09F-4638-9A9C-6B448E9ED4E7",
             @"{EC26F57E-1E65-4A90-B949-0661159D0546}",
             @"87D5B05C93614F8EBFADF7BC10F7AE8C",
             @"someone@someplace.com",
             @"id with spaces",
             @"...",
             @" .",
             @"'id' with single quotes",
             [@"id with 256 characters " stringByPaddingToLength:255 withString:@"." startingAtIndex:0],
             @"id with Japanese 私の車はどこですか？",
             @"id with Arabic أين هو سيارتي؟",
             @"id with Russian Где моя машина",
             @"id with some URL significant characters % # &",
             @"id with allowed ascii characters ",
             @"id with allowed ascii characters  !#$%&'()*,-.0123456789:;<=>@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_abcdefghijklmnopqrstuvwxyz{|}",
             @"id with allowed extended ascii characters ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþ"
    ];
};

+ (NSArray *) testEmptyStringIdsIncludingNull:(bool)includeNull
{
    NSArray *ids = @[ @"" ];
    if(includeNull) {
        [ids arrayByAddingObject:[NSNull null]];
    }
    return ids;
};

+ (NSArray *) testInvalidStringIds
{
    return @[
             @".",
             @"..",
             [@"id with 256 characters " stringByPaddingToLength:256 withString:@"." startingAtIndex:0],
             @"\r",
             @"\n",
             @"\t",
             @"id\twith\ttabs",
             @"id\rwith\rreturns",
             @"id\nwith\n\newline",
             @"id with fowardslash \\",
             @"id with backslash /",
             @"1/8/2010 8:00:00 AM"
             @"""idWithQuotes""",
             @"?",
             @"\\",
             @"/",
             @"`",
             @"+"
    ];
    //Enumerable.Range(0, 32).Select(number => char.ToString((char)number)))
    //Enumerable.Range(127, 159 - 127).Select(number => char.ToString((char)number)))
}

+(NSArray *) testValidIntIds
{
    return @[
             @1,
             @INT_MAX,
             @LONG_LONG_MAX
    ];
}

+(NSArray *) testInvalidIntIds
{
    return @[
             [NSNumber numberWithInt:-1],
             @INT_MIN,
             @LONG_LONG_MIN
    ];
}

+(NSArray *) testNonStringNonIntValidJsonIds
{
    return @[
             @YES,
             @NO,
             @1.0,
             @-1.0,
             @0.0
    ];
};

+(NSArray *) testNonStringNonIntIds
{
    return @[
        [[NSObject alloc] init],
        [NSDate date],
        @1.0
    ];
};


@end
