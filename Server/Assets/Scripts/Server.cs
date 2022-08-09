/*
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text;

public class Server : MonoBehaviour
{
    #region "Private Members"
    int port;
    Socket udp;
    HashSet<EndPoint> connected = new HashSet<EndPoint>();
    string sessionName;
    int maxPlayers;
    #endregion

    [Serializable]
    private class GameConfiguration
    {
        public string name;
        public int maxPlayers;
    }

    void Start()
    {
        // Take port number as command line argument, default to 8000
        port = 8000;

        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-port" && args.Length > i + 1)
            {
                port = Int32.Parse(args[i + 1]);
            }
        }
        
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

        Debug.Log("Server IP Address: " + endPoint.ToString());
        Debug.Log("Port: " + port);

        // create a UDP socket
        udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            udp.Bind(endPoint);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to bind to UDP port: " + e.ToString());
        }

        udp.Blocking = false;

        // TODO: Tell Zeuz the server is ready
    }

    void Update()
    {
        // Every other frame check for new network events
        if (udp != null && Time.frameCount % 2 == 0 && udp.Available > 0)
        {
            byte[] packet = new byte[64];
            EndPoint from = new IPEndPoint(IPAddress.Any, port);

            udp.ReceiveFrom(packet, ref from);
            string data = Encoding.Default.GetString(packet);

            Debug.Log("Server received: " + data);

            if (data[0] == 0x1 && connected.Count < maxPlayers)
            {
                HandleNewClient(from);
            }
            else if (data[0] == 0x2)
            {
                DisconnectClient(from);
            }

            UpdateSessionStatus();
        }
    }

    void UpdateSessionStatus()
    {
        // Send to all connected clients the number of connected players
        foreach (EndPoint addr in connected)
        {
            udp.SendTo(new byte[] {Convert.ToByte(connected.Count)}, addr);
        }

        // TODO: Set the session status in the session manager
    }

    void HandleNewClient(EndPoint addr)
    {
        if (connected.Contains(addr))
        {
            return;
        }

        Debug.Log("New client connected");
        connected.Add(addr);
    }

    void DisconnectClient(EndPoint addr)
    {
        connected.Remove(addr);

        // Exit server if no more players are connected
        if (connected.Count == 0)
        {
            Application.Quit();
        }
    }
}
