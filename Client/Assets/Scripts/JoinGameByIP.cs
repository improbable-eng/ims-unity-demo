using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinGameByIP : MonoBehaviour
{
    #region "Inspector Members"
    [SerializeField] GameObject ipText;
    [SerializeField] GameObject portText;
    #endregion

    public void Go()
    {
        OptionsData.ip = ipText.GetComponentInChildren<TMPro.TMP_InputField>().text;
        OptionsData.port = Int32.Parse(portText.GetComponentInChildren<TMPro.TMP_InputField>().text);

        SceneManager.LoadScene("GameScene");
    }
}
