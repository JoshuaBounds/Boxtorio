using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : Tile
{
    public int goalPriority;
    public string[] goalTileNames;

    public override void NextFrameMove(int priority)
    {
        base.NextFrameMove(priority);

        if (priority != goalPriority) return;

        Vector3Int targetPos = tilePos + Vector3Int.up;
        if (playerController.IsTileSpaceClear(targetPos)) return;
        if (!playerController.IsTileInBounds(targetPos)) return;
        if (playerController.GetTileFromPosition(targetPos).hasMoved) return;
        
        string targetName = playerController.GetTileFromPosition(targetPos).tileName;
        if (!System.Array.Exists<string>(goalTileNames, goalTileName => goalTileName == targetName)) return;
        
        playerController.Remove(tilePos + Vector3Int.up);
    }
}
