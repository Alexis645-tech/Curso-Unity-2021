using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoting : MonoBehaviour
{
    [SerializeField]private GameObject shootingPoint;
    private Animator _animator;

    [SerializeField]private ParticleSystem fireEffect;
    [SerializeField]private AudioSource shootSound;

    public int bulletsAmount;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && bulletsAmount > 0 && Time.timeScale > 0)
        {
            _animator.SetTrigger("ShotBullet");
            Invoke("FireBullet", 0.2f);
            
        }
    }

    void FireBullet()
    {
        GameObject bullet = ObjectPool.SharedInstance.GetFirstPooledObject();
        bullet.layer = LayerMask.NameToLayer("Player Bullet");
        bullet.transform.position = shootingPoint.transform.position;
        bullet.transform.rotation = shootingPoint.transform.rotation;
        bullet.SetActive(true);
        fireEffect.Play();
        Instantiate(shootSound, transform.position, transform.rotation).GetComponent<AudioSource>().Play();
        
        bulletsAmount--;
        if (bulletsAmount < 0)
        {
            bulletsAmount = 0;
        }
    }
}
