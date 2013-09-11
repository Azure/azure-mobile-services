@echo OFF

if "%4"=="" goto :Usage
set _APP_URL=%1
set _APP_KEY=%2
set _UPLOAD_LOG_URL=%3
set _GCM_SENDER_ID=%4

where adb > NUL 2>&1
IF ERRORLEVEL 1 goto :MissingAdb

REM Creates the preferences file to be sent to the app
echo { > tempAutoPrefs.txt
echo "pref_mobile_service_url":"%_APP_URL%", >> tempAutoPrefs.txt
echo "pref_mobile_service_key":"%_APP_KEY%", >> tempAutoPrefs.txt
echo "pref_log_post_url":"%_UPLOAD_LOG_URL%", >> tempAutoPrefs.txt
echo "pref_GCM_sender_id":"%_GCM_SENDER_ID%", >> tempAutoPrefs.txt
echo "pref_run_unattended":true >> tempAutoPrefs.txt
echo } >> tempAutoPrefs.txt

echo Created the automation preferences file

REM Remove signalling file, ignore error if it doesn't exist
adb shell rm /sdcard/zumo/done.txt > NUL

REM Stops any running instances of the app on the device
adb shell am force-stop com.microsoft.windowsazure.mobileservices.zumoe2etestapp

REM Copies the preferences file to the app
adb push tempAutoPrefs.txt /sdcard/zumo/automationPreferences.txt

REM Starts the app on the device
echo Starting the test app...
adb shell am start com.microsoft.windowsazure.mobileservices.zumoe2etestapp/.MainActivity


:WaitForFinish
echo Waiting for the app to finish...
timeout /T 10 /NOBREAK > NUL
adb pull /sdcard/zumo/done.txt IAmDone.txt 2> NUL
IF ERRORLEVEL 1 goto :WaitForFinish

:Done
REM Stops the running instance of the app
adb shell am force-stop com.microsoft.windowsazure.mobileservices.zumoe2etestapp

REM Read the test status
SET /P _TEST_STATUS=<IAmDone.txt

IF "%_TEST_STATUS%"=="PASSED" goto :TestPassed
:TestFailed
echo The test run failed
EXIT /B 1
goto :TheEnd

:TestPassed
echo The test run passed!
EXIT /B 0

:Usage
echo Usage: %0 ^<app url^> ^<app key^> ^<log server url^> ^<GCM sender id^>
goto :TheEnd

:MissingAdb
echo Error: Cannot find adb.exe
echo Make sure that the adb.exe (from ADT's sdk\platform-tools folder) is in the path
goto :TheEnd

:TheEnd
