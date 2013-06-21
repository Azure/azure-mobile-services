# This bash script cleans and builds the Windows Azure Mobile Services QuickStart project

# First, we'll remove all previous artifacts created by this script
rm iOS_ObjC.zip
rm WindowsAzureMobileServices.framework.zip
rm -rf ZUMOAPPNAME/WindowsAzureMobileServices.framework

# Second, build the Windows Azure Mobile Services iOS Framework
bash ../../sdk/iOS/build.command

# Third, copy the framework over into this directory
rsync -rlK ../../sdk/iOS/Build/Release-iphoneos/WindowsAzureMobileServices.framework .

# Fourth, copy the license into the framework and then the framework into the Quick Start App
cp license.rtf WindowsAzureMobileServices.framework
rsync -rlK WindowsAzureMobileServices.framework ZUMOAPPNAME

# Ensure that there is not a build folder in the SDK
rm -rf ZUMOAPPNAME/Build 

# Zip the Quick Start and the Framework and remove .DS_Store files
zip -r iOS_ObjC.zip ZUMOAPPNAME
zip -d iOS_ObjC.zip *.DS_Store
zip -r WindowsAzureMobileServices.framework.zip WindowsAzureMobileServices.framework
zip -d WindowsAzureMobileServices.framework.zip *.DS_Store

# Copy to the build share
if [ "$COPY_TO_SHARE" == "YES" ]; then
  SHARE_PATH_ARRAY=$(echo $SHARE_PATHS | tr ";" "\n")
  for PATH in $SHARE_PATH_ARRAY
  do
    rsync -rlK WindowsAzureMobileServices.framework.zip $PATH
    rsync -rlK iOS_ObjC.zip $PATH
  done
fi


