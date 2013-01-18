End-to-end Test Application for Azure Mobile Services

Setup:
- Create a new (or reuse an existing) Azure Mobile application
- The service setup can be done using the CLI scripts under SetupScripts.
    - Run SetupScripts.bat (for new applications)
    - If the application already exists, run only the commands which make sense.
- To run the login tests, you'll need to have the application setup with the
    four providers
- To run the Push tests, you'll need to have the application setup with the package sid

To run tests:
- Run the application. Select each group on the left menu, then select 'Run tests'
- To see the test logs, after the tests have run select 'Send logs to'
- To upload the test logs to download in another computer, add the URL of a log upload
    service to the text box.
    - One which holds the logs for a couple of minutes after each upload is at
        http://zumotestserver.azurewebsites.net/logs
