using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Direction
{
    private Vector2 firstVector;
    private Vector2 secondVector;

    public Direction(Vector2 firstVector, Vector2 secondVector)
    {
        this.firstVector = firstVector;
        this.secondVector = secondVector;
    }

    public Direction(Vector2 firstVector)
    {
        this.firstVector = firstVector;
        this.secondVector = -firstVector;
    }

    public Vector2 FirstVector { get => firstVector; set => firstVector = value; }
    public Vector2 SecondVector { get => secondVector; set => secondVector = value; }

    public bool Cointains(Vector2 vector)
    {
        return vector == firstVector || vector == secondVector;
    }

}
