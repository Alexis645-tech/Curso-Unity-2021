using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField][Tooltip("Tiempo despues del cual se destruye el objeto")]
    private float destructionDelay;
    private void OnEnable()
    {
        Invoke("HideObject", destructionDelay);
    }

    private void HideObject()
    {
        gameObject.SetActive(false);
    }
}
