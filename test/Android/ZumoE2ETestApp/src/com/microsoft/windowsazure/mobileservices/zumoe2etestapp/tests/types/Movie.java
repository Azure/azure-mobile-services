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

import java.util.Date;
import java.util.Locale;

import com.google.gson.annotations.SerializedName;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;

public class Movie {

	private int id;

	@SerializedName("title")
	private String mTitle;

	@SerializedName("duration")
	private int mDuration;

	@SerializedName("MPAARating")
	private String mMPAARating;

	@SerializedName("releaseDate")
	private Date mReleaseDate;

	@SerializedName("bestPictureWinner")
	private boolean mBestPictureWinner;

	@SerializedName("year")
	private int mYear;

	public Movie() {
		id = 0;
	}

	public Movie(boolean bestPictureWinner, int duration, String mpaaRating, Date releaseDate, String title, int year) {

		id = 0;

		mBestPictureWinner = bestPictureWinner;
		mDuration = duration;
		mMPAARating = mpaaRating;
		mReleaseDate = releaseDate;
		mTitle = title;
		mYear = year;
	}

	public int getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public String getTitle() {
		return mTitle;
	}

	public void setTitle(String title) {
		mTitle = title;
	}

	public int getDuration() {
		return mDuration;
	}

	public void setDuration(int duration) {
		mDuration = duration;
	}

	public String getMPAARating() {
		return mMPAARating;
	}

	public void setMPAARating(String mPAARating) {
		mMPAARating = mPAARating;
	}

	public Date getReleaseDate() {
		return mReleaseDate;
	}

	public void setReleaseDate(Date releaseDate) {
		mReleaseDate = releaseDate;
	}

	public boolean isBestPictureWinner() {
		return mBestPictureWinner;
	}

	public void setBestPictureWinner(boolean bestPictureWinner) {
		mBestPictureWinner = bestPictureWinner;
	}

	public int getYear() {
		return mYear;
	}

	public void setYear(int year) {
		mYear = year;
	}

	@Override
	public boolean equals(Object o) {
		if (o == null)
			return false;

		if (!(o instanceof Movie))
			return false;
		Movie m = (Movie) o;

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
		return String.format(Locale.getDefault(), "Movie[Title=%s,Duration=%d,Rating=%s,ReleaseDate=%s,BestPictureWinner=%B,Year=%d]", mTitle, mDuration,
				mMPAARating, Util.dateToString(mReleaseDate), mBestPictureWinner, mYear);
	}

}
