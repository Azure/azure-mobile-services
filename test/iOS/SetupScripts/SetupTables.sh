#!/bin/bash
azure mobile table create ogfiostestapp iosAdmin
azure mobile table create ogfiostestapp iosApplication
azure mobile table create ogfiostestapp iosAuthenticated
azure mobile table create ogfiostestapp iosTodoItem
azure mobile table create ogfiostestapp iosMovies
azure mobile table create ogfiostestapp iosPushTest

azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin ogfiostestapp iosAdmin
azure mobile table update -p insert=application,read=application,update=application,delete=application ogfiostestapp iosApplication
azure mobile table update -p insert=user,read=user,update=user,delete=user ogfiostestapp iosAuthenticated

azure mobile script upload ogfiostestapp table/iosMovies.insert -f iosMovies.insert.js
azure mobile script upload ogfiostestapp table/iosTodoItem.insert -f iosTodoItem.insert.js
azure mobile script upload ogfiostestapp table/iosPushTest.insert -f iosPushTest.insert.js
