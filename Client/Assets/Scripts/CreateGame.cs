using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class CreateGame : MonoBehaviour
{
    #region "Inspector Members"
    [SerializeField] GameObject nameText;
    [SerializeField] GameObject maxPlayersText;
    #endregion

    [Serializable]
    private class GameConfiguration
    {
        public string name;
        public int maxPlayers;
    }

    public async void Go()
    {
        OptionsData.name = nameText.GetComponentInChildren<TMPro.TMP_InputField>().text;
        OptionsData.maxPlayers = Int32.Parse(maxPlayersText.GetComponentInChildren<TMPro.TMP_InputField>().text);

        IMS.V0CreateSessionResponse res;
        var req = new IMS.V0CreateSessionRequestBody();

        // Initialise configuration
        GameConfiguration config = new GameConfiguration();
        config.name = OptionsData.name;
        config.maxPlayers = OptionsData.maxPlayers;
        req.Session_config = JsonUtility.ToJson(config);

        // Create a new session
        try
        {
            res = await API.sessionManagerApi.CreateSessionV0Async(API.projectId, API.sessionType, req);
        }
        catch (IMS.ApiException e)
        {
            Debug.LogError(e);
            return;
        }

        var ip = res.Address;
        var port = res.Ports.FirstOrDefault().Port;

        Debug.Log("Joining " + ip + ":" + port);
        OptionsData.ip = ip;
        OptionsData.port = port;

        SceneManager.LoadScene("GameScene");
    }
}