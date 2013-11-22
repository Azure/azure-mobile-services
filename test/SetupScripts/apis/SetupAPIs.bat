@echo off
if "%1" == "" goto :Error
goto :Start

:Error
echo usage: %0 ^<application name^>
echo   where application name is a azure mobile Service app.
goto :TheEnd

:Start
call azure mobile api create %1 admin --permissions *=admin
call azure mobile api create %1 application --permissions *=application
call azure mobile api create %1 user  --permissions *=user
call azure mobile api create %1 public --permissions *=public
call azure mobile api create %1 shared --permissions *=admin
call azure mobile api create %1 movieFinder --permissions *=application

call azure mobile script upload %1 api/admin.js -f admin.js
call azure mobile script upload %1 api/application.js -f application.js
call azure mobile script upload %1 api/user.js -f user.js
call azure mobile script upload %1 api/public.js -f public.js
call azure mobile script upload %1 api/shared.js -f shared.js
call azure mobile script upload %1 api/moviefinder.js -f moviefinder.js

:TheEnd
