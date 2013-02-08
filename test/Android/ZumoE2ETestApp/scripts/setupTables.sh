#!/bin/bash

if [ $# -ne 1 ]
then
	echo "Usage: setupTables.sh <AppName>"
else
	azure mobile table create $1 Admin
	azure mobile table create $1 Application
	azure mobile table create $1 Authenticated
	azure mobile table create $1 RoundTripTable
	azure mobile table create $1 Movies
	azure mobile table create $1 PushTest
	azure mobile table create $1 ParamsTestTable

	azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin $1 Admin
	azure mobile table update -p insert=application,read=application,update=application,delete=application $1 Application
	azure mobile table update -p insert=user,read=user,update=user,delete=user $1 Authenticated

	azure mobile script upload $1 table/Movies.insert -f Movies.insert.js
	azure mobile script upload $1 table/RoundTripTable.insert -f RoundTripTable.insert.js
	azure mobile script upload $1 table/RoundTripTable.read -f RoundTripTable.read.js
	azure mobile script upload $1 table/RoundTripTable.update -f RoundTripTable.update.js
	azure mobile script upload $1 table/PushTest.insert -f PushTest.insert.js
	azure mobile script upload $1 table/ParamsTestTable.insert -f ParamsTestTable.insert.js
	azure mobile script upload $1 table/ParamsTestTable.read -f ParamsTestTable.read.js
	azure mobile script upload $1 table/ParamsTestTable.update -f ParamsTestTable.update.js
	azure mobile script upload $1 table/ParamsTestTable.delete -f ParamsTestTable.delete.js	
fi
