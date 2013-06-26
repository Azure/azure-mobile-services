# Custom APIs

The 'api' folder in your repository contains custom APIs that can be invoked with HTTP requests to `http://<servicename>.azure-mobile.net/api/<apiname>`. 

To implement a custom API you need two things:

- a JavaScript file with the API code e.g myapi.js
- a JSON file with the metadata for the API e.g. myapi.json

## API Script

The custom API script is a Node.js module that has one or more exports that correspond to the HTTP method(s) you want the API to respond to. For example, the following custom API script uses `exports.post` to accept HTTP POST requests and sends a 'Hello World' response:

	exports.post = function(request, response) {
	    response.send(200, "Hello World");
	};

## API Metadata

Each custom API .js file needs a companion .json file with the metadata for that API e.g. API permissions. Here is an example .json file:

	{
	    "routes": {
	            "*": {
	                "get":  { "permission": "public" },
	                "post": { "permission": "application" },
	                "patch": { "permission": "user" },
	                "delete": { "permission": "admin" },
	                "put": { "permission": "admin" }
	            }
	    }
	}

## More Information

For more information see the [documentation](http://go.microsoft.com/fwlink/?LinkID=307138&clcid=0x409).