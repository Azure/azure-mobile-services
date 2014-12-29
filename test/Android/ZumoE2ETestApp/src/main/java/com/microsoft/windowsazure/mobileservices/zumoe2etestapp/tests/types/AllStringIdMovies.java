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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types;

import com.google.gson.annotations.SerializedName;

import java.util.List;

public class AllStringIdMovies {
    @SerializedName("id")
    private String mId;

    @SerializedName("status")
    private String mStatus;

    @SerializedName("movies")
    private StringIdMovie[] mMovies;

    public AllStringIdMovies() {
        mMovies = new StringIdMovie[0];
    }

    public String getId() {
        return mId;
    }

    public void setId(String id) {
        mId = id;
    }

    public String getStatus() {
        return mStatus;
    }

    public void setStatus(String status) {
        mStatus = status;
    }

    public StringIdMovie[] getMovies() {
        return mMovies;
    }

    public void setMovies(List<StringIdMovie> movies) {
        mMovies = movies.toArray(mMovies);
    }

    public void setMovies(StringIdMovie[] movies) {
        mMovies = movies;
    }
}