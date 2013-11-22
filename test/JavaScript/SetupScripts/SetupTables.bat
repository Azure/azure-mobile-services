@echo off
if "%1" == "" goto :Error
goto :Start

:Error
echo usage: %0 ^<application name^>
echo   where application name is an  azure mobileService app.
echo   To run the login tests, the application needs to be configured
echo   with all four providers.
goto :TheEnd

:Start
call azure mobile table create --integerId %1 admin
call azure mobile table create --integerId %1 application
call azure mobile table create --integerId %1 authenticated
call azure mobile table create --integerId %1 public
call azure mobile table create --integerId %1 w8jsRoundTripTable
call azure mobile table create --integerId %1 intIdMovies
call azure mobile table create %1 w8jsServerQueryMovies
call azure mobile table create %1 w8PushTest
call azure mobile table create %1 ParamsTestTable

call azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin %1 w8Admin
call azure mobile table update -p insert=application,read=application,update=application,delete=application %1 w8Application
call azure mobile table update -p insert=user,read=user,update=user,delete=user %1 w8Authenticated
call azure mobile table update -p insert=application,read=public,update=public,delete=public %1 w8Public
call azure mobile table update -p insert=admin,read=application,update=admin,delete=admin %1 w8jsServerQueryMovies

call azure mobile script upload %1 table/w8PushTest.insert -f w8PushTest.insert.js
call azure mobile script upload %1 table/ParamsTestTable.insert -f ParamsTestTable.insert.js
call azure mobile script upload %1 table/ParamsTestTable.read -f ParamsTestTable.read.js
call azure mobile script upload %1 table/ParamsTestTable.update -f ParamsTestTable.update.js
call azure mobile script upload %1 table/ParamsTestTable.delete -f ParamsTestTable.delete.js
call azure mobile script upload %1 table/w8Authenticated.insert -f w8Authenticated.insert.js
call azure mobile script upload %1 table/w8Authenticated.read -f w8Authenticated.read.js
call azure mobile script upload %1 table/w8Authenticated.update -f w8Authenticated.update.js
call azure mobile script upload %1 table/w8Authenticated.delete -f w8Authenticated.delete.js
call azure mobile script upload %1 table/w8Public.update -f w8Public.update.js
call azure mobile script upload %1 table/w8jsMovies.insert -f w8jsMovies.insert.js
call azure mobile script upload %1 table/w8jsRoundTripTable.insert -f w8jsRoundTripTable.insert.js
call azure mobile script upload %1 table/w8jsRoundTripTable.read -f w8jsRoundTripTable.read.js
call azure mobile script upload %1 table/w8jsRoundTripTable.update -f w8jsRoundTripTable.update.js
call azure mobile script upload %1 table/w8jsServerQueryMovies.read -f w8jsServerQueryMovies.read.js

:TheEnd
