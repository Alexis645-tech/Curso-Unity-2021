using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMovementUI : MonoBehaviour
{
    [SerializeField] private Text[] movementTexts;
    private int currentSelectedMovement = 0;
    [SerializeField] private Color selectedColor;

    public void SetMovements(List<MoveBase> pokemonMoves, MoveBase newMove)
    {
        currentSelectedMovement = 0;
        
        for (int i = 0; i < pokemonMoves.Count; i++)
        {
            movementTexts[i].text = pokemonMoves[i].Name;
        }

        movementTexts[pokemonMoves.Count].text = newMove.Name;
    }

    public void HandleForgetMoveSelection()
    {
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            int direction = Mathf.FloorToInt(Input.GetAxisRaw("Vertical"));
            currentSelectedMovement -= direction;
            currentSelectedMovement = Mathf.Clamp(currentSelectedMovement, 0, PokemonBase.NUMBER_OF_LEARNABLE_MOVES);
            UpdateColorForgetMoveSelection();
        }
    }

    public void UpdateColorForgetMoveSelection()
    {
        for (int i = 0; i <= PokemonBase.NUMBER_OF_LEARNABLE_MOVES; i++)
        {
            movementTexts[i].color = (i == currentSelectedMovement ? selectedColor : Color.black);
        }
    }
}
