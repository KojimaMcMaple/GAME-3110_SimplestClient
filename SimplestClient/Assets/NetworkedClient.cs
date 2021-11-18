using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedClient : MonoBehaviour
{
    int connectionID;
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;
    byte error;
    bool isConnected = false;
    int ourClientID;
    string ip_address_ = "192.168.1.128"; //CHECK IF IP IS CORRECT FIRST AND FOREMOST

    GameManager game_manager_;

    void Start()
    {
        game_manager_ = FindObjectOfType<GameManager>();
        Connect();
    }

    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.S))
        //    SendMessageToHost("Hello from client");

        UpdateNetworkConnection();
    }

    private void UpdateNetworkConnection()
    {
        if (isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessReceivedMsg(msg, recConnectionID);
                    //Debug.Log("got msg = " + msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }
        }
    }
    
    private void Connect()
    {
        if (!isConnected)
        {
            Debug.Log("Attempting to create connection");

            NetworkTransport.Init();

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelID = config.AddChannel(QosType.Reliable);
            unreliableChannelID = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, maxConnections);
            hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + hostID);

            connectionID = NetworkTransport.Connect(hostID, ip_address_, socketPort, 0, out error); // server is local on network

            if (error == 0)
            {
                isConnected = true;
                Debug.Log("Connected, id = " + connectionID);
            }
            else
            {
                Debug.Log(">>> CANNOT CONNECT. CHECK IP ADDRESS: " + ip_address_);
            }
        }
    }
    
    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }
    
    public void SendMessageToHost(string msg)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessReceivedMsg(string msg, int id)
    {
        Debug.Log("msg received = " + msg + ".  connection id = " + id);
        string[] csv = msg.Split(',');
        NetworkEnum.ServerToClientSignifier signifier = (NetworkEnum.ServerToClientSignifier)System.Enum.Parse(typeof(NetworkEnum.ServerToClientSignifier), csv[0]);
        switch (signifier)
        {
            case NetworkEnum.ServerToClientSignifier.LoginComplete:
                Debug.Log(">>> Login done!");
                game_manager_.ChangeState(GameEnum.State.MainMenu);
                break;
            case NetworkEnum.ServerToClientSignifier.LoginFailed:
                Debug.Log(">>> Login FAILED!");
                //game_manager_.ChangeState(GameEnum.State.LoginMenu);
                break;
            case NetworkEnum.ServerToClientSignifier.AccountCreationComplete:
                Debug.Log(">>> Creating Account done!");
                game_manager_.ChangeState(GameEnum.State.MainMenu);
                break;
            case NetworkEnum.ServerToClientSignifier.AccountCreationFailed:
                Debug.Log(">>> Creating Account FAILED!");
                //game_manager_.ChangeState(GameEnum.State.LoginMenu);
                break;
            case NetworkEnum.ServerToClientSignifier.GameStart:
                Debug.Log(">>> GameStart!");
                game_manager_.ChangeState(GameEnum.State.TicTacToe);
                break;
            case NetworkEnum.ServerToClientSignifier.GameDoTurn:
                Debug.Log(">>> GameDoTurn!");
                game_manager_.SetTurn(true);
                break;
            case NetworkEnum.ServerToClientSignifier.GameWaitForTurn:
                Debug.Log(">>> GameWaitForTurn!");
                game_manager_.SetTurn(false);
                break;
            case NetworkEnum.ServerToClientSignifier.GameMarkSpace:
                Debug.Log(">>> GameMarkSpace!");
                string x = csv[1];
                string y = csv[2];
                string t = csv[3];
                game_manager_.SetTicTacToeButtonToken(int.Parse(x), int.Parse(y), t);
                break;
            case NetworkEnum.ServerToClientSignifier.GameCurrPlayerWin:
                Debug.Log(">>> GameCurrPlayerWin!");
                game_manager_.ChangeState(GameEnum.State.TicTacToeWin);
                break;
            case NetworkEnum.ServerToClientSignifier.GameOtherPlayerWin:
                Debug.Log(">>> GameOtherPlayerWin!");
                game_manager_.ChangeState(GameEnum.State.TicTacToeLose);
                break;
            case NetworkEnum.ServerToClientSignifier.GameDraw:
                Debug.Log(">>> GameOtherPlayerWin!");
                game_manager_.ChangeState(GameEnum.State.TicTacToeDraw);
                break;
            default:
                break;
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }
}

public static class NetworkEnum
{
    public enum ClientToServerSignifier
    {
        CreateAccount = 1,
        Login,
        JoinQueueForGameRoom,
        GameWaitForFirstTurn,
        TTTPlay
    }

    public enum ServerToClientSignifier
    {
        LoginComplete = 1,
        LoginFailed,
        AccountCreationComplete,
        AccountCreationFailed,
        GameStart,
        GameDoTurn,
        GameWaitForTurn,
        GameMarkSpace,
        GameDraw,
        GameCurrPlayerWin,
        GameOtherPlayerWin
    }
}