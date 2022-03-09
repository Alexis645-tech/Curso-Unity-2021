using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    ForgetMovement,
    FinishBattle
}

public enum BattleTYpe
{
    WildPOkemon,
    Trainer,
    Leader
}
public class BattleManager : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;

    [SerializeField] private BattleUnit enemyUnit;

    [SerializeField] private BattleDialogueBox battleDialogueBox;

    [SerializeField] private PartyHUD partyHud;

    [SerializeField] private SelectionMovementUI selectMoveUI;

    public BattleState state;
    public BattleTYpe battleTYpe;

    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 1.0f;

    public event Action<bool> OnBattleFinish;

    private PokemonParty playerParty;
    private Pokemon wildPokemon;

    private int currentSelectedAction;
    private int currentSelectedMovement;
    private int currentSelectedPokemon;
    
    private int scapeAttempts;
    private MoveBase moveToLearn;

    [SerializeField] private GameObject pokeball;

    public AudioClip attackClip, damageClip, levelUpClip, endBattleClip, faintedClip, pokeballClip;

    public void HandleStartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        battleTYpe = BattleTYpe.WildPOkemon;
        scapeAttempts = 0;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    } 

    public void HandleStartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, bool isLeader)
    {
        battleTYpe = (isLeader ? BattleTYpe.Leader : BattleTYpe.Trainer);
        //TODO: El resto de batalla contra NPC
    }

    public IEnumerator SetupBattle()
    {
        state = BattleState.StartBattle;
        playerUnit.SetUpPokemon(playerParty.GetFirstNonFaintedPokemon());

        battleDialogueBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
        
        enemyUnit.SetUpPokemon(wildPokemon);

        partyHud.InitPartyHUD();
        
        yield return battleDialogueBox.SetDialogue($"Un {enemyUnit.Pokemon.Base.Name} salvaje a aparecido.");

        yield return ChooseFirstTurn(true);
    }

    IEnumerator ChooseFirstTurn(bool showFirstMsg = false)
    {
        if (enemyUnit.Pokemon.Speed > playerUnit.Pokemon.Speed)
        {
            battleDialogueBox.ToggleDialogueText(true);
            battleDialogueBox.ToggleActions(false);
            battleDialogueBox.ToggleMovements(false);
            if (showFirstMsg)
            {
                yield return battleDialogueBox.SetDialogue("El enemigo ataca primero");
            }

            yield return PerformEnemyMovement();
        }
        else
        {
           PlayerActionSelection(); 
        }
    }

    void BattleFinish(bool playerHasWon)
    {
        SoundManager.SharedInstance.PlaySound(endBattleClip);
        state = BattleState.FinishBattle;
        playerParty.Pokemons.ForEach(p => p.OnBattleFinish());
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
        battleDialogueBox.ToggleActions(false);
        StartCoroutine(ThrowPokeball());
    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;
        if (timeSinceLastClick < timeBetweenClicks || battleDialogueBox.isWriting)
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
        }else if (state == BattleState.ForgetMovement)
        {
            selectMoveUI.HandleForgetMoveSelection((moveIndex) =>
            {
                if (moveIndex < 0)
                {
                    timeSinceLastClick = 0;
                    return;
                }

                StartCoroutine(ForgetOldMove(moveIndex));
            });
        }
    }

    IEnumerator ForgetOldMove(int moveIndex)
    {
        selectMoveUI.gameObject.SetActive(false);
        if (moveIndex == PokemonBase.NUMBER_OF_LEARNABLE_MOVES)
        {
            //No aprendo el nuevo movimiento
           yield return battleDialogueBox.SetDialogue($"{playerUnit.Pokemon.Base.Name} no ha aprendido {moveToLearn.Name}");
        }
        else
        {
            //Olvido el seleccionado y aprendo el nuevo
            var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
            yield return battleDialogueBox.SetDialogue($"{playerUnit.Pokemon.Base.Name} olvidó {selectedMove.Name} y aprendió {moveToLearn.Name}");
            playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        //TODO: Revisar cuando haya entrenadores
        state = BattleState.FinishBattle;
    }
    
    void HandlePlayerActionSelection()
    {
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
                StartCoroutine(TryToScapeFromBattle());
            }
        }
    }
    
    void HandlePlayerMovementSelection()
    {
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
        bool canRunMovement = attacker.Pokemon.OnStartTurn();
        if (!canRunMovement)
        {
            yield return ShowStatsMessages(attacker.Pokemon);
            yield break;
        }
        yield return ShowStatsMessages(attacker.Pokemon);
        
        move.Pp--;
        yield return battleDialogueBox.SetDialogue($"{attacker.Pokemon.Base.Name} ha usado {move.Base.Name}");

        yield return RunMoveAnims(attacker, target);

        if (move.Base.MoveType == MoveType.Stats)
        {
            yield return RunMoveStats(attacker.Pokemon, target.Pokemon, move);
        }
        else
        {
            var damageDescription = target.Pokemon.ReceiveDamage(attacker.Pokemon, move);
            yield return target.Hud.UpdatePokemonData();
            yield return ShowDamageDescription(damageDescription);
        }

        if (target.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(target);
        }

        //Comprobar estados alterados como quemadura o envenenamiento a final de turno
        attacker.Pokemon.OnFinishTurn();
        yield return ShowStatsMessages(attacker.Pokemon);
        yield return attacker.Hud.UpdatePokemonData();
        if (attacker.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(attacker);
        }
    }

    IEnumerator RunMoveAnims(BattleUnit attacker, BattleUnit target)
    {
        attacker.PlayAttackAnimation();
        SoundManager.SharedInstance.PlaySound(attackClip);
        yield return new WaitForSeconds(1f);
        target.PlayReceiveAttackAnimation();
        SoundManager.SharedInstance.PlaySound(damageClip);
        yield return new WaitForSeconds(1f);
    }
    IEnumerator RunMoveStats(Pokemon attacker, Pokemon target, Move move)
    {
        foreach (var boost in move.Base.Effects.Boostings)
        {
            if (boost.target == MoveTarget.Self)
            {
                attacker.ApplyBoost(boost);
            }
            else
            {
                target.ApplyBoost(boost);
            }
        }

        if (move.Base.Effects.Status != StatusConditionID.none)
        {
            if (move.Base.Target == MoveTarget.Other)
            {
                target.SetConditionStatus(move.Base.Effects.Status);
            }
            else
            {
                attacker.SetConditionStatus(move.Base.Effects.Status);
            }
        }

        yield return ShowStatsMessages(attacker);
        yield return ShowStatsMessages(target); 
    }

    IEnumerator ShowStatsMessages(Pokemon pokemon)
    {
        while (pokemon.StatusChangeMessages.Count > 0)
        {
            var message = pokemon.StatusChangeMessages.Dequeue();
            yield return battleDialogueBox.SetDialogue(message);
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
        bool currentPokemonFainted = true;
        if (playerUnit.Pokemon.HP > 0)
        {
            currentPokemonFainted = false;
            yield return battleDialogueBox.SetDialogue($"¡Vuelve {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);
        }

        playerUnit.SetUpPokemon(newPokemon);
        battleDialogueBox.SetPokemonMovements(newPokemon.Moves);
        
        yield return battleDialogueBox.SetDialogue($"¡Ve {newPokemon.Base.Name}!");
        if (currentPokemonFainted)
        {
            yield return ChooseFirstTurn();
        }
        else
        {
            yield return PerformEnemyMovement();
        }
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (battleTYpe != BattleTYpe.WildPOkemon)
        {
            yield return battleDialogueBox.SetDialogue("No puedes robar los pokemons de otros entrenadores");
            state = BattleState.LoseTurn;
            yield break;
        }
        yield return battleDialogueBox.SetDialogue($"Has lanzado una {pokeball.name}");

        SoundManager.SharedInstance.PlaySound(pokeballClip);
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
            
            if (playerParty.AddPokemonToParty(enemyUnit.Pokemon))
            {
                yield return battleDialogueBox.SetDialogue($"{enemyUnit.Pokemon.Base.Name} se ha añadido a tu equipo.");
            }
            else
            {
                yield return battleDialogueBox.SetDialogue("En algun momento, lo enviaremos al pc de bill...");
            }
            
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
    
    IEnumerator TryToScapeFromBattle()
    {
        state = BattleState.Busy;
        if (battleTYpe != BattleTYpe.WildPOkemon)
        {
            yield return battleDialogueBox.SetDialogue("No puedes huir de combates contra entrenadores pokemons");
            state = BattleState.LoseTurn;
            yield break;
        }
        
        //Es contra un pokemon salvaje
        scapeAttempts++;
        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (playerSpeed <= enemySpeed)
        {
            yield return battleDialogueBox.SetDialogue("Has escapado con éxito");
            yield return new WaitForSeconds(1f);
            OnBattleFinish(true);
        }
        else
        {
            float oddsScape = (Mathf.FloorToInt(playerSpeed * 128 / enemySpeed) + 30 * scapeAttempts) % 256;
            if (Random.Range(0, 256) < oddsScape)
            {
                yield return battleDialogueBox.SetDialogue("Has escapado con éxito");
                yield return new WaitForSeconds(1f);
                OnBattleFinish(true);
            }
            else
            {
                yield return battleDialogueBox.SetDialogue("No puedes escapar");
                state = BattleState.LoseTurn;
            }
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return battleDialogueBox.SetDialogue($"{faintedUnit.Pokemon.Base.Name} se ha debilitado");
        SoundManager.SharedInstance.PlaySound(faintedClip);
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(1.5f);

        if (!faintedUnit.IsPlayer)
        {
            int expBase = faintedUnit.Pokemon.Base.ExpBase;
            int level = faintedUnit.Pokemon.Level;
            float multiplier = (battleTYpe == BattleTYpe.WildPOkemon ? 1 : 1.5f);
            int wonExp = Mathf.FloorToInt(expBase * level * multiplier / 7);

            playerUnit.Pokemon.Experience *= wonExp;
            yield return battleDialogueBox.SetDialogue($"{playerUnit.Pokemon.Base.Name} ha ganado {wonExp} puntos de experiencia");
            yield return playerUnit.Hud.SetExpSmooth();
            yield return new WaitForSeconds(1f);
            
            //Check new level
            while (playerUnit.Pokemon.NeedsToLevelUp())
            {
                SoundManager.SharedInstance.PlaySound(levelUpClip);
                playerUnit.Hud.SetLevelText();
                playerUnit.Pokemon.HasHpChanged = true;
                yield return playerUnit.Hud.UpdatePokemonData();
                yield return new WaitForSeconds(1);
                yield return battleDialogueBox.SetDialogue($"¡{playerUnit.Pokemon.Base.Name} sube de nivel!");

                //Intentar aprender un nuevo movimiento
                var newLearnableMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
                if (newLearnableMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.NUMBER_OF_LEARNABLE_MOVES)
                    {
                        playerUnit.Pokemon.LearnMove(newLearnableMove);
                        yield return battleDialogueBox.SetDialogue($"{playerUnit.Pokemon.Base.Name} ha aprendido {newLearnableMove.Move.Name}");
                        battleDialogueBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return battleDialogueBox.SetDialogue($"{playerUnit.Pokemon.Base.Name} intenta aprender {newLearnableMove.Move.Name}");
                        yield return battleDialogueBox.SetDialogue($"Pero no puede aprender mas de {PokemonBase.NUMBER_OF_LEARNABLE_MOVES} movimientos");
                        yield return ChooseMovementToForget(playerUnit.Pokemon, newLearnableMove.Move);
                        yield return new WaitUntil(() => state != BattleState.ForgetMovement);
                    }
                }
                yield return playerUnit.Hud.SetExpSmooth(true);
            }
        }
        
        CheckForBattleFinish(faintedUnit);
    }

    IEnumerator ChooseMovementToForget(Pokemon learner, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return battleDialogueBox.SetDialogue("Selecciona el movimiento que quieres olvidar");
        selectMoveUI.gameObject.SetActive(true);
        selectMoveUI.SetMovements(learner.Moves.Select(mv => mv.Base).ToList(), newMove);
        moveToLearn = newMove;
        state = BattleState.ForgetMovement;
    }
}
