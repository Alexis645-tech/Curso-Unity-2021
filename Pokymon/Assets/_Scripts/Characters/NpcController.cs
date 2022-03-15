using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum NpcState
{
    Idle,
    Walking,
    Talking
}
public class NpcController : MonoBehaviour, Interactable
{
    [SerializeField] private Dialogue dialogue;
    private NpcState state;
    [SerializeField] private float idleTime = 3f;
    private float idleTimer = 0f;
    [SerializeField] private List<Vector2> moveDirections;
    private int currentDirection;

    private Character _character;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    public void Interact()
    {
        if (state == NpcState.Idle)
        {
            state = NpcState.Talking;
            DialogueManager.SharedInstace.ShowDialogue(dialogue, () =>
            {
                idleTimer = 0f;
                state = NpcState.Idle;
            });
        }
    }

    private void Update()
    {
        if (state == NpcState.Idle)
        {
            idleTimer = Time.deltaTime;
            if (idleTimer > idleTime)
            {
                idleTimer = 0f;
                StartCoroutine(Walk());

            }
        }
        _character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NpcState.Walking;
        var direction = Vector2.zero;
        if (moveDirections.Count > 0)
        {
            direction = moveDirections[currentDirection];
            currentDirection = (currentDirection + 1) % moveDirections.Count;
        }
        else
        {
            direction = new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));
        }

        yield return _character.MoveTowards(moveDirections[currentDirection]);
        state = NpcState.Idle;
    }
}
