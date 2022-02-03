using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Tooltip("Cantidad de puntos que se obtienen al eliminar al enemigo")]
    [SerializeField]private int pointsAmount;

    private void Awake()
    {
        var life = GetComponent<Life>();
        life.OnDeath.AddListener(DestroyEnemy);
    }

    private void Start()
    {
        EnemyManager.SharedInstance.AddEnemy(this);
    }

    private void DestroyEnemy()
    {
        Animator anim = GetComponent<Animator>();
        anim.SetTrigger("PlayDie");
        var life = GetComponent<Life>();
        life.OnDeath.RemoveListener(DestroyEnemy);
        Destroy(gameObject, 3);

        EnemyManager.SharedInstance.RemoveEnemy(this);
        ScoreManager.SharedInstance.Amount += pointsAmount;
    }

    void PlayDestruction()
    {
        ParticleSystem explosion = GetComponent<ParticleSystem>();
        explosion.Play();
    }
}
