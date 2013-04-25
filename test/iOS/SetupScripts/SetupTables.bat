@echo off
if "%1" == "" goto :Error
goto :Start

:Error
echo usage: %0 ^<application name^>
echo   where application name is a azure mobile  Service app.
echo   To run the login tests, the application needs to be configured
echo   with all four providers.
goto :TheEnd

:Start
  call azure mobile  table create %1 iosPublic
  call azure mobile  table create %1 iosAdmin
  call azure mobile  table create %1 iosApplication
  call azure mobile  table create %1 iosAuthenticated
  call azure mobile  table create %1 iosRoundTripTable
  call azure mobile  table create %1 iosMovies
  call azure mobile  table create %1 iosPushTest
  call azure mobile  table create %1 ParamsTestTable

  call azure mobile  table update -p insert=admin,read=admin,update=admin,delete=admin %1 iosAdmin
  call azure mobile  table update -p insert=application,read=application,update=application,delete=application %1 iosApplication
  call azure mobile  table update -p insert=user,read=user,update=user,delete=user %1 iosAuthenticated
  call azure mobile  table update -p insert=application,read=public,update=public,delete=public %1 iosPublic

  call azure mobile  script upload %1 table/iosMovies.insert -f iosMovies.insert.js
  call azure mobile  script upload %1 table/iosRoundTripTable.insert -f iosRoundTripTable.insert.js
  call azure mobile  script upload %1 table/iosPushTest.insert -f iosPushTest.insert.js
  call azure mobile  script upload %1 table/iosAuthenticated.read -f iosAuthenticated.read.js
  call azure mobile  script upload %1 table/ParamsTestTable.insert -f ParamsTestTable.insert.js
  call azure mobile  script upload %1 table/ParamsTestTable.read -f ParamsTestTable.read.js
  call azure mobile  script upload %1 table/ParamsTestTable.delete -f ParamsTestTable.delete.js
  call azure mobile  script upload %1 table/ParamsTestTable.update -f ParamsTestTable.update.js

:TheEnd

