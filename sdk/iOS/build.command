# This bash script cleans and builds the Microsoft Azure Mobile Services iOS Framework

# Make sure we are executing in this script's directory
cd "$( cd "$( dirname "$0" )" && pwd )"

# Clean by removing the build directory
rm -rf Build
rm WindowsAzureMobileServices.framework.zip
rm -rf WindowsAzureMobileServices.framework

# Generate the build version
VERSION_START_YEAR=2013
DATE_VERSION=$((($(date +%Y) - $VERSION_START_YEAR) + 1))$(date +%m%d)

# Update the WindowsAzureMobileServices.h file with the build version
if [ "$SET_BUILD_VERSION" == "YES" ]; then
  HEADER_FILE=src/WindowsAzureMobileServices.h
  HEADER_BAK_FILE=$HEADER_FILE.bak
  mv $HEADER_FILE $HEADER_BAK_FILE
  more $HEADER_BAK_FILE | sed "s/\(WindowsAzureMobileServicesSdkBuildVersion\) 0/\1 $DATE_VERSION/" > $HEADER_FILE
fi

# Build the framework
xcodebuild -target Framework OBJROOT=./Build SYMROOT=./Build

# Move back to the original WindowsAzureMobileServices.h file
if [ "$SET_BUILD_VERSION" == "YES" ]; then
  rm $HEADER_FILE
  mv $HEADER_BAK_FILE $HEADER_FILE
  echo "BUILD VERSION SET."
fi

# Copy the framework into this directory and add the license
rsync -rlK ../../sdk/iOS/Build/Release-iphoneos/WindowsAzureMobileServices.framework .
cp license.rtf WindowsAzureMobileServices.framework

# Zip the framework
zip -r WindowsAzureMobileServices.framework.zip WindowsAzureMobileServices.framework 

# Lastly, copy to the build share
if [ "$COPY_TO_SHARE" == "YES" ]; then
  SHARE_PATH_ARRAY=$(echo $BUILD_SHARE_PATHS | tr ";" "\n")
  for PATH in $SHARE_PATH_ARRAY
  do
    rsync -rlK WindowsAzureMobileServices.framework.zip $PATH
  done
fi 