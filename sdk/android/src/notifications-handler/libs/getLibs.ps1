$client = new-object System.Net.WebClient 
$shell_app = new-object -com shell.application
$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath

$filename = "google_play_services_3159130_r09.zip"

Write-Host "Downloading Google Play Services R9"
$client.DownloadFile("https://dl-ssl.google.com/android/repository/google_play_services_3159130_r09.zip", "$dir\$filename") 

Write-Host "Decompressing..."
$zip_file = $shell_app.namespace("$dir\$filename")
$destination = $shell_app.namespace("$dir")
$destination.Copyhere($zip_file.items())

Write-Host "Removing zip"
Remove-Item "$dir\$filename"

Write-Host "Move library to destination"
Move-Item "$dir\google-play-services\libproject\google-play-services_lib\libs\google-play-services.jar" "$dir"

Write-Host "Remove extra files"
Remove-Item "$dir\google-play-services\" -recurse