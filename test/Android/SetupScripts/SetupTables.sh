#!/bin/bash
azure mobile table create ogfiostestapp droidPublic
azure mobile table create ogfiostestapp droidAdmin
azure mobile table create ogfiostestapp droidApplication
azure mobile table create ogfiostestapp droidAuthenticated
azure mobile table create ogfiostestapp droidRoundTripTable
azure mobile table create ogfiostestapp droidMovies
azure mobile table create ogfiostestapp droidPushTest

azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin ogfiostestapp droidAdmin
azure mobile table update -p insert=application,read=application,update=application,delete=application ogfiostestapp droidApplication
azure mobile table update -p insert=user,read=user,update=user,delete=user ogfiostestapp droidAuthenticated
azure mobile table update -p insert=application,read=public,update=public,delete=public ogfiostestapp droidPublic

azure mobile script upload ogfiostestapp table/droidMovies.insert -f droidMovies.insert.js
azure mobile script upload ogfiostestapp table/droidRoundTripTable.insert -f droidRoundTripTable.insert.js
azure mobile script upload ogfiostestapp table/droidPushTest.insert -f droidPushTest.insert.js
