using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : MonoBehaviour
{
    [SerializeField]private float distance;
    [SerializeField]private float angle;

    [SerializeField]private LayerMask targetLayers;
    [SerializeField]private LayerMask obstacleLayers;
}
