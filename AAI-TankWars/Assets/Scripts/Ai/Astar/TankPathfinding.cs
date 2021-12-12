using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPathfinding
{

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static TankPathfinding Instance { get; private set; }

    private TankGrid grid;
    private List<TankPathNode> openList;
    private List<TankPathNode> closedList;
    private Vector3 originPosition;

    public TankPathfinding(int width, int height, Vector3 originPosition)
    {
        Instance = this;
        grid = new TankGrid(width, height, .5f, originPosition, (TankGrid grid, int x, int y) => new TankPathNode(grid,x,y));
    }

    public TankGrid GetGrid()
    {
        return grid;
    }

    
    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        //Debug.Log("(Vectors) Start: (" + startX + ", " + startY+")");
        //Debug.Log("(Vectors) End: (" + endX + ", " + endY+")");
        //Debug.Log("Direction: (" + tankDirection.x + ", " + tankDirection.y+")");
        //Debug.Log("(Vectors) endX: " + endX + " | endY: " + endY);

        List<TankPathNode> path = FindPath(startX, startY, endX, endY);

        if (path == null)
        {
            return null;
        }
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (TankPathNode pathNode in path) // NON VA
            {
                //Debug.Log("Node: (" + pathNode.x + ", " + pathNode.y + ")");
                Vector3 v = new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * 0.5f;
                //Debug.Log("NodeVector: (" + v.x + ", " + v.y + ")");
                vectorPath.Add(v);
            }
            /* Questo più o meno va
            for (int i =0; i< path.Count-1; i++)
            {
                //Debug.Log("Node: (" + path[i].x + ", " + path[i].y + ")");
                Vector3 v = new Vector3(path[i+1].x - path[i].x, path[i+1].y - path[i].y).normalized;
                //Debug.Log("NodeVector: (" + v.x + ", " + v.y + ")");
                vectorPath.Add(v);
            }*/
            //vectorPath.Add(Vector3.zero);
            return vectorPath;
        }
    }

    public List<TankPathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        TankPathNode startNode = grid.GetGridObject(startX, startY);
        TankPathNode endNode = grid.GetGridObject(endX, endY);
        //Debug.Log("endX: " + endX + " | endY: " + endY);

        if(startNode == null)
        {
            Debug.Log("Start Node Null");
        }
        if (endNode == null)
        {
            Debug.Log("End Node Null");
        }

        openList = new List<TankPathNode> { startNode };
        closedList = new List<TankPathNode>();

        for (int x=0; x<grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                TankPathNode pathNode = grid.GetGridObject(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count >0)
        {
            TankPathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                //Reached final node
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (TankPathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.isWalkable)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }

        }

        //out of nodes on the openList
        return null;

    }

    private List<TankPathNode> GetNeighbourList(TankPathNode currenNode)
    {
        List<TankPathNode> neighbourList = new List<TankPathNode>();

        if (currenNode.x -1 >= 0)
        {
            // Left
            neighbourList.Add(GetNode(currenNode.x-1, currenNode.y));
            // Left Down
            if (currenNode.y - 1 >= 0) neighbourList.Add(GetNode(currenNode.x - 1, currenNode.y - 1));
            // Left Up
            if (currenNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currenNode.x - 1, currenNode.y + 1));
        }
        if (currenNode.x + 1 < grid.GetWidth())
        {
            // Right
            neighbourList.Add(GetNode(currenNode.x+1, currenNode.y));
            // Right Down
            if (currenNode.y - 1 >= 0) neighbourList.Add(GetNode(currenNode.x + 1, currenNode.y - 1));
            // Right Up
            if (currenNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currenNode.x + 1, currenNode.y + 1));
        }
        // Down
        if (currenNode.y - 1 >= 0) neighbourList.Add(GetNode(currenNode.x, currenNode.y - 1));
        // Up
        if (currenNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currenNode.x, currenNode.y + 1));

        return neighbourList;
    }

    private TankPathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<TankPathNode> CalculatePath(TankPathNode endNode)
    {
        List<TankPathNode> path = new List<TankPathNode>();
        path.Add(endNode);
        TankPathNode currentNode = endNode;
        while(currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(TankPathNode a, TankPathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private TankPathNode GetLowestFCostNode(List<TankPathNode> pathNodeList)
    {
        TankPathNode lowestFCostNode = pathNodeList[0];
        for (int i=1; i<pathNodeList.Count; i++)
        {
            if(pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

}
