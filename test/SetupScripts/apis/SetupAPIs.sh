#!/bin/bash
if [ "$1" = "" ]; then
  echo Usage: $0 \<applicationName\>
  echo .  Where \<applicationName\> is an Azure Mobile Service app.
else
  azure mobile api create $1 admin —-permissions *=admin
  azure mobile api create $1 application —-permissions *=application
  azure mobile api create $1 user —-permissions *=user
  azure mobile api create $1 public —-permissions *=public
  azure mobile api create $1 shared —-permissions *=admin
  azure mobile api create $1 movieFinder —-permissions *=application
  azure mobile api create $1 runtimeInfo —-permissions GET=public,POST=admin

  azure mobile script upload $1 api/admin.js -f admin.js
  azure mobile script upload $1 api/application -f application.js
  azure mobile script upload $1 api/user -f user.js
  azure mobile script upload $1 api/public -f public.js
  azure mobile script upload $1 api/shared -f shared.js
  azure mobile script upload $1 api/movieFinder -f movieFinder.js
  azure mobile script upload $1 api/runtimeInfo -f runtimeInfo.js

fi
