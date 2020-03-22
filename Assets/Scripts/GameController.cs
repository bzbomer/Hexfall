using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    #region Public fields
    // Public 
    public volatile bool playable;
    public bool isGamePlaying;

    public GameObject[] hexagons; //prefabs
    public GameObject[] bombs;    //prefabs

    public GameObject[,] gameBoard;
    public char[,] charGameBoard;
    public int score;

    //a list keeps bombs
    public List<GameObject> bombsInGameBoard;
    #endregion

    #region Private fields

    //Private
    private int moveCounter;
    private int bombPoint;

    private int pointsPerExplodingHexagon = 5;

    private int topIndex,
                bottomIndex,
                leftIndex,
                rightIndex;

    private int layer_mask;
    private RaycastHit2D hit;

    //height and width
    [SerializeField, Range(5, 9)]
    private int height = 9;
    [SerializeField, Range(4, 8)]
    private int width = 8;


    /*
     * Requirements for the arrangement of hexagons on the screen.
     * Horizontal distance between hexagon
     * vertical distance between hexagon
     */
    private readonly double xPositionEven = -2.35f;
    private readonly double yPositionEven = 2.65f;
    private readonly double xPositionOdd = -1.68f;
    private readonly double yPositionOdd = 2.295f;
    private readonly double xDistanceBetween2Hexagon = 1.35f;
    private readonly double yDistanceBetween2Hexagon = 0.74f;
    #endregion



    #region Singleton Pattern
    //Instance  ( Singleton Design Pattern)
    public static GameController Instance { get; private set; } 

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

    void Start()
    {
        playable = false;
        layer_mask = LayerMask.GetMask("Hexagon");
        Play();
    }


    /* Comment
     * Runs when a new game starting
     * Reset everything
     * If there is a hexagonal group of the same color at the beginning of the game, it is exploded and the score is not changed
     */
    public void Play()
    {
        isGamePlaying = false;
        moveCounter = 0;
        bombPoint = 1000;
        HexagonalGroup.Instance.Reset();
        InitBoard();
        checkSameColorOccur(); 
        score = 0;
        isGamePlaying = true;
    }

    #region getters
    public int getWidth()
    {
        return width;
    }

    public int getHeight()
    {
        return height;
    }
    #endregion

    /*
     * Runs when successful move
     * Increases movecounter
     * Updates the MoveCounter text
     * Notify bombs
     */
    public void successfulMove()
    {
        ++moveCounter;
        UIManager.Instance.updateMoveCounter(moveCounter);

        //notify if there is a bomb
        foreach (GameObject bomb in bombsInGameBoard)
        {
            if(bomb != null)
            {
                bomb.GetComponent<Bomb>().notify();
            }
        }
    }

    /*
     * checks the counters of bombs. Game over if there is 0
     * */
    public bool checkBombCounters()
    {
        bool result = false;
        foreach (GameObject bomb in bombsInGameBoard)
        {
            if (bomb != null && bomb.GetComponent<Bomb>().counter <= 0)
                result = true;
        }
        if (result)
        {
            isGamePlaying = false;
            UIManager.Instance.showGameOverScreen();
        }
        return result;
    }

    /*
     * Returns vector2 based on the row and col values ​​from the parameter
     */
    public Vector2 calculatePosition(int row, int col)
    {
        double resultPositionX = 0f, tempPositionY = 0f;

        switch (col % 2)
        {
            case 0:
                resultPositionX = xPositionEven;
                tempPositionY = yPositionEven;

                break;

            case 1:
                resultPositionX = xPositionOdd;
                tempPositionY = yPositionOdd;
                break;
        }

        float x = (float)resultPositionX + (float)(xDistanceBetween2Hexagon * (col / 2));
        float y = (float)tempPositionY - (row * (float)yDistanceBetween2Hexagon);

        return new Vector2(x, y);
    }

    /*
     * returns vector2 based on the row and col values of a Hexagon ​​from the parameter
     */
    public Vector2 calculatePosition(GameObject Hexagon)
    {
        int row = Hexagon.GetComponent<Hexagon>().row;
        int col = Hexagon.GetComponent<Hexagon>().col;

        return calculatePosition(row, col);
    }


    /*Comment
     * Swaps 3 hexagons according to the direction of rotation
     * There are 2 types of rotation : clockwise and counterclockwise
     * if it is clockwise rotation        :  Moving -=> first to third,  second to first, third to second
     * if it is counterclockwise rotation :  Moving -=> first to second, second to third, third to first
     * saves rows and cols
     * calculates and updates new positions of hexagons according to row and column values
     */
    public void updateBoard(GameObject firstHexagon, GameObject secondHexagon, GameObject thirdHexagon)
    {
        if (firstHexagon == null || secondHexagon == null || thirdHexagon == null)
            return;

        int firstRow, firstCol, secondRow, secondCol, thirdRow, thirdCol;
        
        firstRow = firstHexagon.GetComponent<Hexagon>().row;
        firstCol = firstHexagon.GetComponent<Hexagon>().col;

        secondRow = secondHexagon.GetComponent<Hexagon>().row;
        secondCol = secondHexagon.GetComponent<Hexagon>().col;

        thirdRow = thirdHexagon.GetComponent<Hexagon>().row;
        thirdCol = thirdHexagon.GetComponent<Hexagon>().col;

        if (InputManager.directionOfRotation == DirectionOfRotation.Clockwise)
        {  
            firstHexagon.GetComponent<Hexagon>().setPositionValues(thirdRow, thirdCol);
            secondHexagon.GetComponent<Hexagon>().setPositionValues(firstRow, firstCol);
            thirdHexagon.GetComponent<Hexagon>().setPositionValues(secondRow, secondCol);
        }
        else
        { 
            firstHexagon.GetComponent<Hexagon>().setPositionValues(secondRow, secondCol);
            secondHexagon.GetComponent<Hexagon>().setPositionValues(thirdRow, thirdCol);
            thirdHexagon.GetComponent<Hexagon>().setPositionValues(firstRow, firstCol);
        }
        firstHexagon.GetComponent<Hexagon>().writeColorValueToGameBoard();
        secondHexagon.GetComponent<Hexagon>().writeColorValueToGameBoard();
        thirdHexagon.GetComponent<Hexagon>().writeColorValueToGameBoard();
    }


    /* Comment
     * checks same colors for every 3 hexagonal group.
     * adds to the list of explodedHexagons if found and returns true
     * updates score according to the size of the explodedHexagons
     * deletes all hexagons in explodedHexagons list
     * calls fillEmptyCells to fill boards
     */
    public bool checkSameColorOccur()
    {
        playable = false;
        bool result = false;
        bool hexagonWillBeExploded = false;
        List<GameObject> explodedHexagons = new List<GameObject>();

        #region check 3 same color
        for (int col = 0; col < width; ++col)
        {
            
            for (int row = 0; row < height; ++row)
            {
                hexagonWillBeExploded = false;
                topIndex    = row - 1;
                bottomIndex = row + 1;
                leftIndex   = col - 1;
                rightIndex  = col + 1;

                switch (col % 2)
                {
                    case 0:
                        //Right Top
                        if(topIndex >= 0 && rightIndex < width)
                        {
                            if (charGameBoard[topIndex, col] == charGameBoard[row, col] && charGameBoard[row, col] == charGameBoard[topIndex, rightIndex])
                                hexagonWillBeExploded = true;
                        }
                        //Right Side
                        if(topIndex >= 0 && rightIndex < width)
                        {
                            if (charGameBoard[row, col] == charGameBoard[row, rightIndex] && charGameBoard[row, col] == charGameBoard[topIndex, rightIndex])
                                hexagonWillBeExploded = true;   
                        }
                        //Right Bottom
                        if(bottomIndex < height && rightIndex < width)
                        {
                            if (charGameBoard[row, col] == charGameBoard[bottomIndex, col] && charGameBoard[row, col] == charGameBoard[row, rightIndex])
                                hexagonWillBeExploded = true;
                        }
                        //Left Top
                        if(topIndex >= 0 && leftIndex >= 0)
                        {
                            if (charGameBoard[topIndex, leftIndex] == charGameBoard[row, col] && charGameBoard[row, col] == charGameBoard[topIndex, col])
                                hexagonWillBeExploded = true;
                        }
                        //Left Side
                        if (topIndex >= 0 && leftIndex >= 0)
                        {
                            if (charGameBoard[topIndex, leftIndex] == charGameBoard[row, leftIndex] && charGameBoard[row, leftIndex] == charGameBoard[row, col])
                                hexagonWillBeExploded = true;
                        }
                        //Left Bottom
                        if(bottomIndex < height && leftIndex >= 0)
                        {
                            if (charGameBoard[row, leftIndex] == charGameBoard[bottomIndex, col] && charGameBoard[bottomIndex, col] == charGameBoard[row, col])
                                hexagonWillBeExploded = true;
                        }
                            break;
                    case 1:
                        //Right Top
                        if(topIndex >= 0 && rightIndex < width)
                        {
                            if (charGameBoard[topIndex, col] == charGameBoard[row, col] && charGameBoard[row, col] == charGameBoard[row, rightIndex])
                                hexagonWillBeExploded = true;
                        }
                        //Right Side
                        if (bottomIndex < height && rightIndex < width)
                        {
                            if (charGameBoard[row, col] == charGameBoard[bottomIndex, rightIndex] && charGameBoard[bottomIndex, rightIndex] == charGameBoard[row, rightIndex])
                                hexagonWillBeExploded = true;    
                        }
                        //Right Bottom
                        if (bottomIndex < height && rightIndex < width)
                        {
                            if (charGameBoard[row, col] == charGameBoard[bottomIndex, col] && charGameBoard[bottomIndex, col] == charGameBoard[bottomIndex, rightIndex])
                                hexagonWillBeExploded = true;
                        }
                        //Left Top
                        if (topIndex >= 0 && leftIndex >= 0)
                        {
                            if (charGameBoard[row, leftIndex] == charGameBoard[row, col] && charGameBoard[row, col] == charGameBoard[topIndex, col])
                                hexagonWillBeExploded = true;
                        }
                        //Left Side
                        if (bottomIndex < height && leftIndex >= 0)
                        {
                            if (charGameBoard[row, leftIndex] == charGameBoard[bottomIndex, leftIndex] && charGameBoard[bottomIndex, leftIndex] == charGameBoard[row, col])
                                hexagonWillBeExploded = true;
                        }
                        //Left Bottom
                        if(bottomIndex < height && leftIndex >= 0)
                        {
                            if (charGameBoard[bottomIndex, leftIndex] == charGameBoard[bottomIndex, col] && charGameBoard[bottomIndex, col] == charGameBoard[row, col])
                                hexagonWillBeExploded = true;
                        }
                            break;
                    default:
                        break;
                }
                if (hexagonWillBeExploded)
                {
                    gameBoard[row, col].GetComponent<Hexagon>().signToExplode();
                    explodedHexagons.Add(gameBoard[row,col]);
                    result = true;
                }
            }
        }
        #endregion

        if (HexagonalGroup.Instance.HasHexagonalGroup) // if it returns false , we sure it is the start of the game
            score += explodedHexagons.Count * pointsPerExplodingHexagon;

        foreach(GameObject hexagon in explodedHexagons)
            hexagon.GetComponent<Hexagon>().deleteFromGameBoard();

        UIManager.Instance.updateScore(score);

        if (result)
        {
            StartCoroutine(fillEmptyCells());
        }
        else
        {
            checkBombCounters();
            playable = true;
        }

        return result;
    }


    /* Comment
     * slides the above hexagons from top to bottom to fill empty cells
     * if there is no hexagon or bomb above empty cell, it creates a new hexagon or bomb
     * Creates bomb when score >= bombPoint
     * the new object is positioned on top of the screen and replaced by animation
     * renews hexagonal group hexagons
     */
    private IEnumerator fillEmptyCells()
    {
        yield return new WaitForSeconds(1f);

        for (int col = 0; col < width; ++col)
        {
            for(int row = height -1; row >=0; --row)
            {
                if(charGameBoard[row,col] == ' ')
                {
                    bool foundAnObject = false;
                    for(int tempRow = row -1; !foundAnObject && tempRow >=0; --tempRow)
                    {
                        if (charGameBoard[tempRow, col] != ' ')
                        {
                            charGameBoard[tempRow, col] = ' ';
                            gameBoard[tempRow, col].GetComponent<Hexagon>().moveWithAnimation(calculatePosition(row,col));
                            gameBoard[tempRow, col].GetComponent<Hexagon>().setPositionValues(row,col);
                            gameBoard[tempRow, col].GetComponent<Hexagon>().writeColorValueToGameBoard();
                            foundAnObject = true;
                        }
                    }
                    if(foundAnObject == false)
                    {
                        double tempPositionX = 0f, tempPositionY = 0f;
                        switch (col % 2)
                        {
                            case 0:
                                tempPositionX = xPositionEven;
                                tempPositionY = yPositionEven;

                                break;

                            case 1:
                                tempPositionX = xPositionOdd;
                                tempPositionY = yPositionOdd;
                                break;
                        }
                        float x = (float)tempPositionX + (float)(xDistanceBetween2Hexagon * (col / 2)); ;
                        if(score >= bombPoint)
                        {
                            gameBoard[row, col] = Instantiate(bombs[Random.Range(0, bombs.Length)], new Vector2(x, 6.40f), Quaternion.identity);
                            bombPoint += 1000;
                        }
                        else
                            gameBoard[row,col] = Instantiate(hexagons[Random.Range(0, hexagons.Length)], new Vector2(x, 6.40f), Quaternion.identity);
                        gameBoard[row, col].GetComponent<Hexagon>().moveWithAnimation(calculatePosition(row,col));
                        gameBoard[row, col].GetComponent<Hexagon>().setPositionValues(row,col);
                        gameBoard[row, col].GetComponent<Hexagon>().writeColorValueToGameBoard();
                    }
                }
            }
        }
        if(checkSameColorOccur() == false)
            HexagonalGroup.Instance.renewHexagons();
    }

    /*
     * Creates hexagon objects and fills the boards
     */
    private void InitBoard()
    {
        //Allocate Memory
        initGameBoard();

        int randomNumber;
        for (int col = 0; col < width; ++col)
        {
            for (int row = 0; row < height; ++row)
            {
                randomNumber = Random.Range(0, hexagons.Length);
                gameBoard[row, col] = Instantiate(hexagons[randomNumber], calculatePosition(row,col), Quaternion.identity);
                gameBoard[row, col].GetComponent<Hexagon>().setPositionValues(row, col);
                charGameBoard[row, col] = gameBoard[row, col].GetComponent<Hexagon>().color;
            }
        }

    }

    /*
     * Does 2 things
     * if it is first game : Allocates memory for game boards and bombList
     * if it is new game   : Deletes old values and clears bombList
     */
    private void initGameBoard()
    {
        if (gameBoard == null)
        {
            charGameBoard = new char[height, width];
            gameBoard = new GameObject[height, width];
            bombsInGameBoard = new List<GameObject>();
        }
        else //Play Again
        {
            bombsInGameBoard.Clear();
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    Destroy(gameBoard[i, j].gameObject);
                    charGameBoard[i, j] = ' ';
                }
            }
        }
    }

    /*
     * Prints the charGameBoard on the console
     */
    void printBoard()
    {
        for (int i = 0; i < height; i++)
        {
            string s = "" + charGameBoard[i, 0];
            for (int j = 1; j < width; ++j)
                s = s + "," + charGameBoard[i, j];
            print(s);
        }
    }
}
