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
REM Tables specific to functional tests
call azure mobile table create --integerId %1 blog_posts
call azure mobile table create --integerId %1 blog_comments
call azure mobile table create --integerId %1 books
call azure mobile table create %1 stringId_test_table
call azure mobile table create --integerId %1 test_table
call azure mobile table create --integerId %1 types

REM Tables specific to E2E tests
call azure mobile table create --integerId %1 admin
call azure mobile table create --integerId %1 application
call azure mobile table create --integerId %1 authenticated
call azure mobile table create --integerId %1 intIdMovies
call azure mobile table create --integerId %1 ParamsTestTable
call azure mobile table create --integerId %1 public
call azure mobile table create %1 stringIdRoundTripTable
call azure mobile table create %1 stringIdMovies

REM Tables specific to JS tests
call azure mobile table create --integerId %1 w8jsRoundTripTable
call azure mobile table create %1 w8jsServerQueryMovies

REM Tables specific to Managed tests
call azure mobile table create %1 --integerId w8RoundTripTable

REM Tables specific to iOS tests
call azure mobile table create --integerId %1 iosRoundTripTable

REM Tables specific to push tests
call azure mobile table create %1 iosPushTest
call azure mobile table create %1 w8PushTest
call azure mobile table create %1 wp8PushTest

REM Permissions
REM Tables specific to unit tests
call azure mobile table update --addColumn title=string,commentCount=number,showComments=boolean,data=string -p insert=public,read=public,update=application,delete=public %1 blog_posts
call azure mobile table update --addColumn postId=string,commentText=string,name=string,test=number -p insert=public,read=public,update=public,delete=public %1 blog_comments
call azure mobile table update --addColumn title=string,type=string,pub_id=string,price=number,advance=number,royalty=number,ytd_sales=number,notes=string,pubdate=date -p insert=public,read=public,update=public,delete=public %1 books
call azure mobile table update --addColumn col5=boolean,name=string -p insert=public,read=public,update=public,delete=public %1 stringId_test_table
call azure mobile table update --addColumn col5=boolean,__anything=string -p insert=public,read=public,update=public,delete=public %1 test_table
call azure mobile table update --addColumn numCol=number,stringCol=string,dateCol=date,boolCol=boolean -p insert=public,read=public,update=public,delete=public %1 types

REM Tables specific to E2E tests
call azure mobile table update -p insert=admin,read=admin,update=admin,delete=admin %1 admin
call azure mobile table update -p insert=application,read=application,update=application,delete=application %1 application
call azure mobile table update -p insert=user,read=user,update=user,delete=user %1 authenticated
call azure mobile table update -p insert=application,read=public,update=public,delete=public %1 public
call azure mobile table update -p insert=admin,read=application,update=admin,delete=admin %1 w8jsServerQueryMovies

REM Scripts
REM Tables specific to unit tests
call azure mobile script upload %1 table/blog_comments.insert -f blog_comments.insert.js
call azure mobile script upload %1 table/blog_comments.read -f blog_comments.read.js

REM Tables specific to E2E tests
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

REM Tables specific to push tests
call azure mobile script upload %1 table/iosPushTest.insert -f iosPushTest.insert.js
call azure mobile script upload %1 table/w8PushTest.insert -f w8PushTest.insert.js
call azure mobile script upload %1 table/wp8PushTest.insert -f wp8PushTest.insert.js

:TheEnd
