Note that due to complications with JSON.NET nuget packaging interacting badly with the ahead-of-time compilation 
used for Xamarin.iOS, this project does NOT reference the JSON.NET nuget package because NuGet will pick the wrong 
platform target. The direct reference to the portable-sl4-etc version of JSON.NET is deliberate.