using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterAnimator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private string trainerName;
    [SerializeField] private Sprite trainerSprite;
    public string TrainerName => trainerName;
    public Sprite TrainerSprite => trainerSprite;
    
    private Vector2 input;

    private Character _character;

    public event Action OnPokemonEncountered;
    public event Action<Collider2D> OnEnterTrainerFov;
    
    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 1.0f;

    private void Start()
    {
        _character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;
        if(!_character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            
            if (input != Vector2.zero)
            {
                StartCoroutine(_character.MoveTowards(input, OnMoveFinish));
            }
        }
        _character.HandleUpdate();

        if (Input.GetAxisRaw("Submit") != 0)
        {
            if (timeSinceLastClick >= timeBetweenClicks)
            {
                Interact();
            }
        }
    }

    void OnMoveFinish()
    {
        CheckForPokemon();
        CheckForTrainersFov();
    }

        private void Interact()
    {
        timeSinceLastClick = 0;
        
        var facingDirection = new Vector3(_character.Animator.moveX, _character.Animator.moveY);
        var interactPosition = transform.position + facingDirection;
        
        Debug.DrawLine(transform.position, interactPosition, Color.magenta, 1.0f);
        var collider = Physics2D.OverlapCircle(interactPosition, 0.2f, GameLayers.SharedInstance.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform.position);
        }
    }

    [SerializeField] private float verticalOffset = 0.2f;
    private void CheckForPokemon()
    {
        if (Physics2D.OverlapCircle(transform.position - new Vector3(0, verticalOffset), 0.2f, GameLayers.SharedInstance.PokemonLayer) != null)
        {
            if (Random.Range(0, 100) < 10)
            {
                _character.Animator.isMoving = false;
                OnPokemonEncountered();
            }
        }
    }
    private void CheckForTrainersFov()
    {
        var collider = Physics2D.OverlapCircle(transform.position - new Vector3(0, verticalOffset), 0.2f,
            GameLayers.SharedInstance.FOVLayer);
        if ( collider != null)
        {
            _character.Animator.isMoving = false;
            OnEnterTrainerFov?.Invoke(collider);
        }
    }
}
