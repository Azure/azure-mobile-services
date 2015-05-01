# Phonegap QS Build Script
# Installs the required plugins and builds the platforms supported when
# using OSX.

phonegap local plugin add com.microsoft.azure-mobile-services

# Plugins required for push notifications
# phonegap local plugin add org.apache.cordova.device
# phonegap local plugin add https://github.com/phonegap-build/PushPlugin.git

# Now build supported platforms
phonegap local build android
phonegap local build ios
