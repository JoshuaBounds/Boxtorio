using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pusher : Tile
{
    public int pushPriority;

    [HideInInspector]
    bool isExtended;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void NextFrameMove(int priority)
    {   
        base.NextFrameMove(priority);
        
        if (priority != pushPriority) return;

        if (isExtended) {
            isExtended = false;
            animator.SetBool("isExtended", false);
            return;
        }

        Vector3Int pushDirection = Vector3Int.RoundToInt(transform.rotation * Vector3.forward);
        Vector3Int targetTilePos = tilePos + pushDirection;

        if (playerController.MoveTile(targetTilePos, pushDirection)) {
            isExtended = true;
        }
        
        if (isExtended) {
            animator.SetBool("isExtended", true);
        }
    }

    public override void Reset()
    {
        base.Reset();

        isExtended = false;
        animator.SetBool("isExtended", false);
    }
}
