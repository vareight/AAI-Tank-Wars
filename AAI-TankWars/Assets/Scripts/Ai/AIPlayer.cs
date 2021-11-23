using SharpNeat.Phenomes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySharpNEAT;

public class AIPlayer : UnitController
{
    [SerializeField]
    private TankController tank;

    [SerializeField]
    private TankMover tankMover;

    // general control variables
    //public float Speed = 5f;
    //public float TurnSpeed = 180f;
    public float SensorRange = 20;

    public int hp = 100;
    public int enemiesDestroyed = 0;
    public int bulletsShot = 0;
    public int obstaclesHit = 0;

    /** SENSORS
     * 0) obstacles
     * 1) enemies
     * 2) bullets
     */
    private float[][] sensors = new float[8][];

    // cache the initial transform of this unit, to reset it on deactivation
    private Vector3 _initialPosition = default;
    private Quaternion _initialRotation = default;

    private void Start()
    {
        // cache the inital transform of this Unit, so that when the Unit gets reset, it gets put into its initial state
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    private void Awake()
    {
        tank = GetComponentInChildren<TankController>();
        tankMover = GetComponentInChildren<TankMover>();
        //var damagable = transform.GetComponentInChildren<Damagable>();
        //hp = damagable.Health;
    }

    public override float GetFitness()
    {
        // Called during the evaluation phase (at the end of each trail)

        // The performance of this unit, i.e. it's fitness, is retrieved by this function.
        // Implement a meaningful fitness function here
        //float result = hp + 10 * enemiesDestroyed - bulletsShot - obstaclesHit;
        float result = tankMover.currentSpeed;
        return result;
    }

    protected override void HandleIsActiveChanged(bool newIsActive)
    {
        // Called whenever the value of IsActive has changed

        // Since NeatSupervisor.cs is making use of Object Pooling, this Unit will never get destroyed. 
        // Make sure that when IsActive gets set to false, the variables and the Transform of this Unit are reset!
        // Consider to also disable MeshRenderers until IsActive turns true again.
        if (transform != null)
        {
            if (newIsActive == false)
            {
                // the unit has been deactivated, IsActive was switched to false

                // reset transform
                transform.position = _initialPosition;
                transform.rotation = _initialRotation;

                // reset members
                hp = 100;
                enemiesDestroyed = 0;
                bulletsShot = 0;
                obstaclesHit = 0;
            }

            // hide/show children 
            // the children happen to be the car meshes => we hide this Unit when IsActive turns false and show it when it turns true
            //foreach (Transform t in transform)
            //{
            //    t.gameObject.SetActive(newIsActive);
            //}
        }
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
        

        InitializeSensors();
        Radar();
        // modify the ISignalArray object of the blackbox that was passed into this function, by filling it with the sensor information.
        // Make sure that NeatSupervisor.NetworkInputCount fits the amount of sensors you have
        for (int i = 0; i<8; i++)
        {
            for (int j = 0; j<3; j++)
            {
                inputSignalArray[3 * i + j] = sensors[i][j];
            }
        }
    }

    protected override void UseBlackBoxOutpts(ISignalArray outputSignalArray)
    {
        // Called by the base class after the inputs have been processed

        // Read the outputs and do something with them
        // The size of the array corresponds to NeatSupervisor.NetworkOutputCount

        /** Tell the AI Tank WHAT to do:
         * 
         * 1) Aim & Shoot (done)
         * 2) Move on the field in a smart way
         * 3) Dodge enemy bullets
         * 
         * We can use sort of a priority mask in order to have a hierarchy in the decisions.
         * The 2 main behaviours are SHOOT and MOVE
         * Shoot - Dodge
         * Search - Dodge
         * Shoot - Move
         * Search - Move
         * 
         * [ 1 - 1 - 1]
         * 
         * HOW to implement these 3 behaviours will be told by simpler AI algorithms.
         */

        //int shootAction = (int)outputSignalArray[0] * 2 - 1; //sparare o non sparare
        //var shootingAngle = (float)outputSignalArray[1] * 2 - 1;

        var moveToX = (float)outputSignalArray[0] * 2 - 1;
        var moveToY = (float)outputSignalArray[1] * 2 - 1;

        var moveDist = moveToX * tankMover.movementData.acceleration * Time.deltaTime;
        var turnAngle = moveToY * tankMover.movementData.rotationSpeed * Time.deltaTime * moveDist;

        //float shootingAngleX = Mathf.Cos(shootingAngle);
        //float shootingAngleY = Mathf.Sin(shootingAngle);

        Vector2 moveTo = new Vector2(moveDist, turnAngle);
        //Vector2 shootTo = new Vector2(shootingAngleX, shootingAngleY);

        tank.HandleMoveBody(moveTo);
        //tank.HandleTurretMovement(shootTo);
        //if (shootAction == 1)
        //{
        //    tank.HandleShoot();
        //    bulletsShot++;
        //}
    }
    private int CheckCollision(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Obstacle"))
        {
            return 0;
        }
        else if (hit.collider.CompareTag("Enemy"))
        {
            return 1;
        }
        else if (hit.collider.CompareTag("Bullet"))
        {
            return 2;
        }
        return -1;
    }

    private void Radar()
    {
        // 8 raycasts into different directions each measure how far a object is away.
        RaycastHit hit;
        int index;
        if (Physics.Raycast(transform.position + transform.forward * 1.1f, transform.TransformDirection(new Vector3(0, 0, 1).normalized), out hit, SensorRange))
        {
            index = CheckCollision(hit);
            if (index != -1)
            {
                sensors[1][index] = 1 - hit.distance / SensorRange;
            }
        }

        if (Physics.Raycast(transform.position + transform.forward * 1.1f, transform.TransformDirection(new Vector3(0.5f, 0, 1).normalized), out hit, SensorRange))
        {
            index = CheckCollision(hit);
            if (index != -1)
            {
                sensors[2][index] = 1 - hit.distance / SensorRange;
            }
        }

        if (Physics.Raycast(transform.position + transform.forward * 1.1f, transform.TransformDirection(new Vector3(1, 0, 0).normalized), out hit, SensorRange))
        {
            index = CheckCollision(hit);
            if (index != -1)
            {
                sensors[3][index] = 1 - hit.distance / SensorRange;
            }
        }

        if (Physics.Raycast(transform.position + transform.forward * 1.1f, transform.TransformDirection(new Vector3(-0.5f, 0, 1).normalized), out hit, SensorRange))
        {
            index = CheckCollision(hit);
            if (index != -1)
            {
                sensors[0][index] = 1 - hit.distance / SensorRange;
            }
        }

        if (Physics.Raycast(transform.position + transform.forward * 1.1f, transform.TransformDirection(new Vector3(-1, 0, 0).normalized), out hit, SensorRange))
        {
            index = CheckCollision(hit);
            if (index != -1)
            {
                sensors[7][index] = 1 - hit.distance / SensorRange;
            }
        }

        if (Physics.Raycast(transform.position + transform.forward * 1.1f, transform.TransformDirection(new Vector3(0, 0, -1).normalized), out hit, SensorRange))
        {
            index = CheckCollision(hit);
            if (index != -1)
            {
                sensors[5][index] = 1 - hit.distance / SensorRange;
            }
        }

        if (Physics.Raycast(transform.position + transform.forward * 1.1f, transform.TransformDirection(new Vector3(-0.5f, 0, -1).normalized), out hit, SensorRange))
        {
            index = CheckCollision(hit);
            if (index != -1)
            {
                sensors[6][index] = 1 - hit.distance / SensorRange;
            }
        }

        if (Physics.Raycast(transform.position + transform.forward * 1.1f, transform.TransformDirection(new Vector3(0.5f, 0, -1).normalized), out hit, SensorRange))
        {
            index = CheckCollision(hit);
            if (index != -1)
            {
                sensors[4][index] = 1 - hit.distance / SensorRange;
            }
        }
    }

    private void InitializeSensors()
    {
        /**
         * 1) front left -> clockwise [...] -> 8) left
         */
        for (int i = 0; i < 8; i++)
        {
            sensors[i] = new float[3];
            for (int j = 0; j < 3; j++)
            {
                sensors[i][j] = 0;
            }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!IsActive)
            return;
        if (collision.collider.CompareTag("Obstacle"))
        {
            obstaclesHit++;
        }
    }

}
