# IMS Unity Demo Game
## Overview
This is a demo multiplayer project built in Unity. The goal is to demonstrate:
1. How to support [IMS Zeuz](https://docs.ims.improbable.io/docs/ims-zeuz/getting-started) orchestration in your game.
2. How to integrate [IMS Session Manager](https://docs.ims.improbable.io/docs/ims-session-manager/getting-started) functionality into your game.

## Before Getting Started
Before getting started, please read through the IMS [documentation](https://docs.ims.improbable.io/). Follow the tutorial to [run your first game server](https://docs.ims.improbable.io/docs/ims-zeuz/guides/my-first-payload) to familiarize yourself with IMS. You should use this project as an example of what changes you need to make to your game to support IMS services.

Your game should have a separate Unity projects for the client, and the dedicated server. In order to build your dedicated server you must first install the Linux Dedicated Server Build module (and optionally the Windows one for testing locally). This can be done by opening the Unity Hub > Installs > Click gear icon on chosen editor version > Add modules > Linux Dedicated Server Build Support.

In order to start, you must already have:
- An IMS project
- An account linked to IMS
- The [IMS CLI](https://docs.ims.improbable.io/docs/ims-cli/installation) downloaded
- IMS zeuz cluster
- dotnet
- npm

If you do not have any of these please reach out on your Improbable Slack channel.

## Setting up OpenAPI Library
Navigate to the Assets directory of your project. Open a PowerShell terminal.

[Payload API](https://docs.ims.improbable.io/openapi/ims-zeuz/payload-local-api) setup
```
:: Download PayloadLocal API (do this in the server project)
Invoke-WebRequest https://docs.ims.improbable.io/redocusaurus/ims-zeuz-payload-api.yaml -OutFile Scripts\ims-zeuz-payload-api.yaml

:: Run NSwag OpenAPI generator
npx nswag openapi2csclient /input:ims-zeuz-payload-api.yaml /classname:PayloadApi /namespace:IMS /output:ims-payload-api.cs

:: Download dependency (this may fail if already installed)
dotnet add package System.ComponentModel.Annotations --version 4.5.0

:: Install dependency
copy ~\.nuget\packages\system.componentmodel.annotations\4.5.0\lib\netstandard2.0\System.ComponentModel.Annotations.dll .

:: Clearup
del Scripts\ims-zeuz-payload-api.yaml

:: Change env variable to point to Mock PayloadApi server
setx ORCHESTRATION_PAYLOAD_API localhost:8080
```

[Session Manager API](https://docs.ims.improbable.io/openapi/ims-session-manager/session-manager-api) setup:
```
:: Download SessionManagerV0 API (do this in the client project)
Invoke-WebRequest https://docs.ims.improbable.io/redocusaurus/ims-session-manager-api.yaml -OutFile Scripts\ims-session-manager-api.yaml

:: Run NSwag OpenAPI generator
npx nswag openapi2csclient /input:ims-session-manager-api.yaml /classname:SessionManagerApi /namespace:IMS /output:ims-session-manager-api.cs

:: Download dependency (this may fail if already installed)
dotnet add package System.ComponentModel.Annotations --version 4.5.0

:: Install dependency
copy ~\.nuget\packages\system.componentmodel.annotations\4.5.0\lib\netstandard2.0\System.ComponentModel.Annotations.dll .

:: Clearup
del Scripts\ims-session-manager-api.yaml
```

The OpenAPI library provides a wrapper to communicate with Zeuz and the Session Manager. It is also possible (although not recommended) to interact with the API directly via HTTP requests. Here is an example of sending a Payload ready request:
```
var httpClient = new HttpClient();
var content = new FormUrlEncodedContent(new Dictionary<string, string>());
var response = await httpClient.PostAsync("http://" + ORCHESTRATION_PAYLOAD_API + "/api/v0/ready", content);
var responseString = await response.Content.ReadAsStringAsync();
// Here you must parse the JSON in 'responseString' differently depending on the returned status code
```
The rest of this guide will refer to the OpenAPI wrapper functions.

## Testing / Uploading
```
:: Run a mock Payload Api server locally on port 8080 for testing
ims orchestration payload-local-api

:: Create and upload a server image to IMS Zeuz
ims image publish --project-id "..." --name "..." --description "..." --version "..." --directory "..."
```
You also need to create an [allocation](https://docs.ims.improbable.io/docs/ims-zeuz/allocation) for your game that uses your uploaded server image. Open the [IMS Allocations Console](https://console.ims.improbable.io/orchestration/allocations) and follow [this guide](https://docs.ims.improbable.io/docs/ims-zeuz/guides/my-first-payload#create-an-allocation).

## Integrating IMS Zeuz on the server
Follow the [integration guide](https://docs.ims.improbable.io/docs/ims-zeuz/guides/integration-patterns).

### [Payload ready](https://docs.ims.improbable.io/openapi/ims-zeuz/payload-local-api#tag/PayloadLocal/operation/ReadyV0)

This code alerts Zeuz that the server is ready to start accepting connections from clients.
```
// Create an instance of the API library
IMS.PayloadApi payloadApi = new IMS.PayloadApi(new HttpClient());

// Set IMS Zeuz Url
var ORCHESTRATION_PAYLOAD_API = Environment.GetEnvironmentVariable("ORCHESTRATION_PAYLOAD_API");
if (String.IsNullOrEmpty(ORCHESTRATION_PAYLOAD_API)) {
    Debug.LogError("Must set the ORCHESTRATION_PAYLOAD_API environment variable!");
}
else {
    payloadApi.BaseUrl = "http://" + ORCHESTRATION_PAYLOAD_API;
}

// Tell Zeuz we are ready to start accepting connections
// Retry this call in case the PayloadLocal API is initially unavailable
var attempts = 0;
while (true)
{
    try
    {
        await payloadApi.ReadyV0Async();
        break;
    }
    catch (Exception e)
    {
        Debug.LogError(e);
        attempts++;
        if (attempts >= 3)
        {
            Application.Quit();
        }
    }
}
```
The server proccess should terminate once the game is finished and players are disconnected to make space for other payloads to start.

## Integrating Session Manager on the server
Follow the [integration guide](https://docs.ims.improbable.io/docs/ims-session-manager/guides/integration#game-server).
![integration diagram](https://docs.ims.improbable.io/assets/images/reading-config-setting-status-9227d2038ccdebb3d816681461a42e93.svg)
### [Check when payload has been reserved](https://docs.ims.improbable.io/openapi/ims-zeuz/payload-local-api#tag/PayloadLocal/operation/GetPayloadV0)
This code polls Zeuz to get details about the current payload and checks whether it is in the reserved state (meaning that a session has been allocated to this payload).
```
// Run this in a loop

IMS.GetPayloadResponseV0 res;
try {
    res = await payloadApi.GetPayloadV0Async();
}
catch (IMS.ApiException e) {
    Debug.LogError(e);
    // Handle error
}

if (res.Result.Status.State == IMS.PayloadStatusStateV0.Reserved)
{
    // Payload has been reserved
    
    // You may want to get the session config here (see below)

    // You may want to set the session status here (also see below)

    // Wait for other players to connect then start the game
}
```

### [Get session config](https://docs.ims.improbable.io/openapi/ims-zeuz/payload-local-api#tag/SessionManagerLocal/operation/GetSessionConfigV0)
See: [configuration](https://docs.ims.improbable.io/docs/ims-session-manager/guides/integration#key-definitions)
```
IMS.SessionConfigV0 config;
try
{
    config = await payloadApi.GetSessionConfigV0Async();
}
catch (IMS.ApiException e)
{
    Debug.LogError(e);
    // Handle error
}

// Do stuff with the config
```

### [Set session status](https://docs.ims.improbable.io/openapi/ims-zeuz/payload-local-api#tag/SessionManagerLocal/paths/~1api~1v0~1session-manager~1status/post)
See: [status](https://docs.ims.improbable.io/docs/ims-session-manager/guides/integration#key-definitions)
```
try
{
    await payloadApi.SetSessionStatusV0Async(new Dictionary<string, string> {
        {"game_mode", "king-of-the-hill"},
        {"player_count", "3"}
    });
}
catch (IMS.ApiException e) {
    Debug.LogError(e);
    // Handle error
}
```

## Integrating Session Manager on the client
### [Authentication with PlayFab and creating API instance](https://docs.ims.improbable.io/docs/ims-session-manager/guides/authentication#playfab)
The client must be authenticated in order to make session manager API calls.
First install and set up the Playfab SDK, refer to the guide [here](https://docs.microsoft.com/en-us/gaming/playfab/sdks/unity3d/quickstart).
```
// Run when the project loads
[RuntimeInitializeOnLoadMethod]
public static void Load()
{
    var httpClient = new HttpClient();
    // Create an instance of the API library
    var sessionManagerApi = new IMS.SessionManagerApi(httpClient);

    if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId)){
        /*
        Please change the titleId below to your own titleId from PlayFab Game Manager.
        If you have already set the value in the Editor Extensions, this can be skipped.
        */
        PlayFabSettings.staticSettings.TitleId = "42";
    }
    var request = new LoginWithCustomIDRequest { CustomId = "my_custom_id", CreateAccount = true};
    PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
}

private static void OnLoginSuccess(LoginResult result)
{
    // Successfully connected to PlayFab!
    var sessionTicket = result.SessionTicket;

    // Send ticket as a header on future requests
    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer secret-token:playfab/" + sessionTicket);
}

private static void OnLoginFailure(PlayFabError error)
{
    Debug.LogError(error.GenerateErrorReport());
    // Handle authentication error
}

// We can now use sessionManagerApi to make API calls
```

### [Creating a new session](https://docs.ims.improbable.io/openapi/ims-session-manager/session-manager-api#tag/SessionManagerV0/operation/CreateSessionV0)
Here we create a session (sending the configuration), and receive the IP address and port number of the server running the new session.
See: [session type](https://docs.ims.improbable.io/docs/ims-session-manager/guides/integration#key-definitions)
```
var req = new IMS.V0CreateSessionRequestBody();

// Set the session config
req.Session_config = "{}";

IMS.V0CreateSessionResponse res;
try
{
    res = await sessionManagerApi.CreateSessionV0Async("my_project_id", "my_session_type", req);
}
catch (IMS.ApiException e)
{
    Debug.LogError(e);
    // Handle error
}

var ip = res.Address;
var port = res.Ports.FirstOrDefault().Port;
// Connect to server using the ip address and port
```

### [Listing sessions](https://docs.ims.improbable.io/openapi/ims-session-manager/session-manager-api#tag/SessionManagerV0/operation/ListSessionsV0)
```
IMS.V0ListSessionsResponse res;

try
{
    res = await sessionManagerApi.ListSessionsV0Async("my_project_id", "my_session_type");
}
catch (IMS.ApiException e)
{
    Debug.LogError(e);
    // Handle error
}

// Iterate through session list
foreach (IMS.V0Session session in res.Sessions)
{
    var id = session.Id;
    var ip = session.Address;
    var port = session.Ports.FirstOrDefault().Port;
    var status = session.Session_status;
    
    // Do stuff with this session
}
```
