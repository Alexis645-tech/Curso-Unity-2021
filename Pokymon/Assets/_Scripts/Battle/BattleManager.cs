using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleHUD playerHUD;
    
    [SerializeField] private BattleUnit enemyUnit;
    [SerializeField] private BattleHUD enemyHUD;

    [SerializeField] private BattleDialogueBox battleDialogueBox;

    private void Start()
    {
        SetupBattle();
    }

    public void SetupBattle()
    {
        playerUnit.SetUpPokemon();
        playerHUD.SetPokemonData(playerUnit.Pokemon);
        
        enemyUnit.SetUpPokemon();
        enemyHUD.SetPokemonData(enemyUnit.Pokemon);
        
        StartCoroutine(battleDialogueBox.SetDialogue($"Un {enemyUnit.Pokemon.Base.Name} salvaje a aparecido."));
    }
}
