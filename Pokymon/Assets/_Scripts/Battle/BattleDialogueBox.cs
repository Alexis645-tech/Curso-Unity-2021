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
    
    [SerializeField] GameObject yesNoBox;
    
    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> movementTexts;

    [SerializeField] private Text ppText;
    [SerializeField] private Text typeText;
    [SerializeField] private Text yesText, noText;

    public float timeToWaitAfterText = 1f;
    
    public float charactersPerSecond;
    public bool isWriting;

    public IEnumerator SetDialogue(string message)
    {
        isWriting = true;
        dialogueText.text = "";
        foreach (var character in message)
        {
            if (character != ' ')
            {
                SoundManager.SharedInstance.PlayRandomCharacterSound();
            }
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

    public void ToggleYesNoBox(bool activated)
    {
        yesNoBox.SetActive(activated);
    }
    public void SelectAction(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            actionTexts[i].color = (i == selectedAction ? ColorManager.SharedInstance.selectedColor : ColorManager.SharedInstance.defaultColor);
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
            movementTexts[i].color = (i == selectedMovement ? ColorManager.SharedInstance.selectedColor : ColorManager.SharedInstance.defaultColor);
        }

        ppText.text = $"PP {move.Pp}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString().ToUpper();

        ppText.color = ColorManager.SharedInstance.PpColor((float)move.Pp/move.Base.PP);
    }
    public void SelectYesNoAction(bool yesSelected)
    {
        if (yesSelected)
        {
            yesText.color = ColorManager.SharedInstance.selectedColor;
            noText.color = ColorManager.SharedInstance.defaultColor;
        }
        else
        {
            yesText.color = ColorManager.SharedInstance.defaultColor;
            noText.color = ColorManager.SharedInstance.selectedColor;
        }
    }
}
