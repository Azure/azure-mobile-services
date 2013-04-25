# Windows Azure Mobile Services 

With Windows Azure Mobile Services you can add a scalable backend to your connected client applications in minutes. To learn more, visit our [Developer Center](http://www.windowsazure.com/en-us/develop/mobile).

##Offline Data and Sync

This branch contains a prototype extension to do caching and synchronization for offline scenarios built by @jlaanstra. It currently has the following features:

- Cache json responses from tables into a local SQLite database.
- Store local changes and sync them as soon as the network is available.
- Interpret queries and send them to the local SQLite database.
- Only retrieve incremental changes from the server since the last request was made.

*This is a prototype. The above features may contain bugs. If you come across a bug, please open an issues so it can be fixed.*

## Getting Started

To use Offline Data and Sync in your Mobile Service there are a couple things you need to do:

- Update the database schema (see altertable.sql in sql folder).
- Update server scripts (see scripts in scripts folder).
- Add the `CacheHandler` to the `MobileServiceClient`.
- Choos a `CacheProvider`.
- Think about your business logic if all your assumptions are still valid in offline scenario's and make changes if necessary.




