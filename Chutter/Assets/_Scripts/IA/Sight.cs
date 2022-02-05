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

    private void Update()
    {
        //filtro de distancia
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, targetLayers);
        foreach (var collider in colliders)
        {
            Vector3 directionToCollider = Vector3.Normalize(collider.bounds.center - transform.position);
            float angleToCollider = Vector3.Angle(transform.forward, directionToCollider);
        }
    }
}
