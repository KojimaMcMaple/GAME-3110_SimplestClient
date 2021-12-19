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
    Queue<string> incoming_record_data_;

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

    private void ProcessReceivedMsg(string msg, int id) //[TODO] decouple game logic to something - WATCH LAB
    {
        Debug.Log("msg received = " + msg + ".  connection id = " + id);
        string[] csv = msg.Split(',');
        NetworkEnum.ServerToClientSignifier signifier = (NetworkEnum.ServerToClientSignifier)System.Enum.Parse(typeof(NetworkEnum.ServerToClientSignifier), csv[0]);
        switch (signifier)
        {
            case NetworkEnum.ServerToClientSignifier.LoginComplete:
                {
                    Debug.Log(">>> Login done!");
                    game_manager_.ChangeState(GameEnum.State.MainMenu);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.LoginFailed:
                {
                    Debug.Log(">>> Login FAILED!");
                    //game_manager_.ChangeState(GameEnum.State.LoginMenu);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.AccountCreationComplete:
                {
                    Debug.Log(">>> Creating Account done!");
                    game_manager_.ChangeState(GameEnum.State.MainMenu);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.AccountCreationFailed:
                {
                    Debug.Log(">>> Creating Account FAILED!");
                    //game_manager_.ChangeState(GameEnum.State.LoginMenu);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.GameStart:
                {
                    Debug.Log(">>> GameStart!");
                    string t1 = csv[1];
                    string t2 = csv[2];
                    game_manager_.SetPlayer1Token(t1);
                    game_manager_.SetPlayer2Token(t2);
                    game_manager_.ChangeState(GameEnum.State.TicTacToe);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.GameStartForObserver:
                {
                    Debug.Log(">>> GameStartForObserver!");
                    string t1 = csv[1];
                    string t2 = csv[2];
                    game_manager_.SetPlayer1Token(t1);
                    game_manager_.SetPlayer2Token(t2);
                    game_manager_.ChangeState(GameEnum.State.TicTacToeObserve);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.GameDoTurn:
                {
                    Debug.Log(">>> GameDoTurn!");
                    game_manager_.SetTurn(true);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.GameWaitForTurn:
                {
                    Debug.Log(">>> GameWaitForTurn!");
                    game_manager_.SetTurn(false);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.GameMarkSpace:
                {
                    Debug.Log(">>> GameMarkSpace!");
                    string x = csv[1];
                    string y = csv[2];
                    string t = csv[3];
                    game_manager_.SetTicTacToeButtonToken(int.Parse(x), int.Parse(y), t);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.GameCurrPlayerWin:
                {
                    Debug.Log(">>> GameCurrPlayerWin!");
                    game_manager_.ChangeState(GameEnum.State.TicTacToeWin);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.GameOtherPlayerWin:
                {
                    Debug.Log(">>> GameOtherPlayerWin!");
                    game_manager_.ChangeState(GameEnum.State.TicTacToeLose);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.GameDraw:
                {
                    Debug.Log(">>> GameOtherPlayerWin!");
                    game_manager_.ChangeState(GameEnum.State.TicTacToeDraw);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.ChatRelay:
                {
                    Debug.Log(">>> ChatRelay!");
                    string str = csv[1];
                    if (csv.Length > 1)
                    {
                        for (int i = 2; i < csv.Length; i++)
                        {
                            str = str + "," + csv[i];
                        }
                    }
                    game_manager_.UpdateChat(str);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.RecordingTransferDataStart:
                {
                    Debug.Log(">>> RecordingTransferDataStart!");
                    incoming_record_data_ = new Queue<string>();
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.RecordingTransferData:
                {
                    Debug.Log(">>> RecordingTransferData!");
                    string str = csv[1];
                    if (csv.Length > 1)
                    {
                        for (int i = 2; i < csv.Length; i++)
                        {
                            str = str + "," + csv[i];
                        }
                    }
                    incoming_record_data_.Enqueue(str);
                    break;
                }
            case NetworkEnum.ServerToClientSignifier.RecordingTransferDataEnd:
                {
                    Debug.Log(">>> RecordingTransferDataEnd!");
                    game_manager_.LoadGameRecording(incoming_record_data_);
                    break;
                }
            //case NetworkEnum.ServerToClientSignifier.ReplayRelay:
            //    {
            //        Debug.Log(">>> ReplayRelay!");
            //        string x = csv[1];
            //        string y = csv[2];
            //        string t = csv[3];
            //        game_manager_.SetTokenAtCoord(int.Parse(x), int.Parse(y), t); //[TODO] queue for replay
            //        SendMessageToHost(NetworkEnum.ClientToServerSignifier.NextReplayMove + "");
            //        break;
            //    }
            //case NetworkEnum.ServerToClientSignifier.ReplayEnd:
            //    {
            //        Debug.Log(">>> ReplayEnd!");
            //        game_manager_.ChangeState(game_manager_.GetLastState());
            //        break;
            //    }
            default:
                break;
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }
}


public class GameRecording
{
    public int player_id_1, player_id_2;
    public int grid_size_x, grid_size_y;
    public System.DateTime start_datetime;
    public Queue<GameMove> game_move_queue;

    public struct GameMove
    {
        public GameEnum.PlayerTurn turn;
        public int grid_coord_x;
        public int grid_coord_y;
        public System.DateTime datetime;

        public GameMove(GameEnum.PlayerTurn turn, int grid_coord_x, int grid_coord_y, System.DateTime datetime)
        {
            this.turn = turn;
            this.grid_coord_x = grid_coord_x;
            this.grid_coord_y = grid_coord_y;
            this.datetime = datetime;
        }
    }
    
    public GameRecording(int id_1, int id_2, int grid_size_x, int grid_size_y)
    {
        player_id_1 = id_1;
        player_id_2 = id_2;
        this.grid_size_x = grid_size_x;
        this.grid_size_y = grid_size_y;
        start_datetime = System.DateTime.Now;
        game_move_queue = new Queue<GameMove>();
    }

    public void AddGameMoveWithCurrTime(GameEnum.PlayerTurn turn, int grid_coord_x, int grid_coord_y)
    {
        game_move_queue.Enqueue(new GameMove(turn, grid_coord_x, grid_coord_y, System.DateTime.Now));
    }

    public Queue<string> Serialize()
    {
        Queue<string> data = new Queue<string>();
        data.Enqueue((int)GameEnum.RecordDataId.kRoomSettingId + "," +
            player_id_1 + "," + player_id_2 + "," +
            grid_size_x + "," + grid_size_y + "," +
            start_datetime.Year.ToString() + "," + start_datetime.Month.ToString() + "," + start_datetime.Day.ToString() + "," +
            start_datetime.Hour.ToString() + "," + start_datetime.Minute.ToString());
        foreach (GameMove item in game_move_queue)
        {
            data.Enqueue((int)GameEnum.RecordDataId.kMoveDataId + "," +
                (int)item.turn + "," + item.grid_coord_x + "," + item.grid_coord_y + "," +
            item.datetime.Year.ToString() + "," + item.datetime.Month.ToString() + "," + item.datetime.Day.ToString() + "," +
            item.datetime.Hour.ToString() + "," + item.datetime.Minute.ToString());
        }
        return data;
    }

    public void Deserialize(Queue<string> data)
    {
        foreach (string line in data)
        {
            Debug.Log(">>> line = " +line);
            string[] csv = line.Split(',');
            GameEnum.RecordDataId record_data_id = (GameEnum.RecordDataId)int.Parse(csv[0]);
            switch (record_data_id)
            {
                case GameEnum.RecordDataId.kRoomSettingId:
                    player_id_1 = int.Parse(csv[1]);
                    player_id_2 = int.Parse(csv[2]);
                    grid_size_x = int.Parse(csv[3]);
                    grid_size_y = int.Parse(csv[4]);
                    start_datetime = new System.DateTime(int.Parse(csv[5]), int.Parse(csv[6]), int.Parse(csv[7]), int.Parse(csv[8]), int.Parse(csv[9]), 0);
                    break;
                case GameEnum.RecordDataId.kMoveDataId:
                    game_move_queue.Enqueue(new GameMove((GameEnum.PlayerTurn)int.Parse(csv[1]), int.Parse(csv[2]), int.Parse(csv[3]),
                        new System.DateTime(int.Parse(csv[4]), int.Parse(csv[5]), int.Parse(csv[6]), int.Parse(csv[7]), int.Parse(csv[8]), 0)));
                    break;
            }
        }
    }
}

public static class NetworkEnum
{
    public enum ClientToServerSignifier
    {
        CreateAccount = 1,
        Login,
        JoinQueueForGameRoom,
        JoinQueueForGameRoomAsObserver,
        GameWaitForFirstTurn,
        TTTPlay,
        ChatSend,
        DoReplay,
        NextReplayMove,
        RecordingTransferDataStart = 100,
        RecordingTransferData = 101,
        RecordingTransferDataEnd = 102
    }

    public enum ServerToClientSignifier
    {
        LoginComplete = 1,
        LoginFailed,
        AccountCreationComplete,
        AccountCreationFailed,
        GameStart,
        GameStartForObserver,
        GameDoTurn,
        GameWaitForTurn,
        GameMarkSpace,
        GameDraw,
        GameCurrPlayerWin,
        GameOtherPlayerWin,
        ChatRelay,
        ReplayRelay,
        ReplayEnd,
        RecordingTransferDataStart = 100,
        RecordingTransferData = 101,
        RecordingTransferDataEnd = 102
    }
}