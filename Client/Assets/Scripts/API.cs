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

    [RuntimeInitializeOnLoadMethod]
    public static void Load()
    {
        Debug.Log("API initialising!!");
        httpClient = new HttpClient();
        sessionManagerApi = new IMS.SessionManagerApi(httpClient);
    }
}