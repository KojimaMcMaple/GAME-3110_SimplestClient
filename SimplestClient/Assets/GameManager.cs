using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    Button submit_button_;
    InputField username_input_, password_input_;
    Toggle create_toggle_, login_toggle_;
    NetworkedClient networked_client_;

    void Awake()
    {
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
            msg = (int)GlobalEnum.ClientToServerSignifier.CreateAccount + "," + n + "," + p;
        }
        else
        {
            msg = (int)GlobalEnum.ClientToServerSignifier.Login + "," + n + "," + p;
        }
        networked_client_.SendMessageToHost(msg);
        Debug.Log(msg);
    }

    public void LoginToggleChanged(bool value)
    {
        create_toggle_.SetIsOnWithoutNotify(!value);
    }

    public void CreateToggleChanged(bool value)
    {
        login_toggle_.SetIsOnWithoutNotify(!value);
    }
}
