using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Data component for tile game object.
/// </summary>
public class Tile : MonoBehaviour, IEquatable<Tile>
{
    #region Variables
    //The color code assigned to the tile
    public int TileColor;
    //The image displayed on the tile. In this demo it's a solid color
    public Image TileImage;
    //Indicates whether the tile is empty
    public bool IsEmpty;
    //Indicates whether the tile in the grid is a blocking tile
    public bool IsBlocking;
    //X coordinate of the tile in the game matrix
    public int X;
    //Y coordinate of the tile in the game matrix
    public int Y;
    //Reference to the GameMaster component
    public GameMaster GameMasterObject;
    //Reference to the RectTransform of this game object
    public RectTransform RectTransformTile;

    //Used for movement and animation in the update loop
    private bool _isDropping;
    //Used as a movement target for the tile when it's swapping or dropping
    private Vector2 _moveDir;
    #endregion
    #region Tile
    private void Awake()
    {
        GameMasterObject = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        RectTransformTile = GetComponent<RectTransform>();
    }

    void Update()
    {
        if(_isDropping && !IsEmpty)
        {
            float step = GameMasterObject.GameSpeed * Time.deltaTime;
            RectTransformTile.anchoredPosition = Vector2.MoveTowards(RectTransformTile.anchoredPosition, _moveDir, step);

            if (RectTransformTile.anchoredPosition == _moveDir)
            {
                GameMasterObject.TileDropped(this);
                _isDropping = false;
            }
        }
    }

    /// <summary>
    /// Assigns a color to the tile
    /// </summary>
    /// <param name="color"></param>
    public void AssignColor(int color)
    {
        TileColor = color;
        TileImage.color = Utils.IntToColor(TileColor);
    }

    /// <summary>
    /// Initializes the tile with position and blocking parameter. Use AssignColor to assign color
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="isBlocking"></param>
    public void Init(int x,int y, bool isBlocking)
    {
        X = x;
        Y = y;
        IsBlocking = isBlocking;
    }

    /// <summary>
    /// Notifies the Tile component that user has interacted with the tile
    /// </summary>
    public void OnClick()
    {
        GameMasterObject.TileClicked(this);
    }

    /// <summary>
    /// Start dropping the tile to a given position
    /// </summary>
    public void DropTheTile()
    {
        _moveDir = new Vector2(50f * X, -50f * Y);
        _isDropping = true;
    }

    /// <summary>
    /// Swaps this tile with another tile
    /// </summary>
    /// <param name="toSwap"></param>
    public void SwapWithTile(Tile toSwap)
    {
        int thisX = X;
        int thisY = Y;

        X = toSwap.X;
        Y = toSwap.Y;

        toSwap.X = thisX;
        toSwap.Y = thisY;
    }

    /// <summary>
    /// Override equals for this component. 2 tiles are the same if their coordinates match
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Tile other)
    {
        if (X == other.X && Y == other.Y)
            return true;
        else
            return false;
    }
    #endregion
}
