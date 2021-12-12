using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;

public class Testing : MonoBehaviour
{

    [SerializeField]
    private AIBehaviour patrolBehaviour, shootBehaviour;

    [SerializeField]
    private AIDetector detector;

    [SerializeField]
    private TankController tankController;
    private TankPathfinding tankPathfinding;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Tilemap obstacles;

    [SerializeField]
    private int threshold = 3;

    Vector3 originPosition;

    public UnityEvent OnMoveBody;

    private GameObject[] enemyTanks;
    private Vector3 closestTank;

    private Vector3 GetClosestEnemyTank()
    {
        Vector3 result = Vector3.zero;
        float minDistance = float.PositiveInfinity;

        foreach (GameObject go in enemyTanks)
        {
            var tempDistance = Vector2.Distance(this.tankController.transform.position, go.transform.position);
            if (tempDistance < minDistance)
            {
                minDistance = tempDistance;
                result = go.transform.position;
            }
        }

        return result;
    }

    // Start is called before the first frame update
    void Start()
    {
        enemyTanks = GameObject.FindGameObjectsWithTag("Enemy");

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>() as Camera;
        obstacles = GameObject.FindGameObjectWithTag("Obstacle").GetComponent<Tilemap>() as Tilemap;

        originPosition = new Vector3(-9, -5);
        tankPathfinding = new TankPathfinding(40, 40, originPosition);

        TankGrid tankGrid = tankPathfinding.GetGrid();

        BoundsInt bounds = obstacles.cellBounds;
        TileBase[] allTiles = obstacles.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    TankPathNode cell = tankGrid.GetGridObject(x - 1, y - 1);
                    if (cell != null)
                    {
                        cell.isWalkable = false;
                    }

                    //Debug.Log("x:" + x + " y:" + y + " tile:" + tile.name);
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamera.nearClipPlane;
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        //Debug.Log("mouse: " + mouseWorldPosition.ToString());
        //if (Input.GetMouseButtonDown(0))
        if (enemyTanks.Length != 0)
        {
            //tankPathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

            closestTank = GetClosestEnemyTank();
            tankPathfinding.GetGrid().GetXY(closestTank, out int x, out int y);
            Vector3 tankPosition = tankController.transform.position;

            tankPathfinding.GetGrid().GetXY(tankPosition, out int tankX, out int tankY);
            //int tankX = (int)(tankPosition.x - (originPosition.x * .5f + 1));
            //int tankY = (int)(tankPosition.y - (originPosition.y * .5f + 1));

            //Debug.Log("From (" + tankX + " | " + tankY + "), to (" + x + " | " + y+")");

            /*List<TankPathNode> path = tankPathfinding.FindPath(tankX, tankY, x, y);
            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    //Debug.Log("cell: " + path[i].x + ", " + path[i].y);
                    Debug.DrawLine(new Vector3(path[i].x, path[i].y) * .5f + Vector3.one * .25f + originPosition, new Vector3(path[i + 1].x, path[i + 1].y) * .5f + Vector3.one * .25f + originPosition, Color.white, 3f);
                    //Vector3 dir = new Vector2(path[i].x - path[i+1].x, path[i].y - path[i+1].y).normalized;
                    //tankController.transform.position = tankController.transform.position + dir * tankController.tankMover.movementData.acceleration * Time.deltaTime;
                    //tankController.HandleMoveBody(dir);
                    //Debug.DrawLine(tankPosition, dir, Color.red, 3f);
                }
            }
            else
            {
                Debug.Log("path null");
            }*/
            //Debug.Log(path.ToString());
            tankController.tankMover.SetTargetPosition(closestTank);
            patrolBehaviour.endPath = false;
            //tankController.HandleMoveBodyAI(mouseWorldPosition);

            //tankController.HandleMoveBody(mousePosition);
        }
        //OnMoveBody?.Invoke();

        if (detector.TargetVisible)
        {
            shootBehaviour.PerformAction(tankController, detector);
        }
        //else
        //{
        //  patrolBehaviour.PerformAction(tankController, detector);
        //}
        if (Vector2.Distance(this.tankController.transform.position, closestTank) > threshold) {
            patrolBehaviour.PerformAction(tankController, detector);
        }
    }
}
