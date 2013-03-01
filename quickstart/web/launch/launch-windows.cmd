@echo off
set sitename=ZUMOAPPNAME-quickstart
set port=8000

rem Find IIS Express or abort if not installed
if defined "programfiles(x86)" (
    set iisexpressdir="%programfiles(x86)%\IIS Express"
) else (
    set iisexpressdir="%programfiles%\IIS Express"
)
set iisexpressdir=%iisexpressdir:~1,-1%
if not exist "%iisexpressdir%" (
    echo IIS Express not found. Please make sure it is installed.
    pause
    start http://www.microsoft.com/web/gallery/install.aspx?appid=IISExpress
    goto :end
)

rem Recreate the site in IIS Express
set siteexists=false
for /f %%i in ('"%iisexpressdir%\appcmd" list site %sitename%') do set siteexists=true
if "%siteexists%" == "true" ( "%iisexpressdir%\appcmd" delete site %sitename% )
pushd .
cd /d "%~dp0..\"
"%iisexpressdir%\appcmd" add site /name:%sitename% /bindings:"http/*:%port%:localhost" /physicalPath:"%cd%"
popd

rem Start IIS Express
"%iisexpressdir%\iisexpress" /site:%sitename%

:end