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

    private Rigidbody _rigidbody;
    private Animator _animator;
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        
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
        _rigidbody.AddRelativeForce(dir.normalized*space);

        float angle = rotationSpeed * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        //transform.Rotate(0, mouseX*angle, 0);
        _rigidbody.AddRelativeTorque(0, mouseX*angle, 0);
        
        _animator.SetFloat("Velocity", _rigidbody.velocity.magnitude);

        /*
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _animator.SetFloat("Velocity", _rigidbody.velocity.magnitude);
        }
        else
        {
            if (Mathf.Abs(horizontal) < 0.01f && Mathf.Abs(vertical) < 0.01f)
            {
                _animator.SetFloat("Velocity", 0);
            }
            else
            {
                _animator.SetFloat("Velocity", 0.15f);
            }
        }

        _animator.SetFloat("MoveX", horizontal);
        _animator.SetFloat("MoveY", vertical);*/
    }
}
