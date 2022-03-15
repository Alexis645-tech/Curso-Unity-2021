using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterAnimator))]
public class PlayerController : MonoBehaviour
{
    private Vector2 input;

    private Character _character;

    public event Action OnPokemonEncountered;
    
    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 1.0f;

    private void Start()
    {
        _character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;
        if(!_character.isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            
            if (input != Vector2.zero)
            {
                StartCoroutine(_character.MoveTowards(input, CheckForPokemon));
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

    private void Interact()
    {
        timeSinceLastClick = 0;
        
        var facingDirection = new Vector3(_character.Animator.moveX, _character.Animator.moveY);
        var interactPosition = transform.position + facingDirection;
        
        Debug.DrawLine(transform.position, interactPosition, Color.magenta, 1.0f);
        var collider = Physics2D.OverlapCircle(interactPosition, 0.2f, GameLayers.SharedInstance.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    [SerializeField] private float verticalOffset = 0.2f;
    private void CheckForPokemon()
    {
        if (Physics2D.OverlapCircle(transform.position - new Vector3(0, verticalOffset), 0.2f, GameLayers.SharedInstance.PokemonLayer) != null)
        {
            if (Random.Range(0, 100) < 10)
            {
                OnPokemonEncountered();
            }
        }
    }
}
