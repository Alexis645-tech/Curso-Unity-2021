using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private List<Sprite> sprites;

    private CustomAnimator animator;

    private void Start()
    {
        animator = new CustomAnimator(GetComponent<SpriteRenderer>(), sprites);
        animator.Start();
    }

    private void Update()
    {
        animator.HandleUpdate();
    }

    public void Interact()
    {
        DialogueManager.SharedInstace.ShowDialogue(dialogue);
    }
}
