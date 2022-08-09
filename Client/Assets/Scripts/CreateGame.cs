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

    public void Go()
    {
        OptionsData.name = nameText.GetComponentInChildren<TMPro.TMP_InputField>().text;
        OptionsData.maxPlayers = Int32.Parse(maxPlayersText.GetComponentInChildren<TMPro.TMP_InputField>().text);

        // TODO: Create a session with Zeuz and use the returned IP and port
        var ip = "127.0.0.1";
        var port = 8000;

        Debug.Log("Joining " + ip + ":" + port);
        OptionsData.ip = ip;
        OptionsData.port = port;

        SceneManager.LoadScene("GameScene");
    }
}
