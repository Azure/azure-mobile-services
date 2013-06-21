# This bash script cleans and builds the Windows Azure Mobile Services iOS Framework

# First, make sure we are executing in this script's directory
cd "$( cd "$( dirname "$0" )" && pwd )"

# Second, clean by removing the build directory
rm -rf Build

# Third, generate the build version
VERSION_START_YEAR=2013
DATE_VERSION=$((($(date +%Y) - $VERSION_START_YEAR) + 1))$(date +%m%d)

# Fourth, update the WindowsAzureMobileServices.h file with the build version
if [ "$SET_BUILD_VERSION" == "YES" ]; then
  HEADER_FILE=src/WindowsAzureMobileServices.h
  HEADER_BAK_FILE=$HEADER_FILE.bak
  mv $HEADER_FILE $HEADER_BAK_FILE
  more $HEADER_BAK_FILE | sed "s/\(WindowsAzureMobileServicesSdkBuildVersion\) 0/\1 $DATE_VERSION/" > $HEADER_FILE
fi

# Fifth, build the framework
xcodebuild -target Framework OBJROOT=./Build SYMROOT=./Build

# Sixth, move back to the original WindowsAzureMobileServices.h file
if [ "$SET_BUILD_VERSION" == "YES" ]; then
  rm $HEADER_FILE
  mv $HEADER_BAK_FILE $HEADER_FILE
  echo "BUILD VERSION SET."
fi