#!/bin/bash
if [ "$1" = "" ]; then
  echo Usage: $0 \<applicationName\>
  echo .  Where \<applicationName\> is an Azure Mobile Service app.
  echo .  To run the login tests, the application needs to be configured
  echo .    with all four login providers.
else
  azure mobile table create $1 iosPublic
  azure mobile table create $1 iosAdmin
  azure mobile table create $1 iosApplication
  azure mobile table create $1 iosAuthenticated
  azure mobile table create $1 iosRoundTripTable
  azure mobile table create $1 iosMovies
  azure mobile table create $1 iosPushTest
  azure mobile table create $1 ParamsTestTable

  azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin $1 iosAdmin
  azure mobile table update -p insert=application,read=application,update=application,delete=application $1 iosApplication
  azure mobile table update -p insert=user,read=user,update=user,delete=user $1 iosAuthenticated
  azure mobile table update -p insert=application,read=public,update=public,delete=public $1 iosPublic

  azure mobile script upload $1 table/iosMovies.insert -f iosMovies.insert.js
  azure mobile script upload $1 table/iosRoundTripTable.insert -f iosRoundTripTable.insert.js
  azure mobile script upload $1 table/iosPushTest.insert -f iosPushTest.insert.js
  azure mobile script upload $1 table/iosAuthenticated.read -f iosAuthenticated.read.js
  azure mobile script upload $1 table/ParamsTestTable.insert -f ParamsTestTable.insert.js
  azure mobile script upload $1 table/ParamsTestTable.read -f ParamsTestTable.read.js
  azure mobile script upload $1 table/ParamsTestTable.delete -f ParamsTestTable.delete.js
  azure mobile script upload $1 table/ParamsTestTable.update -f ParamsTestTable.update.js

fi
