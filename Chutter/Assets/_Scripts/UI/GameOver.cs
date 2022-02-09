using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public TextMeshProUGUI actualScore, actualTime, bestScore, bestTime;
    [SerializeField]private bool playerHasWon;
    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (playerHasWon)
        {
            actualScore.text = "Actual Score: " + PlayerPrefs.GetInt("Last Score");
            actualTime.text = "Time: " + PlayerPrefs.GetFloat("Last Time");
            bestScore.text = "Best: " + PlayerPrefs.GetInt("High Score");
            bestTime.text = "Best: " + PlayerPrefs.GetFloat("Low Time");
        }
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene("Level1");
    }
}
