using System;
using SharpNeat.Phenomes;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySharpNEAT;
using Debug = UnityEngine.Debug;

public class AIPlayer : UnitController
{
    private const int NUM_SENSORS = 128;
    private const int INDEX_OBSTACLE = 0;
    private const int INDEX_TANK = 1;
    private const int INDEX_BULLET = 2;

    [SerializeField]
    private TankController tank;

    [SerializeField]
    private AIBehaviour shootBehaviour, patrolBehaviour;

    [SerializeField]
    private AIDetector detector;

    private Damagable damageble;

    GameObject[] enemyTanks;
    GameObject[] bullets;
    //float totHealth;

    // visibility mask for the inputs
    [SerializeField]
    private LayerMask visibilityLayer;
    [SerializeField]
    private LayerMask dangerLayer;

    // general control variables
    public float SensorRange = 7;

    /** SENSORS
     * 0) obstacles
     * 1) enemies
     * 2) bullets
     */
    private float[][] sensors = new float[NUM_SENSORS][];
    private Vector3[] initialPositionTanks;
    private Quaternion[] initialRotationTanks;

    // cache the initial transform of this unit, to reset it on deactivation
    private Vector3 _initialPosition = default;
    private Quaternion _initialRotation = default;

    private float totVelocity = 0;
    private NeatSupervisor neatObject;

    private Stopwatch timer;

    // (x,y)
    /*private Vector2[] directions = { new Vector2(-1, 1).normalized, new Vector2(-0.5f,1).normalized, Vector2.up.normalized,
                                    new Vector2(0.5f,1).normalized, new Vector2(1, 1).normalized, new Vector2(1,0.5f).normalized,
                                    Vector2.right.normalized, new Vector2(1,-0.5f).normalized, new Vector2(1,-1).normalized,
                                    new Vector2(0.5f, -1).normalized, Vector2.down.normalized, new Vector2(-0.5f,-1).normalized,
                                    new Vector2(-1,-1).normalized, new Vector2(-1,-0.5f).normalized, Vector2.left.normalized,
                                    new Vector2(-1,0.5f).normalized};
    */
    private Vector2[] directions = new Vector2[NUM_SENSORS];

    public int bonusBulletsDodged = 0;
    public int totBulletsIncoming = 0;

    private void Start()
    {
        this.timer.Start();
        // cache the inital transform of this Unit, so that when the Unit gets reset, it gets put into its initial state
        _initialPosition = tank.transform.position;
        _initialRotation = tank.transform.rotation;
        enemyTanks = GameObject.FindGameObjectsWithTag("Enemy");
        bullets = GameObject.FindGameObjectsWithTag("Bullet");
        GameObject neatGameObj = GameObject.FindGameObjectWithTag("Neat");
        if (neatGameObj != null)
            neatObject = neatGameObj.GetComponent<NeatSupervisor>() as NeatSupervisor; 
        /*totHealth = 0f;
        foreach (GameObject tank in enemyTanks)
        {
            var damageble = tank.GetComponentInChildren<Damagable>();
            totHealth += damageble.Health;
        }*/
        initialPositionTanks = new Vector3[enemyTanks.Length];
        initialRotationTanks = new Quaternion[enemyTanks.Length];
        int i = 0;
        TankController tankBody;
        foreach (GameObject tt in enemyTanks)
        {
            tankBody = tt.GetComponentInChildren<TankController>();
            initialPositionTanks[i] = tankBody.transform.position;
            initialRotationTanks[i] = tankBody.transform.rotation;
            i++;
        }

        //bulletLayer = LayerMask.NameToLayer("Bullet");
        InitializeSensors();
        float x, y;
        float alpha = 360 / (float)NUM_SENSORS;
        
        for (int s = 0; s < NUM_SENSORS; s++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad*alpha * s);
            y = Mathf.Cos(Mathf.Deg2Rad*alpha * s);
            directions[s] = new Vector2(x,y).normalized;
            //Debug.Log("alpha= " + alpha*s+ "("+x+","+y+")");
        }
    }

    private void Awake()
    {
        tank = GetComponentInChildren<TankController>();
        damageble = tank.GetComponentInChildren<Damagable>();
        detector = tank.GetComponentInChildren<AIDetector>();
        timer = new Stopwatch();
        //var damagable = transform.GetComponentInChildren<Damagable>();
        //hp = damagable.Health;
    }

    private void Update()
    {
        totVelocity += this.tank.tankMover.rb2d.velocity.magnitude;
        //Radar();
    }

    public void StopTimer()
    {
        this.timer.Stop();
        //Debug.Log("STOP");
    }

    public override float GetFitness()
    {
        // Called during the evaluation phase (at the end of each trail)

        // The performance of this unit, i.e. it's fitness, is retrieved by this function.
        // Implement a meaningful fitness function here

        /* relevant information for the fitness function:
         * 1) HEALTH POINTS
         * 2) ENEMY DESTROYED/DAMAGE CAUSED TO ENEMY TANKS
         * 
         * less relevant but useful
         * 3) bullets shot
         * 4) obstacles hitted
         * 5) moving ability
         */

        //float result = hp + 10 * enemiesDestroyed + bulletsShot - obstaclesHit;


        /*GameObject[] currentEnemyTanks = GameObject.FindGameObjectsWithTag("Enemy");
        float currentTotHealth = 0f;
        foreach (GameObject tank in currentEnemyTanks)
        {
            var dmg = tank.GetComponentInChildren<Damagable>();
            currentTotHealth += dmg.Health;
        }

        float avgDmgCaused = (this.totHealth - currentTotHealth) / (this.enemyTanks.Length + 1);
        */

        //float velocity = Mathf.Sqrt(Mathf.Pow(this.tank.tankMover.rb2d.velocity.x, 2) + Mathf.Pow(this.tank.tankMover.rb2d.velocity.y, 2));
        //Debug.Log("velocity: " + velocity);
        //Debug.Log("hit: " + this.tank.ObstaclesHit);

        float avgVelocity = totVelocity / neatObject.TrialDuration;
        //Debug.Log("avg velocity: " + avgVelocity);
        float timeAlive = (float)timer.Elapsed.TotalSeconds * Time.timeScale;
        int bulletBonus = ((2 * bonusBulletsDodged - totBulletsIncoming)+1) * 10;

        if (bulletBonus < 0) bulletBonus = 0;
        
        //Debug.Log("Time Alive: " + timeAlive);
        //Debug.Log("Delta Time: " + Time.fixedDeltaTime);
        float result = this.damageble.Health + this.tank.DmgDealt + this.tank.TankKilled * 10 + (5 - this.tank.ObstaclesHit) * 20 + timeAlive + bulletBonus + avgVelocity;
        result /= 2f;
        if (result < 0)
            return 0;
        else
            return result;
    }

    protected override void HandleIsActiveChanged(bool newIsActive)
    {
        // Called whenever the value of IsActive has changed

        // Since NeatSupervisor.cs is making use of Object Pooling, this Unit will never get destroyed. 
        // Make sure that when IsActive gets set to false, the variables and the Transform of this Unit are reset!
        // Consider to also disable MeshRenderers until IsActive turns true again.
        if (newIsActive == false)
        {
            // the unit has been deactivated, IsActive was switched to false

            // reset transform
            this.tank.transform.position = _initialPosition;
            this.tank.transform.rotation = _initialRotation;

            ResetSensors();
            this.damageble.Health = this.damageble.MaxHealth;
            this.tank.DmgDealt = 0;
            this.tank.TankKilled = 0;
            this.tank.ObstaclesHit = 0;
            this.gameObject.SetActive(true);
            this.totVelocity = 0;
            this.timer.Reset();
            this.timer.Start();
            this.bonusBulletsDodged = 0;
            this.totBulletsIncoming = 0;
            int i = 0;

            TankController tankBody;
            foreach (GameObject tt in enemyTanks)
            {
                tt.SetActive(true);
                var dmg = tt.GetComponentInChildren<Damagable>();
                tankBody = tt.GetComponentInChildren<TankController>();
                dmg.Health = dmg.MaxHealth;
                tankBody.transform.position = initialPositionTanks[i];
                tankBody.transform.rotation = initialRotationTanks[i];
                i++;
            }
        }

        // hide/show children 
        // the children happen to be the car meshes => we hide this Unit when IsActive turns false and show it when it turns true
        //foreach (Transform t in transform)
        //{
        //    t.gameObject.SetActive(newIsActive);
        //}
        //}
    }

    protected override void UpdateBlackBoxInputs(ISignalArray inputSignalArray)
    {
        // Called by the base class on FixedUpdate

        // Feed inputs into the Neural Net (IBlackBox) by modifying its InputSignalArray
        // The size of the input array corresponds to NeatSupervisor.NetworkInputCount

        /** Behaviours:
         * 1) constantly AIMING to the enemy position/new possible position of the enemy tank => enemy position in the field (how to give just a number?)
         * 2) constantly MOVING in order to don't get stuck in obstacles => 4 sensors for proximity collision
         * 3) MOVING away from bullets, dodging when they're arriving => 
         * 4) MOVING in order to get better vision for shooting enemy tanks =>
         * 5) SHOOTING at the enemy if reload time is ok, enemy in range, no obstacles in the way
         * 6) Health??
         * 
         * 1) 8 sensori di movimento/rilevamento proiettili/tank nemici
         * 
         */

        //Inputs: 8 sensors for distance from objects x 3 different objects

        Radar();
        //IncomingDanger();
        // modify the ISignalArray object of the blackbox that was passed into this function, by filling it with the sensor information.
        // Make sure that NeatSupervisor.NetworkInputCount fits the amount of sensors you have

        for (int i = 0; i < NUM_SENSORS; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                inputSignalArray[3 * i + j] = sensors[i][j];
            }
        }
        //LogInput(inputSignalArray);
    }

    protected override void UseBlackBoxOutpts(ISignalArray outputSignalArray)
    {
        // Called by the base class after the inputs have been processed

        // Read the outputs and do something with them
        // The size of the array corresponds to NeatSupervisor.NetworkOutputCount

        /** Tell the AI Tank WHAT to do.
         * 
         * HOW to implement these 3 behaviours will be told by simpler AI algorithms.
         * 1) A* --> move in order to get in vision of enemy tank (avoid obstacles) or better position for killing it
         * 2) shoot when in vision
         * 3) dodge enemy bullets inside safe area
         */

        //float shootAction = (float)outputSignalArray[2] * 2 - 1; //sparare o non sparare
        //var shootingX = (float)outputSignalArray[3] * 2 - 1;
        //var shootingY = (float)outputSignalArray[4] * 2 - 1;

        var shootAction = (float)outputSignalArray[0] * 2 - 1; // *********THIS IS OK*************
        //var dodgeAction = (float)outputSignalArray[3] * 2 - 1;
        var moveToX = (float)outputSignalArray[1] * 2 - 1;
        var moveToY = (float)outputSignalArray[2] * 2 - 1;

        //var moveDist = moveToX * tankMover.movementData.acceleration * Time.fixedDeltaTime;
        //var turnAngle = moveToY * tankMover.movementData.rotationSpeed * Time.fixedDeltaTime * moveDist;
        //Debug.Log("move to: "+ moveDist + " - " + turnAngle);
        //if (moveToX != 0 && moveToY != 0)
        //  Debug.Log("X: "+ moveToX + " - Y: " + moveToY);

        //float shootingAngleX = Mathf.Cos(shootingAngle);
        //float shootingAngleY = Mathf.Sin(shootingAngle);

        Vector2 moveTo = new Vector2(moveToX, moveToY);
        //Vector2 shootTo = new Vector2(shootingX, shootingY);

        this.tank.HandleMoveBody(moveTo);
        //tank.HandleTurretMovement(shootTo);
        // *********THIS IS OK*************
        /*if (shootAction < 0.5)
        {

            if (detector.TargetVisible)
            {
                shootBehaviour.PerformAction(tank, detector);
            }
        }
        //else
        //{
        patrolBehaviour.PerformAction(tank, detector);
        //}
        */
        //}
        //else
        //{
        //    Debug.Log(transform.ToString()+" don't shoot");
        //}
    }


    private void IncomingDanger(RaycastHit2D dangerHit, int sensorPosition)
    {
        // OLD VERSION
        /*RaycastHit2D hit;
        bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            hit = Physics2D.Raycast(bullet.transform.position, bullet.transform.up, SensorRange);
            //Debug.DrawRay(bullet.transform.position, bullet.transform.up * SensorRange, Color.red, 1f);
            if (hit.collider != null)
            {
                if (hit.collider.transform == this.tank.transform)
                {
                    Debug.Log("Incoming Danger at " + hit.distance);
                    sensors[sensorPosition][INDEX_BULLET] = hit.fraction;
                }
            }
        }*/

        if (dangerHit.collider == null)
        {    // no bullet detected from the Radar() function
            if (sensors[sensorPosition][INDEX_BULLET] > 0.15f) bonusBulletsDodged++;
            sensors[sensorPosition][INDEX_BULLET] = 0;
        }
        else                                // bullet detected from the Radar() function
        {
            //Debug.Log("Bullet Detected!");
            RaycastHit2D hit = Physics2D.Raycast(dangerHit.collider.transform.position, dangerHit.collider.transform.up, SensorRange);
            //Debug.DrawRay(dangerHit.collider.transform.position, dangerHit.collider.transform.up * SensorRange, Color.red, 1f);
            if (hit.collider != null)       // bullet raycast colliding with something
            {
                if (hit.collider.transform == this.tank.transform)      // collision with the AI tank -> DANGER!
                {
                    //Debug.Log("Incoming Danger at " + hit.distance);
                    if (sensors[sensorPosition][INDEX_BULLET] == 0) totBulletsIncoming++;
                    sensors[sensorPosition][INDEX_BULLET] = hit.fraction;
                }
                else                                                    // collision with something else -> we don't care
                {
                    //Debug.Log("Not a danger..");
                    if (sensors[sensorPosition][INDEX_BULLET] > 0.15f) bonusBulletsDodged++;
                    sensors[sensorPosition][INDEX_BULLET] = 0;
                }
            }
            else
            {                           // bullet raycast no collision
                if (sensors[sensorPosition][INDEX_BULLET] > 0.15f) bonusBulletsDodged++;
                sensors[sensorPosition][INDEX_BULLET] = 0;
            }
        }
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
        /*else if (hit.collider.CompareTag("Bullet"))
        {
            //return 2;
            IncomingDanger(sensorPosition);
        }*/
        return -1;
    }

    private void Radar()
    {
        // 16 raycasts into different directions each measure how far a object is away.
        /** it's like a clock
         * 0) front left -> clockwise [...] -> 15) left - front left 
         *
         * SENSORS index
         * 0) obstacles
         * 1) enemies
         * 2) bullets
         */

        //Debug.Log("Tank position = " + this.tank.transform.position);
        RaycastHit2D hit, dangerHit;
        int index;


        int sensorPosition = 0;
        int[] possibleIndex = { INDEX_OBSTACLE, INDEX_TANK };
        foreach (Vector2 v in directions)
        {
            hit = Physics2D.Raycast(this.tank.transform.position, v, SensorRange, visibilityLayer);
            dangerHit = Physics2D.Raycast(this.tank.transform.position, v, SensorRange, dangerLayer);

            IncomingDanger(dangerHit, sensorPosition);
            /*if (sensors[sensorPosition][INDEX_BULLET] != 0)
            {
                Debug.DrawRay(this.tank.transform.position, v, Color.red, 1f);
            }*/
            //Debug.DrawRay(this.tank.transform.position, v*10, Color.red, 1f);

            index = CheckCollision(hit);
            if (index != -1)
            {
                sensors[sensorPosition][index] = hit.fraction; //1 - hit.distance / SensorRange;
                //if (sensors[sensorPosition][index] != 0)
                //{
                //    Debug.DrawRay(this.tank.transform.position, v, Color.red, 1f);
                //}

            }
            foreach (int j in possibleIndex)
            {
                if (j != index)
                {
                    sensors[sensorPosition][j] = 0;
                }
            }
            sensorPosition++;
            //Debug.Log(v.ToString() + ":" + hit.collider.ToString());
        }

        //LogSensors();
    }

    private void InitializeSensors()
    {
        /**
         * 1) front left -> clockwise [...] -> 8) left
         */
        for (int i = 0; i < NUM_SENSORS; i++)
        {
            sensors[i] = new float[3];
            for (int j = 0; j < 3; j++)
            {
                sensors[i][j] = 0;
            }
        }
    }

    private void ResetSensors()
    {
        /**
         * 1) front left -> clockwise [...] -> 8) left
         */
        for (int i = 0; i < NUM_SENSORS; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                sensors[i][j] = 0;
            }
        }
    }

    private void LogInput(ISignalArray input)
    {
        string result = "";
        string[] dir = { "up-left", "up", "up-right", "right", "down-right", "down", "down-left", "left" };
        for (int i = 0; i < NUM_SENSORS; i++)
        {
            result += dir[i] + " | ";
            for (int j = 0; j < 3; j++)
            {
                result += input[3 * i + j] + " | ";
            }
            result += "\n";
        }
        Debug.Log(result);
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

    /*private void OnCollisionEnter(Collision collision)
    {
        if (!IsActive)
            return;
        if (collision.collider.CompareTag("Obstacle"))
        {
            obstaclesHit++;
        }
    }*/

    /*public void TargetHit()
    {
        enemiesHit++;
        Debug.Log("HIT!");
    }
    */
}
