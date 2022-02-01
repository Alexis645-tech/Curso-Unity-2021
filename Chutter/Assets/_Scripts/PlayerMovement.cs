using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("Fuerza de movimiento del personaje en newtons por segundo.")]
    [Range(0, 1000)][SerializeField]private float speed;

    [Tooltip("Fuerza de rotación del personaje en newtons por segundo")]
    [Range(0, 360)][SerializeField]private float rotationSpeed;

    private Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float space = speed * Time.deltaTime;
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(horizontal, 0, vertical);
        //transform.Translate(dir.normalized*space);
        rb.AddRelativeForce(dir.normalized*space);

        float angle = rotationSpeed * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        //transform.Rotate(0, mouseX*angle, 0);
        rb.AddRelativeTorque(0, mouseX*angle, 0);

    }
}
