using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public Transform cameraRotate;
    public float cameraRotateSpeed;
    Quaternion cameraRotation;
    
    public Transform cursor;
    public float cursorSpeed;
    public float cursorRotSpeed;
    Vector3Int cursorPos;
    Vector3Int cursorDirection;
    
    public Tile[] tileTypes;
    public float tileMoveSpeed;
    public float tileRotateSpeed;
    public int tileMovePriorityRange;
    int tileTypeIndex;
    
    public Transform tileGroup;
    public Vector3Int boardSize;
    Tile[,,] board;
    List<Tile> placedTiles = new List<Tile>();

    public Transform spawnerTileGroup;
    public Tile[] spawnerTypes;
    
    bool isPlaying;
    public float isPlayingDelay;
    double isPlayingDelayTime;
    int frameID;

    PlayerInput playerInput;

    public Transform[] levelSets;
    Transform loadedLevelSet;

    public TextMeshProUGUI hudLevelSetNumber;
    public TextMeshProUGUI hudTileTypeName;
    public Image hudTileTypeImage;
    public TextMeshProUGUI hudIsPlayingBool;
    public TextMeshProUGUI hudFrameIDNumber;
    public Transform hudHelp;

    void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Default.Enable();
        playerInput.Default.MoveCursor.performed     += MoveCursorAction;
        playerInput.Default.Place.performed          += PlaceAction;
        playerInput.Default.Remove.performed         += RemoveAction;
        playerInput.Default.SwitchTileType.performed += SwitchTileTypeAction;
        playerInput.Default.Play.performed           += PlayAction;
        playerInput.Default.NextFrame.performed      += NextFrameAction;
        playerInput.Default.Menu.performed           += context => hudHelp.gameObject.SetActive(!hudHelp.gameObject.activeSelf);
        playerInput.Default.Reset.performed          += context => Reset();
        playerInput.Default.ClearBoard.performed     += context => ClearBoard();
        playerInput.Default.RotateCursor.performed   += RotateCursorAction;
        playerInput.Default.RotateView.performed     += RotateViewAction;
        playerInput.Default.LoadLevel01.performed    += context => LoadLevelSet(0);
        playerInput.Default.LoadLevel02.performed    += context => LoadLevelSet(1);
        playerInput.Default.LoadLevel03.performed    += context => LoadLevelSet(2);
        playerInput.Default.LoadLevel04.performed    += context => LoadLevelSet(3);
        playerInput.Default.LoadLevel05.performed    += context => LoadLevelSet(4);
        playerInput.Default.LoadLevel06.performed    += context => LoadLevelSet(5);
        playerInput.Default.LoadLevel07.performed    += context => LoadLevelSet(6);
        playerInput.Default.LoadLevel08.performed    += context => LoadLevelSet(7);
        playerInput.Default.LoadLevel09.performed    += context => LoadLevelSet(8);
        playerInput.Default.LoadLevel10.performed    += context => LoadLevelSet(9);

        board = GetBlankBoard();
    }

    void PlayAction(InputAction.CallbackContext context)
    {
        isPlaying = !isPlaying;

        hudIsPlayingBool.text = isPlaying.ToString();
    }

    void Start()
    {
        cursorPos = Vector3Int.RoundToInt(transform.position);
        cursorDirection = Vector3Int.RoundToInt(transform.rotation * Vector3.forward);
        cameraRotation = cameraRotate.rotation;

        Tile tile = tileTypes[tileTypeIndex];
        hudTileTypeName.text = tile.tileName;
        hudTileTypeImage.sprite = tile.tileSprite;
    }

    void Update()
    {
        cameraRotate.position = cursor.position;
        cameraRotate.rotation = Quaternion.RotateTowards(cameraRotate.rotation, cameraRotation, cameraRotateSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        cursor.transform.position = Vector3.MoveTowards(cursor.transform.position, cursorPos, cursorSpeed * Time.fixedDeltaTime);
        cursor.transform.rotation = Quaternion.RotateTowards(cursor.transform.rotation, Quaternion.LookRotation(cursorDirection, Vector3.up), cursorRotSpeed * Time.fixedDeltaTime);

        if (isPlaying && Time.timeSinceLevelLoadAsDouble - isPlayingDelayTime > isPlayingDelay) {
            NextFrame();
            isPlayingDelayTime = Time.timeSinceLevelLoadAsDouble;
        }
    }

    void MoveCursorAction(InputAction.CallbackContext context)
    {
        Vector2 moveVector2D = context.ReadValue<Vector2>();
        Vector3 moveVectorFromCameraRotation = cameraRotation * new Vector3(moveVector2D.x, boardSize.y, moveVector2D.y);

        cursorPos += Vector3Int.RoundToInt(moveVectorFromCameraRotation);
        ResetCursorHeight();
    }

    void RotateCursorAction(InputAction.CallbackContext context)
    {
        int rotation = Mathf.RoundToInt(context.ReadValue<float>());

        cursorDirection = Vector3Int.RoundToInt(Quaternion.Euler(0, 90 * rotation, 0) * cursorDirection);
    }

    void RotateViewAction(InputAction.CallbackContext context)
    {
        int rotation = Mathf.RoundToInt(context.ReadValue<float>());
        cameraRotation = cameraRotation * Quaternion.Euler(0, 90 * rotation, 0);
    }

    void PlaceAction(InputAction.CallbackContext context)
    {
        if (frameID > 0) return;
        if (cursorPos.y > boardSize.y - 2) return;  // Prevents cursor from pushing itself oob
        
        cursorPos.y++;

        Vector3Int spawnPos = cursorPos + Vector3Int.down;
        Tile tile = Place(
            tileTypes[tileTypeIndex], 
            spawnPos,
            spawnPos + new Vector3(0, -0.5f, 0),
            cursorDirection,
            Quaternion.LookRotation(cursorDirection, Vector3.up) * Quaternion.Euler(0, 90, 0),
            tileGroup
        );
        tile.transform.localScale = Vector3.zero;
    }

    public Tile Place(Tile tileType, Vector3Int boardPos, Vector3 spawnPos, Vector3Int forwardDirection, Quaternion spawnRotation, Transform parent)
    {
        if (!IsTileInBounds(boardPos)) return null;
        if (!IsTileSpaceClear(boardPos)) return null;

        Tile tile = Instantiate(tileType, spawnPos, spawnRotation, parent);
        tile.playerController = this;
        tile.tilePos          = boardPos;
        tile.tileSpawnPos     = boardPos;
        tile.tileDirection    = forwardDirection;
        tile.moveSpeed        = tileMoveSpeed;
        tile.rotateSpeed      = tileRotateSpeed;

        if (tile is Spawner) {
            Spawner spawnerTile = (Spawner)tile;
            spawnerTile.spawnTypes = spawnerTypes;
            spawnerTile.spawnParent = spawnerTileGroup;
        }
        
        SetTileFromPosition(boardPos, tile);
        placedTiles.Add(tile);

        return tile;
    }

    void LoadLevelSet(int levelSetIndex)
    {
        ClearBoard();
        
        if (loadedLevelSet != null) Destroy(loadedLevelSet.gameObject);
        
        Reset();

        loadedLevelSet = Instantiate(levelSets[levelSetIndex], this.transform);   
        for (int i = 0; i < loadedLevelSet.childCount; i++) {
            Transform levelSetTileTransform = loadedLevelSet.GetChild(i);
            
            Tile levelSetTile = levelSetTileTransform.GetComponent<Tile>();
            levelSetTile.playerController = this;
            
            Vector3Int levelSetTilePos = Vector3Int.RoundToInt(levelSetTileTransform.position);
            levelSetTile.tilePos = levelSetTilePos;
            levelSetTile.tileSpawnPos = levelSetTilePos;

            Vector3Int levelSetTileDirection = Vector3Int.RoundToInt(levelSetTileTransform.rotation * Vector3.forward);
            levelSetTile.tileDirection = levelSetTileDirection;

            levelSetTile.moveSpeed = tileMoveSpeed;
            levelSetTile.moveSpeed = tileRotateSpeed;

            if (levelSetTile is Spawner) {
                Spawner spawnerTile = (Spawner)levelSetTile;
                spawnerTile.spawnTypes = spawnerTypes;
                spawnerTile.spawnParent = spawnerTileGroup;
            }
        
            SetTileFromPosition(levelSetTilePos, levelSetTile);
            placedTiles.Add(levelSetTile);
        }

        ResetCursorHeight();

        hudLevelSetNumber.text = levelSetIndex.ToString();
    }

    void RemoveAction(InputAction.CallbackContext context)
    {
        if (frameID > 0) return;
        if (!Remove(cursorPos + Vector3Int.down)) return;

        ResetCursorHeight();
    }

    public bool Remove(Vector3Int targetPos)
    {   
        if (!IsTileInBounds(targetPos)) return false;
        if (IsTileSpaceClear(targetPos)) return false;

        Tile tile = GetTileFromPosition(targetPos);
        SetTileFromPosition(targetPos, null);
        placedTiles.Remove(tile);
        Destroy(tile.gameObject);

        return true;
    }

    void ClearBoard()
    {
        for (int i = 0; i < board.GetLength(0); i++)
            for (int j = 0; j < board.GetLength(1); j++)
                for (int k = 0; k < board.GetLength(2); k++)
                    Remove(new Vector3Int(i, j, k));
        
        ResetCursorHeight();
    }

    void SwitchTileTypeAction(InputAction.CallbackContext context)
    {
        int switchDirection = Mathf.RoundToInt(context.ReadValue<float>());

        tileTypeIndex += switchDirection;
        if (tileTypeIndex < 0) tileTypeIndex = tileTypes.Length - 1;
        else if (tileTypeIndex == tileTypes.Length) tileTypeIndex = 0;

        Tile tile = tileTypes[tileTypeIndex];
        hudTileTypeName.text = tile.tileName;
        hudTileTypeImage.sprite = tile.tileSprite;
    }

    void NextFrameAction(InputAction.CallbackContext context) {
        if (isPlaying) isPlaying = false;
        else NextFrame();
    }

    void NextFrame()
    {   
        frameID++;     
        hudFrameIDNumber.text = frameID.ToString();

        for (int i = tileMovePriorityRange; i > -1; i--)
            for (int j = 0; j < placedTiles.Count; j++)
                placedTiles[j].NextFrameMove(i);
            
        for (int j = 0; j < placedTiles.Count; j++)
            placedTiles[j].NextFrameEnd();
        
        ResetCursorHeight();
    }

    void ResetCursorHeight()
    {
        Vector3Int newCursorPosition = Vector3Int.Min(Vector3Int.Max(cursorPos, Vector3Int.zero), boardSize - Vector3Int.one);
        newCursorPosition.y = boardSize.y - 1;
        while (newCursorPosition.y > 0 && board[newCursorPosition.x,newCursorPosition.y - 1,newCursorPosition.z] == null) newCursorPosition.y--;        
        cursorPos = newCursorPosition;
    }

    void Reset()
    {
        frameID = 0;
        hudFrameIDNumber.text = frameID.ToString();

        for (int i = 0; i < spawnerTileGroup.childCount; i++)
            Remove(spawnerTileGroup.GetChild(i).GetComponent<Tile>().tilePos);

        for (int i = 0; i < placedTiles.Count; i++)
            placedTiles[i].Reset();

        ResetCursorHeight();
    }

    public bool IsTileInBounds(Vector3Int tilePos)
    {
        if (tilePos.x < 0 || boardSize.x <= tilePos.x) return false;
        if (tilePos.y < 0 || boardSize.y <= tilePos.y) return false;
        if (tilePos.z < 0 || boardSize.z <= tilePos.z) return false;

        return true;
    }

    public bool IsTileSpaceClear(Vector3Int tileSpace)
    {
        if (board[tileSpace.x,tileSpace.y,tileSpace.z] == null) return true;
        return false;
    }

    public Tile GetTileFromPosition(Vector3Int tilePos)
    {
        if (!IsTileInBounds(tilePos)) return null;
        return board[tilePos.x,tilePos.y,tilePos.z];
    }

    void SetTileFromPosition(Vector3Int tilePos, Tile tile)
    {
        board[tilePos.x,tilePos.y,tilePos.z] = tile;
    }
    
    
    public bool MoveTile(Vector3Int targetPos, Vector3Int translation)
    {
        if (!IsTileInBounds(targetPos)) return false;
        if (IsTileSpaceClear(targetPos)) return false;
        
        Tile targetTile = GetTileFromPosition(targetPos);
        if (!targetTile.isMoveable) return false;
        if (targetTile.hasMoved) return false;
        
        Vector3Int destinationPos = targetPos + translation;
        if (!IsTileInBounds(destinationPos)) return false;
        if (!IsTileSpaceClear(destinationPos)) return false;

        board[destinationPos.x,destinationPos.y,destinationPos.z] = targetTile;
        board[targetPos.x,targetPos.y,targetPos.z] = null;
        targetTile.tilePos = destinationPos;
        targetTile.hasMoved = true;

        return true;
    }

    public void SwapTilePos(Vector3Int target, Vector3Int destination)
    {
        Tile targetTile = board[target.x,target.y,target.z];
        Tile destinationTile = board[destination.x,destination.y,destination.z];

        board[target.x,target.y,target.z] = destinationTile;
        board[destination.x,destination.y,destination.z] = targetTile;

        if (targetTile != null) targetTile.tilePos = destination;
        if (destinationTile != null) destinationTile.tilePos = target;
    }

    public Tile[,,] GetBlankBoard()
    {
        return new Tile[boardSize.x,boardSize.y,boardSize.z];
    }
}
