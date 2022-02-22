using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using Random = UnityEngine.Random;

public enum BattleState
{
    StartBattle,
    ActionSelection,
    MovementSelection,
    PartySelectScreen,
    Busy,
    LoseTurn,
    PerformMovement,
    ItemSelectScreen,
    FinishBattle
}
public class BattleManager : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;

    [SerializeField] private BattleUnit enemyUnit;

    [SerializeField] private BattleDialogueBox battleDialogueBox;

    [SerializeField] private PartyHUD partyHud;

    public BattleState state;

    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 1.0f;

    public event Action<bool> OnBattleFinish;

    private PokemonParty playerParty;
    private Pokemon wildPokemon;

    private int currentSelectedAction;
    private int currentSelectedMovement;
    private int currentSelectedPokemon;

    [SerializeField] private GameObject pokeball;

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

        battleDialogueBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
        
        enemyUnit.SetUpPokemon(wildPokemon);

        partyHud.InitPartyHUD();
        
        yield return battleDialogueBox.SetDialogue($"Un {enemyUnit.Pokemon.Base.Name} salvaje a aparecido.");

        if (enemyUnit.Pokemon.Speed > playerUnit.Pokemon.Speed)
        {
            battleDialogueBox.ToggleDialogueText(true);
            battleDialogueBox.ToggleActions(false);
            battleDialogueBox.ToggleMovements(false);
            yield return battleDialogueBox.SetDialogue("El enemigo ataca primero");
            yield return PerformEnemyMovement();
        }
        else
        {
            PlayerActionSelection();
        }
    }

    void BattleFinish(bool playerHasWon)
    {
        state = BattleState.FinishBattle;
        OnBattleFinish(playerHasWon);
    }

    void PlayerActionSelection()
    {
        state = BattleState.ActionSelection;
        StartCoroutine(battleDialogueBox.SetDialogue("Selecciona una acción"));
        battleDialogueBox.ToggleDialogueText(true);
        battleDialogueBox.ToggleActions(true);
        battleDialogueBox.ToggleMovements(false);
        currentSelectedAction = 0;
        battleDialogueBox.SelectAction(currentSelectedAction);
    }

    void PlayerMovementSelection()
    {
        state = BattleState.MovementSelection;
        battleDialogueBox.ToggleDialogueText(false);
        battleDialogueBox.ToggleActions(false);
        battleDialogueBox.ToggleMovements(true);
        currentSelectedMovement = 0;
        battleDialogueBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
    }
    
    void OpenPartySelectionScreen()
    {
        state = BattleState.PartySelectScreen;
        partyHud.SetPartyData(playerParty.Pokemons);
        partyHud.gameObject.SetActive(true);
        currentSelectedPokemon = playerParty.GetPositionFromPokemon(playerUnit.Pokemon);
        partyHud.UdpdateSelectedPokemon(currentSelectedPokemon);
    }
    
    void OpenInventoryScreen()
    {
        StartCoroutine(ThrowPokeball());
    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;
        if (battleDialogueBox.isWriting)
        {
            return;
        }
        if (state == BattleState.ActionSelection)
        {
            HandlePlayerActionSelection();
        }
        else if (state == BattleState.MovementSelection)
        {
            HandlePlayerMovementSelection();
        }else if (state == BattleState.PartySelectScreen)
        {
            HandlePlayerPartySelection();
        }else if (state == BattleState.LoseTurn)
        {
            StartCoroutine(PerformEnemyMovement());
        }
    }
    
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
                PlayerMovementSelection();
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
            currentSelectedMovement = (currentSelectedMovement + 1) % 2 + 2 * Mathf.FloorToInt(currentSelectedAction / 2);

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
            PlayerActionSelection();
        }
    }
    
    void HandlePlayerPartySelection()
    {
        if (timeSinceLastClick < timeBetweenClicks)
        {
            return;
        }

        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedPokemon -= (int)Input.GetAxisRaw("Vertical") * 2;
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedPokemon += (int)Input.GetAxisRaw("Horizontal");
        }

        currentSelectedPokemon = Mathf.Clamp(currentSelectedPokemon, 0, playerParty.Pokemons.Count - 1);
        partyHud.UdpdateSelectedPokemon(currentSelectedPokemon);

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            var selectedPokemon = playerParty.Pokemons[currentSelectedPokemon];
            if (selectedPokemon.HP <= 0)
            {
                partyHud.SetMessage("No puedes enviar un pokemon debilitado");
                return;
            }else if (selectedPokemon == playerUnit.Pokemon)
            {
                partyHud.SetMessage("No puedes selecionar el pokemon en batalla");
                return;
            }

            partyHud.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedPokemon));
        }

        if (Input.GetAxisRaw("Cancel") != 0)
        {
            partyHud.gameObject.SetActive(false);
            PlayerActionSelection();
        }
    }
    
    IEnumerator PerformPlayerMovement()
    {
        state = BattleState.PerformMovement;
        Move move = playerUnit.Pokemon.Moves[currentSelectedMovement];
        if (move.Pp <= 0)
        {
            PlayerMovementSelection();
            yield break;
        }
        
        yield return RunMovement(playerUnit, enemyUnit, move);

        if (state == BattleState.PerformMovement)
        {
            StartCoroutine(PerformEnemyMovement());
        }
    }
    IEnumerator PerformEnemyMovement()
    {
        state = BattleState.PerformMovement;
        Move move = enemyUnit.Pokemon.RandomMove();

        yield return RunMovement(enemyUnit, playerUnit, move);

        if (state == BattleState.PerformMovement)
        {
            PlayerActionSelection();
        }
    }

    IEnumerator RunMovement(BattleUnit attacker, BattleUnit target, Move move)
    {
        move.Pp--;
        yield return battleDialogueBox.SetDialogue($"{attacker.Pokemon.Base.Name} ha usado {move.Base.Name}");
            
        var oldHPValeu = target.Pokemon.HP;
            
        attacker.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        target.PlayReceiveAttackAnimation();
            
        var damageDescription = target.Pokemon.ReceiveDamage(attacker.Pokemon, move);
        yield return target.Hud.UpdatePokemonData(oldHPValeu);
        yield return ShowDamageDescription(damageDescription);
            
        if (damageDescription.Fainted)
        {
            yield return battleDialogueBox.SetDialogue($"{target.Pokemon.Base.Name} se ha debilitado");
            target.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);
            CheckForBattleFinish(target);
        }
    }

    void CheckForBattleFinish(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayer)
        {
            var nextPokemon = playerParty.GetFirstNonFaintedPokemon();
            if (nextPokemon != null)
            {
                OpenPartySelectionScreen();
            }
            else
            {
                BattleFinish(false);
            }
        }
        else
        {
            BattleFinish(true);
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

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return battleDialogueBox.SetDialogue($"¡Vuelve {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);
        }

        playerUnit.SetUpPokemon(newPokemon);
        battleDialogueBox.SetPokemonMovements(newPokemon.Moves);
        
        yield return battleDialogueBox.SetDialogue($"¡Ve {newPokemon.Base.Name}!");
        StartCoroutine(PerformEnemyMovement());
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;
        yield return battleDialogueBox.SetDialogue($"Has lanzado una {pokeball.name}");

        var pokeballInst = Instantiate(pokeball, playerUnit.transform.position + 
                                                 new Vector3(-2, 0), Quaternion.identity);

        var pokeballSpt = pokeballInst.GetComponent<SpriteRenderer>();
        yield return pokeballSpt.transform.DOLocalJump(enemyUnit.transform.position + 
                                          new Vector3(0, 1.5f), 2f, 1, 1f).WaitForCompletion();

        yield return enemyUnit.PlayCapturedAnimation();
        yield return pokeballSpt.transform.DOLocalMoveY(enemyUnit.transform.position.y - 1.5f, 1f).WaitForCompletion();

        var numberOfShakes = TryToCatchPokemon(enemyUnit.Pokemon);
        for (int i = 0; i < Mathf.Min(numberOfShakes, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeballSpt.transform.DOPunchRotation(new Vector3(0, 0, 15), 0.6f).WaitForCompletion();
        }

        if (numberOfShakes == 4)
        {
            yield return battleDialogueBox.SetDialogue($"¡{enemyUnit.Pokemon.Base.Name} capturado!");
            yield return pokeballSpt.DOFade(0, 1.5f).WaitForCompletion();
            
            Destroy(pokeballInst);
            BattleFinish(true);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            pokeballSpt.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();
            if (numberOfShakes < 2)
            {
                yield return battleDialogueBox.SetDialogue($"¡{enemyUnit.Pokemon.Base.Name} ha escapado!");
            }
            else
            {
                yield return battleDialogueBox.SetDialogue("Casi lo has atrapado");
            }
            Destroy(pokeballInst);
            state = BattleState.LoseTurn;
        }
    }

    int TryToCatchPokemon(Pokemon pokemon)
    {
        float bonusPokeball = 1;
        float bonusStat = 1;
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * bonusPokeball * bonusStat/(3*pokemon.MaxHp);
        if (a >= 255)
        {
            return 4;
        }

        float b = 1048560/Mathf.Sqrt(Mathf.Sqrt(16711680/a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (Random.Range(0, 65535) >= b)
            {
                break;
            }
            else
            {
                shakeCount++;
            }
        }

        return shakeCount;
    }
}
