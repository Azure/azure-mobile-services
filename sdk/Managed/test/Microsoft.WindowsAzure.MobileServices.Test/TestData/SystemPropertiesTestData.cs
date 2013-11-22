// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class SystemPropertiesTestData
    {
        public static MobileServiceSystemProperties[] SystemProperties = new MobileServiceSystemProperties[] {
            MobileServiceSystemProperties.None,
            MobileServiceSystemProperties.All,
            MobileServiceSystemProperties.CreatedAt | MobileServiceSystemProperties.UpdatedAt | MobileServiceSystemProperties.Version,
            MobileServiceSystemProperties.CreatedAt | MobileServiceSystemProperties.UpdatedAt,
            MobileServiceSystemProperties.UpdatedAt | MobileServiceSystemProperties.Version,
            MobileServiceSystemProperties.Version | MobileServiceSystemProperties.CreatedAt,
            MobileServiceSystemProperties.CreatedAt,
            MobileServiceSystemProperties.UpdatedAt,
            MobileServiceSystemProperties.Version
        };

        public static string[] ValidSystemProperties = new string[] {
            "__createdAt",
            "__updatedAt",
            "__version",
            "__CreatedAt",
            "__futureSystemProperty"
        };

        public static string[] NonSystemProperties = new string[] {
            "someProperty",
            "createdAt",
            "updatedAt",
            "version",
            "_createdAt",
            "_updatedAt",
            "_version",
            "X__createdAt"
        };

        public static string[] ValidSystemPropertyQueryStrings = new string[] {
            // General
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
        };

        public static string[] InvalidSystemPropertyQueryStrings = new string[] {
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
            "__systemProperties=*,__updatedAt",
        };

        // Missing ‘__’ prefix to systemProperties query string parameter
        public static string InvalidSystemParameterQueryString = "_systemProperties=__createdAt,__version";
    }
}
