using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void LoadCreateGame()
    {
        SceneManager.LoadScene("CreateGame");
    }

    public void LoadListGames()
    {
        SceneManager.LoadScene("ListGames");
    }

    public void LoadJoinByIP()
    {
        SceneManager.LoadScene("JoinByIP");
    }
}
