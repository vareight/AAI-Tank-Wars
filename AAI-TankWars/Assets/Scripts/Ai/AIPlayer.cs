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
    private const int NUM_SENSORS = 8;
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
    private int[] sensorsMask = new int[NUM_SENSORS];

    private List<GameObject> currentDangerousBullets;
    private List<GameObject> totDangerousBullets;

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
    public float bonusSafePosition = 0;


    private void Start()
    {
        this.timer.Start();
        // cache the inital transform of this Unit, so that when the Unit gets reset, it gets put into its initial state
        _initialPosition = tank.transform.position;
        _initialRotation = tank.transform.rotation;
        enemyTanks = GameObject.FindGameObjectsWithTag("Enemy");
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
            x = Mathf.Sin(Mathf.Deg2Rad * alpha * s);
            y = Mathf.Cos(Mathf.Deg2Rad * alpha * s);
            directions[s] = new Vector2(x, y).normalized;
            //Debug.Log("alpha= " + alpha*s+ "("+x+","+y+")");
            sensorsMask[s] = 0;
        }
        currentDangerousBullets = new List<GameObject>();
        totDangerousBullets = new List<GameObject>();
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
        //ResetSensorMask();
        //BulletRadar();
        //Radar();
        bonusSafePosition += CalculateSafePositions();
        Debug.Log("Position: " + this.tank.transform.position);
        //Debug.Log("Fitness: " + bonusSafePosition);
    }

    public void StopTimer()
    {
        this.timer.Stop();
        //Debug.Log("STOP");
    }

    /* Does it make sense to use in the fitness function a map giving points depending on the position? What is the sense of the sensors then?
     * è come elaborare gli input che diamo in pasto alla rete(?)
     */
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

        CalcolaBonusDodged();
        //Debug.Log("Dodges: " + bonusBulletsDodged);

        //Debug.Log("Time Alive: " + timeAlive);
        //Debug.Log("Delta Time: " + Time.fixedDeltaTime);
        //float result = this.damageble.Health + this.tank.DmgDealt + this.tank.TankKilled * 10 + (5 - this.tank.ObstaclesHit) * 20 + timeAlive + bulletBonus;
        //float result = this.damageble.Health + (5 - this.tank.ObstaclesHit) * 10 + bonusBulletsDodged*5;
        float result = bonusBulletsDodged;
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
            ResetSensorMask();
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
            this.currentDangerousBullets = new List<GameObject>();
            this.totDangerousBullets = new List<GameObject>();
            this.damageble.Hits = 0;

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

        ResetSensorMask();
        BulletRadar();
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

        //var shootAction = (float)outputSignalArray[0] * 2 - 1; // *********THIS IS OK*************
        //var dodgeAction = (float)outputSignalArray[3] * 2 - 1;
        //var moveToX = (float)outputSignalArray[1] * 2 - 1; // *********THIS IS OK*************
        //var moveToY = (float)outputSignalArray[2] * 2 - 1; // *********THIS IS OK*************

        double maxOutput = -1f;
        int maxIndex = -1;
        for(int i =0; i<outputSignalArray.Length; i++)
        {
            if (outputSignalArray[i] > maxOutput)
            {
                maxOutput = outputSignalArray[i];
                maxIndex = i;
            }
        }

        if (maxIndex == 8) // don't move
        {
            this.tank.HandleMoveBody(Vector2.zero);
        }
        else // move in one of the 8 directions
        {
            this.tank.HandleMoveBody(directions[maxIndex]);
        }

        //var moveDist = moveToX * tankMover.movementData.acceleration * Time.fixedDeltaTime;
        //var turnAngle = moveToY * tankMover.movementData.rotationSpeed * Time.fixedDeltaTime * moveDist;
        //Debug.Log("move to: "+ moveDist + " - " + turnAngle);
        //if (moveToX != 0 && moveToY != 0)
        //  Debug.Log("X: "+ moveToX + " - Y: " + moveToY);

        //float shootingAngleX = Mathf.Cos(shootingAngle);
        //float shootingAngleY = Mathf.Sin(shootingAngle);

        //Vector2 moveTo = new Vector2(moveToX, moveToY); // *********THIS IS OK*************
        //Vector2 shootTo = new Vector2(shootingX, shootingY);

        //this.tank.HandleMoveBody(moveTo); // *********THIS IS OK*************
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

    private int GetIncomingDangerSector(GameObject bullet)
    {
        int result = 0;
        Vector2 dir = bullet.transform.position - this.tank.transform.position;

        //Debug.DrawLine(this.tank.transform.position, bullet.transform.position, Color.red, 2f);
        float alpha = -Vector2.SignedAngle(Vector2.up, dir);
        if (alpha < 0) alpha += 360;
        //Debug.Log("alpha = " + alpha);

        float alphaSector = 360 / (float)NUM_SENSORS;

        for (int s = 0; s < NUM_SENSORS; s++)
        {
            if (alpha >= alphaSector * s && alpha <= alphaSector * (s + 1))
            {
                result = s;
                break;
            }
            //Debug.Log("alpha= " + alpha*s+ "("+x+","+y+")");
        }
        return result;
    }

    private void IncomingDanger(GameObject bullet)
    {
        Debug.DrawRay(bullet.transform.position, bullet.transform.up * SensorRange, Color.yellow, 0.1f);

        if (!totDangerousBullets.Contains(bullet))      // first time this bullet is dangerous for the AI tank
        {
            totBulletsIncoming++;
            totDangerousBullets.Add(bullet);
        }
        currentDangerousBullets.Add(bullet);
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

    private void CalcolaBonusDodged()
    {
        bonusBulletsDodged = 0;
        foreach (GameObject bullet in totDangerousBullets)
        {
            if (!currentDangerousBullets.Contains(bullet))
            {
                bonusBulletsDodged++;
            }
        }
        bonusBulletsDodged -= this.damageble.Hits;
    }

    private void BulletRadar()
    {
        currentDangerousBullets = new List<GameObject>();
        
        bullets = GameObject.FindGameObjectsWithTag("Bullet");      // find all the bullets on the scene
        foreach (GameObject bullet in bullets)
        {
            //Debug.Log("Detected Bullet");

            RaycastHit2D[] hits = Physics2D.RaycastAll(bullet.transform.position, bullet.transform.up, SensorRange);

            if (hits.Length != 0) // at least 1 hit
            {
                foreach (RaycastHit2D hit in hits)
                {

                    if (hit.collider != null)       // bullet raycast colliding with something
                    {
                        if (hit.collider.transform == this.tank.transform)      // collision with the AI tank -> DANGER!
                        {

                            IncomingDanger(bullet);
                            //Debug.Log("Incoming Danger at " + hit.distance);
                            //if (sensors[sensorPosition][INDEX_BULLET] == 0) totBulletsIncoming++;
                            //sensors[sensorPosition][INDEX_BULLET] = hit.fraction;
                            int sector = GetIncomingDangerSector(bullet);
                            //Debug.Log("Sector = " + sector);
                            if (sensors[sector][INDEX_BULLET] < hit.fraction)     // update with the lower value of hit.fraction (distance)
                                sensors[sector][INDEX_BULLET] = hit.fraction;
                            //sensorsMask[sector] = 1;
                            Debug.DrawRay(this.tank.transform.position, directions[sector], Color.white, 1f);
                            Debug.DrawRay(this.tank.transform.position, directions[(sector + 1) % NUM_SENSORS], Color.white, 1f);

                        }
                        else                                                    // collision with something else -> we don't care
                        {
                            //Debug.Log("Not a danger..");
                            //if (sensors[sensorPosition][INDEX_BULLET] > 0.05f) bonusBulletsDodged++;
                            //sensors[sensorPosition][INDEX_BULLET] = 0;
                        }
                    }
                    else
                    {                           // bullet raycast no collision
                                                //if (sensors[sensorPosition][INDEX_BULLET] > 0.05f) bonusBulletsDodged++;
                                                //sensors[sensorPosition][INDEX_BULLET] = 0;
                    }
                }
            }
            CalcolaBonusDodged();

        }

        //CalcolaBonusDodged();

        /*for (int i = 0; i < NUM_SENSORS; i++)
        {
            if (sensorsMask[i] == 0)
            {
                if (sensors[i][INDEX_BULLET] > 0.08f) bonusBulletsDodged++;
                sensors[i][INDEX_BULLET] = 0;
            }
        }*/
    }

    private float CalculateSafePositions()
    {
        bullets = GameObject.FindGameObjectsWithTag("Bullet");      // find all the bullets on the scene
        Dictionary<Vector2, float> mapRisks = new Dictionary<Vector2, float>();
        foreach (GameObject bullet in bullets)
        {
            //ray = Physics2D.Raycast(bullet.transform.position, bullet.transform.up, SensorRange, LayerMask.NameToLayer("Nothing"));
            for (int i = 0; i<50; i++)
            {
                Vector2 nextBulletPosition = bullet.transform.position + bullet.transform.up *i/10f ; //bullet line until a distance of 5
                nextBulletPosition.x = Mathf.Round(nextBulletPosition.x * 100.0f) * 0.01f;
                nextBulletPosition.y = Mathf.Round(nextBulletPosition.y * 100.0f) * 0.01f;
                if (mapRisks.ContainsKey(nextBulletPosition))
                {
                    if (mapRisks[nextBulletPosition] > i / 50f)
                    {
                        mapRisks[nextBulletPosition] = i / 50f;
                    }
                }
                else
                {
                    mapRisks.Add(nextBulletPosition, i / 50f);
                }
                //Debug.DrawLine(nextBulletPosition, nextBulletPosition + (Vector2)bullet.transform.up*0.1f, Color.yellow, 0.1f);
            }
        }
        String s = "";
        foreach(Vector2 riskPosition in mapRisks.Keys)
        {
           s += "Risk position: " + riskPosition + ", safeness: " + mapRisks[riskPosition]+"\n";
            
        }

        Debug.Log(s);

        float result;

        //Debug.Log("TANK POSITION: "+ this.tank.transform.position);

        if (CheckTankDimensions(mapRisks, out Vector2 tankPosition))
        {
            Debug.Log("Collision incoming at "+ tankPosition);
            result = mapRisks[tankPosition];
        }
        else
        {
            result = 0;
        }

        //Debug.Log("Result = " + result);
        return result;
    }

    private bool CheckTankDimensions(Dictionary<Vector2, float> mapRisks, out Vector2 tankPosition)
    {
        float step = 0.1f;
        Vector2 position = this.tank.transform.position;
        Vector2 posTopRight, posTopLeft, posBottomRight, posBottomLeft;
        for (int i = 0; i<4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                posTopRight = new Vector2(position.x + step * i, position.y + step * j);
                posTopLeft = new Vector2(position.x - step * i, position.y + step * j);
                posBottomRight = new Vector2(position.x + step * i, position.y - step * j);
                posBottomLeft = new Vector2(position.x - step * i, position.y - step * j);
                Debug.Log("Neigh: " + posTopRight);
                Debug.Log("Neigh: " + posTopLeft);
                Debug.Log("Neigh: " + posBottomRight);
                Debug.Log("Neigh: " + posBottomLeft);

                posTopRight.x = Mathf.Round(posTopRight.x * 100.0f) * 0.01f;
                posTopRight.y = Mathf.Round(posTopRight.y * 100.0f) * 0.01f;
                posTopLeft.x = Mathf.Round(posTopLeft.x * 100.0f) * 0.01f;
                posTopLeft.y = Mathf.Round(posTopLeft.y * 100.0f) * 0.01f;
                posBottomRight.x = Mathf.Round(posBottomRight.x * 100.0f) * 0.01f;
                posBottomRight.y = Mathf.Round(posBottomRight.y * 100.0f) * 0.01f;
                posBottomLeft.x = Mathf.Round(posBottomLeft.x * 100.0f) * 0.01f;
                posBottomLeft.y = Mathf.Round(posBottomLeft.y * 100.0f) * 0.01f;
                
                if (mapRisks.ContainsKey(posTopRight))
                {
                    tankPosition = posTopRight;
                    return true;
                }
                if (mapRisks.ContainsKey(posTopLeft))
                {
                    tankPosition = posTopLeft;
                    return true;
                }
                if (mapRisks.ContainsKey(posBottomRight))
                {
                    tankPosition = posBottomRight;
                    return true;
                }
                if (mapRisks.ContainsKey(posBottomLeft))
                {
                    tankPosition = posBottomLeft;
                    return true;
                }
            }
        }
        tankPosition = default;
        return false;
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
        RaycastHit2D hit;
        int index;


        int sensorPosition = 0;
        int[] possibleIndex = { INDEX_OBSTACLE, INDEX_TANK };
        foreach (Vector2 v in directions)
        {
            hit = Physics2D.Raycast(this.tank.transform.position, v, SensorRange, visibilityLayer);

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

    private void ResetSensorMask()
    {
        for (int i = 0; i < NUM_SENSORS; i++)
        {
            sensorsMask[i] = 0;
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
