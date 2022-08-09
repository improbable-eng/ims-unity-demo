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

    public async void Refresh()
    {
        // Remove all old buttons
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();

        // Get a list of sessions
        IMS.V0ListSessionsResponse res;
        try
        {
            res = await API.sessionManagerApi.ListSessionsV0Async(API.projectId, API.sessionType);
        }
        catch (IMS.ApiException e)
        {
            Debug.LogError(e);
            return;
        }

        foreach (IMS.V0Session session in res.Sessions)
        {
            var name = session.Session_status["name"];
            var connected = Int32.Parse(session.Session_status["connected"]);
            var maxPlayers = Int32.Parse(session.Session_status["maxPlayers"]);

            // Do not display a game that is full
            if (connected == maxPlayers)
            {
                continue;
            }

            // Create a new button for this session
            var button = Instantiate(buttonPrefab, contentContainer.transform);
            
            button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name + " (" + connected + " / " + maxPlayers + ")";
            button.GetComponent<JoinGameButton>().ip = session.Address;
            button.GetComponent<JoinGameButton>().port = session.Ports.FirstOrDefault().Port;
            button.GetComponent<JoinGameButton>().name = name;
            button.GetComponent<JoinGameButton>().maxPlayers = maxPlayers;

            button.transform.localScale = Vector2.one;
            buttons.Add(button);
        }
    }
}