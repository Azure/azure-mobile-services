$client = new-object System.Net.WebClient 
$shell_app = new-object -com shell.application
$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath

$filename = "google-gson-2.2.2-release.zip"

Write-Host "Downloading Google Gson 2.2.2"
$client.DownloadFile("http://google-gson.googlecode.com/files/google-gson-2.2.2-release.zip", "$dir\$filename") 

Write-Host "Decompressing..."
$zip_file = $shell_app.namespace("$dir\$filename")
$destination = $shell_app.namespace("$dir")
$destination.Copyhere($zip_file.items())

Write-Host "Removing zip"
Remove-Item "$dir\$filename"

Write-Host "Move library to destination"
Move-Item "$dir\google-gson-2.2.2\gson-2.2.2.jar" "$dir"

Write-Host "Remove extra files"
Remove-Item "$dir\google-gson-2.2.2\" -recurse

$filename = "gcm_r03.zip"

Write-Host "Downloading Google Cloud Messaging R3"
$client.DownloadFile("https://dl-ssl.google.com/android/repository/gcm_r03.zip", "$dir\$filename") 

Write-Host "Decompressing..."
$zip_file = $shell_app.namespace("$dir\$filename")
$destination = $shell_app.namespace("$dir")
$destination.Copyhere($zip_file.items())

Write-Host "Removing zip"
Remove-Item "$dir\$filename"

Write-Host "Move library to destination"
Move-Item "$dir\gcm_r03\gcm-client\dist\gcm.jar" "$dir"

Write-Host "Remove extra files"
Remove-Item "$dir\gcm_r03\" -recurse