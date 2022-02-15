using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public enum BattleState
{
    StartBattle,
    PlayerSelectAction,
    PlayerMove,
    EnemyMove,
    Busy
}
public class BattleManager : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleHUD playerHUD;
    
    [SerializeField] private BattleUnit enemyUnit;
    [SerializeField] private BattleHUD enemyHUD;

    [SerializeField] private BattleDialogueBox battleDialogueBox;

    public BattleState state;

    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 1.0f; 

    private void Start()
    {
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        state = BattleState.StartBattle;
        playerUnit.SetUpPokemon();
        playerHUD.SetPokemonData(playerUnit.Pokemon);
        
        battleDialogueBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
        
        enemyUnit.SetUpPokemon();
        enemyHUD.SetPokemonData(enemyUnit.Pokemon);
        
        yield return battleDialogueBox.SetDialogue($"Un {enemyUnit.Pokemon.Base.Name} salvaje a aparecido.");

        if (enemyUnit.Pokemon.Speed > playerUnit.Pokemon.Speed)
        {
            StartCoroutine(battleDialogueBox.SetDialogue("El enemigo ataca primero"));
            EnemyAction();
        }
        else
        {
            PlayerAction();
        }
    }

    void PlayerAction()
    {
        state = BattleState.PlayerSelectAction;
        StartCoroutine(battleDialogueBox.SetDialogue("Selecciona una acción"));
        battleDialogueBox.ToggleDialogueText(true);
        battleDialogueBox.ToggleActions(true);
        battleDialogueBox.ToggleMovements(false);
        currentSelectedAction = 0;
        battleDialogueBox.SelectAction(currentSelectedAction);
    }

    void PlayerMovement()
    {
        state = BattleState.PlayerMove;
        battleDialogueBox.ToggleDialogueText(false);
        battleDialogueBox.ToggleActions(false);
        battleDialogueBox.ToggleMovements(true);
        currentSelectedMovement = 0;
        battleDialogueBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
    }

    private void Update()
    {
        timeSinceLastClick += Time.deltaTime;
        if (battleDialogueBox.isWriting)
        {
            return;
        }
        if (state == BattleState.PlayerSelectAction)
        {
            HandlePlayerActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandlePlayerMovementSelection();
        }
    }

    private int currentSelectedAction;
    void HandlePlayerActionSelection()
    {
        if (timeSinceLastClick < timeBetweenClicks)
        {
            return;
        }
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedAction = (currentSelectedAction + 1) % 2;
            battleDialogueBox.SelectAction(currentSelectedAction);
        }

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            if (currentSelectedAction == 0)
            {
                PlayerMovement();
            }else if (currentSelectedAction == 1)
            {
                //TODO: implementar la huida
            }
        }
    }

    private int currentSelectedMovement;
    void HandlePlayerMovementSelection()
    {
        if (timeSinceLastClick < timeBetweenClicks)
        {
            return;
        }

        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            var oldSelectedMovement = currentSelectedMovement;
            currentSelectedMovement = (currentSelectedMovement + 2) % 4;
            if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count)
            {
                currentSelectedMovement = oldSelectedMovement;
            }
            battleDialogueBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            var oldSelectedMovement = currentSelectedMovement;
            if (currentSelectedMovement <= 1)
            {
                currentSelectedMovement = (currentSelectedMovement + 1) % 2;
            }
            else
            {
                currentSelectedMovement = (currentSelectedMovement + 1) % 2 + 2;
            }

            if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count)
            {
                currentSelectedMovement = oldSelectedMovement;
            }
            battleDialogueBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
        }

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            battleDialogueBox.ToggleMovements(false);
            battleDialogueBox.ToggleDialogueText(true);
            StartCoroutine(PerformPlayerMovement());
        }

        IEnumerator PerformPlayerMovement()
        {
            Move move = playerUnit.Pokemon.Moves[currentSelectedMovement];
            yield return battleDialogueBox.SetDialogue($"{playerUnit.Pokemon.Base.Name} ha usado {move.Base.Name}");
            
            var oldHPValeu = enemyUnit.Pokemon.HP;
            
            playerUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            enemyUnit.PlayReceiveAttackAnimation();
            
            bool pokemonFainted = enemyUnit.Pokemon.ReceiveDamage(playerUnit.Pokemon, move);
            enemyHUD.UpdatePokemonData(oldHPValeu);
            if (pokemonFainted)
            {
                yield return battleDialogueBox.SetDialogue($"{enemyUnit.Pokemon.Base.Name} se ha debilitado");
                enemyUnit.PlayFaintAnimation();
            }
            else
            {
                StartCoroutine(EnemyAction());
            }
        }
    }
    IEnumerator EnemyAction()
    {
        state = BattleState.EnemyMove;
        Move move = enemyUnit.Pokemon.RandomMove();
        yield return battleDialogueBox.SetDialogue($"{enemyUnit.Pokemon.Base.Name} ha usado {move.Base.Name}");
        
        var oldHPValeu = playerUnit.Pokemon.HP;
        
        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        playerUnit.PlayReceiveAttackAnimation();
        
        bool pokemonFainted = playerUnit.Pokemon.ReceiveDamage(enemyUnit.Pokemon, move);
        playerHUD.UpdatePokemonData(oldHPValeu);
        if (pokemonFainted)
        {
            yield return battleDialogueBox.SetDialogue($"{playerUnit.Pokemon.Base.Name} ha sido debilitado");
            playerUnit.PlayFaintAnimation();
        }
        else
        {
            PlayerAction();
        }
    }
}
