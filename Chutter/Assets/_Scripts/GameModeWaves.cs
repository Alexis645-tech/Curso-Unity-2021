using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeWaves : MonoBehaviour
{
    [SerializeField]private Life playerLife;
    [SerializeField]private Life BaseLife;

    private void Start()
    {
        playerLife.OnDeath.AddListener(CheckLoseCondition);
        BaseLife.OnDeath.AddListener(CheckLoseCondition);
        
        EnemyManager.SharedInstance.onEnemyChanged.AddListener(CheckWinCondition);
        WaveManager.SharedInstance.OnWaveChaged.AddListener(CheckWinCondition);
    }

    void CheckLoseCondition()
    {
        //Perder
        SceneManager.LoadScene("LossScene");
    }

    void CheckWinCondition()
    {
        //Ganar
        if (EnemyManager.SharedInstance.EnemyCount <= 0 &&
            WaveManager.SharedInstance.WavesCount <= 0)
        {
            SceneManager.LoadScene("WinScene");
        }
    }
}
