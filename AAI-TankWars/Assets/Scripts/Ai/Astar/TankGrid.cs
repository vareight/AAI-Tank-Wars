using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TankGrid
{

    public event EventHandler<OnGridChangeEventArgs> OnGridObjectChanged;

    public class OnGridChangeEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private TankPathNode[,] gridArray;
    private float cellSize;
    private Vector3 originPosition;

    public TankGrid(int width, int height, float cellSize, Vector3 originPosition, Func<TankGrid, int, int, TankPathNode> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.gridArray = new TankPathNode[width, height];
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        for (int x = 0; x<gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }
    }

    public void SetCellSize(float cellSize)
    {
        this.cellSize = cellSize;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public float GetWidth()
    {
        return width;
    }

    public float GetHeight()
    {
        return height;
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public void SetGridObject(int x, int y, TankPathNode value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
        }
    }

    public void SetGridObject(Vector3 worldPosition, TankPathNode value)
    {
        GetXY(worldPosition, out int x, out int y);
        SetGridObject(x, y, value);

    }

    public TankPathNode GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else return default(TankPathNode);
    }

    public TankPathNode GetGridObject(Vector3 worldPosition)
    {
        GetXY(worldPosition, out int x, out int y);
        return GetGridObject(x,y);
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridChangeEventArgs { x = x, y = y });
    }

    public Vector3 GetOriginPosition()
    {
        return originPosition;
    }
}
