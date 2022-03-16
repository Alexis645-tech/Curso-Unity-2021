using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private GameObject exclamationMessage;
    [SerializeField] private GameObject fov;
    private Character _character;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovDirection(_character.Animator.DefaultDirection);
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        exclamationMessage.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamationMessage.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVect = diff - diff.normalized;
        moveVect = new Vector2(Mathf.RoundToInt(moveVect.x), Mathf.RoundToInt(moveVect.y));
        yield return _character.MoveTowards(moveVect);
        
        DialogueManager.SharedInstace.ShowDialogue(dialogue, () =>
        {
            //TODO: Inicio de la batalla pokemon
        });
    }

    public void SetFovDirection(FacingDirection direction)
    {
        float angle = 0f;
        if (direction == FacingDirection.Right)
        {
            angle = 90f;
        }else if (direction == FacingDirection.Up)
        {
            angle = 180f;
        }else if (direction == FacingDirection.Left)
        {
            angle = 270f;
        }

        fov.transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
