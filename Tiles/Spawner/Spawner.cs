using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : Tile
{
    public Tile[] spawnTypes;
    int spawnTypeIndex;
    public Transform spawnParent;
    public int spawnPriority;

    public override void NextFrameMove(int priority)
    {
        base.NextFrameMove(priority);

        if (priority != spawnPriority) return;

        Vector3Int targetPos = tilePos + tileDirection;        
        Tile tile = playerController.Place(
            spawnTypes[spawnTypeIndex],
            targetPos,
            tilePos,
            tileDirection,
            Quaternion.LookRotation(tileDirection, Vector3.up),
            spawnParent
        );
        
        if (tile == null) return;
        tile.hasMoved = true;
    }
}
