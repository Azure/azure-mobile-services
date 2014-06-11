// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

exports.validStringIds = [
             "id",
             "true",
             "false",
             "00000000-0000-0000-0000-000000000000",
             "aa4da0b5-308c-4877-a5d2-03f274632636",
             "69C8BE62-A09F-4638-9A9C-6B448E9ED4E7",
             "{EC26F57E-1E65-4A90-B949-0661159D0546}",
             "87D5B05C93614F8EBFADF7BC10F7AE8C",
             "someone@someplace.com",
             "id with spaces",
             //"...", // .NET not supported
             " .",
             "'id' with single quotes",
             "id with 128 characters " + new Array(128 - 24).join('A'), 
             "id with Japanese 私の車はどこですか？",
             "id with Arabic أين هو سيارتي؟",
             "id with Russian Где моя машина",
             "id with some URL significant characters % # &",
             "id with ascii characters  !#%&'()*,-.0123456789:;<=>@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_abcdefghijklmnopqrstuvwxyz{|}", // '$' .NET not supported
             "id with extended chars ascii ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþ"
];


exports.emptyStringIds = [""];
exports.invalidStringIds = [
            ".",
            "..",
            "id with 256 characters " + new Array(257 - 23).join('A'),
            "\r",
            "\n",
            "\t",
            "id\twith\ttabs",
            "id\rwith\rreturns",
            "id\nwith\n\newline",
            "id with backslash \\",
            "id with forwardslash \/",
            "1/8/2010 8:00:00 AM",
            "\"idWithQuotes\"",
            "?",
            "\\",
            "\/",
            "`",
            "+",
            " ",
            "control character between 0 and 31 " + String.fromCharCode(16),
            "control character between 127 and 159" + String.fromCharCode(130)
];

exports.validIntIds = [1, 925000];
exports.invalidIntIds = [-1];
exports.nonStringNonIntValidJsonIds = [
    true,
    false,
    1.0,
    -1.0,
    0.0,
];

exports.nonStringNonIntIds = [
    new Date(2010, 1, 8),
    {},
    1.0,
    "aa4da0b5-308c-4877-a5d2-03f274632636"
];

exports.testNonSystemProperties = ["someProperty", "createdAt", "updatedAt", "version", "_createdAt", "_updatedAt", "_version", "X__createdAt"];
exports.testValidSystemProperties = ["__createdAt", "__updatedAt", "__version", "__CreatedAt", "__futureSystemProperty"];
exports.testSystemProperties = [
    WindowsAzure.MobileServiceTable.SystemProperties.None,
    WindowsAzure.MobileServiceTable.SystemProperties.All,
    WindowsAzure.MobileServiceTable.SystemProperties.CreatedAt | WindowsAzure.MobileServiceTable.SystemProperties.UpdatedAt | WindowsAzure.MobileServiceTable.SystemProperties.Version,
    WindowsAzure.MobileServiceTable.SystemProperties.CreatedAt | WindowsAzure.MobileServiceTable.SystemProperties.UpdatedAt,
    WindowsAzure.MobileServiceTable.SystemProperties.CreatedAt | WindowsAzure.MobileServiceTable.SystemProperties.Version,
    WindowsAzure.MobileServiceTable.SystemProperties.CreatedAt,
    WindowsAzure.MobileServiceTable.SystemProperties.UpdatedAt | WindowsAzure.MobileServiceTable.SystemProperties.Version,
    WindowsAzure.MobileServiceTable.SystemProperties.UpdatedAt,
    WindowsAzure.MobileServiceTable.SystemProperties.Version
];

exports.testValidSystemPropertyQueryStrings = [
    "__systemProperties=*",
    "__systemProperties=__createdAt",
    "__systemProperties=__createdAt,__updatedAt",
    "__systemProperties=__createdAt,__version",
    "__systemProperties=__createdAt,__updatedAt,__version",
    "__systemProperties=__createdAt,__version,__updatedAt",
    "__systemProperties=__updatedAt",
    "__systemProperties=__updatedAt,__createdAt",
    "__systemProperties=__updatedAt,__createdAt,__version",
    "__systemProperties=__updatedAt,__version",
    "__systemProperties=__updatedAt,__version, __createdAt",
    "__systemProperties=__version",
    "__systemProperties=__version,__createdAt",
    "__systemProperties=__version,__createdAt,__updatedAt",
    "__systemProperties=__version,__updatedAt",
    "__systemProperties=__version,__updatedAt, __createdAt",
             
    // Trailing commas, extra commas
    "__systemProperties=__createdAt,",
    "__systemProperties=__createdAt,__updatedAt,",
    "__systemProperties=__createdAt,__updatedAt,__version,",
    "__systemProperties=,__createdAt",
    "__systemProperties=__createdAt,,__updatedAt",
    "__systemProperties=__createdAt, ,__updatedAt,__version",
    "__systemProperties=__createdAt,,",
    "__systemProperties=__createdAt, ,",
             
    // Trailing, leading whitespace
    "__systemProperties= *",
    "__systemProperties=\t*\t",
    "__systemProperties= __createdAt ",
    "__systemProperties=\t__createdAt,\t__updatedAt\t",
    "__systemProperties=\r__createdAt,\r__updatedAt,\t__version\r",
    "__systemProperties=\n__createdAt\n",
    "__systemProperties=__createdAt,\n__updatedAt",
    "__systemProperties=__createdAt, __updatedAt, __version",
             
    // Different casing
    "__SystemProperties=*",
    "__SystemProperties=__createdAt",
    "__SYSTEMPROPERTIES=__createdAt,__updatedAt",
    "__systemproperties=__createdAt,__updatedAt,__version",
    "__SystemProperties=__CreatedAt",
    "__SYSTEMPROPERTIES=__createdAt,__UPDATEDAT",
    "__systemproperties=__createdat,__UPDATEDAT,__veRsion",
             
    // Sans __ prefix
    "__systemProperties=createdAt",
    "__systemProperties=updatedAt,createdAt",
    "__systemProperties=UPDATEDAT,createdat",
    "__systemProperties=updatedAt,version,createdAt",
             
    // Combinations of above
    "__SYSTEMPROPERTIES=__createdAt, updatedat",
    "__systemProperties=__CreatedAt,,\t__VERSION",
    "__systemProperties= updatedat ,,"
];

exports.testInvalidSystemPropertyQueryStrings = [
    // Unknown system Properties
    "__systemProperties=__created",
    "__systemProperties=updated At",
    "__systemProperties=notASystemProperty",
    "__systemProperties=_version",
             
    // System properties not comma separated
    "__systemProperties=__createdAt __updatedAt",
    "__systemProperties=__createdAt\t__version",
    "__systemProperties=createdAt updatedAt version",
    "__systemProperties=__createdAt__version",
             
    // All and individual system properties requested
    "__systemProperties=*,__updatedAt"
];

exports.testInvalidSystemParameterQueryString = "_systemProperties=__createdAt,__version";