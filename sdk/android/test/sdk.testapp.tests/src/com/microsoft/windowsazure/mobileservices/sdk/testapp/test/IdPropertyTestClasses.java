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
package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

import com.google.gson.annotations.SerializedName;

// Class with an invalid Id property but using a valid gson serialized name
class IdPropertyWithGsonAnnotation {
	@SerializedName("id")
	private int myId;

	private String name;

	public IdPropertyWithGsonAnnotation(String name) {
		this.name = name;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public int getId() {
		return myId;
	}

	public void setId(int id) {
		myId = id;
	}
}

// Class with a different cased id property
class IdPropertyWithDifferentIdPropertyCasing {
	private int ID;

	private String name;

	public IdPropertyWithDifferentIdPropertyCasing(String name) {
		this.name = name;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public int getId() {
		return ID;
	}

	public void setId(int id) {
		ID = id;
	}
}

//Class with multiple id properties
class IdPropertyMultipleIdsTestObject {
	private int id;
	private int ID;
	private int iD;
	
	private String name;

	public IdPropertyMultipleIdsTestObject(String name) {
		this.name = name;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public int getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public int getID() {
		return ID;
	}

	public void setID(int iD) {
		ID = iD;
	}

	public int getiD() {
		return iD;
	}

	public void setiD(int iD) {
		this.iD = iD;
	}

}

//Class with no Id property
class NoIdProperty {

	private String name;

	public NoIdProperty(String name) {
		this.name = name;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}
}

class IdTestData
{
    public static String[] ValidStringIds = new String[] {
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
        "...",
        " .",
        "'id' with single quotes",
        "id with 255 characters " + repeat("x", 232),
        "id with Japanese 私の車はどこですか？",
        "id with Arabic أين هو سيارتي؟",
        "id with Russian Где моя машина",
        "id with some URL significant characters % # &",
        "id with allowed ascii characters " + getValidASCII(),
        "id with allowed extended ascii characters " + getValidExtendedASCII(),
        "   ",
    };

    public static String[] EmptyStringIds = new String[] {
        "",
    };

    public static String[] InvalidStringIds = concat(new String[] {
        ".",
        "..",
        "id with 256 characters " + repeat("x", 233),
        "\r",
        "\n",
        "\t",
        "id\twith\ttabs",
        "id\rwith\rreturns",
        "id\nwith\n\newline",
        "id with fowardslash \\",
        "id with backslash /",
        "\"idWithQuotes\"",
        "?",
        "\\",
        "/",
        "`",
        "+",
    }, getInvalidASCII());

    public static long[] ValidIntIds = new long[] {
        1,
        Integer.MAX_VALUE,
        Long.MAX_VALUE
    };

    public static long[] InvalidIntIds = new long[] {
        -1,
        Integer.MIN_VALUE,
        Long.MIN_VALUE,
    };

    public static Object[] NonStringNonIntValidJsonIds = new Object[] {
        true,
        false,
        1.0,
        -1.0,
        0.0,
    };

    public static Object[] NonStringNonIntIds = new Object[] {
        new NoIdProperty(""),
        new Object(),
        1.0,
        UUID.fromString("aa4da0b5-308c-4877-a5d2-03f274632636")
    };
    
    private static String repeat(String value, int count) {
    	StringBuilder builder = new StringBuilder();
    	
    	for(int i = 0; i < count; i++) {
    	    builder.append(value);
    	}
    	
    	return builder.toString();
    }
    
    private static String getValidASCII() {
    	StringBuilder builder = new StringBuilder();
    	
    	for(int i = 32; i <= 126; i++) {
    		if (i != 34 && i != 43 && i != 47 && i != 63 && i != 92 && i != 96) {
    			builder.append(Character.toChars(i));
    		}
    	}
    	
    	return builder.toString();
    }
    
    private static String getValidExtendedASCII() {
    	StringBuilder builder = new StringBuilder();
    	
    	for(int i = 160; i <= 255; i++) {
			builder.append(Character.toChars(i));
    	}
    	
    	return builder.toString();
    }
    
    private static String[] getInvalidASCII() {
    	String[] array = new String[65];
    	
    	for(int i = 0; i <= 31; i++) {
    		array[i] = String.valueOf(Character.toChars(i));
    	}
    	
    	for(int i = 127; i <= 159; i++) {
    		array[i - 95] = String.valueOf(Character.toChars(i));
    	}
    	
    	return array;
    }
    
    public static String[] concat(String[] array1, String[] array2) {
    	int length1 = array1.length;
    	int length2 = array2.length;
    	int length = length1 + length2;
    	
    	String[] array = new String[length];
    	
    	System.arraycopy(array1, 0, array, 0, length1);
    	System.arraycopy(array2, 0, array, length1, length2);
    	
    	return array;
    }
    
    public static Object[] concat(Object[] array1, Object[] array2) {
    	int length1 = array1.length;
    	int length2 = array2.length;
    	int length = length1 + length2;
    	
    	Object[] array = new Object[length];
    	
    	System.arraycopy(array1, 0, array, 0, length1);
    	System.arraycopy(array2, 0, array, length1, length2);
    	
    	return array;
    }
    
    public static long[] concat(long[] array1, long[] array2) {
    	int length1 = array1.length;
    	int length2 = array2.length;
    	int length = length1 + length2;
    	
    	long[] array = new long[length];
    	
    	System.arraycopy(array1, 0, array, 0, length1);
    	System.arraycopy(array2, 0, array, length1, length2);
    	
    	return array;
    }
    
	public static Object[] convert(long[] array) {
    	List<Object> resultList = new ArrayList<Object>();
    	
    	for (long elem :array)
    	{
    		resultList.add((Object)elem);
    	}
    	
		return resultList.toArray();
    }
}

class StringIdType {
    public String Id;

    public String String;
}

class LongIdType {
    public long Id;

    public String String;
}

