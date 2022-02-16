using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private bool isMoving;
    
    [SerializeField]private float speed;
    private Vector2 input;

    private Animator _animator;

    [SerializeField] private LayerMask solidObjectsLayer, pokemonLayer;

    public event Action OnPokemonEncountered;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void HandleUpdate()
    {
        if(!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            if (input.x != 0)
            {
                input.y = 0;
            }
            if (input != Vector2.zero)
            {
                _animator.SetFloat("MoveX", input.x);
                _animator.SetFloat("MoveY", input.y);
                var targetPosition = transform.position;
                targetPosition.x += input.x;
                targetPosition.y += input.y;

                if (IsAvialable(targetPosition))
                {
                    StartCoroutine(MoveTowards(targetPosition));
                }
            }
        }
    }

    private void LateUpdate()
    {
        _animator.SetBool("Is Moving", isMoving);
    }

    IEnumerator MoveTowards(Vector3 destination)
    {
        isMoving = true;
        while(Vector3.Distance(transform.position, destination) > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = destination;
        isMoving = false;
        
        CheckForPokemon();
    }

    /// <summary>
    /// El método comprueba que la zona a la que queremos acceder este disponible
    /// </summary>
    /// <param name="target">Zona a la que queremos acceder</param>
    /// <returns>True: si el target esta disponible, false: en caso contrario</returns>
    private bool IsAvialable(Vector3 target)
    {
        if (Physics2D.OverlapCircle(target, 0.2f, solidObjectsLayer) != null)
        {
            return false;
        }

        return true;
    }

    private void CheckForPokemon()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, pokemonLayer) != null)
        {
            if (Random.Range(0, 100) < 10)
            {
                OnPokemonEncountered();
            }
        }
    }
}
