using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeButtonController : MonoBehaviour
{
    private Button button_;
    private Text button_text_;
    private Vector2Int grid_coord_;
    private GameEnum.TicTacToeButtonState state_;
    private GameManager game_manager_;
    private NetworkedClient networked_client_;

    void Awake()
    {
        button_ = transform.GetComponent<Button>();
        button_text_ = transform.GetComponentInChildren<Text>();
        button_text_.text = "";
        button_.onClick.AddListener(SetSpace);

        game_manager_ = FindObjectOfType<GameManager>();
        networked_client_ = FindObjectOfType<NetworkedClient>();
    }

    public void SetGridCoord(int x, int y)
    {
        grid_coord_ = new Vector2Int(x, y);
    }

    public GameEnum.TicTacToeButtonState GetState()
    {
        return state_;
    }

    public void SetState(GameEnum.TicTacToeButtonState state)
    {
        state_ = state;
    }

    public void SetText(string text)
    {
        button_text_.text = text;
    }

    public void SetSpace()
    {
        if (game_manager_.IsTurn())
        {
            networked_client_.SendMessageToHost(NetworkEnum.ClientToServerSignifier.TTTPlay + ","+ grid_coord_.x + "," + grid_coord_.y);
            game_manager_.SetTurn(false);
            button_.interactable = false;
        }
        //button_text_.text = game_manager_.GetPlayToken();
        //button_.interactable = false;
        //state_ = (GameEnum.TicTacToeButtonState)game_manager_.GetCurrPlayerId();
        //game_manager_.CheckGridCoord(grid_coord_); //[OM] - offline mode
    }
}
