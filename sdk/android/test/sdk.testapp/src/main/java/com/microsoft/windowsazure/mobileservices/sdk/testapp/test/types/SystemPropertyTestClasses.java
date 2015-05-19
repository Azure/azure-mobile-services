/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */
package com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types;

import com.google.gson.annotations.SerializedName;

import java.util.Date;

public class SystemPropertyTestClasses {
    public static class CreatedAtType {
        public String Id;

        @SerializedName("__createdAt")
        public Date CreatedAt;
    }

    public static class UpdatedAtType {
        public String Id;

        @SerializedName("__updatedAt")
        public Date UpdatedAt;
    }

    public static class VersionType {
        public String Id;

        @SerializedName("__version")
        public String Version;
    }

    public static class AllSystemPropertiesType {
        public String Id;

        @SerializedName("__createdAt")
        public Date CreatedAt;

        @SerializedName("__updatedAt")
        public Date UpdatedAt;

        @SerializedName("__version")
        public String Version;
    }

    public static class NamedSystemPropertiesType {
        public String Id;

        public Date __createdAt;
    }

    public static class NotSystemPropertyCreatedAtType {
        public String Id;

        public Date CreatedAt;
    }

    public static class NotSystemPropertyUpdatedAtType {
        public String Id;

        public Date _UpdatedAt;
    }

    public static class NotSystemPropertyVersionType {
        public String Id;

        public String version;
    }

    public static class StringType {
        public int Id;
        public String String;
    }

    public static class IntegerIdNotSystemPropertyCreatedAtType {
        public int Id;

        public Date __createdAt;
    }

    public static class IntegerIdWithNamedSystemPropertiesType {
        public int Id;

        public Date __createdAt;
    }

    public static class LongIdWithNamedSystemPropertiesType {
        public long Id;

        public Date __createdAt;
    }

    public static class DoubleNamedSystemPropertiesType {
        public String Id;

        public Date __createdAt;

        public Date CreatedAt;
    }

    public static class NamedDifferentCasingSystemPropertiesType {
        public String Id;

        public Date __CreatedAt;
    }

    public static class StringCreatedAtType {
        public String Id;

        @SerializedName("__createdAt")
        public String CreatedAt;
    }

    public static class StringUpdatedAtType {
        public String Id;

        @SerializedName("__updatedAt")
        public String UpdatedAt;
    }
}