using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPatrolPathBehaviour : AIBehaviour
{
    public PatrolPath patrolPath;
    [Range(0.1f, 1)]
    public float arriveDistance = 1;
    public float waitTime = 0.5f;
    [SerializeField]
    private bool isWaiting = false;
    [SerializeField]
    Vector2 currentPatrolTarget = Vector2.zero;
    bool isInitialized = false;

    private int currentIndex = -1;

    private void Awake()
    {
        this.endPath = false;
        if (patrolPath == null)
            patrolPath = GetComponentInChildren<PatrolPath>();
    }

    public override void PerformAction(TankController tank, AIDetector detector)
    {
        if (!isWaiting)
        {

            if (this.endPath)
            {
                //Debug.Log("Fine");
                tank.HandleMoveBody(Vector2.zero);
                return;
            }

            if (patrolPath.Length < 2)
            {
                //tank.HandleMoveBody(Vector2.down);
                return;
            }
            if (!isInitialized)
            {
                //Debug.Log("Inizializzo..");
                var currentPathPoint = patrolPath.GetClosestPathPoint(tank.transform.position);
                this.currentIndex = currentPathPoint.Index;
                this.currentPatrolTarget = currentPathPoint.Position;
                isInitialized = true;
                //Debug.Log("index = " + currentIndex);
            }



            if (Vector2.Distance(tank.transform.position, currentPatrolTarget) < arriveDistance)
            {
                isWaiting = true;

                StartCoroutine(WaitCoroutine());

                //patrolPath.DeletePreviousPathPoint(currentIndex);
                return;
            }
            Vector2 directionToGo = currentPatrolTarget - (Vector2)tank.tankMover.transform.position;
            var dotProduct = Vector2.Dot(tank.tankMover.transform.up, directionToGo.normalized);

            if (dotProduct < 0.98f)
            {
                var crossProduct = Vector3.Cross(tank.tankMover.transform.up, directionToGo.normalized);
                int rotationResult = crossProduct.z >= 0 ? -1 : 1;
                tank.HandleMoveBody(new Vector2(rotationResult, 0));
            }
            else
            {
                tank.HandleMoveBody(Vector2.up);
            }

        }

        IEnumerator WaitCoroutine()
        {
            yield return new WaitForSeconds(waitTime);

            if (currentIndex + 1 == patrolPath.Length)
            {
                //Debug.Log("index = " + currentIndex);
                tank.HandleMoveBody(Vector2.down);
                this.endPath = true;
                currentIndex = -1;
                isInitialized = false;
                isWaiting = false;
                //Debug.Log("Stop");
            }
            else
            {
                var nextPathPoint = patrolPath.GetNextPathPoint(currentIndex);
                currentPatrolTarget = nextPathPoint.Position;
                currentIndex = nextPathPoint.Index;
                isWaiting = false;
            }
        }
    }
}
