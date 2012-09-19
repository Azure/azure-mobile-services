# Doto

Doto is a sample application for Windows Azure Mobile Services.

## Introduction

Doto is a simple, social todo list application that demonstrates the features of Windows Azure Mobile Services and how easy it is to add the power of Windows Azure to your Windows 8 app.

Doto uses the following features from Windows Azure Mobile Services:

 - Integrated authentication via Microsoft Account and Live Connect
 - Structured storage with server scripts to provide validation and authorization
 - Integration with Windows Notification Services to send Live Tile updates and Toast notifications

The sample contains a Windows 8 application that allows you to create todo lists, manage your todo items and even share your lists with other users. 

## Prerequisites

To run doto you'll need the following:

- [Visual Studio 2012 Express](http://go.microsoft.com/fwlink/?LinkID=257546&clcid=0x409) (or Ultimate)
- [Windows Live SDK](http://go.microsoft.com/fwlink/?LinkID=262253&clcid=0x409)
- [Mobile Services SDK](http://go.microsoft.com/fwlink/?LinkID=257545&clcid=0x409)
- and a Windows Azure account (get the [Free Trial](http://www.windowsazure.com/en-us/pricing/free-trial/))

## Getting Started

To complete the scenario, you'll need to create a Windows Azure Mobile Service and follow the instructions in ```Setup.rtf```.

## How to doto

When first starting doto, you'll be asked to register. To improve the user experience we pre-fill your name and city using your Windows Live profile. Once registered, you'll be asked to create a new list. Click the 'create a new list' button and enter a name and click Save.

You can create new lists at any time using the app bar (right click the screen to show the app bar). You can also use the app bar to add and remove items, refresh the current list, invite other users or leave a list.

You can switch between your multiple lists using by clicking on the name of your list at the top left of the screen. You'll see a dropdown with the names of all of your list.

Doto was designed to be extremely simple - items are either on your list todo list or deleted. There is no idea of edit task or complete task. To remove items from the list, just select them and click remove items in the app bar.

You can invite other users to share any of your lists by clicking on the invite user button and searching for people by name (to test this feature, you might want to sign out using the settings charm and sign in with a second live account).

The invited user should receive a Toast Notification, and can click the View Invite button in the app bar to accept (or reject) your invite. Note, that an invited user gets full permissions over your list including the ability to add other users.

Enjoy! Remember, to get started you'll need to follow the instructions in the Setup.rdf
