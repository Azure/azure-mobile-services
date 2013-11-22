@echo off
if "%1" == "" goto :Error
goto :Start

:Error
echo usage: %0 ^<application name^>
echo   where application name is an call azure mobile Service app.
echo   To run the login tests, the application needs to be configured
echo   with all four providers.
goto :TheEnd

:Start
call azure mobile  table create %1 droidPublic
call azure mobile  table create %1 droidAdmin
call azure mobile  table create %1 droidApplication
call azure mobile  table create %1 droidAuthenticated
call azure mobile  table create %1 droidRoundTripTable
call azure mobile  table create %1 droidMovies
call azure mobile  table create %1 droidPushTest
call azure mobile  table create %1 ParamsTestTable
call azure mobile  table create %1 droidStringIdTable

call azure mobile  table update -p insert=admin,read=admin,update=admin,delete=admin %1 droidAdmin
call azure mobile  table update -p insert=application,read=application,update=application,delete=application %1 droidApplication
call azure mobile  table update -p insert=user,read=user,update=user,delete=user %1 droidAuthenticated
call azure mobile  table update -p insert=application,read=public,update=public,delete=public %1 droidPublic

call azure mobile  script upload %1 table/droidMovies.insert -f droidMovies.insert.js
call azure mobile  script upload %1 table/droidRoundTripTable.insert -f droidRoundTripTable.insert.js
call azure mobile  script upload %1 table/droidRoundTripTable.read -f droidRoundTripTable.read.js
call azure mobile  script upload %1 table/droidRoundTripTable.update -f droidRoundTripTable.update.js
call azure mobile  script upload %1 table/droidPushTest.insert -f droidPushTest.insert.js
call azure mobile  script upload %1 table/droidAuthenticated.read -f droidAuthenticated.read.js
call azure mobile  script upload %1 table/ParamsTestTable.insert -f ParamsTestTable.insert.js
call azure mobile  script upload %1 table/ParamsTestTable.read -f ParamsTestTable.read.js
call azure mobile  script upload %1 table/ParamsTestTable.delete -f ParamsTestTable.delete.js
call azure mobile  script upload %1 table/ParamsTestTable.update -f ParamsTestTable.update.js


:TheEnd