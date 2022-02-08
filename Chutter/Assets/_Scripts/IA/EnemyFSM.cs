using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Sight))]
public class EnemyFSM : MonoBehaviour
{
    public enum EnemyState
    {
        GoToBase,
        AttackBase,
        ChasePlayer,
        AttackPlayer
    }

    public EnemyState currentState;

    private Sight _sight;

    private Transform baseTransform;
    public float baseAttackDistance, playerAttackDistance;
    private NavMeshAgent _agent;
    private Animator _animator;

    private float lastShootTime;
    public float shootRate;
    private void Awake()
    {
        _sight = GetComponent<Sight>();
        baseTransform = GameObject.FindWithTag("Base").transform;
        _agent = GetComponentInParent<NavMeshAgent>();
        _animator = GetComponentInParent<Animator>();
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.GoToBase:
                GoToBase();
                break;

            case EnemyState.AttackBase:
                AttackBase();
                break;

            case EnemyState.ChasePlayer:
                ChasePlayer();
                break;

            case EnemyState.AttackPlayer:
                AttackPlayer();
                break;

            default:
                //TODO: caso por defecto
                break;
        }
    }

    void GoToBase()
    {
        print("Ir a Base");

        _agent.isStopped = false;
        _agent.SetDestination(baseTransform.position);
        
        if (_sight.detectedTarget != null)
        {
            currentState = EnemyState.ChasePlayer;
            return;
        }

        float distanceToBase = Vector3.Distance(transform.position, baseTransform.position);
        if (distanceToBase < baseAttackDistance)
        {
            currentState = EnemyState.AttackBase;
        }
    }

    void AttackBase()
    {
        print("Atacar la base enemiga");
        
        _agent.isStopped = true;
        ShootTarget();
        lookAt(baseTransform.position);
    }

    void ChasePlayer()
    {
        print("Perseguir al jugador");
        
        if (_sight.detectedTarget == null)
        {
            currentState = EnemyState.GoToBase;
            return;
        }

        _agent.isStopped = false;
        _agent.SetDestination(_sight.detectedTarget.transform.position);

        float distanceToPlayer = Vector3.Distance(transform.position, _sight.detectedTarget.transform.position);
        if (distanceToPlayer < playerAttackDistance)
        {
            currentState = EnemyState.AttackPlayer;
        }
    }

    void AttackPlayer()
    {
        print("Atacar al jugador");
        
        _agent.isStopped = true;
        
        if (_sight.detectedTarget == null)
        {
            currentState = EnemyState.GoToBase;
            return;
        }
        
        lookAt(_sight.detectedTarget.transform.position);
        ShootTarget();
        
        float distanceToPlayer = Vector3.Distance(transform.position, _sight.detectedTarget.transform.position);
        if (distanceToPlayer > playerAttackDistance*1.5f)
        {
            currentState = EnemyState.ChasePlayer;
        }
    }

    void ShootTarget()
    {
        if (Time.timeScale > 0)
        {
            var timeSinceLastShoot = Time.time - lastShootTime;
            if (timeSinceLastShoot < shootRate)
            {
                return;
            }

            _animator.SetTrigger("ShotBullet");
            lastShootTime = Time.time;
            var bullet = ObjectPool.SharedInstance.GetFirstPooledObject();
            bullet.transform.position = transform.position;
            bullet.transform.rotation = transform.rotation;
            bullet.layer = LayerMask.NameToLayer("Enemy Bullet");
            bullet.SetActive(true);
        }
    }

    void lookAt(Vector3 targetPos)
    {
        var directionToLook = Vector3.Normalize(targetPos - transform.position);
        directionToLook.y = 0;
        transform.parent.forward = directionToLook;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, playerAttackDistance);
            
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, baseAttackDistance);
    }
}
