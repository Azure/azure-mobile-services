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
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;

import java.util.Date;
import java.util.Locale;

public class StringIdMovie implements Movie {

    @SerializedName("id")
    private String mId;

    @SerializedName("title")
    private String mTitle;

    @SerializedName("duration")
    private int mDuration;

    @SerializedName("mpaaRating")
    private String mMPAARating;

    @SerializedName("releaseDate")
    private Date mReleaseDate;

    @SerializedName("bestPictureWinner")
    private boolean mBestPictureWinner;

    @SerializedName("year")
    private int mYear;

    public StringIdMovie() {
    }

    public StringIdMovie(String id, boolean bestPictureWinner, int duration, String mpaaRating, Date releaseDate, String title, int year) {
        mId = id;
        mBestPictureWinner = bestPictureWinner;
        mDuration = duration;
        mMPAARating = mpaaRating;
        mReleaseDate = releaseDate;
        mTitle = title;
        mYear = year;
    }

    public StringIdMovie(String id, IntIdMovie movie) {
        mId = id;
        mBestPictureWinner = movie.isBestPictureWinner();
        mDuration = movie.getDuration();
        mMPAARating = movie.getMPAARating();
        mReleaseDate = movie.getReleaseDate();
        mTitle = movie.getTitle();
        mYear = movie.getYear();
    }

    public String getId() {
        return mId;
    }

    public void setId(String id) {
        this.mId = id;
    }

    @Override
    public String getTitle() {
        return mTitle;
    }

    @Override
    public void setTitle(String title) {
        mTitle = title;
    }

    @Override
    public int getDuration() {
        return mDuration;
    }

    @Override
    public void setDuration(int duration) {
        mDuration = duration;
    }

    @Override
    public String getMPAARating() {
        return mMPAARating;
    }

    @Override
    public void setMPAARating(String mPAARating) {
        mMPAARating = mPAARating;
    }

    @Override
    public Date getReleaseDate() {
        return mReleaseDate;
    }

    @Override
    public void setReleaseDate(Date releaseDate) {
        mReleaseDate = releaseDate;
    }

    @Override
    public boolean isBestPictureWinner() {
        return mBestPictureWinner;
    }

    @Override
    public void setBestPictureWinner(boolean bestPictureWinner) {
        mBestPictureWinner = bestPictureWinner;
    }

    @Override
    public int getYear() {
        return mYear;
    }

    @Override
    public void setYear(int year) {
        mYear = year;
    }

    @Override
    public boolean equals(Object o) {
        if (o == null)
            return false;

        if (!(o instanceof StringIdMovie))
            return false;
        StringIdMovie m = (StringIdMovie) o;

        if (!Util.compare(mTitle, m.mTitle))
            return false;
        if (!Util.compare(mDuration, m.mDuration))
            return false;
        if (!Util.compare(mMPAARating, m.mMPAARating))
            return false;
        if (mReleaseDate != null) {
            if (m.mReleaseDate == null)
                return false;
            if (!Util.compare(mReleaseDate, m.mReleaseDate))
                return false;
        }
        if (!Util.compare(mBestPictureWinner, m.mBestPictureWinner))
            return false;
        if (!Util.compare(mYear, m.mYear))
            return false;
        return true;
    }

    @Override
    public String toString() {
        return String.format(Locale.getDefault(), "Movie/string id[Title=%s,Duration=%d,Rating=%s,ReleaseDate=%s,BestPictureWinner=%B,Year=%d]", mTitle,
                mDuration, mMPAARating, Util.dateToString(mReleaseDate), mBestPictureWinner, mYear);
    }
}