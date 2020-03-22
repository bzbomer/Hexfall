using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HexagonalGroup : MonoBehaviour
{
    #region Public fields

    //animation status
    public bool isAnimating = false;

    //3 hexagons
    public GameObject firstHexagon, 
                      secondHexagon, 
                      thirdHexagon;
    #endregion

    #region Private fields
    private GameObject tempHexagon; //for swapping

    private bool hasHexagonalGroup;

    //Positions for 3 hexagons (x,y)
    private Vector2 firstHexagonPosition;
    private Vector2 secondHexagonPosition;
    private Vector2 thirdHexagonPosition;

    //row and col for each hexagon
    //transforms change during rotation, to calculate their position correctly
    private int rowIndexFirstHexagon,  colIndexFirstHexagon,
                rowIndexSecondHexagon, colIndexSecondHexagon,
                rowIndexThirdHexagon,  colIndexThirdHexagon;
    #endregion

    //getter
    public bool HasHexagonalGroup { get => hasHexagonalGroup; }

    #region Singleton Design Pattern
    public static HexagonalGroup Instance { get; private set; }

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
        DOTween.Init();
    }

    //runs when new game starts
    public void Reset()
    {
        hasHexagonalGroup = false;
    }

    //Outlines of hexagons are Activated
    private void setHexagonsChildsActive()
    {
        if (firstHexagon == null || secondHexagon == null || thirdHexagon == null)
            return;

        this.firstHexagon.transform.GetChild(0).gameObject.SetActive(true);
        this.secondHexagon.transform.GetChild(0).gameObject.SetActive(true);
        this.thirdHexagon.transform.GetChild(0).gameObject.SetActive(true);
    }

    //Outlines of hexagons are inActivated
    private void setHexagonsChildsInActive()
    {
        if (firstHexagon == null || secondHexagon == null || thirdHexagon == null)
            return;

        this.firstHexagon.transform.GetChild(0).gameObject.SetActive(false);
        this.secondHexagon.transform.GetChild(0).gameObject.SetActive(false);
        this.thirdHexagon.transform.GetChild(0).gameObject.SetActive(false);
    }

    //assign row and col values
    private void saveRowAndCol()
    {
        rowIndexFirstHexagon = this.firstHexagon.GetComponent<Hexagon>().row;
        colIndexFirstHexagon = this.firstHexagon.GetComponent<Hexagon>().col;

        rowIndexSecondHexagon = this.secondHexagon.GetComponent<Hexagon>().row;
        colIndexSecondHexagon = this.secondHexagon.GetComponent<Hexagon>().col;

        rowIndexThirdHexagon = this.thirdHexagon.GetComponent<Hexagon>().row;
        colIndexThirdHexagon = this.thirdHexagon.GetComponent<Hexagon>().col;
    }

    /* Comment
     * sets Hexagons
     * disables outlines of old hexagons
     * assigns new references
     * saves rows and cols
     * activates outlines of new hexagons
     */
    public void setHexagons(GameObject firstHexagon, GameObject secondHexagon, GameObject thirdHexagon)
    {
        if (firstHexagon == null || secondHexagon == null || thirdHexagon == null)
            return;

        setHexagonsChildsInActive();

        this.firstHexagon     = firstHexagon;
        this.secondHexagon    = secondHexagon;
        this.thirdHexagon     = thirdHexagon;

        saveRowAndCol();

        setHexagonsChildsActive();

        //Now it has a hexagonal group
        this.hasHexagonalGroup = true;
    }

    /* Comment
     * Renew
     * Last 3 hexagons might be exploded or their positions might be changed.
     * assigns current references
     */
    public void renewHexagons()
    {
        if (hasHexagonalGroup == false)
            return;

        this.setHexagons(GameController.Instance.gameBoard[rowIndexFirstHexagon, colIndexFirstHexagon],
                         GameController.Instance.gameBoard[rowIndexSecondHexagon, colIndexSecondHexagon],
                         GameController.Instance.gameBoard[rowIndexThirdHexagon, colIndexThirdHexagon]);
    }

    /* Comment
     * Rotates hexagon group (clockwise or counterclockwise)
     * Calculates the position vectors of all 3 hexagons by row and column.
     * Need to rotate clockwise or counterclockwise
     * sets isAnimating = true
     */
    public void DoRotate(DirectionOfRotation directionOfRotation)
    {
        if (firstHexagon == null || secondHexagon == null || thirdHexagon == null)
            return;

        isAnimating = true;

        firstHexagonPosition = GameController.Instance.calculatePosition(firstHexagon);
        secondHexagonPosition = GameController.Instance.calculatePosition(secondHexagon);
        thirdHexagonPosition  = GameController.Instance.calculatePosition(thirdHexagon);

        StartCoroutine(MovingAnimation(directionOfRotation));
    }

    /* Comment
     * Rotates hexagon group with animation
     * Rotates 3 times and updates boards
     * in the first 2 rotations checks if there are hexagon groups of the same color:
     * terminates the rotation process, if any
     * if not, it returns to its starting positions
     * 
     * Rotations:
     * Rotate - Clockwise
     * Moving -=> first to third,  second to first, third to second
     * Rotate - CounterClockwise
     * Moving -=> first to second,  second to third, third to first
     */
    IEnumerator MovingAnimation(DirectionOfRotation directionOfRotation)
    {
        if (directionOfRotation == DirectionOfRotation.Clockwise)
        {
            for (int i = 0; i < 3; ++i)
            {
                this.firstHexagon.GetComponent<Hexagon>().moveWithAnimation(thirdHexagonPosition);
                this.secondHexagon.GetComponent<Hexagon>().moveWithAnimation(firstHexagonPosition);
                this.thirdHexagon.GetComponent<Hexagon>().moveWithAnimation(secondHexagonPosition);

                yield return new WaitForSeconds(1);

                GameController.Instance.updateBoard(firstHexagon, secondHexagon, thirdHexagon);

                if (i < 2 && GameController.Instance.checkSameColorOccur())
                {
                    this.setHexagonsChildsInActive(); 
                    GameController.Instance.successfulMove();
                    break;
                }
                //swap references
                tempHexagon = firstHexagon;
                firstHexagon = secondHexagon;
                secondHexagon = thirdHexagon;
                thirdHexagon = tempHexagon;
            }
        }
        else 
        {
            for (int i = 0; i < 3; ++i)
            {
                this.firstHexagon.GetComponent<Hexagon>().moveWithAnimation(secondHexagonPosition);
                this.secondHexagon.GetComponent<Hexagon>().moveWithAnimation(thirdHexagonPosition);
                this.thirdHexagon.GetComponent<Hexagon>().moveWithAnimation(firstHexagonPosition);

                yield return new WaitForSeconds(1);

                GameController.Instance.updateBoard(firstHexagon, secondHexagon, thirdHexagon);

                if (i < 2 && GameController.Instance.checkSameColorOccur())
                {
                    this.setHexagonsChildsInActive();
                    GameController.Instance.successfulMove();
                    break;
                }
                //swap references
                tempHexagon = firstHexagon;
                firstHexagon = thirdHexagon;
                thirdHexagon = secondHexagon;
                secondHexagon = tempHexagon;
            }
        }
        //End of animation
        isAnimating = false;
    }
}


