using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damagable : MonoBehaviour
{
    /*[SerializeField]
    private int hits = 0;

    public int Hits
    {
        get { return hits; }
        set
        {
            hits = value;
        }
    }*/

    public int MaxHealth = 100;

    [SerializeField]
    private int health = 0;

    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            OnHealthChange?.Invoke((float)Health / MaxHealth);
        }
    }


    public UnityEvent OnDead;
    public OnHealthChangeEvent OnHealthChange;
    public UnityEvent OnHit, OnHeal;


    private void Start()
    {
        if(health == 0)
            Health = MaxHealth;
    }

    internal void Hit(int damagePoints)
    {
        Health -= damagePoints;
        //hits++;
        if (Health <= 0)
        {
            OnDead?.Invoke();
        }
        else
        {
            OnHit?.Invoke();
        }
    }

    public void Heal(int healthBoost)
    {
        //hits++;
        Health += healthBoost;
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        OnHeal?.Invoke();
    }
}
