# Microsoft Azure Mobile Services

With Microsoft Azure Mobile Services you can add a scalable backend to your connected client applications in minutes.
To learn more, visit our [Developer Center](http://azure.microsoft.com/en-us/develop/mobile/).

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

## Change log

see [Change Log](CHANGELOG.md)

## Managed Windows 8 and Windows Phone 8 Client SDK

Our managed portable library for Windows 8 and Windows Phone 8 Client SDK makes it incredibly easy to use Mobile Services from your Windows Store
and Windows Phone 8 applications. The [Microsoft Azure Mobile Services SDK](http://nuget.org/packages/WindowsAzure.MobileServices/) is available 
as a Nuget package or you can download the source using the instructions above. The managed portable library also supports the full .NET 4.5 platform.

### Prerequisites

The SDK requires Visual Studio 2012 RTM.

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
4. In the ```setUp``` code, replace the ```<Microsoft Azure Mobile Service App URL>``` and ```<Application Key>``` with the valid URL and Application Key for a working Mobile Service.
5. Run the tests using Command-U.

## Android SDK
Microsoft Azure Mobile Services can be used with an Android-based device using our Android SDK. You can [download the Android SDK](https://go.microsoft.com/fwLink/?LinkID=280126&clcid=0x409) directly or you can download the source code using the instructions above.  

### Prerequisites

The SDK requires Eclipse and the latest [Android Development Tools](http://developer.android.com/tools/sdk/eclipse-adt.html).

### Building and Referencing the SDK

1. In the folder `\azure-mobile-services\sdk\android\src\sdk\libs`, run either the `getLibs.ps1` script if you are running on Windows or the `getLibs.sh` script if you are running on Linux to download the required dependencies.
2. Import the `\azure-mobile-services\sdk\android\src\sdk` project into your workspace
3. Once Eclipse is done compiling, the resulting .jar file will be located in `\azure-mobile-services\sdk\android\src\sdk\bin`.
4. To optionally build JavaDocs, right-click the `javadoc.xml` file and select Run As > Ant Build.

### Running the Tests

The SDK has a suite of unit tests that you can easily run.

1. Import the `\azure-mobile-services\sdk\android\test\sdk.testapp.tests` project in your Eclipse workspace
2. Right-click the project name and select Run As > Android JUnit Test

It also contains an end-to-end test application. 

1. Use the [Azure portal](http://manage.windowsazure.com) to create a new mobile service. Note down the service name and application key.
2. If you want to run the authentication tests, configure all four authentication providers on the "Identity" tab
3. [Install node.js](http://nodejs.org/) and then run the command `npm install azure-cli -g`. This installs the Azure command-line tool.
4. Use the `azure account` command to configure the tool to work with your Azure subscription
5. Run the `SetupTables.sh` script in the `\azure-mobile-services\test\Android\SetupScripts` folder, which uses the tool to automatically create the tables needed for the test application to work.
6. In the folder `\azure-mobile-services\test\Android\ZumoE2ETestApp\libs`, run either the `getLibs.ps1` script if you are running on Windows or the `getLibs.sh` script if you are running on Linux to download the required dependencies.
7. Import the `\azure-mobile-services\test\Android\ZumoE2ETestApp` project in your Eclipse workspace
8. Once the app is running, go to Settings type your mobile service URL and application key
9. If you also want to test push support, get a Google Cloud Messaging API key from the [Google APIs Console](https://code.google.com/apis/console/) and paste the key in the text box labeled GCM Sender Id
10. Check the tests you want to run and then select "Run selected tests"

## JavaScript SDK

Our JavaScript SDK makes it easy to use our Microsoft Azure Mobile Services in a Windows 8 application or an HTML client. The [Microsoft Azure Mobile Services for WinJS SDK](http://nuget.org/packages/WindowsAzure.MobileServices.WinJS/) is available as a Nuget package or you can download the source for both WinJS and HTML using the instructions above. 

### Prerequisites

The Microsoft Azure Mobile Services for WinJS SDK requires Windows 8 RTM and Visual Studio 2012 RTM. 

### Building and Referencing the SDK

1. Open the ```sdk\JavaScript\Microsoft.WindowsAzure.Mobile.JS.sln``` file in Visual Studio.
2. Press F6 to build the solution. This will generate a single merged JavaScript file that will be used by your application.

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

## Need Help?

Be sure to check out the Mobile Services [Developer Forum](http://social.msdn.microsoft.com/Forums/en-US/azuremobile/) if you are having trouble. The Mobile Services product team actively monitors the forum and will be more than happy to assist you.

## Contribute Code or Provide Feedback

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/Azure/azure-mobile-services/issues) section of the project.

## Learn More
[Microsoft Azure Mobile Services Developer Center](http://azure.microsoft.com/en-us/develop/mobile)
