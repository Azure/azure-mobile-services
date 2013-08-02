@echo off
IF "%2" == "" GOTO :Error
goto :Start

:Error
echo usage: %0 ^<output directory^> ^<zip version^>
echo   where output directory is where the .jar files will be placed
echo   and zip version is the version to be appended to the .jar files
goto :TheEnd

:Start
IF NOT EXIST "%1" (
    md "%1"
    echo . Created output directory %1
)

echo Creating the jar with the binaries...
jar cf "%1\mobileservices-%2.jar" -C ..\sdk\bin\classes .
jar cf "%1\notifications-%2.jar" -C ..\notifications-handler\bin\classes .
echo ... done

echo Creating the jar with the docs...
jar cf "%1\mobileservices-%2-javadoc.jar" -C ..\sdk\doc .
jar cf "%1\notifications-%2-javadoc.jar" -C ..\notifications-handler\doc .
echo ... done

echo Creating the jar with the sources...
jar cf "%1\mobileservices-%2-sources.jar" -C ..\sdk\src .
jar cf "%1\notifications-%2-sources.jar" -C ..\notifications-handler\src .
echo ... done

echo Creating the properties file...
echo src=mobileservices-%2-sources.jar > "%1\mobileservices-%2.jar.properties"
echo doc=mobileservices-%2-javadoc.jar >> "%1\mobileservices-%2.jar.properties"
echo src=notifications-%2-sources.jar > "%1\notifications-%2.jar.properties"
echo doc=notifications-%2-javadoc.jar >> "%1\notifications-%2.jar.properties"
echo ... done.

echo Copying the license files...
copy /y License.rtf "%1"
copy /y thirdpartynotices.rtf "%1"
echo ... done.

echo Finished.

:TheEnd
