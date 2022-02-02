using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager SharedInstance;

    [Tooltip("Cantidad de puntos de la partida actual")]
    [SerializeField]private int amount;

    public int Amount
    {
        get => amount;
        set => amount = value;
    }

    private void Awake()
    {
        if (SharedInstance == null)
        {
            SharedInstance = this;
        }
        else
        {
            Debug.LogError("Shared Instance duplicado; deben ser eliminados");
            Destroy(gameObject);
        }
    }
}
