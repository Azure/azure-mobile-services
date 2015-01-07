#! /bin/sh

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