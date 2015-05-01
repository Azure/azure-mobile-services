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

import android.annotation.SuppressLint;

import com.google.gson.annotations.SerializedName;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;

import java.util.Date;
import java.util.GregorianCalendar;
import java.util.Random;

@SuppressLint("DefaultLocale")
public class IntIdRoundTripTableElement {
    public Long id;

    public String name;

    public Double number;

    public Boolean bool;

    public Date date1;

    public Integer integer;

    public IntIdRoundTripTableElement() {
        this(false);
    }

    public IntIdRoundTripTableElement(boolean initialized) {
        if (initialized) {
            name = "Sample Data";
            number = 10.5;
            bool = true;
            date1 = new GregorianCalendar().getTime();
            integer = 42;
        } else {
            name = null;
            number = null;
            bool = null;
            date1 = null;
            integer = null;
        }
    }

    public IntIdRoundTripTableElement(Random rndGen) {
        name = Util.createSimpleRandomString(rndGen, 10);
        number = rndGen.nextDouble();
        bool = rndGen.nextBoolean();
        date1 = new GregorianCalendar(rndGen.nextInt(20) + 1980, rndGen.nextInt(12), rndGen.nextInt(27) + 1, rndGen.nextInt(24), rndGen.nextInt(60),
                rndGen.nextInt(60)).getTime();
        integer = rndGen.nextInt();
    }

    public IntIdRoundTripTableElement(IntIdRoundTripTableElement other) {
        id = Long.valueOf(other.id);
        name = String.valueOf(other.name);
        number = Double.valueOf(other.number);
        bool = Boolean.valueOf(other.bool);
        date1 = new Date(other.date1.getTime());
        integer = Integer.valueOf(other.integer);
    }

    @Override
    public boolean equals(Object o) {
        if (!(o instanceof IntIdRoundTripTableElement))
            return false;

        IntIdRoundTripTableElement element = (IntIdRoundTripTableElement) o;
        if (!Util.compare(element.id, id))
            return false;
        if (!Util.compare(element.name, name))
            return false;
        if (!Util.compare(element.number, number))
            return false;
        if (!Util.compare(element.bool, bool))
            return false;
        if (!Util.compare(element.date1, date1))
            return false;
        if (!Util.compare(element.integer, integer))
            return false;
        return true;
    }

    @Override
    public String toString() {
        return String.format("IntIdRoundTripTableItem[Bool=%B,Date1=%s,Integer=%s,Name=%s,Number=%s]", bool == null ? "<<NULL>>" : bool.toString(),
                date1 == null ? "<<NULL>>" : Util.dateToString(date1), integer == null ? "<<NULL>>" : integer.toString(), name, number == null ? "<<NULL>>"
                        : number.toString());
    }
}