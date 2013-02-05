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
$client.DownloadFile("http://apache.xfree.com.ar//httpcomponents/httpclient/binary/httpcomponents-client-4.2.3-bin.zip", "httpcomponents-client-4.2.3-bin.zip") 

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
