using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoting : MonoBehaviour
{
    private Animator _animator;

    public int bulletsAmount;

    public Weapon weapon;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.timeScale > 0)
        {
            _animator.SetTrigger("ShotBullet");
            if (bulletsAmount > 0 && weapon.ShootBullet("Player Bullet", 0.25f))
            {
                bulletsAmount--;
                if (bulletsAmount < 0)
                {
                    bulletsAmount = 0;
                }
                
            }
        }
    }
}
