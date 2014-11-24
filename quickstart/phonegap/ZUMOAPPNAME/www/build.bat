REM Phonegap QS Build Script
REM Installs the required plugins and builds the platforms supported when
REM using Windows.

call phonegap local plugin add com.microsoft.azure-mobile-services

REM Plugins required for push notifications
REM call phonegap local plugin add org.apache.cordova.device
REM call phonegap local plugin add https://github.com/phonegap-build/PushPlugin.git

REM Build supported platforms
call phonegap local build wp8
call phonegap local build android