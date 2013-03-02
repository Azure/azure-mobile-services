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

echo "Downloading Apache HttpClient 4.2.3"
curl -O http://apache.mirrors.pair.com//httpcomponents/httpclient/binary/httpcomponents-client-4.2.3-bin.zip

echo "Decompressing..."
unzip httpcomponents-client-4.2.3-bin.zip

echo "Removing zip"
rm httpcomponents-client-4.2.3-bin.zip

echo "Move library to destination"
mv ./httpcomponents-client-4.2.3/lib/httpclient-4.2.3.jar ./

echo "Remove extra files"
rm -rf ./httpcomponents-client-4.2.3
