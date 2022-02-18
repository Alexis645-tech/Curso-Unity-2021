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
    
    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> movementTexts;

    [SerializeField] private Text ppText;
    [SerializeField] private Text typeText;
    
    [SerializeField] Color selectedColor = Color.blue;

    public float timeToWaitAfterText = 1f;
    
    public float charactersPerSecond = 10.0f;
    public bool isWriting;

    public IEnumerator SetDialogue(string message)
    {
        isWriting = true;
        dialogueText.text = "";
        foreach (var character in message)
        {
            dialogueText.text += character;
            yield return new WaitForSeconds(1 / charactersPerSecond);
        }

        yield return new WaitForSeconds(timeToWaitAfterText);
        isWriting = false;
    }

    public void ToggleDialogueText(bool activated)
    {
        dialogueText.enabled = activated;
    }

    public void ToggleActions(bool activated)
    {
        actionSelect.SetActive(activated);
    }

    public void ToggleMovements(bool activated)
    {
        movementSelect.SetActive(activated);
        movementDescription.SetActive(activated);
    }

    public void SelectAction(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            actionTexts[i].color = (i == selectedAction ? selectedColor : Color.black);
        }
    }

    public void SetPokemonMovements(List<Move> moves)
    {
        for (int i = 0; i < movementTexts.Count; i++)
        {
            if (i < moves.Count)
            {
                movementTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                movementTexts[i].text = "---";
            }
        }
    }
    
    public void SelectMovement(int selectedMovement, Move move)
    {
        for (int i = 0; i < movementTexts.Count; i++)
        {
            movementTexts[i].color = (i == selectedMovement ? selectedColor : Color.black);
        }

        ppText.text = $"PP {move.Pp}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString().ToUpper();

        ppText.color = (move.Pp <= 0 ? Color.red : Color.black);
    }
}
