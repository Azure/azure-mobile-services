#!/bin/bash
azure mobile table create ogfiostestapp iosPublic
azure mobile table create ogfiostestapp iosAdmin
azure mobile table create ogfiostestapp iosApplication
azure mobile table create ogfiostestapp iosAuthenticated
azure mobile table create ogfiostestapp iosRoundTripTable
azure mobile table create ogfiostestapp iosMovies
azure mobile table create ogfiostestapp iosPushTest

azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin ogfiostestapp iosAdmin
azure mobile table update -p insert=application,read=application,update=application,delete=application ogfiostestapp iosApplication
azure mobile table update -p insert=user,read=user,update=user,delete=user ogfiostestapp iosAuthenticated
azure mobile table update -p insert=public,read=public,update=public,delete=public ogfiostestapp iosPublic

azure mobile script upload ogfiostestapp table/iosMovies.insert -f iosMovies.insert.js
azure mobile script upload ogfiostestapp table/iosRoundTripTable.insert -f iosRoundTripTable.insert.js
azure mobile script upload ogfiostestapp table/iosPushTest.insert -f iosPushTest.insert.js
