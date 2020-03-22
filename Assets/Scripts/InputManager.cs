using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Enum Swipe
public enum Swipe { Up, Down, Left, Right };

//Enum Direction of Rotation
public enum DirectionOfRotation { Clockwise, CounterClockwise }




public class InputManager : MonoBehaviour
{
    #region Private fields
    private Vector2 firstClickPos;          // Vector location when mouse button down
    private Vector2 secondClickPos;         // Vector location when mouse button up
    private Vector3 currentSwipe;           // = (secondClickPos - firstClickPos)
    private Vector2 vectorHexagonalGroup;   // central position of the hexagonal group

    private float minSwipeLength = 70f;     // it is for being sure it is tap or swipe

    private int layer_mask;
    private RaycastHit2D hit;
    #endregion

    #region Public fields
    public static Swipe swipeDirection;     
    public static DirectionOfRotation directionOfRotation;
    #endregion

    void Start()
    {
        layer_mask = LayerMask.GetMask("Hexagon");  //only hexagons should be selectable
    }


    /* Comment
     * Does 2 things
     * When mouseButtonDown: Selects a Hexagon
     * When mouseButtonUp  : Calculates Direction of Rotation, then Rotates Hexagonal Group
     */
    void Update()
    {
        //if the screen is touched, the menu is not open, the game is not finished, can play, the rotation animation is not active
        if (Input.GetMouseButtonDown(0) && !UIManager.Instance.isCanvasActive && GameController.Instance.isGamePlaying && GameController.Instance.playable && HexagonalGroup.Instance.isAnimating == false)
        {
            firstClickPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
        //if the mouse button up, the menu is not open, the game is not finished, can play, the rotation animation is not active
        if (Input.GetMouseButtonUp(0) && !UIManager.Instance.isCanvasActive && GameController.Instance.isGamePlaying && GameController.Instance.playable && HexagonalGroup.Instance.isAnimating == false)
        {
            secondClickPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            currentSwipe = new Vector3(secondClickPos.x - firstClickPos.x, secondClickPos.y - firstClickPos.y);

            //===Tap===
            if (currentSwipe.magnitude < minSwipeLength)
            {
                GameController.Instance.playable = false;
                SelectObject();
                GameController.Instance.playable = true;
                return;
            }

            //===Swipe===
            //if doesn't have hexagonalGroup, return
            if (HexagonalGroup.Instance.HasHexagonalGroup == false)
                return;

            GameController.Instance.playable = false;

            currentSwipe.Normalize();

            firstClickPos = Camera.main.ScreenToWorldPoint(firstClickPos);

            //Swipe directional check
            // Swipe up
            if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
                swipeDirection = Swipe.Up;

                //rotation direction calculation
                if (firstClickPos.x < vectorHexagonalGroup.x)
                    directionOfRotation = DirectionOfRotation.Clockwise;
                else
                    directionOfRotation = DirectionOfRotation.CounterClockwise;
            }
            // Swipe down
            else if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
                swipeDirection = Swipe.Down;

                //rotation direction calculation
                if (firstClickPos.x > vectorHexagonalGroup.x)
                    directionOfRotation = DirectionOfRotation.Clockwise;
                else
                    directionOfRotation = DirectionOfRotation.CounterClockwise;
            }
            // Swipe left
            else if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            {
                swipeDirection = Swipe.Left;

                //rotation direction calculation
                if (firstClickPos.y < vectorHexagonalGroup.y)
                    directionOfRotation = DirectionOfRotation.Clockwise;
                else
                    directionOfRotation = DirectionOfRotation.CounterClockwise;
            }
            // Swipe right
            else if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            {
                swipeDirection = Swipe.Right;

                //rotation direction calculation
                if (firstClickPos.y > vectorHexagonalGroup.y)
                    directionOfRotation = DirectionOfRotation.Clockwise;
                else
                    directionOfRotation = DirectionOfRotation.CounterClockwise;
            }

            //rotate hexagon group
            HexagonalGroup.Instance.DoRotate(directionOfRotation);
        }

    }

    //selects hexagonal object
    private void SelectObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hit = Physics2D.Raycast(ray.origin, ray.direction * 10, Mathf.Infinity, layer_mask);
        if (hit.collider != null && hit.collider && hit.collider.CompareTag("Hexagon"))
        {
            //Find the closest hexagonal group by clicked location
            vectorHexagonalGroup = hit.collider.GetComponent<Hexagon>().FindClosestHexagonalGroup(ray.origin);
        }
    }
}
