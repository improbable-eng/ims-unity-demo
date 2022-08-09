using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinGameButton : MonoBehaviour
{
    public string ip;
    public string name;
    public int port;
    public int maxPlayers;

    public void JoinGame() {
        OptionsData.ip = ip;
        OptionsData.port = port;
        OptionsData.name = name;
        OptionsData.maxPlayers = maxPlayers;
        SceneManager.LoadScene("GameScene");
    }
}
