using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private float speed;
    private CharacterAnimator _animator;

    public bool isMoving { get; private set; }

    public CharacterAnimator Animator => _animator;

    private void Awake()
    {
        _animator = GetComponent<CharacterAnimator>();
    }

    public IEnumerator MoveTowards(Vector2 moveVector, Action OnMoveFinish = null)
    {
        if (moveVector.x != 0)
        {
            moveVector.y = 0;
        }
        _animator.moveX = Mathf.Clamp(moveVector.x, -1, 1);
        _animator.moveY = Mathf.Clamp(moveVector.y, -1, 1);
        var targetPosition = transform.position;
        targetPosition.x += moveVector.x;
        targetPosition.y += moveVector.y;

        if (!IsPathAvailable(targetPosition))
        {
            yield break;
        }

        isMoving = true;
        while (Vector3.Distance(transform.position, targetPosition) > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;

        OnMoveFinish?.Invoke();
    }

    public void HandleUpdate()
    {
        _animator.isMoving = isMoving;
    }

    private bool IsPathAvailable(Vector3 target)
    {
        var path = target - transform.position;
        var direction = path.normalized;
        return !Physics2D.BoxCast(transform.position + direction, new Vector2(0.3f, 0.3f), 0f, direction,
            path.magnitude - 1,
            GameLayers.SharedInstance.SolidObjectsLayer | GameLayers.SharedInstance.InteractableLayer);
    }

    /// <summary>
    /// El método comprueba que la zona a la que queremos acceder este disponible
    /// </summary>
    /// <param name="target">Zona a la que queremos acceder</param>
    /// <returns>True: si el target esta disponible, false: en caso contrario</returns>
    private bool IsAvialable(Vector3 target)
    {
        if (Physics2D.OverlapCircle(target, 0.2f, GameLayers.SharedInstance.SolidObjectsLayer | GameLayers.SharedInstance.InteractableLayer) != null)
        {
            return false;
        }

        return true;
    }
}
