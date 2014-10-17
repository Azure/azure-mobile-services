# Phonegap QS Build Script
# Installs the required plugins and builds the platforms supported when
# using OSX.

# Plugins required for authentication
phonegap local plugin add org.apache.cordova.inappbrowser

# Plugins required for push notifications
# phonegap local plugin add org.apache.cordova.device
# phonegap local plugin add https://github.com/phonegap-build/PushPlugin.git

# Now build supported platforms on OSX
phonegap local build android
phonegap local build ios
