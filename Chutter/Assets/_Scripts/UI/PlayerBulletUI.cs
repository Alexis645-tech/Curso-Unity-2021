using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerBulletUI : MonoBehaviour
{
    private TextMeshProUGUI _text;
    public PlayerShoting targetShoting;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _text.text = "BULLETS: " + targetShoting.bulletsAmount;
    }
}
