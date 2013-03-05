$client = new-object System.Net.WebClient 
$shell_app = new-object -com shell.application

Write-Host "Downloading Google Gson 2.2.2"
$client.DownloadFile("http://google-gson.googlecode.com/files/google-gson-2.2.2-release.zip", "google-gson-2.2.2-release.zip") 

Write-Host "Decompressing..."
$filename = "google-gson-2.2.2-release.zip"
$zip_file = $shell_app.namespace((Get-Location).Path + "\$filename")
$destination = $shell_app.namespace((Get-Location).Path)
$destination.Copyhere($zip_file.items())

Write-Host "Removing zip"
Remove-Item google-gson-2.2.2-release.zip

Write-Host "Move library to destination"
Move-Item .\google-gson-2.2.2\gson-2.2.2.jar .\

Write-Host "Remove extra files"
Remove-Item .\google-gson-2.2.2\ -recurse

Write-Host "Downloading Apache HttpClient 4.2.3"
$client.DownloadFile("http://apache.mirrors.pair.com//httpcomponents/httpclient/binary/httpcomponents-client-4.2.3-bin.zip", "httpcomponents-client-4.2.3-bin.zip") 

Write-Host "Decompressing..."
$filename = "httpcomponents-client-4.2.3-bin.zip"
$zip_file = $shell_app.namespace((Get-Location).Path + "\$filename")
$destination = $shell_app.namespace((Get-Location).Path)
$destination.Copyhere($zip_file.items())

Write-Host "Removing zip"
Remove-Item httpcomponents-client-4.2.3-bin.zip

Write-Host "Move library to destination"
Move-Item .\httpcomponents-client-4.2.3\lib\httpclient-4.2.3.jar .\

Write-Host "Remove extra files"
Remove-Item .\httpcomponents-client-4.2.3\ -recurse

Write-Host "Downloading Android Support V4 R11"
$client.DownloadFile("https://dl-ssl.google.com/android/repository/support_r11.zip", "support_r11.zip") 

Write-Host "Decompressing..."
$filename = "support_r11.zip"
$zip_file = $shell_app.namespace((Get-Location).Path + "\$filename")
$destination = $shell_app.namespace((Get-Location).Path)
$destination.Copyhere($zip_file.items())

Write-Host "Removing zip"
Remove-Item support_r11.zip

Write-Host "Move library to destination"
Move-Item .\support\v4\android-support-v4.jar .\

Write-Host "Remove extra files"
Remove-Item .\support\ -recurse

Write-Host "Downloading Google Cloud Messaging R3"
$client.DownloadFile("https://dl-ssl.google.com/android/repository/gcm_r03.zip", "gcm_r03.zip") 

Write-Host "Decompressing..."
$filename = "gcm_r03.zip"
$zip_file = $shell_app.namespace((Get-Location).Path + "\$filename")
$destination = $shell_app.namespace((Get-Location).Path)
$destination.Copyhere($zip_file.items())

Write-Host "Removing zip"
Remove-Item gcm_r03.zip

Write-Host "Move library to destination"
Move-Item .\gcm_r03\gcm-client\dist\gcm.jar .\

Write-Host "Remove extra files"
Remove-Item .\gcm_r03 -recurse