using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    Button submit_button_, join_gameroom_button_, tttsquare_button_;
    InputField username_input_, password_input_;
    Toggle create_toggle_, login_toggle_;
    NetworkedClient networked_client_;
    //LinkedList<Button>

    //static GameObject instance;
    void Awake()
    {
        //instance = this.gameObject;
        GameObject[] all_objects = FindObjectsOfType<GameObject>();
        foreach (GameObject item in all_objects)
        {
            switch (item.name)
            {
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
                case "TTTSquareButton":
                    tttsquare_button_ = item.GetComponent<Button>();
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
                default:
                    break;
            }
        }
        submit_button_.onClick.AddListener(SubmitButtonPressed);
        login_toggle_.onValueChanged.AddListener(LoginToggleChanged);
        create_toggle_.onValueChanged.AddListener(CreateToggleChanged);
        create_toggle_.isOn = false;
        join_gameroom_button_.onClick.AddListener(JoinGameRoomButtonPressed);
        tttsquare_button_.onClick.AddListener(TTTSquareButtonPressed);
        //join_gameroom_button_.gameObject.SetActive(false);

        ChangeState(GameEnum.State.LoginMenu);
    }

    void Update()
    {
        
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
        networked_client_.SendMessageToHost(NetworkEnum.ClientToServerSignifier.TTTPlay + "");
        //ChangeState(GameEnum.State.WaitingInQueueForOtherPlayer);
    }

    public void ChangeState(GameEnum.State state)
    {
        join_gameroom_button_.gameObject.SetActive(false);
        submit_button_.gameObject.SetActive(false);
        username_input_.gameObject.SetActive(false);
        password_input_.gameObject.SetActive(false);
        create_toggle_.gameObject.SetActive(false);
        login_toggle_.gameObject.SetActive(false);
        //tttsquare_button_.gameObject.SetActive(false);
        switch (state)
        {
            case GameEnum.State.LoginMenu:
                submit_button_.gameObject.SetActive(true);
                username_input_.gameObject.SetActive(true);
                password_input_.gameObject.SetActive(true);
                create_toggle_.gameObject.SetActive(true);
                login_toggle_.gameObject.SetActive(true);
                break;
            case GameEnum.State.MainMenu:
                join_gameroom_button_.gameObject.SetActive(true);
                break;
            case GameEnum.State.WaitingInQueueForOtherPlayer:
                
                break;
            case GameEnum.State.TicTacToe:
                tttsquare_button_.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
}

public static class GameEnum
{
    public enum State
    {
        LoginMenu = 1,
        MainMenu,
        WaitingInQueueForOtherPlayer,
        TicTacToe
    }
}