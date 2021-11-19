using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefixMsgButtonController : MonoBehaviour
{
    private Button button_;
    private Text button_text_;
    private int msg_id_;
    private GameManager game_manager_;
    private NetworkedClient networked_client_;

    void Awake()
    {
        button_ = transform.GetComponent<Button>();
        button_text_ = transform.GetComponentInChildren<Text>();
        button_.onClick.AddListener(SendPrefixMsg);

        game_manager_ = FindObjectOfType<GameManager>();
        networked_client_ = FindObjectOfType<NetworkedClient>();
    }

    public void SetMsgId(int id)
    {
        msg_id_ = id;
    }

    public void SendPrefixMsg()
    {
        networked_client_.SendMessageToHost(NetworkEnum.ClientToServerSignifier.ChatSend + "," + game_manager_.GetPrefixMsgFromId(msg_id_));
    }
}
