#!/bin/bash

if [ $# -ne 1 ]
then
	echo "Usage: setupTables.sh <AppName>"
else
	azure mobile table create $1 droidAdmin
	azure mobile table create $1 droidApplication
	azure mobile table create $1 droidAuthenticated
	azure mobile table create $1 droidRoundTripTable
	azure mobile table create $1 droidMovies
	azure mobile table create $1 droidPushTest
	azure mobile table create $1 ParamsTestTable

	azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin $1 droidAdmin
	azure mobile table update -p insert=application,read=application,update=application,delete=application $1 droidApplication
	azure mobile table update -p insert=user,read=user,update=user,delete=user $1 droidAuthenticated

	azure mobile script upload $1 table/droidMovies.insert -f droidMovies.insert.js
	azure mobile script upload $1 table/droidRoundTripTable.insert -f droidRoundTripTable.insert.js
	azure mobile script upload $1 table/droidRoundTripTable.read -f droidRoundTripTable.read.js
	azure mobile script upload $1 table/droidRoundTripTable.update -f droidRoundTripTable.update.js
	azure mobile script upload $1 table/droidPushTest.insert -f droidPushTest.insert.js
	azure mobile script upload $1 table/ParamsTestTable.insert -f ParamsTestTable.insert.js
	azure mobile script upload $1 table/ParamsTestTable.read -f ParamsTestTable.read.js
	azure mobile script upload $1 table/ParamsTestTable.update -f ParamsTestTable.update.js
	azure mobile script upload $1 table/ParamsTestTable.delete -f ParamsTestTable.delete.js	
fi
