using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public float moveX, moveY;
    public bool isMoving;

    [SerializeField] private List<Sprite> walkDownSprite, walkUpSprite, walkLeftSprite, walkRightSprite;
    private CustomAnimator walkDownAnim, walkUpAnim, walkLeftAnim, walkRightAnim;
    private CustomAnimator currentAnimator;
    private SpriteRenderer renderer;

    private bool wasPreviouslyMoving = false;
    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new CustomAnimator(renderer, walkDownSprite);
        walkUpAnim = new CustomAnimator(renderer, walkUpSprite);
        walkLeftAnim = new CustomAnimator(renderer, walkLeftSprite);
        walkRightAnim = new CustomAnimator(renderer, walkRightSprite);

        currentAnimator = walkDownAnim;
    }

    private void Update()
    {
        var previousAnimator = currentAnimator;
        if (moveX == 1)
        {
            currentAnimator = walkRightAnim;
        }else if (moveX == -1)
        {
            currentAnimator = walkLeftAnim;
        }else if (moveY == 1)
        {
            currentAnimator = walkUpAnim;
        }else if (moveY == -1)
        {
            currentAnimator = walkDownAnim;
        }

        if (previousAnimator != currentAnimator || isMoving != wasPreviouslyMoving)
        {
            currentAnimator.Start();
        }
        if (isMoving)
        {
            currentAnimator.HandleUpdate();
        }
        else
        {
            renderer.sprite = currentAnimator.AnimFrames[0];
        }

        wasPreviouslyMoving = isMoving;
    }
}
