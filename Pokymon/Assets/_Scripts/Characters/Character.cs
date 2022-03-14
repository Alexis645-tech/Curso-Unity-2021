using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]private float speed;
    private CharacterAnimator _animator;

    public CharacterAnimator Animator => _animator;

    private void Awake()
    {
        _animator = GetComponent<CharacterAnimator>();
    }

    public IEnumerator MoveTowards(Vector2 moveVector, Action OnMoveFinish = null)
    {
        _animator.moveX = moveVector.x;
        _animator.moveY = moveVector.y;
        var targetPosition = transform.position;
        targetPosition.x += moveVector.x;
        targetPosition.y += moveVector.y;
        
        if (!IsAvialable(targetPosition))
        {
            yield break;
        }
        
        _animator.isMoving = true;
        while(Vector3.Distance(transform.position, targetPosition) > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        _animator.isMoving = false;
        
        OnMoveFinish?.Invoke();
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
