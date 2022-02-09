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
        RegisterScore();
        RegisterTime();
        SceneManager.LoadScene("LossScene");
    }

    void CheckWinCondition()
    {
        //Ganar
        if (EnemyManager.SharedInstance.EnemyCount <= 0 &&
            WaveManager.SharedInstance.WavesCount <= 0)
        {
            RegisterScore();
            RegisterTime();
            SceneManager.LoadScene("WinScene");
        }
    }

    void RegisterScore()
    {
        var actualScore = ScoreManager.SharedInstance.Amount;
        PlayerPrefs.SetInt("Last Score", actualScore);
        var highScore = PlayerPrefs.GetInt("High Score", 0);
        if (actualScore > highScore)
        {
            PlayerPrefs.SetInt("High Score", actualScore);
        }
    }
    
    void RegisterTime()
    {
        var actualTime = Time.time;
        PlayerPrefs.SetFloat("Last Time", actualTime);
        var lowTime = PlayerPrefs.GetFloat("Low Score", 999999.0f);
        if (actualTime > lowTime)
        {
            PlayerPrefs.SetFloat("Low Time", actualTime);
        }
    }
}
