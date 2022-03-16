using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FacingDirection
{
    Down, Up, Right, Left
}
public class CharacterAnimator : MonoBehaviour
{
    public float moveX, moveY;
    public bool isMoving;

    [SerializeField] private List<Sprite> walkDownSprite, walkUpSprite, walkLeftSprite, walkRightSprite;
    [SerializeField] private FacingDirection defaultDirection = FacingDirection.Down;
    public FacingDirection DefaultDirection => defaultDirection;

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

        SetFacingDirection(defaultDirection);
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

    public void SetFacingDirection(FacingDirection direction)
    {
        if (direction == FacingDirection.Down)
        {
            moveY = -1;
        }else if (direction == FacingDirection.Up)
        {
            moveY = 1;
        }else if (direction == FacingDirection.Left)
        {
            moveX = -1;
        }else if (direction == FacingDirection.Right)
        {
            moveX = 1;
        }
    }
}
