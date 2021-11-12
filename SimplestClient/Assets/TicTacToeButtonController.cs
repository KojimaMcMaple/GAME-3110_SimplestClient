using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeButtonController : MonoBehaviour
{
    private Button button_;
    private Text button_text_;
    private string play_token_ = "X";

    void Awake()
    {
        button_ = transform.GetComponent<Button>();
        button_text_ = transform.GetComponentInChildren<Text>();
        button_text_.text = "";
        button_.onClick.AddListener(SetSpace);
    }

    public void SetSpace()
    {
        button_text_.text = play_token_;
        button_.interactable = false;
    }
}
