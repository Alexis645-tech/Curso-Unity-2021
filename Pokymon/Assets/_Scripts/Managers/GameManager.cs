using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    Travel,
    Battle,
    Dialogue,
    Cutscene
}

[RequireComponent(typeof(ColorManager))]
public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private Camera worldMainCamera;
    [SerializeField] private Image transitionPanel;

    private GameState _gameState;

    public AudioClip worldClip, battleClip;

    private void Awake()
    {
        _gameState = GameState.Travel;
    }

    private void Start()
    {
        StatusConditionFactory.InitFactory();
        SoundManager.SharedInstance.PlayMusic(worldClip);
        playerController.OnPokemonEncountered += StartPokemonBattle;
        playerController.OnEnterTrainerFov += (Collider2D trainerCollider) =>
        {
            var trainer = trainerCollider.GetComponentInParent<TrainerController>();
            if (trainer != null)
            {
                _gameState = GameState.Cutscene;
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            }
        };
        battleManager.OnBattleFinish += FinishPokemonBattle;
        DialogueManager.SharedInstace.OnDialogueStart += () =>
        {
            _gameState = GameState.Dialogue;
        };

        DialogueManager.SharedInstace.OnDialogueFinish += () =>
        {
            if (_gameState == GameState.Dialogue)
            {
                _gameState = GameState.Travel;
            }
            //TODO: Si el dialogo es con un entrenador pokemon, no vamos a travel, si no a battle
        };
    }

    void StartPokemonBattle()
    {
        StartCoroutine(FadeInBattle());
    }

    IEnumerator FadeInBattle()
    {
        SoundManager.SharedInstance.PlayMusic(battleClip);
        _gameState = GameState.Battle;
        
        yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);
        
        battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<PokemonMapArea>().GetComponent<PokemonMapArea>().getRandomWildPokemon();

        var wildPokemonCOpy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
        
        battleManager.HandleStartBattle(playerParty, wildPokemon);

        yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();
    }

    void FinishPokemonBattle(bool playerHasWon)
    {
        StartCoroutine(FadeOutBattle(playerHasWon));
    }

    IEnumerator FadeOutBattle(bool playerHasWon)
    {
        yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);
        
        SoundManager.SharedInstance.PlayMusic(worldClip);
        battleManager.gameObject.SetActive(false);
        worldMainCamera.gameObject.SetActive(true);
        
        if (!playerHasWon)
        {
            //TODO: diferencias entre victoria y derrota
        }

        yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();
        _gameState = GameState.Travel;
    }

    private void Update()
    {
        if (_gameState == GameState.Travel)
        {
            playerController.HandleUpdate();
        }else if (_gameState == GameState.Battle)
        {
            battleManager.HandleUpdate();
        }else if (_gameState == GameState.Dialogue)
        {
            DialogueManager.SharedInstace.HandleUpdate();
        }
    }
}
