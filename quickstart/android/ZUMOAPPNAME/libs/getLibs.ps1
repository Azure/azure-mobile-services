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


$filename = "support_r11.zip"

Write-Host "Downloading Android Support V4 R11"
$client.DownloadFile("https://dl-ssl.google.com/android/repository/support_r11.zip", "$dir\$filename") 

Write-Host "Decompressing..."
$zip_file = $shell_app.namespace("$dir\$filename")
$destination = $shell_app.namespace("$dir")
$destination.Copyhere($zip_file.items())

Write-Host "Removing zip"
Remove-Item "$dir\$filename"

Write-Host "Move library to destination"
Move-Item "$dir\support\v4\android-support-v4.jar" "$dir"

Write-Host "Remove extra files"
Remove-Item "$dir\support\" -recurse