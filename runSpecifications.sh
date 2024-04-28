#!/bin/bash

pushd ./specification
dotnet build
popd
pwsh specification/Specifications/bin/Debug/net9.0/playwright.ps1 install --with-deps

echo '#####'
echo '# Playwright setup complete'
echo '#####'

dotnet run --project src/MyWeirdStuff.AppHost/ &
APP_PID=$!

# Wait for the app to start
#TODO: wait for health check
sleep 15

echo '#####'
echo '# System Under Test started'
echo '#####'

pushd ./specification
SPECIFICATIONS_BASEADDRESS=http://localhost:5200 dotnet test
popd

kill $APP_PID
