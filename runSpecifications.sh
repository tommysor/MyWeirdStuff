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

# Shared timeout counter
i=0
maxLoopCount=99
timeout=35

echo "## CHECK Azurite"
azuriteIsHealthy=true
for ((; i < $maxLoopCount; i++)); do
    # Not really a health check, but shows that Azurite is up and running
    curl \
        -X OPTIONS \
        --output /dev/null \
        --silent \
        --header "origin: http://localhost" \
        --header "Access-Control-Request-Method: POST" \
        http://localhost:10002/comics \
        && break
    echo "## CHECK Azurite $i"
    if [ $i -gt $timeout ]; then
        echo "## Azurite not started"
        azuriteIsHealthy=false
        break
    fi
    sleep 1
done
echo "## END Azurite"

echo "## CHECK Web"
webIsHealthy=true
for ((; i < $maxLoopCount; i++)); do
    curl \
        --silent \
        http://localhost:5200/health \
        && break
    echo "## CHECK Web $i"
    if [ $i -gt $timeout ]; then
        echo "## Web not started"
        webIsHealthy=false
        break
    fi
    sleep 1
done
echo "## END Web"

echo "## CHECK ApiService"
apiServiceIsHealthy=true
for ((; i < $maxLoopCount; i++)); do
    curl \
        --silent \
        http://localhost:5311/health \
        && break
    echo "## CHECK ApiService $i"
    if [ $i -gt $timeout ]; then
        echo "## ApiService not started"
        apiServiceIsHealthy=false
        break
    fi
    sleep 1
done
echo "## END ApiService"

# Give a little extra time for Azurite (not a proper health check)
sleep 2

echo "## WebIsHealthy: $webIsHealthy"
echo "## ApiServiceIsHealthy: $apiServiceIsHealthy"
echo "## AzuriteIsHealthy: $azuriteIsHealthy"
if [[ "$webIsHealthy" = true && "$apiServiceIsHealthy" = true && "$azuriteIsHealthy" = true ]]; then
    echo '#####'
    echo '# System Under Test started'
    echo '#####'

    pushd ./specification
    SPECIFICATIONS_BASEADDRESS=http://localhost:5200 dotnet test
    popd
else
    echo '#####'
    echo '# Failed during startup'
    echo '#####'
fi
kill $APP_PID
