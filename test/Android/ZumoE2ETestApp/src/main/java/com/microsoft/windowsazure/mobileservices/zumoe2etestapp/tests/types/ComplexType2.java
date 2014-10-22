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

import java.util.Random;

import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;

public class ComplexType2 {

	public String name;
	public Integer age;
	public String[] friends;

	public ComplexType2() {
	}

	public ComplexType2(Random rndGen) {
		this.name = Util.createSimpleRandomString(rndGen, 10);
		this.age = rndGen.nextInt(80);
		this.friends = new String[5];

		for (int i = 0; i < 5; i++) {
			this.friends[i] = Util.createSimpleRandomString(rndGen, 5);
		}
	}

	@Override
	public boolean equals(Object o) {
		if (o == null)
			return false;
		if (!(o instanceof ComplexType2))
			return false;

		ComplexType2 element = (ComplexType2) o;

		if (!Util.compare(element.name, name))
			return false;
		if (!Util.compare(element.age, age))
			return false;
		if (!Util.compareArrays(element.friends, friends))
			return false;

		return true;
	}

}
