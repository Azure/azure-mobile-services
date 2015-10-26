# Microsoft Azure Mobile Services

With Microsoft Azure Mobile Services you can add a scalable backend to your connected client applications in minutes. To learn more, visit our [Developer Center](http://azure.microsoft.com/en-us/develop/mobile).

## Getting Started

If you are new to Mobile Services, you can get started by following our tutorials for connecting your Mobile
Services cloud backend to [Windows Store apps](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-windows-store-get-started/),
[Windows Phone 8 apps](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-windows-phone-get-started/),
[iOS apps](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-ios-get-started/),
and [Android apps](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-android-get-started/).

## Download Source Code

To get the source code of our SDKs and samples via **git** just type:

    git clone https://github.com/Azure/azure-mobile-services.git
    cd ./azure-mobile-services/

## Reference Documentation

#### Azure App Service Mobile Apps
* [iOS](http://azure.github.io/azure-mobile-services/iOS/v3)

#### Mobile Services
* [iOS](http://azure.github.io/azure-mobile-services/iOS/v2)
* [Android](http://dl.windowsazure.com/androiddocs/)

## Change log

- [iOS SDK](CHANGELOG.ios.md)
- [Managed SDK](CHANGELOG.managed.md)
- [Android SDK](CHANGELOG.android.md)
- [JavaScript SDK](CHANGELOG.javascript.md)

## Managed Windows Client SDK

Our managed portable library for Windows 8, Windows Phone 8, Windows Phone 8.1, and Windows Runtime Universal C# Client SDK makes it incredibly easy to use Mobile Services from your Windows applications. The [Microsoft Azure Mobile Services SDK](http://nuget.org/packages/WindowsAzure.MobileServices/) is available 
as a Nuget package or you can download the source using the instructions above. The managed portable library also supports the full .NET 4.5 platform.

### Prerequisites

The SDK requires Visual Studio 2013.

###Building and Referencing the SDK

The managed portable library solution includes a core portable assembly and platform-specific assemblies for each of the supported platforms: Windows 8,
Windows Phone 8 and .NET 4.5. The core portable platform project is ```Microsoft.WindowsAzure.Mobile```. The platform-specific assembly projects are
named using a ```Microsoft.WindowsAzure.Mobile.Ext.<Platform>``` convention. The Windows Phone 8 platform also
include a ```Microsoft.WindowsAzure.Mobile.UI.<Platform>``` project that contain UI components. To build the Managed Portable Libray:

1. Open the ```sdk\Managed\Microsoft.WindowsAzure.Mobile.Managed.sln``` solution file in Visual Studio 2012.
2. Press F6 to build the solution.

### Running the Tests

The managed portable library ```Microsoft.WindowsAzure.Mobile.Managed.sln``` has a test application for each of the supported platforms: Windows 8,
Windows Phone 8 and .NET 4.5.

1. Open the ```sdk\Managed\Microsoft.WindowsAzure.Mobile.Managed.sln``` solution file in Visual Studio 2012.
2. Right-click on the test project for a given platform in the Solution Explorer and select ```Set as StartUp Project```.
3. Press F5 to run the application in debug mode.
4. An application will appear with a prompt for a runtime Uri and Tags. You can safely ignore this prompt and just click the Start button.
5. The test suite will run and display the results.

## iOS Client SDK
Add a cloud backend to your iOS application in minutes with our iOS client SDK. You can [download the iOS SDK](https://go.microsoft.com/fwLink/?LinkID=266533&clcid=0x409) directly or you can download the source code using the instructions above.  

### Prerequisites

The SDK requires XCode 4.6.3 or greater.

###Building and Referencing the SDK

1. Open the ```sdk\iOS\WindowsAzureMobileServices.xcodeproj``` file in XCode.
2. Set the active scheme option to ```Framework\iOS Device```.
3. Build the project using Command-B. The ```WindowsAzureMobileServices.framework``` folder should be found in the build output folder under ```Products\<build configuration>-iphoneos```.
4. Drag and drop the ```WindowsAzureMobileServices.framework``` from a Finder window into the Frameworks folder of the Project Navigator panel of your iOS application XCode project.

### Running the Tests

1. Open the ```sdk\iOS\WindowsAzureMobileServices.xcodeproj``` file in XCode.
2. Set the active scheme option to ```WindowsAzureMobileServices\* Simulator```.
3. Open the ```Test\WindowsAzureMobileServicesFunctionalTests.m``` file in the Project Navigator panel of XCode.
4. In the ```settings.plist``` file, set ```TestAppUrl``` and ```TestAppApplicationKey``` to a valid URL and Application Key for a working Mobile Service.
5. Run the tests using Command-U.

## Android SDK
Microsoft Azure Mobile Services can be used with an Android-based device using our Android SDK. You can get the Android SDK in one of the following two ways or you can download the source code using the instructions above.

1. For an Android studio project, include the line `compile 'com.microsoft.azure:azure-mobile-services-android-sdk:2.0.3'` in the dependencies section of build.gradle file of the app
2. Eclipse users can [download the Android SDK](https://go.microsoft.com/fwLink/?LinkID=280126&clcid=0x409) directly or can download the source code using the instructions above.

### Prerequisites
The SDK requires Android Studio.

### Building and Referencing the SDK
1. Open the folder `\azure-mobile-services\sdk\android` using the option `Open an existing Android Studio Project` in Android Studio.
2. Project should be built automatically, In case it does not build, Right click on `sdk` folder and select `Make Module 'sdk'`
3. The file `sdk-release.aar` should be present at `\azure-mobile-services\sdk\android\src\sdk\build\outputs\aar` 
4. Rename the file `sdk-release.aar` to `sdk-release.zip`
5. Extract the zip file, `classes.jar` should be present in the root folder.

### Running the Tests

The SDK has a suite of unit tests that you can easily run.

1. Open the folder `\azure-mobile-services\sdk\android` using the option `Open an existing Android Studio Project` in Android Studio.
2. Project should be built automatically, In case it does not build, Right click on `sdk` folder and select `Make Module 'sdk.testapp'`
3. Expand `sdk.testapp` and sub folder `java`
4. Right click on `com.microsoft.windowsazure.mobileservices.sdk.testapp`, Select `Run`, Select `Tests in com.microsoft.windowsazure.mobileservices.sdk.testapp` (with Android tests icon)

## JavaScript SDK

Our JavaScript SDK makes it easy to use our Microsoft Azure Mobile Services in a Windows 8 application or an HTML client. The [Microsoft Azure Mobile Services for WinJS SDK](http://nuget.org/packages/WindowsAzure.MobileServices.WinJS/) is available as a Nuget package or you can download the source for both WinJS and HTML using the instructions above. 

### Prerequisites

The Microsoft Azure Mobile Services for WinJS SDK requires Windows 8.1 and Visual Studio 2013 Update 3. 

### Building and Referencing the SDK

1. Install Node.js and grunt-cli (globally) for building in Visual Studio
2. Install the Task Runner Explorer(https://visualstudiogallery.msdn.microsoft.com/8e1b4368-4afb-467a-bc13-9650572db708) add on for VS 2013 
3. Open the ```sdk\JavaScript\Microsoft.WindowsAzure.Mobile.JS.sln``` file in Visual Studio.
4. Right click on the gruntfile.js in the solution, and select Task Runner Explorer
5. Run the default build option

Alternatively, you can use Grunt from the command line to build the project as well.

For WinJS Windows Store apps, copy the ```Generated/MobileServices[.min].js```, ```Generated/MobileServices.DevIntellisense.js``` and ```Generated/MobileService.pri``` files into your WinJS project. For HTML applications, copy the ```Generated/MobileServices.Web[.min].js``` and the ```Generated/MobileServices.DevIntellisense.js``` files into your HTML\JavaScript project.

### Running the Tests

To run the WinJS Windows Store test app:

1. Open the ```sdk\JavaScript\Microsoft.WindowsAzure.Mobile.JS.sln``` file in Visual Studio.
2. In the Solution Explorer, right-click on the ```Microsoft.WindowsAzure.Mobile.WinJS.Test``` project in the Solution Explorer and select ```Set as StartUp Project```.
3. Press F5 to run the application in debug mode.
4. A Windows Store application will appear with a prompt for a Runtime Uri and Tags. You can safely ignore this prompt and just click the Start button.
5. The test suite will run and display the results.

To run the HTML tests:

1. Open the ```sdk\JavaScript\Microsoft.WindowsAzure.Mobile.JS.sln``` file in Visual Studio.
2. In the Solution Explorer, select the Microsoft.WindowsAzure.Mobile.WinJS.Test project and right-click to select 'View in Browser'.
3. The default browser will launch and run the test HTML application. Some tests may fail because due to an 'Unexpected connection failure'. This is because the test is configured to connect to a Mobile Service that does not exist. These failures can be ignored.

## Useful Resources

* [Quickstarts](https://github.com/Azure/azure-mobile-services-quickstarts)
* [E2E Test Suite](https://github.com/Azure/azure-mobile-services-test)
* [Samples](https://github.com/Azure/mobile-services-samples)
* Tutorials and product overview are available at [Microsoft Azure Mobile Services Developer Center](http://azure.microsoft.com/en-us/develop/mobile).
* Our product team actively monitors the [Mobile Services Developer Forum](http://social.msdn.microsoft.com/Forums/en-US/azuremobile/) to assist you with any troubles.

## Contribute Code or Provide Feedback

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/Azure/azure-mobile-services/issues) section of the project.
