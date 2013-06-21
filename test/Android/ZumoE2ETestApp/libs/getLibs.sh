#! /bin/sh

echo "Downloading Google Gson 2.2.2"
curl -O http://google-gson.googlecode.com/files/google-gson-2.2.2-release.zip

echo "Decompressing..."
unzip google-gson-2.2.2-release.zip

echo "Removing zip"
rm google-gson-2.2.2-release.zip

echo "Move library to destination"
mv ./google-gson-2.2.2/gson-2.2.2.jar ./

echo "Remove extra files"
rm -rf ./google-gson-2.2.2

echo "Downloading Google Cloud Messaging R3"
curl -O https://dl-ssl.google.com/android/repository/gcm_r03.zip

echo "Decompressing..."
unzip gcm_r03.zip

echo "Removing zip"
rm gcm_r03.zip

echo "Move library to destination"
mv ./gcm_r03/gcm-client/dist/gcm.jar ./

echo "Remove extra files"
rm -rf ./gcm_r03