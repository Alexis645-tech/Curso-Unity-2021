using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public enum BattleState
{
    StartBattle,
    ActionSelection,
    MovementSelection,
    PartySelectScreen,
    Busy,
    YesNoChoice,
    RunTurn,
    ItemSelectScreen,
    ForgetMovement,
    FinishBattle
}

public enum BattleAction
{
    Move, SwitchPokemon, UseItem, Run
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

    [SerializeField] private Image playerImage, trainerImage;
    
    public BattleState state;
    public BattleState? previousState;
    public BattleTYpe battleTYpe;

    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 1.0f;

    public event Action<bool> OnBattleFinish;

    private PokemonParty playerParty;
    private PokemonParty trainerParty;
    private Pokemon wildPokemon;

    private int currentSelectedAction;
    private int currentSelectedMovement;
    private int currentSelectedPokemon;
    private bool currentSelectedChoice = true;
    
    private int scapeAttempts;
    private MoveBase moveToLearn;

    [SerializeField] private GameObject pokeball;

    public AudioClip attackClip, damageClip, levelUpClip, endBattleClip, faintedClip, pokeballClip;

    private PlayerController player;
    private TrainerController trainer;
    public void HandleStartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        battleTYpe = BattleTYpe.WildPOkemon;
        scapeAttempts = 0;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    } 

    public void HandleStartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, bool isLeader = false)
    {
        battleTYpe = (isLeader ? BattleTYpe.Leader : BattleTYpe.Trainer);
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        state = BattleState.StartBattle;
        playerUnit.ClearHud();
        enemyUnit.ClearHud();
        
        if (battleTYpe == BattleTYpe.WildPOkemon)
        {
            playerUnit.SetUpPokemon(playerParty.GetFirstNonFaintedPokemon());

            battleDialogueBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
        
            enemyUnit.SetUpPokemon(wildPokemon);

            yield return battleDialogueBox.SetDialogue($"Un {enemyUnit.Pokemon.Base.Name} salvaje a aparecido.");
        }
        else //Entrenador y Lider
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            var playerInitialPosition = playerImage.transform.localPosition;
            playerImage.transform.localPosition = playerInitialPosition - new Vector3(400f, 0, 0);
            playerImage.transform.DOLocalMoveX(playerInitialPosition.x, 0.5f);
            
            var trainerInitialPosition = trainerImage.transform.localPosition;
            trainerImage.transform.localPosition = trainerInitialPosition - new Vector3(400f, 0, 0);
            trainerImage.transform.DOLocalMoveX(trainerInitialPosition.x, 0.5f);
            
            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.TrainerSprite;
            trainerImage.sprite = trainer.TrainerSprite;

            yield return battleDialogueBox.SetDialogue($"¡{trainer.TrainerName} quiere luchar!");
            
            //Enviar el primer pokemon del entrenador
            yield return trainerImage.transform.DOLocalMoveX(trainerImage.transform.localPosition.x + 400, 0.5f).WaitForCompletion();
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetFirstNonFaintedPokemon();
            enemyUnit.SetUpPokemon(enemyPokemon);
            yield return battleDialogueBox.SetDialogue($"{trainer.TrainerName} ha enviado a {enemyPokemon.Base.Name}");
            trainerImage.transform.localPosition = trainerInitialPosition;
            
            //Enviar el primer pokemon del jugador
            yield return playerImage.transform.DOLocalMoveX(playerImage.transform.localPosition.x - 400, 0.5f).WaitForCompletion();
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetFirstNonFaintedPokemon();
            playerUnit.SetUpPokemon(playerPokemon);
            battleDialogueBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
            yield return battleDialogueBox.SetDialogue($"Ve {playerPokemon.Base.Name}");
            playerImage.transform.localPosition = playerInitialPosition;
        }
        partyHud.InitPartyHUD();
        
        PlayerActionSelection();
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

    IEnumerator YesNoChoice(Pokemon newTrainerPokemon)
    {
        state = BattleState.Busy;
        yield return battleDialogueBox.SetDialogue($"{trainer.TrainerName} va a sacar a {newTrainerPokemon.Base.Name}. ¿Quieres cambiar tu pokemon?");
        state = BattleState.YesNoChoice;
        battleDialogueBox.ToggleYesNoBox(true);
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
        }else if (state == BattleState.YesNoChoice)
        {
            HandleYesNoChoice();
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
                //Cambiar Pokemon
                previousState = state;
                OpenPartySelectionScreen();
            }else if (currentSelectedAction == 2)
            {
                //Mochila
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }else if (currentSelectedAction == 3)
            {
                //Huir
                StartCoroutine(RunTurns(BattleAction.Run));
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
            StartCoroutine(RunTurns(BattleAction.Move));
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
            battleDialogueBox.ToggleActions(false);
            if (previousState == BattleState.ActionSelection)
            {
                previousState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedPokemon));
            }
        }

        if (Input.GetAxisRaw("Cancel") != 0)
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyHud.SetMessage("Tienes que seleccionar un pokemon para continuar...");
                return;
            }
            partyHud.gameObject.SetActive(false);
            if (previousState == BattleState.YesNoChoice)
            {
                previousState = null;
                StartCoroutine(SendNextTrainerPokemonToBattle());
            }
            else
            {
                PlayerActionSelection();
            }
        }
    }

    void HandleYesNoChoice()
    {
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedChoice = !currentSelectedChoice;
        }
        battleDialogueBox.SelectYesNoAction(currentSelectedChoice);

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            battleDialogueBox.ToggleYesNoBox(false);
            if (currentSelectedChoice)
            {
                previousState = BattleState.YesNoChoice;
                OpenPartySelectionScreen();
            }
            else
            {
                StartCoroutine(SendNextTrainerPokemonToBattle());
            }
        }

        if (Input.GetAxisRaw("Cancel") != 0)
        {
            timeSinceLastClick = 0;
            battleDialogueBox.ToggleYesNoBox(false);
            StartCoroutine(SendNextTrainerPokemonToBattle());
        }
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunTurn;
        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentSelectedMovement];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.RandomMove();
            bool playerGoesFirst = true;
            int enemyPriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;
            int playerPriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            if (enemyPriority > playerPriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyPriority == playerPriority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst ? playerUnit : enemyUnit);
            var secondUnit = (playerGoesFirst ? enemyUnit : playerUnit);

            var secondPokemon = secondUnit.Pokemon;
            //Primer turno
            yield return RunMovement(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.FinishBattle)
            {
                yield break;
            }

            if (secondPokemon.HP > 0)
            {
                //Segundo turno
                yield return RunMovement(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.FinishBattle)
                {
                    yield break;
                }
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemons[currentSelectedPokemon];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }else if (playerAction == BattleAction.UseItem)
            {
                battleDialogueBox.ToggleActions(false);
                yield return ThrowPokeball();
            }else if (playerAction == BattleAction.Run)
            {
                yield return TryToScapeFromBattle();
            }
            //Turno del enemigo
            var enemyMove = enemyUnit.Pokemon.RandomMove();
            yield return RunMovement(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.FinishBattle)
            {
                yield break;
            }
        }

        if (state != BattleState.FinishBattle)
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
            yield return attacker.Hud.UpdatePokemonData();
            yield break;
        }
        yield return ShowStatsMessages(attacker.Pokemon);
        
        move.Pp--;
        yield return battleDialogueBox.SetDialogue($"{attacker.Pokemon.Base.Name} ha usado {move.Base.Name}");

        if (MoveHits(move, attacker.Pokemon, target.Pokemon))
        {
            yield return RunMoveAnims(attacker, target);

            if (move.Base.MoveType == MoveType.Stats)
            {
                yield return RunMoveStats(attacker.Pokemon, target.Pokemon, move.Base.Effects, move.Base.Target);
            }
            else
            {
                var damageDescription = target.Pokemon.ReceiveDamage(attacker.Pokemon, move);
                yield return target.Hud.UpdatePokemonData();
                yield return ShowDamageDescription(damageDescription);
            }
            //Chequear posibles efectos secundarios
            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0)
            {
                foreach (var sec in move.Base.SecondaryEffects)
                {
                    if ((sec.Target == MoveTarget.Other && target.Pokemon.HP > 0) || 
                        (sec.Target == MoveTarget.Self && attacker.Pokemon.HP > 0))
                    {
                        var rnd = Random.Range(0, 100);
                        if (rnd < sec.Chance)
                        {
                            yield return RunMoveStats(attacker.Pokemon, target.Pokemon, sec, sec.Target);
                        }
                    }
                }
            }

            if (target.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(target);
            }
        }
        else
        {
            yield return battleDialogueBox.SetDialogue($"El ataque de {attacker.Pokemon.Base.Name} ha fallado");
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
    IEnumerator RunMoveStats(Pokemon attacker, Pokemon target, MoveStatEffect effect, MoveTarget moveTarget)
    {
        foreach (var boost in effect.Boostings)
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
        //Status Condition
        if (effect.Status != StatusConditionID.none)
        {
            if (moveTarget == MoveTarget.Other)
            {
                target.SetConditionStatus(effect.Status);
            }
            else
            {
                attacker.SetConditionStatus(effect.Status);
            }
        }
        //Volatile Status Condition
        if (effect.VolatileStatus != StatusConditionID.none)
        {
            if (moveTarget == MoveTarget.Other)
            {
                target.SetVolatileConditionStatus(effect.VolatileStatus);
            }
            else
            {
                attacker.SetVolatileConditionStatus(effect.VolatileStatus);
            }
        }

        yield return ShowStatsMessages(attacker);
        yield return ShowStatsMessages(target); 
    }

    bool MoveHits(Move move, Pokemon attacker, Pokemon target)
    {
        if (move.Base.AlwaysHit)
        {
            return true;
        }
        float rnd = Random.Range(0, 100);
        float moveAcc = move.Base.Accuracy;

        float accuracy = attacker.StatsBoosted[Stat.Accuracy];
        float evasion = target.StatsBoosted[Stat.Evasion];
        float multiplierAcc = 1.0f + Mathf.Abs(accuracy) / 3.0f;
        float multiplierEvs = 1.0f + Mathf.Abs(evasion) / 3.0f;
        if (accuracy > 0)
        {
            moveAcc *= multiplierAcc;
        }
        else
        {
            moveAcc /= multiplierAcc;
        }

        if (evasion > 0)
        {
            moveAcc /= multiplierEvs;
        }
        else
        {
            moveAcc *= multiplierEvs;
        }
        return rnd < moveAcc;  
    }
    IEnumerator ShowStatsMessages(Pokemon pokemon)
    {
        while (pokemon.StatusChangeMessages.Count > 0)
        {
            var message = pokemon.StatusChangeMessages.Dequeue();
            yield return battleDialogueBox.SetDialogue(message);
        }
    }

    IEnumerator RunAfterTurn(BattleUnit attacker)
    {
        if (state == BattleState.FinishBattle)
        {
            yield break;
        }

        yield return new WaitUntil(() => state == BattleState.RunTurn);
        //Comprobar estados alterados como quemadura o envenenamiento a final de turno
        attacker.Pokemon.OnFinishTurn();
        yield return ShowStatsMessages(attacker.Pokemon);
        yield return attacker.Hud.UpdatePokemonData();
        if (attacker.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(attacker);
        }
        yield return new WaitUntil(() => state == BattleState.RunTurn);
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
            if (battleTYpe == BattleTYpe.WildPOkemon)
            {
                BattleFinish(true);
            }
            else //Batalla contra un entrenador
            {
                var nextPokemon = trainerParty.GetFirstNonFaintedPokemon();
                if (nextPokemon != null)
                {
                    //Enviar el siguiente pokemon a batalla
                    StartCoroutine(YesNoChoice(nextPokemon));

                }
                else
                {
                    BattleFinish(true);
                }
            }
        }
    }

    IEnumerator SendNextTrainerPokemonToBattle()
    {
        state = BattleState.Busy;
        var nextPokemon = trainerParty.GetFirstNonFaintedPokemon();
        enemyUnit.SetUpPokemon(nextPokemon);
        yield return battleDialogueBox.SetDialogue($"{trainer.name} ha enviado a {nextPokemon.Base.Name}");
        state = BattleState.RunTurn;
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
        yield return new WaitForSeconds(1.0f);
        if (previousState == null)
        {
            state = BattleState.RunTurn;
        }else if (previousState == BattleState.YesNoChoice)
        {
            yield return SendNextTrainerPokemonToBattle();
        }
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (battleTYpe != BattleTYpe.WildPOkemon)
        {
            yield return battleDialogueBox.SetDialogue("No puedes robar los pokemons de otros entrenadores");
            state = BattleState.RunTurn;
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
            state = BattleState.RunTurn;
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
            state = BattleState.RunTurn;
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
                state = BattleState.RunTurn;
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
