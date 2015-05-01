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

import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;

import java.util.Date;
import java.util.GregorianCalendar;
import java.util.Random;

@SuppressLint("DefaultLocale")
public class RoundTripTableElement {
    public Integer id;

    public String string1;

    public Date date1;

    public Boolean bool1;

    // Number types
    public Double double1;
    public Long long1;
    public Integer int1;

    public EnumType enumType;

    // Complex type array.
    public ComplexType[] complexType1;

    // Complex type
    public ComplexType2 complexType2;

    public RoundTripTableElement() {
        this(false);
    }

    public RoundTripTableElement(boolean initialized) {
        if (initialized) {
            string1 = "Sample Data";
            date1 = new GregorianCalendar().getTime();
            bool1 = true;
            double1 = 10.5;
            long1 = 5000000000L;
            int1 = 42;
            enumType = EnumType.First;
            Random rndGen = new Random();
            complexType1 = new ComplexType[1];
            complexType1[0] = new ComplexType(rndGen);
            complexType2 = new ComplexType2(rndGen);
        } else {
            string1 = null;
            date1 = null;
            bool1 = null;
            double1 = null;
            long1 = null;
            int1 = null;
            enumType = null;
            complexType1 = null;
            complexType2 = null;
        }
    }

    public RoundTripTableElement(Random rndGen) {
        string1 = Util.createSimpleRandomString(rndGen, 10);
        date1 = new GregorianCalendar(rndGen.nextInt(20) + 1980, rndGen.nextInt(12), rndGen.nextInt(27) + 1, rndGen.nextInt(24), rndGen.nextInt(60),
                rndGen.nextInt(60)).getTime();
        bool1 = rndGen.nextBoolean();
        double1 = rndGen.nextDouble();
        long1 = rndGen.nextInt(3000) + 5000000000L;
        int1 = rndGen.nextInt();
        enumType = EnumType.values()[rndGen.nextInt(4)];
        complexType1 = new ComplexType[1];
        complexType1[0] = new ComplexType(rndGen);
        complexType2 = new ComplexType2(rndGen);
    }

    @Override
    public boolean equals(Object o) {
        if (!(o instanceof RoundTripTableElement))
            return false;

        RoundTripTableElement element = (RoundTripTableElement) o;
        if (!Util.compare(element.id, id))
            return false;
        if (!Util.compare(element.string1, string1))
            return false;
        if (!Util.compare(element.date1, date1))
            return false;
        if (!Util.compare(element.bool1, bool1))
            return false;
        if (!Util.compare(element.double1, double1))
            return false;
        if (!Util.compare(element.long1, long1))
            return false;
        if (!Util.compare(element.int1, int1))
            return false;
        if (!Util.compareArrays(element.complexType1, complexType1))
            return false;
        if (!Util.compare(element.complexType2, complexType2))
            return false;
        return true;
    }

    @Override
    public String toString() {
        return String.format("RoundTripTableItem[Bool1=%B,ComplexType1=%s,ComplexType2=%s,Date1=%s,Double1=%s,EnumType=%s,Int1=%d,Long1=%d,String1=%s]",
                bool1 == null ? "<<NULL>>" : bool1.toString(), Util.arrayToString(complexType1), complexType2 == null ? "<<NULL>>" : complexType2.toString(),
                date1 == null ? "<<NULL>>" : Util.dateToString(date1), double1 == null ? "<NULL>" : double1.toString(), enumType.toString(),
                int1 == null ? "<<NULL>>" : int1, long1, string1);

    }
}