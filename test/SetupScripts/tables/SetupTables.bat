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
call azure mobile table create %1 stringIdRoundTripTable
call azure mobile table create %1 stringIdMovies
call azure mobile table create --integerId %1 intIdMovies
call azure mobile table create --integerId %1 ParamsTestTable
call azure mobile table create --integerId %1 admin
call azure mobile table create --integerId %1 application
call azure mobile table create --integerId %1 authenticated
call azure mobile table create --integerId %1 public

REM Tables specific to JS tests
call azure mobile table create --integerId %1 w8jsRoundTripTable
call azure mobile table create %1 w8jsServerQueryMovies

REM Tables specific to Managed tests
call azure mobile table create %1 --integerId w8RoundTripTable

REM Tables specific to iOS tests
call azure mobile table create --integerId %1 iosRoundTripTable
call azure mobile table create --integerId %1 iosPushTest

call azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin %1 admin
call azure mobile table update -p insert=application,read=application,update=application,delete=application %1 application
call azure mobile table update -p insert=user,read=user,update=user,delete=user %1 authenticated
call azure mobile table update -p insert=application,read=public,update=public,delete=public %1 public
call azure mobile table update -p insert=admin,read=application,update=admin,delete=admin %1 w8jsServerQueryMovies

call azure mobile script upload %1 table/stringIdRoundTripTable.insert -f stringIdRoundTripTable.insert.js
call azure mobile script upload %1 table/stringIdRoundTripTable.read -f stringIdRoundTripTable.read.js
call azure mobile script upload %1 table/stringIdRoundTripTable.update -f stringIdRoundTripTable.update.js
call azure mobile script upload %1 table/stringIdMovies.insert -f bothIdTypeMovies.insert.js
call azure mobile script upload %1 table/intIdMovies.insert -f bothIdTypeMovies.insert.js
call azure mobile script upload %1 table/ParamsTestTable.insert -f ParamsTestTable.insert.js
call azure mobile script upload %1 table/ParamsTestTable.read -f ParamsTestTable.read.js
call azure mobile script upload %1 table/ParamsTestTable.update -f ParamsTestTable.update.js
call azure mobile script upload %1 table/ParamsTestTable.delete -f ParamsTestTable.delete.js
call azure mobile script upload %1 table/authenticated.insert -f authenticated.insert.js
call azure mobile script upload %1 table/authenticated.read -f authenticated.read.js
call azure mobile script upload %1 table/authenticated.update -f authenticated.update.js
call azure mobile script upload %1 table/authenticated.delete -f authenticated.delete.js

REM Tables specific to JS tests
call azure mobile script upload %1 table/w8jsRoundTripTable.insert -f w8jsRoundTripTable.insert.js
call azure mobile script upload %1 table/w8jsRoundTripTable.read -f w8jsRoundTripTable.read.js
call azure mobile script upload %1 table/w8jsRoundTripTable.update -f w8jsRoundTripTable.update.js
call azure mobile script upload %1 table/w8jsServerQueryMovies.read -f w8jsServerQueryMovies.read.js

REM Tables specific to managed tests
call azure mobile script upload %1 table/w8RoundTripTable.insert -f w8RoundTripTable.insert.js
call azure mobile script upload %1 table/w8RoundTripTable.read -f w8RoundTripTable.read.js
call azure mobile script upload %1 table/w8RoundTripTable.update -f w8RoundTripTable.update.js

REM Tables specific to iOS tests
call azure mobile script upload %1 table/iosRoundTripTable.insert -f iosRoundTripTable.insert.js
call azure mobile script upload %1 table/iosPushTest.insert -f iosPushTest.insert.js

:TheEnd
