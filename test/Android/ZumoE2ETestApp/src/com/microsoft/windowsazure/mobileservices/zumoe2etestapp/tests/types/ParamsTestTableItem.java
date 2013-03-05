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

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

public class ParamsTestTableItem {
	public int id;
	public String parameters;

	public ParamsTestTableItem() {
		this(0);
	}

	public ParamsTestTableItem(int id) {
		this.id = id;
	}

	public JsonObject getParameters() {
		return new JsonParser().parse(parameters).getAsJsonObject();
	}
}
