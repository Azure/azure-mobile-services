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
import java.util.GregorianCalendar;
import java.util.Random;

import android.annotation.SuppressLint;

import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;

@SuppressLint("DefaultLocale")
public class StringIdJsonElement {
	public String id;

	public String name;

	public Double number;

	public Boolean bool;

	public Date date1;

	// Complex type
	public ComplexType complex;

	public StringIdJsonElement() {
		this(false);
	}

	public StringIdJsonElement(String id) {
		this(true);
		this.id = id;
	}
	
	public StringIdJsonElement(boolean initialized) {
		if (initialized) {
			name = "Sample Data";
			number = 10.5;
			bool = true;
			date1 = new GregorianCalendar().getTime();
			Random rndGen = new Random();
			complex = new ComplexType(rndGen);
		} else {
			name = null;
			number = null;
			bool = null;
			date1 = null;
			complex = null;
		}
	}

	public StringIdJsonElement(Random rndGen) {
		name = Util.createSimpleRandomString(rndGen, 10);
		number = rndGen.nextDouble();
		bool = rndGen.nextBoolean();
		date1 = new GregorianCalendar(rndGen.nextInt(20) + 1980, rndGen.nextInt(12), rndGen.nextInt(27) + 1, rndGen.nextInt(24), rndGen.nextInt(60),
				rndGen.nextInt(60)).getTime();
		complex = new ComplexType(rndGen);
	}

	public StringIdJsonElement(StringIdJsonElement other) {
		id = String.valueOf(other.id);
		name = String.valueOf(other.name);
		number = Double.valueOf(other.number);
		bool = Boolean.valueOf(other.bool);
		date1 = new Date(other.date1.getTime());
		complex = new ComplexType(other.complex);
	}

	@Override
	public boolean equals(Object o) {
		if (!(o instanceof StringIdJsonElement))
			return false;

		StringIdJsonElement element = (StringIdJsonElement) o;
		if (!Util.compare(element.id, id))
			return false;
		if (!Util.compare(element.name, name))
			return false;
		if (!Util.compare(element.bool, bool))
			return false;
		if (!Util.compare(element.number, number))
			return false;
		if (!Util.compare(element.date1, date1))
			return false;
		if (!Util.compare(element.complex, complex))
			return false;
		return true;
	}

	@Override
	public String toString() {
		return String.format("StringIdRoundTripTableItem[Bool=%B,ComplexType=%s,Date1=%s,Name=%s,Number=%s]", bool == null ? "<<NULL>>" : bool.toString(),
				complex == null ? "<<NULL>>" : complex.toString(), date1 == null ? "<<NULL>>" : Util.dateToString(date1), name, number == null ? "<NULL>"
						: number.toString());

	}
}