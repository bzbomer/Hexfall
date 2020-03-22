using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


//extends Hexagon class
public class Bomb : Hexagon
{
    #region Public field
    
    public int counter; //action counter for explosion
    #endregion

    //counter is initialized
    private void Awake()
    {
        counter = Random.Range(6, 10);
        updateCounterText();
    }

    //Override virtual writeColorValueToGameBoard method
    /*
     * Writes the first letter of the color to charGameBoard (2d char array) 
     * Adds its own reference to the gameBoard (2d GameObject array) 
     * Adds its own reference to bombsInGameBoard (GameObject List) 
    */
    public
    override void writeColorValueToGameBoard()
    {
        base.writeColorValueToGameBoard();
        if (!GameController.Instance.bombsInGameBoard.Contains(gameObject))
            GameController.Instance.bombsInGameBoard.Add(gameObject);
    }

    //Override virtual deleteFromGameBoard method
    /*
     * Runs when it matches as a hexagon group (3 same colors)
     * Deletes gameobject
     */
    public
    override void deleteFromGameBoard()
    {
        GameController.Instance.charGameBoard[row, col] = ' ';
        if (GameController.Instance.bombsInGameBoard.Contains(gameObject))
            GameController.Instance.bombsInGameBoard.Remove(gameObject);

        Destroy(this.gameObject, 1);
    }


    //run when successful move
    public void notify()
    {
        --counter;
        updateCounterText();
    }

    //updates counter text
    private void updateCounterText()
    {
        transform.GetChild(2).gameObject.GetComponent<TextMeshPro>().text = "" + counter;
    }
}
