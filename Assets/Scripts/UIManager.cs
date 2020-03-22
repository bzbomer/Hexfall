using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Public fields
    public GameObject MenuPanel;
    public GameObject continueButton;
    public GameObject gameOverText;
    public GameObject scoreText;
    public GameObject moveCountText;

    public bool isCanvasActive;
    #endregion


    #region Sigleton Design Pattern
    public static UIManager Instance { get; private set; }

    
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    private void Start()
    {
        isCanvasActive = false;
    }

    //run when clicking the menu button
    public void MenuButtonOnClick()
    {
        isCanvasActive = !isCanvasActive;
        MenuPanel.SetActive( isCanvasActive );
        isGamePlaying();//some conditions for the state of play
    }

    //run when clicking the continue button
    public void ContinueButtonOnClick()
    {
        MenuPanel.SetActive(false);
        isCanvasActive = false;
    }

    //run when clicking the new game button
    public void NewGameButtonOnClick()
    {

        gameOverText.SetActive(false);
        MenuPanel.SetActive(false);
        isCanvasActive = false;
        GameController.Instance.Play();
    }

    //updates score text
    public void updateScore(int score)
    {
        scoreText.GetComponent<Text>().text = ""+score;
    }

    //updates move counter text
    public void updateMoveCounter(int moveCount)
    {
        moveCountText.GetComponent<Text>().text = "" + moveCount;
    }

    //some conditions for the state of play
    private void isGamePlaying()
    {
        //if the game is being played, the continue button must be active, otherwise false
        bool status = GameController.Instance.isGamePlaying;
        continueButton.SetActive( status );
    }

    //game over screen is displayed when the game is over
    public void showGameOverScreen()
    {
        gameOverText.SetActive(true);
        MenuButtonOnClick();
    }

    //run when clicking the exit button
    public void ExitButtonOnClick()
    {
        Application.Quit();
    }
}
