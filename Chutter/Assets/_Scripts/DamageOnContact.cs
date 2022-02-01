using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField]private float damage;
    private void OnTriggerEnter(Collider other)
    {
        Life life = other.GetComponent<Life>();
        if (life != null)
        {
            life.Amount -= damage;
        }
        gameObject.SetActive(false);
        
        /*if (other.CompareTag("Enemy") || other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
        }*/
    }
}
