using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ListSessions : MonoBehaviour
{
    #region "Inspector Members"
    [SerializeField] Transform contentContainer;
    [SerializeField] GameObject buttonPrefab;
    #endregion
    
    #region "Private Members"
    [SerializeField] List<GameObject> buttons;
    #endregion

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        // Remove all old buttons
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();

        // TODO: Get a list of sessions from the session manager to create join buttons for each

        for (int i = 0; i < 5; i++)
        {
            var name = "myGame";
            var connected = 5;
            var maxPlayers = 10;

            // Do not display a game that is full
            if (connected == maxPlayers)
            {
                continue;
            }

            // Create a new button for this session
            var button = Instantiate(buttonPrefab, contentContainer.transform);
            
            button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name + " (" + connected + " / " + maxPlayers + ")";
            button.GetComponent<JoinGameButton>().ip = "127.0.0.1";
            button.GetComponent<JoinGameButton>().port = 8000;
            button.GetComponent<JoinGameButton>().name = name;
            button.GetComponent<JoinGameButton>().maxPlayers = maxPlayers;

            button.transform.localScale = Vector2.one;
            buttons.Add(button);
        }
    }
}
