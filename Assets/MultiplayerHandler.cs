using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using System.Collections.Generic;

public class MultiplayerHandler : NetworkManager {

    bool done;
    public GameObject text;
    
    // Use this for initialization
    void Start()
    {
        this.StartMatchMaker();
        this.matchMaker.ListMatches(0, 1000, "Match", false, 0, 1, OnMatchList);
        text.GetComponent<TextMesh>().text = "Searching";
    }
    
    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        if (success)
        {
            if (matchList.Count != 0)
            {
                text.GetComponent<TextMesh>().text += "\nMatches Found";
                this.matchMaker.JoinMatch(matchList[0].networkId, "", "", "", 0, 1, OnMatchJoined);
            }
            else
            {
                text.GetComponent<TextMesh>().text += "\nNo Matches Found";
                text.GetComponent<TextMesh>().text += "\nCreating Match";
                this.matchMaker.CreateMatch("Match", 2, true, "", "", "", 0, 1, OnMatchCreate);
            }
        }
        else
        {
            text.GetComponent<TextMesh>().text += "\nERROR : Match Search Failure";
        }
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            this.StartHost(matchInfo);
            NetworkServer.Listen(matchInfo, 9000);
            text.GetComponent<TextMesh>().text += "\nMatch Created";

            if (NetworkServer.active)
            {
                //NetworkServer.RegisterHandler(1997, OnServerMessageReceived);
                NetworkServer.RegisterHandler(1997, OnMessageReceived);
            }
        }
        else
        {
            text.GetComponent<TextMesh>().text += "\nERROR : Match Create Failure";
        }
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            this.StartClient(matchInfo);
            this.client.Connect(matchInfo);
            text.GetComponent<TextMesh>().text += "\nMatch Joined";

            //NetworkServer.SendToAll(MsgType.Scene, new StringMessage("hello"));

            //NetworkClient c = new NetworkClient();
            //c.Send(MsgType.Scene, new StringMessage("hello"));

            if (!NetworkServer.active)
            {
                //this.client.connection.RegisterHandler(1997, OnClientMessageReceived);
                this.client.connection.RegisterHandler(1997, OnMessageReceived);
            }
            //NetworkClient c = new NetworkClient();
            //c.Send(MsgType.Scene, new StringMessage(""));
            //GetComponent<NetworkView>().RPC("OnServerMessageReceived", RPCMode.All, "this is a client message");
        }
        else
        {
            text.GetComponent<TextMesh>().text += "\nERROR : Match Join Failure";
        }

    }

    // CALLED WHEN CONNECTED TO SERVER
    public override void OnClientConnect(NetworkConnection conn)
    {
        if (NetworkServer.connections.Count == 0)   // DON'T HAVE ANY CONNECTIONS BECAUSE I'M CLIENT
        {
            text.GetComponent<TextMesh>().text += "\nConnected to host";
            this.client.Send(1997, new StringMessage("I'm cliet"));
        }
    }
    // CALLED WHEN DISCONNETS TO SERVER
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        StopClient();
        text.GetComponent<TextMesh>().text += "\nDisconneted from host";
    }

    // CALLED WHEN A CLIENT CONNECTS
    public override void OnServerConnect(NetworkConnection conn)
    {
        if (NetworkServer.connections.Count > 1)    // ANY OTHER CLIENT CONNECT EXCEPT HOST ITSELF
            text.GetComponent<TextMesh>().text += "\nClient connected";
    }
    // CALLED WHEN A CLIENT DISCONNECTS
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        NetworkServer.DestroyPlayersForConnection(conn);
        text.GetComponent<TextMesh>().text += "\nClient Disconnected";
    }

    // MESSAGE RECEIVED BY CLIENT
    public void OnClientMessageReceived(NetworkMessage netMsg)
    {
        string msg = netMsg.ReadMessage<StringMessage>().value;
        text.GetComponent<TextMesh>().text += "\nServer Says: " + msg;
    }

    // MESSAGE RECEIVED BY SERVER
    public void OnServerMessageReceived(NetworkMessage netMsg)
    {
        string msg = netMsg.ReadMessage<StringMessage>().value;
        text.GetComponent<TextMesh>().text += "\nClient Says: " + msg;
        NetworkServer.SendToAll(1997, new StringMessage("Hello Back!"));
    }

    // MESSAGE RECEIVED
    public void OnMessageReceived(NetworkMessage netMsg)
    {
        string msg = netMsg.ReadMessage<StringMessage>().value;
        text.GetComponent<TextMesh>().text += "\nOpponent Says: " + msg;

        if (NetworkServer.connections.Count == 0)
            this.client.Send(1997, new StringMessage("I'm cliet"));
        else
            NetworkServer.SendToAll(1997, new StringMessage("I'm server"));
    }
}