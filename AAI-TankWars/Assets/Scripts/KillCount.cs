using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillCount : MonoBehaviour
{
    [SerializeField]
    private int numTankKilled = 0;

    public int NumTankKilled
    {
        get { return numTankKilled; }
        set
        {
            numTankKilled = value;
        }
    }

    public void TankKilled()
    {
        numTankKilled++;
    }
}
