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