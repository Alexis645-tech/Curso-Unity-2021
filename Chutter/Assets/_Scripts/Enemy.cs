using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Tooltip("Cantidad de puntos que se obtienen al eliminar al enemigo")]
    [SerializeField]private int pointsAmount;

    private void Start()
    {
        EnemyManager.SharedInstance.enemies.Add(this);
    }

    private void OnDestroy()
    {
        EnemyManager.SharedInstance.enemies.Remove(this);
        ScoreManager.SharedInstance.Amount += pointsAmount;
    }
}
