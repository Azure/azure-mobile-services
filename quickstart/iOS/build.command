# How to build the iOS Framework and Quick Start:
# First, build the framework via Xcode and drop it in the current directory (unzipped)
# Second, execute this script using: bash build.command

# First, we'll remove all previous artifacts created by this script
rm iOS_ObjC.zip
rm WindowsAzureMobileServices.framework.zip
rm -rf ZUMOAPPNAME/WindowsAzureMobileServices.framework

# Copy the license into the framework and then the framework into the Quick Start App
cp license.rtf WindowsAzureMobileServices.framework
rsync -rlK WindowsAzureMobileServices.framework ZUMOAPPNAME

# Zip the Quick Start and the Framework and remove .DS_Store files
zip -r iOS_ObjC.zip ZUMOAPPNAME
zip -d iOS_ObjC.zip *.DS_Store
zip -r WindowsAzureMobileServices.framework.zip WindowsAzureMobileServices.framework
zip -d WindowsAzureMobileServices.framework.zip *.DS_Store