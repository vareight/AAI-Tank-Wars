using System;
using UnityEngine;

public abstract class AIBehaviour : MonoBehaviour
{
    public bool endPath;
    public abstract void PerformAction(TankController tank, AIDetector detector);
}