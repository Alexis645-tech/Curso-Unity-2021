using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Life : MonoBehaviour
{
    private float amount;
    public float maximunLife;

    public UnityEvent OnDeath;

    public float Amount
    {
        get => amount;
        set
        {
            amount = value;
            if (amount <= 0)
            {
                OnDeath.Invoke();
            }
        }
    }

    private void Awake()
    {
        amount = maximunLife;
    }
}
