using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [SerializeField]private GameObject pauseMenu;
    [SerializeField]private Button exitButton;
    
    private void Awake()
    {
        pauseMenu.SetActive(false);
        exitButton.onClick.AddListener(ExitGame);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void ResumeGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}
