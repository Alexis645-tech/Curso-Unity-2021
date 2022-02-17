using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public enum BattleState
{
    StartBattle,
    PlayerSelectAction,
    PlayerSelectMove,
    EnemyMove,
    PartySelectScreen,
    Busy
}
public class BattleManager : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleHUD playerHUD;
    
    [SerializeField] private BattleUnit enemyUnit;
    [SerializeField] private BattleHUD enemyHUD;

    [SerializeField] private BattleDialogueBox battleDialogueBox;

    [SerializeField] private PartyHUD partyHud;

    public BattleState state;

    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 1.0f;

    public event Action<bool> OnBattleFinish;

    private PokemonParty playerParty;
    private Pokemon wildPokemon;

    public void HandleStartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        state = BattleState.StartBattle;
        playerUnit.SetUpPokemon(playerParty.GetFirstNonFaintedPokemon());
        playerHUD.SetPokemonData(playerUnit.Pokemon);
        
        battleDialogueBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
        
        enemyUnit.SetUpPokemon(wildPokemon);
        enemyHUD.SetPokemonData(enemyUnit.Pokemon);
        
        partyHud.InitPartyHUD();
        
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
        state = BattleState.PlayerSelectMove;
        battleDialogueBox.ToggleDialogueText(false);
        battleDialogueBox.ToggleActions(false);
        battleDialogueBox.ToggleMovements(true);
        currentSelectedMovement = 0;
        battleDialogueBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
    }

    public void HandleUpdate()
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
        else if (state == BattleState.PlayerSelectMove)
        {
            HandlePlayerMovementSelection();
        }else if (state == BattleState.PartySelectScreen)
        {
            HandlePlayerPartySelection();
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
            currentSelectedAction = (currentSelectedAction + 2) % 4;
            battleDialogueBox.SelectAction(currentSelectedAction);
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedAction = (currentSelectedAction + 1) % 2 + 2 * Mathf.FloorToInt(currentSelectedAction / 2);
            battleDialogueBox.SelectAction(currentSelectedAction);
        }

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            if (currentSelectedAction == 0)
            {
                //Luchar
                PlayerMovement();
            }else if (currentSelectedAction == 1)
            {
                //Pokemon
                OpenPartySelectionScreen();
            }else if (currentSelectedAction == 2)
            {
                //Mochila
                OpenInventoryScreen();
            }else if (currentSelectedAction == 3)
            {
                //Huir
                OnBattleFinish(false);
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
            var oldSelectedMovement = (currentSelectedMovement + 1) % 2 + 2 * Mathf.FloorToInt(currentSelectedAction / 2);

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

        if (Input.GetAxisRaw("Cancel") != 0)
        {
            PlayerAction();
        }
    }
    
    IEnumerator PerformPlayerMovement()
    {
        Move move = playerUnit.Pokemon.Moves[currentSelectedMovement];
        move.Pp--;
        yield return battleDialogueBox.SetDialogue($"{playerUnit.Pokemon.Base.Name} ha usado {move.Base.Name}");
            
        var oldHPValeu = enemyUnit.Pokemon.HP;
            
        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        enemyUnit.PlayReceiveAttackAnimation();
            
        var damageDescription = enemyUnit.Pokemon.ReceiveDamage(playerUnit.Pokemon, move);
        enemyHUD.UpdatePokemonData(oldHPValeu);
        yield return ShowDamageDescription(damageDescription);
            
        if (damageDescription.Fainted)
        {
            yield return battleDialogueBox.SetDialogue($"{enemyUnit.Pokemon.Base.Name} se ha debilitado");
            enemyUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);
            OnBattleFinish(true);
        }
        else
        {
            StartCoroutine(EnemyAction());
        }
    }
    IEnumerator EnemyAction()
    {
        state = BattleState.EnemyMove;
        Move move = enemyUnit.Pokemon.RandomMove();
        move.Pp--;
        yield return battleDialogueBox.SetDialogue($"{enemyUnit.Pokemon.Base.Name} ha usado {move.Base.Name}");
        
        var oldHPValeu = playerUnit.Pokemon.HP;
        
        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        playerUnit.PlayReceiveAttackAnimation();
        
        var damageDescription = playerUnit.Pokemon.ReceiveDamage(enemyUnit.Pokemon, move);
        playerHUD.UpdatePokemonData(oldHPValeu);
        yield return ShowDamageDescription(damageDescription);
        
        if (damageDescription.Fainted)
        {
            yield return battleDialogueBox.SetDialogue($"{playerUnit.Pokemon.Base.Name} ha sido debilitado");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);

            var nextPokemon = playerParty.GetFirstNonFaintedPokemon();
            if (nextPokemon == null)
            {
                OnBattleFinish(false);
            }
            else
            {
                playerUnit.SetUpPokemon(nextPokemon);
                playerHUD.SetPokemonData(nextPokemon);
                battleDialogueBox.SetPokemonMovements(nextPokemon.Moves);

                yield return battleDialogueBox.SetDialogue($"¡Adelante {nextPokemon.Base.Name}!");
                PlayerAction();
            }
        }
        else
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDescription(DamageDescription description)
    {
        if (description.Critical > 1f)
        {
            yield return battleDialogueBox.SetDialogue("¡Un golpe critico!");
        }

        if (description.Type > 1)
        {
            yield return battleDialogueBox.SetDialogue("¡Es super efectivo!");
        }else if (description.Type < 1)
        {
            yield return battleDialogueBox.SetDialogue("No es muy efectivo...");
        }
    }
    
    void OpenPartySelectionScreen()
    {
        partyHud.SetPartyData(playerParty.Pokemons);
        partyHud.gameObject.SetActive(true);
    }

    void OpenInventoryScreen()
    {
        if (Input.GetAxisRaw("Cancel") != 0)
        {
            PlayerAction();
        }
    }

    private int currentSelectedPokemon;
    void HandlePlayerPartySelection()
    {
        if (timeSinceLastClick < timeBetweenClicks)
        {
            return;
        }

        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            var oldSelectedPokemon = currentSelectedPokemon;
            currentSelectedPokemon = (currentSelectedPokemon + 2) % 6;
            if (currentSelectedPokemon >= playerParty.Pokemons.Count)
            {
                currentSelectedPokemon = oldSelectedPokemon;
            }
            partyHud.UdpdateSelectedPokemon(currentSelectedPokemon);
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            var oldSelectedPokemon = (currentSelectedPokemon + 1) % 2 + 2 * Mathf.FloorToInt(currentSelectedPokemon / 2);

            if (currentSelectedPokemon >= playerParty.Pokemons.Count)
            {
                currentSelectedPokemon = oldSelectedPokemon;
            }
            partyHud.UdpdateSelectedPokemon(currentSelectedPokemon);
        }

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
        }

        if (Input.GetAxisRaw("Cancel") != 0)
        {
            PlayerAction();
        }
    }
}
