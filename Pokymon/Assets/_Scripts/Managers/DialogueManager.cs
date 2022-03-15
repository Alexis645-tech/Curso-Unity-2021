using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager SharedInstace;
    
    [SerializeField] GameObject dialogueBox;
    [SerializeField] Text dialogueText;
    public float charactersPerSecond;

    public event Action OnDialogueStart, OnDialogueFinish; 
    
    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 1.0f;

    private Dialogue currentDialogue;
    private int currentLine = 0;
    private bool isWriting;

    public bool isBeingShown;
    private Action onDialogueClose;
    private void Awake()
    {
        if (SharedInstace == null)
        {
            SharedInstace = this;
        }
    }

    public void ShowDialogue(Dialogue dialogue, Action onDialogueFinish = null)
    {
        OnDialogueStart?.Invoke();
        dialogueBox.SetActive(true);
        
        currentDialogue = dialogue;
        isBeingShown = true;
        this.onDialogueClose = onDialogueFinish;
        StartCoroutine(SetDialogue(currentDialogue.Lines[currentLine]));
    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;
        if (Input.GetAxisRaw("Submit") != 0 && !isWriting)
        {
            if (timeSinceLastClick >= timeBetweenClicks)
            {
                timeSinceLastClick = 0;
                currentLine++;
                if (currentLine < currentDialogue.Lines.Count)
                {
                    StartCoroutine(SetDialogue(currentDialogue.Lines[currentLine]));
                }
                else
                {
                    currentLine = 0;
                    isBeingShown = false;
                    dialogueBox.SetActive(false);
                    onDialogueClose?.Invoke();
                    OnDialogueFinish?.Invoke();
                }
            }
        }
    }
    public IEnumerator SetDialogue(string line)
    {
        isWriting = true;
        dialogueText.text = "";
        foreach (var character in line)
        {
            if (character != ' ')
            {
                SoundManager.SharedInstance.PlayRandomCharacterSound();
            }
            dialogueText.text += character;
            yield return new WaitForSeconds(1 / charactersPerSecond);
        }

        isWriting = false;
    }
}
