using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TankMover : MonoBehaviour
{
    List<Vector2> movementDirections = new List<Vector2> { Vector2.up, new Vector2(1,1).normalized, Vector2.right, new Vector2 (1,-1).normalized,
                                                    Vector2.down, new Vector2(-1,-1).normalized, Vector2.left, new Vector2(-1,1).normalized};

    private Direction[] directions = new Direction[4];

    public Rigidbody2D rb2d;

    public TankMovementData movementData;

    private Vector2 movementVector;
    public float currentSpeed = 0;
    private float currentForewardDirection = 0;
    private Vector2 previousCheckpoint;
    public float targetRotation;

    public OnSpeedChangeEvent OnSpeedChange = new OnSpeedChangeEvent();

    private int currentPathIndex;
    public List<Vector3> pathVectorList;

    private TankController tankController;
    private AIPlayer aiPlayer;

    List<GameObject> gos = new List<GameObject>();

    private void Awake()
    {
        rb2d = GetComponentInParent<Rigidbody2D>();
        tankController = GetComponentInParent<TankController>();
        aiPlayer = GetComponentInParent<AIPlayer>();
        pathVectorList = null;
    }

    private void StopMoving()
    {
        pathVectorList = null;
        if (currentForewardDirection == 1)
        {
            this.Move(new Vector2(0, -1));
        }
        else
        {
            this.Move(new Vector2(0, 1));
        }
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        currentPathIndex = 0;
        previousCheckpoint = this.tankController.transform.position;

        pathVectorList = TankPathfinding.Instance.FindPath(this.tankController.transform.position, targetPosition);

        PatrolPath path = this.aiPlayer.GetComponentInChildren<PatrolPath>();
        List<Transform> points = new List<Transform>();


        foreach (GameObject g in gos)
        {
            Destroy(g);
        }
        gos = new List<GameObject>();
        int i = 0;
        foreach (Vector3 v in pathVectorList)
        {
            GameObject g = new GameObject("Point_" + (i));
            gos.Add(g);
            Transform trans = g.transform;
            trans.SetParent(path.transform);
            points.Add(trans);
            points[i].transform.position = v + TankPathfinding.Instance.GetGrid().GetOriginPosition();
            i++;
        }
        path.patrolPoints = points;
    }

    private Vector2 FindTankDirection()
    {
        Vector2 result;
        int tankRotation = Mathf.FloorToInt(this.tankController.transform.rotation.eulerAngles.z);

        if (tankRotation == 90)
        {
            result = Vector2.left;
        }
        else if (tankRotation == -90)
        {
            result = Vector2.right;
        }
        else
        {
            float x = Mathf.Tan(Mathf.Deg2Rad * tankRotation); ;
            if (tankRotation < 90)
            {
                result = new Vector2(-x, 1);
            }
            else if (tankRotation < 180)
            {
                result = new Vector2(-x, -1);
            }
            else if (tankRotation > -90)
            {
                result = new Vector2(x, -1);
            }
            else
            {
                result = new Vector2(x, 1);
            }
        }
        return result.normalized;
    }

    private bool Neighbourood(int rotation, int threshold)
    {
        if (Mathf.Abs(rotation) < 90)
            return (threshold - Mathf.Abs(rotation) > 0);
        else
            return (180 + threshold - Mathf.Abs(rotation) > 0);
    }

    /* TankRotation:
     * 0°   --> Up      --> (0, 1)
     * 90°  --> Left    --> (-1,0)
     * 180° --> Down    --> (0,-1)
     * 270° --> Right   --> (1, 0)
     */
    public void HandleMovement()
    {
        if (pathVectorList != null)
        {
            Vector2 tankDirection = FindTankDirection();


            Vector3 targetPosition = pathVectorList[currentPathIndex];
            //Debug.Log("Target: (" + targetPosition.x + ", " + targetPosition.y + ")");
            if (targetPosition != Vector3.zero)
            {
                /*int x = Mathf.FloorToInt((targetPosition - this.tankController.transform.position).x);
                int y = Mathf.FloorToInt((targetPosition - this.tankController.transform.position).y);

                Vector2 moveDir = new Vector2(x, y).normalized;
                //Vector2 moveDir = new Vector3(targetPosition.x - this.tankController.transform.position.x, targetPosition.y - this.tankController.transform.position.y).normalized;
                Debug.Log("moveDir: (" + moveDir.x + ", " + moveDir.y + ")");
                */
                //float distanceBefore = Vector3.Distance(transform.position, targetPosition);

                int rotation = Mathf.FloorToInt(Vector2.SignedAngle(tankDirection, (Vector2)targetPosition));
                if (Neighbourood(rotation, 5))
                {
                    if (Mathf.Abs(rotation) < 90)
                    {
                        rotation = 0;
                    }
                    else
                    {
                        rotation = 180;
                    }
                    rb2d.SetRotation(rotation);
                }
                //Debug.Log("Rotation: " + rotation);
                if (rotation != 0 && rotation != 180)
                {
                    //Debug.Log("(Target) MyPos: (" + targetPosition.x + ", " + targetPosition.y + ")");
                    this.Move(new Vector2(targetPosition.x, 0)); // left or right
                    //Debug.Log("Setting rotation to " + rotation);
                }
                else
                {
                    Debug.Log("(ELSE)");
                    if (rotation == 0)
                        this.Move(Vector2.up); // move forward
                    if (rotation == 180)
                        this.Move(Vector2.down); // move backward

                    TankGrid tankGrid = TankPathfinding.Instance.GetGrid();
                    tankGrid.GetXY(this.tankController.transform.position, out int tankX, out int tankY);
                    tankGrid.GetXY(previousCheckpoint + (Vector2)targetPosition * tankGrid.GetCellSize(), out int endX, out int endY);

                    //Debug.Log("(Check) MyPos: (" + tankX + ", " + tankY + ")");
                    //Debug.Log("(Check) Checkpoint: (" + endX + ", " + endY + ")");
                    if (tankX == endX && tankY == endY)
                    {
                        currentPathIndex++;
                        previousCheckpoint = this.tankController.transform.position;
                    }
                }

                //pathVectorList.Remove(targetPosition);
                //Debug.DrawLine(this.tankController.transform.position, targetPosition, Color.red, 3f);
            }
            else
            {

                //if (currentPathIndex >= pathVectorList.Count)
                //{
                StopMoving();
                //    this.Move(Vector3.zero);
                Debug.Log("DONE!");
                //}
            }
        }
        else
        {
            this.Move(Vector3.zero);
        }
    }


    private Vector2 GetClosestVector(Vector2 startDirection)
    {
        float minValue = float.PositiveInfinity;
        float tempValue;
        Vector2 closestVector = Vector2.zero;
        foreach (Vector2 v in movementDirections)
        {
            tempValue = (startDirection - v).x + (startDirection - v).y;
            if (tempValue < minValue)
            {
                minValue = tempValue;
                closestVector = v;
            }
        }
        return closestVector;
    }


    private Vector2 GetOptimalTurn(Vector2 startDirection, Vector2 targetDirection) 
    {
        Vector2 result = Vector2.zero;

        // it's like a clock


        int verticalIndex = movementDirections.IndexOf(startDirection);
        if (verticalIndex == -1)
        {
            //verticalIndex = GetClosestVector(startDirection);
        }

        int horizontalIndex = (verticalIndex + 2) % 8;
        int diagDxIndex = (verticalIndex + 1) % 8;
        int diagSxIndex = (verticalIndex + 7) % 8;

        //Debug.Log("Vertical: " + verticalIndex);
        //Debug.Log("Horizontal: " + horizontalIndex);
        //Debug.Log("Diag right: " + diagDxIndex);
        //Debug.Log("Diag left: " + diagSxIndex);

        directions[0] = new Direction(movementDirections[verticalIndex]);       // vertical
        directions[1] = new Direction(movementDirections[diagSxIndex]);         // left diagonal
        directions[2] = new Direction(movementDirections[diagDxIndex]);         // right diagonal
        directions[3] = new Direction(movementDirections[horizontalIndex]);     // horizontal diagonal

        if (directions[0].Cointains(targetDirection)) // vertical
        {
            if (Vector2.SignedAngle(startDirection, targetDirection) > 0)
                result = Vector2.right;
            else
                result = Vector2.left;
        }

        if (directions[1].Cointains(targetDirection)) // left diagonal
        {
            result = Vector2.left;
        }

        if (directions[2].Cointains(targetDirection)) // right diagonal
        {
            result = Vector2.right;
        }

        if (directions[3].Cointains(targetDirection)) // horizontal
        {
            if (targetDirection == directions[3].FirstVector)
                result = Vector2.right;
            else
                result = Vector2.left;
        }


        return result;
    }

    public void HandleMovement(Vector2 movementVector)
    {
        /* Decomment for delay in the rotation
         * 
        float actualRotation = Vector2.SignedAngle(Vector2.up, this.tankController.transform.up);
        if (targetRotation == 180 && actualRotation < 0) actualRotation = -actualRotation;
        if (Mathf.Abs(targetRotation - actualRotation) > 5)
        {
            return;
        }
        */
        Debug.DrawLine(this.tankController.transform.position, (Vector2)this.tankController.transform.position+movementVector, Color.red, 0.5f);
        if (movementVector == Vector2.zero)
        {
            Move(movementVector);
        }
        else
        {
            float nextTargetRotation = Vector2.SignedAngle(Vector2.up, movementVector);
            //Debug.Log("Target rotation: " + targetRotation);
            if (targetRotation - nextTargetRotation == 0)
            {
                this.Move(Vector2.up); // move forward
            }
            else if (Mathf.Abs(targetRotation - nextTargetRotation) == 180)
            {
                this.Move(Vector2.down); // move backward
            }
            else
            {
                targetRotation = nextTargetRotation;
            }
        }

        /*
        Vector2 tankDirection = FindTankDirection();

        int rotation = Mathf.FloorToInt(Vector2.SignedAngle(tankDirection, movementVector));
        if (Neighbourood(rotation, 3))
        {
            if (Mathf.Abs(rotation) < 90)
            {
                rotation = 0;
            }
            else
            {
                rotation = 180;
            }
            rb2d.SetRotation(rotation);
        }
        //Debug.Log("Rotation: " + rotation);
        if (rotation != 0 && rotation != 180) // need to adjuct the rotation
        {
            //Debug.Log("(Target) MyPos: (" + targetPosition.x + ", " + targetPosition.y + ")");
            Vector2 optimalTurn = GetOptimalTurn(tankDirection, movementVector);
            this.Move(optimalTurn); // left or right (mov.y maybe it's not ok)
            //Debug.Log("Setting rotation to " + rotation);
        }
        else
        {
            Debug.Log("(ELSE)");
            if (rotation == 0)
                this.Move(Vector2.up); // move forward
            if (rotation == 180)
                this.Move(Vector2.down); // move backward
        }

        //pathVectorList.Remove(targetPosition);
        //Debug.DrawLine(this.tankController.transform.position, targetPosition, Color.red, 3f);

        */
    }

    public void Move(Vector2 movementVector)
    {
        this.movementVector = movementVector;
        //Debug.Log("MovVect: (" + movementVector.x + ", " + movementVector.y + ")");
        //CalculateSpeed(movementVector);
        OnSpeedChange?.Invoke(movementVector.magnitude);
        if (movementVector.y > 0)
        {
            if (currentForewardDirection == -1)
                currentSpeed = 0;
            currentForewardDirection = 1;
        }
        else if (movementVector.y < 0)
        {
            if (currentForewardDirection == 1)
                currentSpeed = 0;
            currentForewardDirection = -1;
        }
        else
        {
            currentForewardDirection = 0;
        }
        //Debug.DrawLine(this.tankController.transform.position, movementVector, Color.red, 0.5f);
    }

    /*private void CalculateSpeed(Vector2 movementVector)
    {
        if (Mathf.Abs(movementVector.y) > 0)
        {
            currentSpeed += movementData.acceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed -= movementData.deacceleration * Time.deltaTime;
        }
        currentSpeed = Mathf.Clamp(currentSpeed, 0, movementData.maxSpeed);
    }*/

    private void FixedUpdate()
    {
        //rb2d.velocity = (Vector2)transform.up * currentSpeed * currentForewardDirection * Time.fixedDeltaTime;
        //rb2d.MoveRotation(transform.rotation * Quaternion.Euler(0, 0, -movementVector.x * movementData.rotationSpeed * Time.fixedDeltaTime));//rb2d.velocity = (Vector2)transform.up * currentSpeed * currentForewardDirection * Time.fixedDeltaTime;

        rb2d.velocity = (Vector2)transform.up * movementData.maxSpeed * currentForewardDirection * Time.fixedDeltaTime;
        //rb2d.MoveRotation(transform.rotation * Quaternion.Euler(0, 0, -movementVector.x * movementData.rotationSpeed * Time.fixedDeltaTime));

        rb2d.MoveRotation(Mathf.LerpAngle(rb2d.rotation, targetRotation, movementData.rotationSpeed * Time.fixedDeltaTime));
    }
}
