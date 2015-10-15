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
            MobileServiceSystemProperties.CreatedAt | MobileServiceSystemProperties.UpdatedAt | MobileServiceSystemProperties.Version, MobileServiceSystemProperties.Deleted,
            MobileServiceSystemProperties.CreatedAt | MobileServiceSystemProperties.UpdatedAt | MobileServiceSystemProperties.Version,
            MobileServiceSystemProperties.CreatedAt | MobileServiceSystemProperties.UpdatedAt,
            MobileServiceSystemProperties.UpdatedAt | MobileServiceSystemProperties.Version,
            MobileServiceSystemProperties.Version | MobileServiceSystemProperties.CreatedAt,
            MobileServiceSystemProperties.CreatedAt,
            MobileServiceSystemProperties.UpdatedAt,
            MobileServiceSystemProperties.Version,
            MobileServiceSystemProperties.Deleted
        };

        public static string[] ValidSystemProperties = new string[] {
            "createdAt",
            "updatedAt",
            "version",
            "deleted",
            "CreatedAt",
            "futureSystemProperty"
        };

        public static string[] NonSystemProperties = new string[] {
            "someProperty",
            "__createdAt",
            "__updatedAt",
            "__version",
            "__deleted",
            "_createdAt",
            "_updatedAt",
            "_version",
            "X__createdAt"
        };

        public static string[] ValidSystemPropertyQueryStrings = new string[] {
            // General
            "__systemProperties=*",
            "__systemProperties=createdAt",
            "__systemProperties=createdAt,updatedAt",
            "__systemProperties=createdAt,version",
            "__systemProperties=createdAt,updatedAt,version",
            "__systemProperties=createdAt,version,updatedAt",
            "__systemProperties=createdAt,version,updatedAt,deleted",
            "__systemProperties=updatedAt",
            "__systemProperties=updatedAt,createdAt",
            "__systemProperties=updatedAt,createdAt,version",
            "__systemProperties=updatedAt,version",
            "__systemProperties=updatedAt,version, createdAt",
            "__systemProperties=version",
            "__systemProperties=version,createdAt",
            "__systemProperties=version,createdAt,updatedAt",
            "__systemProperties=version,updatedAt",
            "__systemProperties=version,updatedAt, createdAt",

            // Trailing commas, extra commas
            "__systemProperties=createdAt,",
            "__systemProperties=createdAt,updatedAt,",
            "__systemProperties=createdAt,updatedAt,version,",
            "__systemProperties=,createdAt",
            "__systemProperties=createdAt,,updatedAt",
            "__systemProperties=createdAt, ,updatedAt,version",
            "__systemProperties=createdAt,,",
            "__systemProperties=createdAt, ,",

            // Trailing, leading whitespace
            "__systemProperties= *",
            "__systemProperties=\t*\t",
            "__systemProperties= createdAt ",
            "__systemProperties=\tcreatedAt,\tupdatedAt\t",
            "__systemProperties=\rcreatedAt,\rupdatedAt,\tversion\r",
            "__systemProperties=\ncreatedAt\n",
            "__systemProperties=createdAt,\nupdatedAt",
            "__systemProperties=createdAt, updatedAt, version",

            // Different casing
            "__SystemProperties=*",
            "__SystemProperties=createdAt",
            "__SYSTEMPROPERTIES=createdAt,updatedAt",
            "__systemproperties=createdAt,updatedAt,version",
            "__SystemProperties=CreatedAt",
            "__SYSTEMPROPERTIES=createdAt,UPDATEDAT",
            "__systemproperties=createdat,UPDATEDAT,veRsion",

            // Sans __ prefix
            "__systemProperties=createdAt",
            "__systemProperties=updatedAt,createdAt",
            "__systemProperties=UPDATEDAT,createdat",
            "__systemProperties=updatedAt,version,createdAt",

            // Combinations of above
            "__SYSTEMPROPERTIES=createdAt, updatedat",
            "__systemProperties=CreatedAt,,\tVERSION",
            "__systemProperties= updatedat ,,"
        };

        public static string[] InvalidSystemPropertyQueryStrings = new string[] {
            // Unknown system Properties
            "__systemProperties=__created",
            "__systemProperties=updated At",
            "__systemProperties=notASystemProperty",
            "__systemProperties=_version",

            // System properties not comma separated
            "__systemProperties=createdAt updatedAt",
            "__systemProperties=createdAt\tversion",
            "__systemProperties=createdAt updatedAt version",
            "__systemProperties=createdAtversion",
            
            // All and individual system properties requested
            "__systemProperties=*,updatedAt",
        };

        // Missing ‘__’ prefix to systemProperties query string parameter
        public static string InvalidSystemParameterQueryString = "_systemProperties=createdAt,version";
    }
}
