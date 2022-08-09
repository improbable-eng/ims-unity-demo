using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using PlayFab;
using PlayFab.ClientModels;

public static class API
{
    public static HttpClient httpClient;
    public static IMS.SessionManagerApi sessionManagerApi;
    public static string projectId;
    public static string sessionType;
    public static string customId;

    [RuntimeInitializeOnLoadMethod]
    public static void Load()
    {
        Debug.Log("API initialising!!");
        httpClient = new HttpClient();
        sessionManagerApi = new IMS.SessionManagerApi(httpClient);

        projectId = Environment.GetEnvironmentVariable("PONG_PROJECT_ID");
        sessionType = Environment.GetEnvironmentVariable("PONG_SESSION_TYPE");
        customId = Environment.GetEnvironmentVariable("PONG_CUSTOM_ID");

        if (string.IsNullOrEmpty(projectId))
        {
            Debug.LogError("Must set PONG_PROJECT_ID env variable!");
        }

        if (string.IsNullOrEmpty(sessionType))
        {
            Debug.LogError("Must set PONG_SESSION_TYPE env variable!");
        }

        if (string.IsNullOrEmpty(customId))
        {
            Debug.LogError("Must set PONG_CUSTOM_ID env variable!");
        }

        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId)){
            /*
            Please change the titleId below to your own titleId from PlayFab Game Manager.
            If you have already set the value in the Editor Extensions, this can be skipped.
            */
            PlayFabSettings.staticSettings.TitleId = "42";
        }
        var request = new LoginWithCustomIDRequest { CustomId = customId, CreateAccount = true};
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private static void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Successfully connected to PlayFab!");
        Debug.Log(result.SessionTicket);
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer secret-token:playfab/" + result.SessionTicket);
    }

    private static void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }
}