﻿/*
This file is based on https://github.com/manlaig/basic_multiplayer_unity

MIT License

Copyright (c) 2020 Michael Ganzorig

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    #region "Inspector Members"
    [SerializeField] GameObject playerCountText;
    #endregion

    #region "Private Members"
    Socket udp;
    IPEndPoint server;
    #endregion

    void Awake()
    {        
        Debug.Log(OptionsData.ip + ":" + OptionsData.port);

        server = new IPEndPoint(IPAddress.Parse(OptionsData.ip), OptionsData.port);
        udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udp.Blocking = false;

        // 0x1 is new user
        udp.SendTo(new byte[] {0x1}, server);
    }

    void OnApplicationQuit()
    {
        // 0x2 is quit
        udp.SendTo(new byte[] {0x2}, server);
        udp.Close();
    }

    void Update()
    {
        if(udp.Available != 0)
        {
            byte[] data = new byte[64];
            udp.Receive(data);
            playerCountText.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = OptionsData.name + " (" + ((int) data[0]) + " / " + OptionsData.maxPlayers + ")";
        }
    }
}
