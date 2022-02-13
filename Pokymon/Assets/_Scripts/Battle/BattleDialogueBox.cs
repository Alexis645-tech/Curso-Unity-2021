using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogueBox : MonoBehaviour
{
    [SerializeField] Text dialogueText;
    [SerializeField] private GameObject actionSelect;
    [SerializeField] private GameObject movementSelect;
    [SerializeField] private GameObject movementDescription;
    
    public float charactersPerSecond = 10.0f;

    public IEnumerator SetDialogue(string message)
    {
        dialogueText.text = "";
        foreach (var character in message)
        {
            dialogueText.text += character;
            yield return new WaitForSeconds(1 / charactersPerSecond);
        }
    }
}
