azure mobile table create ogfiostestapp w8Admin
azure mobile table create ogfiostestapp w8Application
azure mobile table create ogfiostestapp w8Authenticated
azure mobile table create ogfiostestapp w8RoundTripTable
azure mobile table create ogfiostestapp w8Movies
azure mobile table create ogfiostestapp w8PushTest
azure mobile table create ogfiostestapp ParamsTestTable

azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin ogfiostestapp w8Admin
azure mobile table update -p insert=application,read=application,update=application,delete=application ogfiostestapp w8Application
azure mobile table update -p insert=user,read=user,update=user,delete=user ogfiostestapp w8Authenticated

azure mobile script upload ogfiostestapp table/w8Movies.insert -f iosMovies.insert.js
azure mobile script upload ogfiostestapp table/w8RoundTripTable.insert -f w8RoundTripTable.insert.js
azure mobile script upload ogfiostestapp table/w8PushTest.insert -f w8PushTest.insert.js