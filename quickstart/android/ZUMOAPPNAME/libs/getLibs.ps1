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