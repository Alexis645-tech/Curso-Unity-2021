using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : MonoBehaviour
{
    [SerializeField]private float distance;
    [SerializeField]private float angle;

    [SerializeField]private LayerMask targetLayers;
    [SerializeField]private LayerMask obstacleLayers;

    [SerializeField]private Collider detectedTarget;

    private void Update()
    {
        //filtro de distancia
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, targetLayers);
        detectedTarget = null;
        foreach (var collider in colliders)
        {
            Vector3 directionToCollider = Vector3.Normalize(collider.bounds.center - transform.position);
            
            //Angulo que forman el vector vision  con el vector objetivo
            float angleToCollider = Vector3.Angle(transform.forward, directionToCollider);
            
            //Si el angulo es menor que el de vision
            if (angleToCollider < angle)
            {
                //Comprobamos que en la linea de vision enemigo -> objetivo no haya obstaculos
                if (!Physics.Linecast(transform.position, collider.bounds.center, obstacleLayers))
                {
                    //Guardamos la referencia del objetivo detectado
                    detectedTarget = collider;
                    break;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distance);
    }
}
