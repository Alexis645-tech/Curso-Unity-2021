using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        WaveManager.SharedInstance.OnWaveChaged.AddListener(RefreshText);
        RefreshText();
    }

    private void RefreshText()
    {
        _text.text = "WAVE: " + (WaveManager.SharedInstance.MaxWaves-WaveManager.SharedInstance.WavesCount) + 
                     "/" + WaveManager.SharedInstance.MaxWaves;
    }
}
