using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    NetworkedClient networked_client_;

    // LOGIN VARS
    GameObject login_panel_;
    Button submit_button_, join_gameroom_button_;
    InputField username_input_, password_input_;
    Toggle create_toggle_, login_toggle_;

    // MAIN GAME VARS
    Button tttsquare_button_; //temp
    Vector2Int grid_size_ = new Vector2Int(3, 3); //(x,y) (col,row)
    TicTacToeButtonController[,] button_list_;
    private string play_token_ = "X";
    private int player_id_ = 1;
    int move_count_ = 0;
    GameObject game_panel_, game_over_panel_;
    Text game_over_text_;
    bool is_turn_ = false;
    GameEnum.State last_state_;

    // CHAT VARS
    GameObject chat_panel_;
    Text chat_msg_text_;
    List<Button> chat_prefix_button_list_;
    Dictionary<int, string> chat_prefix_dict_;
    InputField chat_input_field_;

    // REPLAY VARS
    Button replay_button_;

    //static GameObject instance;
    void Awake()
    {
        //instance = this.gameObject;
        GameObject[] all_objects = FindObjectsOfType<GameObject>();
        foreach (GameObject item in all_objects)
        {
            switch (item.name)
            {
                case "LoginPanel":
                    login_panel_ = item;
                    break;
                case "UsernameInputField":
                    username_input_ = item.GetComponent<InputField>();
                    break;
                case "PasswordInputField":
                    password_input_ = item.GetComponent<InputField>();
                    break; 
                case "SubmitButton":
                    submit_button_ = item.GetComponent<Button>();
                    break;
                case "JoinGameRoomButton":
                    join_gameroom_button_ = item.GetComponent<Button>();
                    break;
                case "LoginToggle":
                    login_toggle_ = item.GetComponent<Toggle>();
                    break;
                case "CreateToggle":
                    create_toggle_ = item.GetComponent<Toggle>();
                    break;
                case "NetworkedClient":
                    networked_client_ = item.GetComponent<NetworkedClient>();
                    break;
                case "TTTSquareButton":
                    tttsquare_button_ = item.GetComponent<Button>();
                    break;
                case "BoardPanel":
                    List<TicTacToeButtonController> children = new List<TicTacToeButtonController>();
                    foreach (Transform child in item.transform)
                    {
                        children.Add(child.GetComponent<TicTacToeButtonController>());
                    }
                    button_list_ = new TicTacToeButtonController[grid_size_.x, grid_size_.y];
                    for (int j = 0; j < grid_size_.y; j++)
                    {
                        for (int i = 0; i < grid_size_.x; i++)
                        {
                            button_list_[i, j] = children[j * grid_size_.x + i];
                            button_list_[i, j].SetGridCoord(i, j);
                        }
                    }
                    break;
                case "GamePanel":
                    game_panel_ = item;
                    break;
                case "GameOverPanel":
                    game_over_panel_ = item;
                    break;
                case "WinText":
                    game_over_text_ = item.GetComponent<Text>();
                    break; 
                case "ChatPanel":
                    chat_panel_ = item;
                    break;
                case "ChatMsgText":
                    chat_msg_text_ = item.GetComponent<Text>();
                    break;
                case "ChatInputField":
                    chat_input_field_ = item.GetComponent<InputField>();
                    break;
                case "ChatPrefixMsgPanel":
                    chat_prefix_button_list_ = new List<Button>();
                    foreach (Transform child in item.transform)
                    {
                        chat_prefix_button_list_.Add(child.GetComponent<Button>());
                    }
                    chat_prefix_dict_ = new Dictionary<int, string>();
                    chat_prefix_dict_.Add(0, "Let's settle this.");
                    chat_prefix_dict_.Add(1, "You're pretty good.");
                    chat_prefix_dict_.Add(2, "Make bridges, not walls.");
                    chat_prefix_dict_.Add(3, "All things end. Even us.");
                    for (int i = 0; i < chat_prefix_button_list_.Count; i++)
                    {
                        chat_prefix_button_list_[i].transform.GetComponentInChildren<Text>().text = chat_prefix_dict_[i]; //[IMPROV] make enums?
                        chat_prefix_button_list_[i].GetComponent<PrefixMsgButtonController>().SetMsgId(i);
                    }
                    break;
                case "WatchReplayButton":
                    replay_button_ = item.GetComponent<Button>();
                    break;
                default:
                    break;
            }
        }
        submit_button_.onClick.AddListener(SubmitButtonPressed);
        login_toggle_.onValueChanged.AddListener(LoginToggleChanged);
        create_toggle_.onValueChanged.AddListener(CreateToggleChanged);
        create_toggle_.isOn = false;
        join_gameroom_button_.onClick.AddListener(JoinGameRoomButtonPressed);
        //tttsquare_button_.onClick.AddListener(TTTSquareButtonPressed);
        //join_gameroom_button_.gameObject.SetActive(false);
        replay_button_.onClick.AddListener(ReplayButtonPressed);

        ChangeState(GameEnum.State.LoginMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (chat_input_field_.text != "")
            {
                networked_client_.SendMessageToHost(NetworkEnum.ClientToServerSignifier.ChatSend + "," + chat_input_field_.text);
                chat_input_field_.text = "";
            }
        }
    }

    public void SubmitButtonPressed()
    {
        string p = password_input_.text;
        string n = username_input_.text;
        string msg;
        if (create_toggle_.isOn)
        {
            msg = (int)NetworkEnum.ClientToServerSignifier.CreateAccount + "," + n + "," + p;
        }
        else
        {
            msg = (int)NetworkEnum.ClientToServerSignifier.Login + "," + n + "," + p;
        }
        networked_client_.SendMessageToHost(msg);
        Debug.Log(">>> Submitting msg: " + msg);
    }

    public void LoginToggleChanged(bool value)
    {
        create_toggle_.SetIsOnWithoutNotify(!value);
    }

    public void CreateToggleChanged(bool value)
    {
        login_toggle_.SetIsOnWithoutNotify(!value);
    }

    public void JoinGameRoomButtonPressed()
    {
        networked_client_.SendMessageToHost(NetworkEnum.ClientToServerSignifier.JoinQueueForGameRoom+"");
        ChangeState(GameEnum.State.WaitingInQueueForOtherPlayer);
    }

    public void TTTSquareButtonPressed()
    {
        //networked_client_.SendMessageToHost(NetworkEnum.ClientToServerSignifier.TTTPlay + "");
        //ChangeState(GameEnum.State.WaitingInQueueForOtherPlayer);
    }

    public void ReplayButtonPressed()
    {
        networked_client_.SendMessageToHost(NetworkEnum.ClientToServerSignifier.DoReplay + "");
    }

    public void ChangeState(GameEnum.State state)
    {
        //join_gameroom_button_.gameObject.SetActive(false);
        //submit_button_.gameObject.SetActive(false);
        //username_input_.gameObject.SetActive(false);
        //password_input_.gameObject.SetActive(false);
        //create_toggle_.gameObject.SetActive(false);
        //login_toggle_.gameObject.SetActive(false);
        //login_panel_.SetActive(false);
        ////tttsquare_button_.gameObject.SetActive(false);
        //game_panel_.SetActive(false);
        //game_over_panel_.SetActive(false);
        //chat_panel_.SetActive(false);
        switch (state)
        {
            case GameEnum.State.LoginMenu:
                login_panel_.SetActive(true);
                game_panel_.SetActive(false);
                game_over_panel_.SetActive(false);
                chat_panel_.SetActive(false);
                submit_button_.gameObject.SetActive(true);
                username_input_.gameObject.SetActive(true);
                password_input_.gameObject.SetActive(true);
                create_toggle_.gameObject.SetActive(true);
                login_toggle_.gameObject.SetActive(true);
                join_gameroom_button_.gameObject.SetActive(false);
                break;
            case GameEnum.State.MainMenu:
                login_panel_.SetActive(true);
                game_panel_.SetActive(false);
                game_over_panel_.SetActive(false);
                chat_panel_.SetActive(false);
                submit_button_.gameObject.SetActive(false);
                username_input_.gameObject.SetActive(false);
                password_input_.gameObject.SetActive(false);
                create_toggle_.gameObject.SetActive(false);
                login_toggle_.gameObject.SetActive(false);
                join_gameroom_button_.gameObject.SetActive(true);
                break;
            case GameEnum.State.WaitingInQueueForOtherPlayer:
                login_panel_.SetActive(true);
                game_panel_.SetActive(false);
                game_over_panel_.SetActive(false);
                chat_panel_.SetActive(false);
                submit_button_.gameObject.SetActive(false);
                username_input_.gameObject.SetActive(false);
                password_input_.gameObject.SetActive(false);
                create_toggle_.gameObject.SetActive(false);
                login_toggle_.gameObject.SetActive(false);
                join_gameroom_button_.gameObject.SetActive(false);
                break;
            case GameEnum.State.TicTacToe:
                //tttsquare_button_.gameObject.SetActive(true);
                login_panel_.SetActive(false);
                game_panel_.SetActive(true);
                game_over_panel_.SetActive(false);
                chat_panel_.SetActive(true);
                break;
            case GameEnum.State.TicTacToeWin:
                GameOver();
                last_state_ = GameEnum.State.TicTacToeWin;
                login_panel_.SetActive(false);
                game_panel_.SetActive(true);
                game_over_panel_.SetActive(true);
                chat_panel_.SetActive(true);
                game_over_text_.text = "You Win!";
                break;
            case GameEnum.State.TicTacToeLose:
                GameOver();
                last_state_ = GameEnum.State.TicTacToeLose;
                login_panel_.SetActive(false);
                game_panel_.SetActive(true);
                game_over_panel_.SetActive(true);
                chat_panel_.SetActive(true);
                game_over_text_.text = "Other Player Wins!";
                break;
            case GameEnum.State.TicTacToeDraw:
                GameOver();
                last_state_ = GameEnum.State.TicTacToeDraw;
                login_panel_.SetActive(false);
                game_panel_.SetActive(true);
                game_over_panel_.SetActive(true);
                chat_panel_.SetActive(true);
                game_over_text_.text = "It's a draw!";
                break;
            case GameEnum.State.TicTacToeReplay:
                ReplayLastGame();
                game_over_text_.text = "Win Text";
                login_panel_.SetActive(false);
                game_panel_.SetActive(true);
                game_over_panel_.SetActive(false);
                chat_panel_.SetActive(true);
                break;
            default:
                break;
        }
    }

    public string GetPlayToken()
    {
        return play_token_;
    }

    public int GetCurrPlayerId()
    {
        return player_id_;
    }

    public bool IsTurn()
    {
        return is_turn_;
    }

    public void SetTurn(bool value)
    {
        is_turn_ = value;
    }

    public void SetTicTacToeButtonToken(int x, int y, string token)
    {
        button_list_[x, y].SetText(token);
        button_list_[x, y].GetComponent<Button>().interactable = false;
    }

    public void CheckGridCoord(Vector2Int coord) //[OM]
    {
        move_count_++;
        // CHECK WITH OTHER COLS
        for (int i = 0; i < grid_size_.x; i++)
        {
            if (button_list_[coord.x,i].GetState() != (GameEnum.TicTacToeButtonState)player_id_)
                break;
            if (i == grid_size_.x - 1)
            {
                //report win for player_id_
                ChangeState(GameEnum.State.TicTacToeWin);
            }
        }
        // CHECK WITH OTHER ROWS
        for (int i = 0; i < grid_size_.x; i++)
        {
            if (button_list_[i, coord.y].GetState() != (GameEnum.TicTacToeButtonState)player_id_)
                break;
            if (i == grid_size_.x - 1)
            {
                //report win for player_id_
                ChangeState(GameEnum.State.TicTacToeWin);
            }
        }
        // CHECK DIAGONALLY
        if (coord.x == coord.y)
        {
            for (int i = 0; i < grid_size_.x; i++)
            {
                if (button_list_[i, i].GetState() != (GameEnum.TicTacToeButtonState)player_id_)
                    break;
                if (i == grid_size_.x - 1)
                {
                    //report win for player_id_
                    ChangeState(GameEnum.State.TicTacToeWin);
                }
            }
        }
        // CHECK REVERSE DIAGONALLY
        if (coord.x + coord.y == grid_size_.x - 1)
        {
            for (int i = 0; i < grid_size_.x; i++)
            {
                if (button_list_[i,(grid_size_.x - 1) - i].GetState() != (GameEnum.TicTacToeButtonState)player_id_)
                    break;
                if (i == grid_size_.x - 1)
                {
                    //report win for player_id_
                    ChangeState(GameEnum.State.TicTacToeWin);
                }
            }
        }
        // CHECK IF IS TIE
        if (move_count_ == (Mathf.Pow(grid_size_.x, 2) - 1))
        {
            ChangeState(GameEnum.State.TicTacToeDraw);
        }
        ChangeSides();
    }

    void ChangeSides() //[OM]
    {
        if (player_id_ == 1)
        {
            player_id_ = 2;
            play_token_ = "O";
        }
        else if (player_id_ == 2)
        {
            player_id_ = 1;
            play_token_ = "X";
        }
    }

    void GameOver()
    {
        for (int j = 0; j < grid_size_.y; j++)
        {
            for (int i = 0; i < grid_size_.x; i++)
            {
                button_list_[i, j].GetComponent<Button>().interactable = false;
            }
        }
    }

    void ReplayLastGame()
    {
        for (int j = 0; j < grid_size_.y; j++)
        {
            for (int i = 0; i < grid_size_.x; i++)
            {
                button_list_[i, j].GetComponent<Button>().interactable = false;
                button_list_[i, j].SetText("");
            }
        }
    }

    public string GetPrefixMsgFromId(int id)
    {
        return chat_prefix_dict_[id];
    }

    public void UpdateChat(string str)
    {
        chat_msg_text_.text = chat_msg_text_.text + "\n" + str;
    }

    public void SetTokenAtCoord(int x, int y, string token)
    {
        button_list_[x, y].SetText(token);
        StartCoroutine(Delay(2.0f));
    }

    public GameEnum.State GetLastState()
    {
        return last_state_;
    }

    /// <summary>
    /// General delay function for level loading, show explosion before game over, etc.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Delay(float time)
    {
        yield return new WaitForSeconds(time);
    }
}

public static class GameEnum
{
    public enum State
    {
        LoginMenu = 1,
        MainMenu,
        WaitingInQueueForOtherPlayer,
        TicTacToe,
        TicTacToeNextPlayer,
        TicTacToeWin,
        TicTacToeLose,
        TicTacToeDraw,
        TicTacToeReplay
    }

    public enum TicTacToeButtonState
    {
        kBlank,
        kPlayer1,
        kPlayer2
    }
}