using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    // temporary
    /*private const int NUM_SENSORS = 16;
    private float[][] sensors = new float[NUM_SENSORS][];
    public float SensorRange = 3;
    private GameObject[] bullets;
    [SerializeField]
    private LayerMask visibilityLayer;*/
    // temporary


    public TankMover tankMover;
    public AimTurret aimTurret;
    public Turret[] turrets;

    [SerializeField]
    private int dmgDealt = 0;
    [SerializeField]
    private int tankKilled = 0;
    [SerializeField]
    private int obstaclesHit = 0;

    public int DmgDealt
    {
        get { return dmgDealt; }
        set
        {
            dmgDealt = value;
        }
    }
    public int TankKilled
    {
        get { return tankKilled; }
        set
        {
            tankKilled = value;
        }
    }
    public int ObstaclesHit
    {
        get { return obstaclesHit; }
        set
        {
            obstaclesHit = value;
        }
    }

    private void Awake()
    {
        if (tankMover == null)
            tankMover = GetComponentInChildren<TankMover>();
        if (aimTurret == null)
            aimTurret = GetComponentInChildren<AimTurret>();
        if (turrets == null || turrets.Length == 0)
        {
            turrets = GetComponentsInChildren<Turret>();
        }
        //InitializeSensors(); // TEMP
    }

    public void HandleShoot()
    {
        foreach (var turret in turrets)
        {
            turret.Shoot();
        }

    }

    

    public void HandleMoveBody(Vector2 movementVector)
    {
        tankMover.HandleMovement(movementVector);
        //tankMover.Move(movementVector);
        //Debug.Log("Rotation: " + this.transform.eulerAngles.z);
        //Radar();
        //IncomingDanger();
    }

    public void HandleMoveBodyAI()
    {
        tankMover.HandleMovement();
    }

    public void HandleTurretMovement(Vector2 pointerPosition)
    {
        aimTurret.Aim(pointerPosition);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ObstaclesHit++;
    }

    /*
    private void IncomingDanger()
    {
        RaycastHit2D hit;
        bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            hit = Physics2D.Raycast(bullet.transform.position, bullet.transform.up, SensorRange);
            Debug.DrawRay(bullet.transform.position, bullet.transform.up * SensorRange, Color.red, 1f);
            if (hit.collider != null)
            {
                if (hit.collider.transform == this.transform)
                {
                    Debug.Log("Incoming Danger at "+ hit.distance);
                }
            }
        }
    }

    private void Radar()
    {
        
        RaycastHit2D hit;
        int index;

        // (x,y)
        Vector2[] directions = { new Vector2(-1, 1).normalized, new Vector2(-0.5f,1).normalized, Vector2.up.normalized,
                                    new Vector2(0.5f,1).normalized, new Vector2(1, 1).normalized, new Vector2(1,0.5f).normalized,
                                    Vector2.right.normalized, new Vector2(1,-0.5f).normalized, new Vector2(1,-1).normalized,
                                    new Vector2(0.5f, -1).normalized, Vector2.down.normalized, new Vector2(-0.5f,-1).normalized,
                                    new Vector2(-1,-1).normalized, new Vector2(-1,-0.5f).normalized, Vector2.left.normalized,
                                    new Vector2(-1,0.5f).normalized};
        int i = 0;
        int[] possibleIndex = { 0, 1, 2 };
        foreach (Vector2 v in directions)
        {
            hit = Physics2D.Raycast(this.transform.position, v, SensorRange, visibilityLayer);

            index = CheckCollision(hit);
            if (index != -1)
            {
                sensors[i][index] = 1 - hit.distance / SensorRange;
                if (sensors[i][index] != 0)
                {
                    Debug.DrawRay(this.transform.position, v, Color.red, 1f);
                }

            }
            foreach (int j in possibleIndex)
            {
                if (j != index)
                {
                    sensors[i][j] = 0;
                }
            }
            i++;
            //Debug.Log(v.ToString() + ":" + hit.collider.ToString());
        }

        //LogSensors();
    }

    private void InitializeSensors()
    {
        
        for (int i = 0; i < NUM_SENSORS; i++)
        {
            sensors[i] = new float[3];
            for (int j = 0; j < 3; j++)
            {
                sensors[i][j] = 0;
            }
        }
    }


    private void LogSensors()
    {
        string result = "";
        //string[] dir = { "1", "up", "up-right", "right", "down-right", "down", "down-left", "left" };
        for (int i = 0; i < NUM_SENSORS; i++)
        {
            result += (i + 1) + " | ";
            for (int j = 0; j < 3; j++)
            {
                result += sensors[i][j] + " | ";
            }
            result += "\n";
        }
        Debug.Log(result);
    }

    private int CheckCollision(RaycastHit2D hit)
    {

        if (hit.collider == null) return -1;
        //Debug.Log("HIT: " + hit.collider.tag);
        if (hit.collider.CompareTag("Obstacle"))
        {
            return 0;
        }
        else if (hit.collider.CompareTag("Enemy"))
        {
            //Debug.Log("HIT: " + hit.collider.ToString());
            return 1;
        }
        else if (hit.collider.CompareTag("Bullet"))
        {
            return 2;
        }
        return -1;
    }
*/
}
