using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class Hexagon : MonoBehaviour
{
    #region Public fields
    public int row, col;
    public char color = ' ';
    #endregion

    #region Private fields
    //the corners of the hexagon
    private Vector2 rightTop,
                    rightSide,
                    rightBottom,
                    leftTop,
                    leftSide,
                    leftBottom;

    //distances between clicked position and diagonals
    //to find the nearest corner to the clicked location
    private double  rightTopDistance,
                    rightSideDistance,
                    rightBottomDistance,
                    leftTopDistance,
                    leftSideDistance,
                    leftBottomDistance;


    //Ordered list of distances of the corners of the hexagon
    private List<double> distanceBetweenCornerAndOrigin;
    #endregion



    void Start()
    {
        DOTween.Init();
    }

    void Update()
    {
        //Empty
    }

    //sets row and col
    public void setPositionValues(int row, int col){

        this.row = row;
        this.col = col;
    }

    //move with Animation
    public void moveWithAnimation(Vector2 vector2)
    {
        transform.DOMove(vector2, 0.3f, false);
    }

    /* Comment
     * Writes the first letter of the color to charGameBoard (2d char array) 
     * Adds its own reference to the gameBoard (2d GameObject array) 
     */
    public virtual void writeColorValueToGameBoard()
    {
        GameController.Instance.charGameBoard[row, col] = color;
        GameController.Instance.gameBoard[row, col] = this.gameObject;
    }

    //deletes gameobject
    public virtual void deleteFromGameBoard()
    {
        GameController.Instance.charGameBoard[row, col] = ' ';
        Destroy(this.gameObject,1);
    }

    //sets child(star) active
    public void signToExplode()
    {
        transform.GetChild(1).gameObject.SetActive(true);
    }

    //calculate the diagonal positions
    private void calculateCornerVectors()
    {
        rightTop = new Vector2(transform.position.x + 0.23f, transform.position.y + 0.37f);
        rightSide = new Vector2(transform.position.x + 0.46f, transform.position.y);
        rightBottom = new Vector2(transform.position.x + 0.23f, transform.position.y - 0.38f);
        leftTop = new Vector2(transform.position.x - 0.23f, transform.position.y + 0.37f);
        leftSide = new Vector2(transform.position.x - 0.46f, transform.position.y);
        leftBottom = new Vector2(transform.position.x - 0.23f, transform.position.y - 0.38f);
    }

    //calculate distances
    private void calculateDistances(Vector2 rayOrigin)
    {
        rightTopDistance = Vector2.Distance(rayOrigin, rightTop);
        rightSideDistance = Vector2.Distance(rayOrigin, rightSide);
        rightBottomDistance = Vector2.Distance(rayOrigin, rightBottom);
        leftTopDistance = Vector2.Distance(rayOrigin, leftTop);
        leftSideDistance = Vector2.Distance(rayOrigin, leftSide);
        leftBottomDistance = Vector2.Distance(rayOrigin, leftBottom);
    }

    //Adds distances to list then sorts the list
    private void addDistancesToSortedList()
    {
        distanceBetweenCornerAndOrigin = new List<double>();
        distanceBetweenCornerAndOrigin.Add(rightTopDistance);
        distanceBetweenCornerAndOrigin.Add(rightSideDistance);
        distanceBetweenCornerAndOrigin.Add(rightBottomDistance);
        distanceBetweenCornerAndOrigin.Add(leftTopDistance);
        distanceBetweenCornerAndOrigin.Add(leftSideDistance);
        distanceBetweenCornerAndOrigin.Add(leftBottomDistance);

        distanceBetweenCornerAndOrigin.Sort();
    }

    /* Comment
     * Find closest hexagonal group and returns its position (vector2)
     *takes a vector2 rayOrigin as a parameter
     */
    public Vector2 FindClosestHexagonalGroup(Vector2 rayOrigin)
    {
        calculateCornerVectors();       // calculate corner vectors
        calculateDistances(rayOrigin);  // calculate their distances
        addDistancesToSortedList();     // add them to the list and sort

        //get width and height from GameController Instance
        int width = GameController.Instance.getWidth();
        int height = GameController.Instance.getWidth();

        /*
         * to get hexagonal group we need some indexes
         * Let's assume that the hexagon (row, col) we choose
         * so,
         * the top element is in    :row - 1
         * the bottom element is in :row + 1
         * the left element is in   :col - 1
         * the right element is in  :col + 1
         * 
         */
        int topIndex    = row - 1,
            bottomIndex = row + 1,
            leftIndex   = col - 1,
            rightIndex  = col + 1;

        //declare 3 hexagons for the hexagon group
        GameObject first, second, third;

        //Starting from the closest corner, look in order
        foreach (double distance in distanceBetweenCornerAndOrigin)
        {
            //if distance equals to rightTopDistance
            if (distance == rightTopDistance) //Right Top
            {
                //check column
                switch (col % 2)
                {
                    /*  
                     * indexes are checked before setting the hexagon group. they may be out of index. if so, the next closest diagonal is checked
                     * For example, the hexagon in the top left corner and the point near the top left corner are selected.
                     *  There is no other hexagon on the left and top of this hexagon, 
                     *  The next close point is checked.
                     */
                    case 0://if even column

                        if (topIndex >= 0 && rightIndex < width)
                        {
                            first   = GameController.Instance.gameBoard[topIndex, col];
                            second  = GameController.Instance.gameBoard[row, col];
                            third   = GameController.Instance.gameBoard[topIndex, rightIndex];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return rightTop;
                        }
                        break;
                    case 1://if odd column
                        if (topIndex >=0 && rightIndex < width)
                        {
                            first   = GameController.Instance.gameBoard[topIndex, col];
                            second  = GameController.Instance.gameBoard[row,col];
                            third   = GameController.Instance.gameBoard[row, rightIndex];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return rightTop;
                        }
                        break;
                    default:
                        break;
                }

            }
            else if (distance == rightSideDistance) //Right Side
            {
                switch (col % 2)
                {
                    case 0:
                        if( topIndex >= 0 && rightIndex < width)
                        {
                            first   = GameController.Instance.gameBoard[row, col];
                            second  = GameController.Instance.gameBoard[row, rightIndex];
                            third   = GameController.Instance.gameBoard[topIndex, rightIndex];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return rightSide;
                        }
                        break;
                    case 1:
                        if(bottomIndex < height && rightIndex < width)
                        {
                            first   = GameController.Instance.gameBoard[row, col];
                            second  = GameController.Instance.gameBoard[bottomIndex, rightIndex];
                            third   = GameController.Instance.gameBoard[row, rightIndex];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return rightSide;
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (distance == rightBottomDistance) //Right Bottom
            {
                switch (col % 2)
                {
                    case 0:
                        if(bottomIndex < height && rightIndex < width)
                        {
                            first   = GameController.Instance.gameBoard[row, col];
                            second  = GameController.Instance.gameBoard[bottomIndex, col];
                            third   = GameController.Instance.gameBoard[row, rightIndex];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return rightBottom;
                        }
                        break;
                    case 1:
                        if(bottomIndex < height && rightIndex < width)
                        {
                            first   = GameController.Instance.gameBoard[row, col];
                            second  = GameController.Instance.gameBoard[bottomIndex, col];
                            third   = GameController.Instance.gameBoard[bottomIndex, rightIndex];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return rightBottom;
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (distance == leftTopDistance) //Left Top
            {
                switch (col % 2)
                {
                    case 0:
                        if(topIndex >= 0 && leftIndex >= 0)
                        {
                            first   = GameController.Instance.gameBoard[topIndex, leftIndex];
                            second  = GameController.Instance.gameBoard[row, col];
                            third   = GameController.Instance.gameBoard[topIndex, col];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return leftTop;
                        }
                        break;
                    case 1:
                        if(topIndex >= 0 && leftIndex >= 0)
                        {
                            first   = GameController.Instance.gameBoard[row, leftIndex];
                            second  = GameController.Instance.gameBoard[row, col];
                            third   = GameController.Instance.gameBoard[topIndex, col];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return leftTop;
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (distance == leftSideDistance) //Left Side
            {
                switch (col % 2)
                {
                    case 0:
                        if(topIndex >= 0 && leftIndex >= 0)
                        {
                            first   = GameController.Instance.gameBoard[topIndex, leftIndex];
                            second  = GameController.Instance.gameBoard[row, leftIndex];
                            third   = GameController.Instance.gameBoard[row, col];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return leftSide;
                        }
                        break;
                    case 1:
                        if(bottomIndex < height && leftIndex >= 0)
                        {
                            first   = GameController.Instance.gameBoard[row, leftIndex];
                            second  = GameController.Instance.gameBoard[bottomIndex, leftIndex];
                            third   = GameController.Instance.gameBoard[row, col];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return leftSide;
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (distance == leftBottomDistance) //Left Bottom
            {
                switch (col % 2)
                {
                    case 0:
                        if(bottomIndex < height && leftIndex >= 0)
                        {
                            first   = GameController.Instance.gameBoard[row, leftIndex];
                            second  = GameController.Instance.gameBoard[bottomIndex, col];
                            third   = GameController.Instance.gameBoard[row, col];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return leftBottom;
                        }
                        break;
                    case 1:
                        if(bottomIndex < height && leftIndex >= 0)
                        {
                            first   = GameController.Instance.gameBoard[bottomIndex, leftIndex];
                            second  = GameController.Instance.gameBoard[bottomIndex, col];
                            third   = GameController.Instance.gameBoard[row, col];

                            HexagonalGroup.Instance.setHexagons(first, second, third);
                            return leftBottom;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        return new Vector2(0f,0f);
    }

}
