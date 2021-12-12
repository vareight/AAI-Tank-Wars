using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPathNode
{
    private TankGrid grid;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable;
    public TankPathNode cameFromNode;

    public TankPathNode(TankGrid grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        isWalkable = true;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public TankGrid GetGrid()
    {
        return grid;
    }
}
