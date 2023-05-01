using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    // [HideInInspector]
    public PlayerController playerController;

    public string tileName = "Tile";
    public Sprite tileSprite;

    [HideInInspector]
    public Vector3Int tilePos;
    [HideInInspector]
    public Vector3Int tileSpawnPos;
    [HideInInspector]
    public Vector3Int tileDirection;

    [HideInInspector]
    public float moveSpeed;
    [HideInInspector]
    public float rotateSpeed;
    [HideInInspector]
    
    public Tile parent;
    [HideInInspector]
    public Tile child;
    
    public bool isMoveable;
    public int fallPriority;
    [HideInInspector]
    public bool hasMoved;

    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, tilePos, moveSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(tileDirection, Vector3.up), rotateSpeed * Time.fixedDeltaTime);
        transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, moveSpeed * Time.fixedDeltaTime);
    }

    public virtual void NextFrameMove(int priority)  // Called for the move phase of the next frame
    {
        if (priority != fallPriority) return;
        
        Vector3Int fallPos = tilePos + Vector3Int.down;
        if (!playerController.IsTileInBounds(fallPos)) return;
        if (!playerController.IsTileSpaceClear(fallPos)) return;
        
        playerController.MoveTile(tilePos, Vector3Int.down);
    }

    public virtual void NextFrameEnd()  // Called for the ending/reset phase of the next frame
    {
        hasMoved = false;
    }

    public virtual void Reset()  // Called by playerController to reset the tile
    {
        hasMoved = false;
        playerController.SwapTilePos(tilePos, tileSpawnPos);
    }

    public virtual Vector3Int[] GetTheseTileSpaces()  // Spaces used by the tile itself
    {
        Vector3Int[] theseTileSpaces = {tilePos};
        return theseTileSpaces;
    }

    public virtual Tile[,,] GetAllTileSpaces()  // Spaces used by the tile and all of its children
    {
        return playerController.GetBlankBoard();
    }

    public virtual Tile[,,] GetMoveSpaces()  // Spaces used by the move action (including destination)
    {
        return playerController.GetBlankBoard();
    }
    
    public virtual Vector3Int[] GetMoveTranslations()  // Single tile translations used to move affected tiles
    {
        Vector3Int[] translations = {};
        return translations;
    }

}
