using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belt : Tile
{
    public int transportPriority;

    public override void NextFrameMove(int priority)
    {   
        base.NextFrameMove(priority);
        
        if (priority != transportPriority) return;

        Vector3Int pushDirection = Vector3Int.RoundToInt(transform.rotation * Vector3.forward);
        Vector3Int targetTilePos = tilePos + Vector3Int.up;

        playerController.MoveTile(targetTilePos, pushDirection);
    }

    public override void Reset()
    {
        base.Reset();
    }
}
