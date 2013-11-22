#!/bin/bash
if [ "$1" = "" ]; then
  echo Usage: $0 \<applicationName\>
  echo .  Where \<applicationName\> is an Azure Mobile Service app.
  echo .  To run the login tests, the application needs to be configured
  echo .    with all four login providers.
else
  azure mobile table create $1 stringIdRoundTripTable
  azure mobile table create $1 stringIdMovies
  azure mobile table create --integerId $1 public
  azure mobile table create --integerId $1 admin
  azure mobile table create --integerId $1 application
  azure mobile table create --integerId $1 authenticated
  azure mobile table create --integerId $1 intIdMovies
  azure mobile table create --integerId $1 ParamsTestTable

  #Tables specific to JS tests
  azure mobile table create --integerId $1 w8jsRoundTripTable
  azure mobile table create $1 w8jsServerQueryMovies

  #Tables specific to Managed tests
  azure mobile table create --integerId $1 w8RoundTripTable

  #Tables specific to iOS tests
  azure mobile table create --integerId $1 iosRoundTripTable
  azure mobile table create --integerId $1 iosPushTest

  azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin $1 admin
  azure mobile table update -p insert=application,read=application,update=application,delete=application $1 application
  azure mobile table update -p insert=user,read=user,update=user,delete=user $1 authenticated
  azure mobile table update -p insert=application,read=public,update=public,delete=public $1 public

  azure mobile script upload $1 table/stringIdRoundTripTable.insert -f stringIdRoundTripTable.insert.js
  azure mobile script upload $1 table/stringIdRoundTripTable.update -f stringIdRoundTripTable.update.js
  azure mobile script upload $1 table/stringIdRoundTripTable.read -f stringIdRoundTripTable.read.js
  azure mobile script upload $1 table/stringIdMovies.insert -f bothIdTypeMovies.insert.js
  azure mobile script upload $1 table/intIdMovies.insert -f bothIdTypeMovies.insert.js
  azure mobile script upload $1 table/ParamsTestTable.insert -f ParamsTestTable.insert.js
  azure mobile script upload $1 table/ParamsTestTable.read -f ParamsTestTable.read.js
  azure mobile script upload $1 table/ParamsTestTable.delete -f ParamsTestTable.delete.js
  azure mobile script upload $1 table/ParamsTestTable.update -f ParamsTestTable.update.js
  azure mobile script upload $1 table/iosAuthenticated.insert -f iosAuthenticated.insert.js
  azure mobile script upload $1 table/iosAuthenticated.read -f iosAuthenticated.read.js
  azure mobile script upload $1 table/iosAuthenticated.update -f iosAuthenticated.update.js
  azure mobile script upload $1 table/iosAuthenticated.delete -f iosAuthenticated.delete.js

  #Tables specific to JS tests
  azure mobile script upload $1 table/w8jsRoundTripTable.insert -f w8jsRoundTripTable.insert.js
  azure mobile script upload $1 table/w8jsRoundTripTable.read -f w8jsRoundTripTable.read.js
  azure mobile script upload $1 table/w8jsRoundTripTable.update -f w8jsRoundTripTable.update.js
  azure mobile script upload $1 table/w8jsServerQueryMovies.read -f w8jsServerQueryMovies.read.js

  #Tables specific to managed tests
  azure mobile script upload $1 table/w8RoundTripTable.insert -f w8RoundTripTable.insert.js
  azure mobile script upload $1 table/w8RoundTripTable.read -f w8RoundTripTable.read.js
  azure mobile script upload $1 table/w8RoundTripTable.update -f w8RoundTripTable.update.js

  #Tables specific to iOS tests
  azure mobile script upload $1 table/iosRoundTripTable.insert -f iosRoundTripTable.insert.js
  azure mobile script upload $1 table/iosPushTest.insert -f iosPushTest.insert.js

fi
