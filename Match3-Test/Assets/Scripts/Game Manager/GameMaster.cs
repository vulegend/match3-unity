using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps track of the game state and enables gameplay mechanics
/// </summary>
public class GameMaster : MonoBehaviour {

    #region Variables
    //Reference to the grid holder game object. This is root object for all tiles
    public GameObject GridHolder;
    //Width of the grid. Assigned from Grid Builder window
    public int Width;
    //Height of the grid. Assigned from the Grid Builder window
    public int Height;
    //Color variations for the game. Assigned from the Grid Builder window
    public int ColorVariations;
    //Swap speed for the movement animations. Assignable from GameMaster inspector window
    public float SwapSpeed = 200f;
    //The main game matrix that stores every color as an int. Used to find matches and control grid shape
    //Grid tile matrix codes are as follows : -1 Empty time, 0 Blocking tile, 1..N Color tiles
    public int[,] GameField;
    //The main tile indexer. Used for tile movement and animations
    public List<Tile> GameTiles;
    //First tile that is interacted with
    public Tile TileSelected;
    //Second tile that is interacted with
    public Tile SwapWithTile;
    //Tells us if the game field is currently interactable
    public bool Interactable = true;
    //Swap and drop speed for the simulator. Assignable through GameMaster inspector window
    public float SimulationSpeed;
    //Total amount of simulation iterations. Assignable through GameMaster inspector window
    public int Iterations;

    /// <summary>
    /// Gets the swap/drop game speed.
    /// </summary>
    public float GameSpeed
    {
        get
        {
            if (_isSimulatorRunning)
                return SimulationSpeed;
            else
                return SwapSpeed;
        }
    }

    //Enables tile animation/movement in the update loop
    private bool _moveTiles;
    //Keeps the track of the first selected tile rect transfom for the movement
    private RectTransform _tileSelectedRectTransform;
    //Keeps the track of the second selected tile rect transform for the movement
    private RectTransform _swapWithTileRectTransform;
    //Anchor positions of the 2 swapping tiles
    private Vector2 _selectedTileAnchoredPos;
    private Vector2 _swapWithTileAnchoredPos;
    //If set to true the Swap function will perform a swap on the tiles and update loop will call InverseSwapFinished when done
    private bool _inverseSwap;
    //Tracks the tiles that have dropped after a match. Used to check the new matches
    private List<Tile> _droppedTiles = new List<Tile>();
    //Total amount of tiles that are expected to drop
    private int _tilesDropping;
    //Tracking the amount of tiles that have dropped
    private int _tilesDropped;
    //Used in update loop to check if all expected tiles are dropped
    private bool _waitForDrop;
    //Used for simulator tracking
    private bool _isSimulatorRunning;
    //Current iterations made by the simulator
    private int _iterationsMade;
    //Object pool for the tiles that have been matched
    private Queue<GameObject> _tilePool = new Queue<GameObject>();

    #endregion
    #region Setup
    private void Start()
    {
        InitializeGameField();
    }

    /// <summary>
    /// Initializes game master. Currently called in the editor window
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="colorVariations"></param>
    public void Init(int width,int height, int colorVariations)
    {
        //Set the seed for randomness
        Random.InitState(System.DateTime.Now.Millisecond);
        Width = width;
        Height = height;
        ColorVariations = colorVariations;
    }

    /// <summary>
    /// De initializes the game master
    /// </summary>
    public void DeInit()
    {
       // GridTileLayout = null;
        Width = 0;
        Height = 0;
        ColorVariations = 0;
    }

    /// <summary>
    /// Checks for 2 adjacent tiles to try and get a match
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public bool CheckMatchLeftSetup(int x,int y, int color)
    {
        return GetLeftAdjacent(x, y) == color && GetLeftAdjacent(x - 1, y) == color;
    }

    /// <summary>
    /// Checks for 2 adjacent tiles to try and get a match
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public bool CheckMatchRightSetup(int x, int y, int color)
    {
        return GetRightAdjacent(x, y) == color && GetRightAdjacent(x + 1, y) == color;
    }

    /// <summary>
    /// Checks for 2 adjacent tiles to try and get a match
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public bool CheckMatchBottomSetup(int x,int y, int color)
    {
        return GetBottomAdjacent(x, y) == color && GetBottomAdjacent(x, y + 1) == color;
    }

    /// <summary>
    /// Checks for 2 adjacent tiles to try and get a match
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public bool CheckMatchTopSetup(int x, int y, int color)
    {
        return GetTopAdjacent(x, y) == color && GetTopAdjacent(x, y - 1) == color;
    }

    /// <summary>
    /// Gets the color of the adjacent tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private int GetRightAdjacent(int x, int y)
    {
        if (x >= Width-1)
            return -1;

        return GameField[x + 1, y];
    }

    /// <summary>
    /// Gets the color of the adjacent tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int GetBottomAdjacent(int x, int y)
    {
        if (y >= Height-1)
            return -1;

        return GameField[x, y + 1];
    }

    /// <summary>
    /// Gets the color of the adjacent tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int GetTopAdjacent(int x, int y)
    {
        if (y <= 0)
            return -1;

        return GameField[x, y - 1];
    }

    /// <summary>
    /// Gets the color of the adjacent tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int GetLeftAdjacent(int x, int y)
    {
        if (x <= 0)
            return -1;

        return GameField[x - 1, y];
    }

    /// <summary>
    /// Picks a color for a given tile so it doesn't give a match 3 during setup
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public int PickColorOnSetup(Tile tile)
    {
        int colorPick = Random.Range(1, ColorVariations + 1);

        //Loop untill non-matching color is found. Can't get stuck on the setup as it goes from top left to bottom right
        while(CheckMatchLeftSetup(tile.X,tile.Y,colorPick) || CheckMatchTopSetup(tile.X,tile.Y,colorPick))
        {
            colorPick = Random.Range(1, ColorVariations + 1);
        }
      
        return colorPick;
    }

    /// <summary>
    /// Initializes the game field grid. Assigns the colors to the tiles and sets up the game matrix for play
    /// </summary>
    public void InitializeGameField()
    {
        GameField = new int[Width, Height];

        for(int x = 0; x < Width; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                //GameTiles are loaded from the GridMaker editor. Easily extendible to work with anything
                Tile getTile = GameTiles.Find(o => o.X == x && o.Y == y);
                //Start generating field from top left ignoring the blocking tiles (if we want custom shapes)
                if (getTile.IsBlocking)
                    continue;

                int colorPick = PickColorOnSetup(getTile);
                GameField[x, y] = colorPick;
                getTile.AssignColor(colorPick);              
            }
        }

        //Check if the current setup is playable. If not initialize the field again.
        if (!CheckIfMatchIsAvialable())
            InitializeGameField();
    }

    #endregion
    #region Gameplay

    /// <summary>
    /// Notifies the game master that the tile is interacted with
    /// </summary>
    /// <param name="tile"></param>
    public void TileClicked(Tile tile)
    {
        //If the field is not interactable do nothing
        if (!Interactable)
            return;

        if (TileSelected != null)
        {
            SwapWithTile = tile;

            if (CanSwap())
                Swap();
            else
                ResetSelection();
        }
        else
            TileSelected = tile;
    }

    /// <summary>
    /// Checks if the 2 selected tiles are adjacent
    /// </summary>
    /// <returns></returns>
    public bool CanSwap()
    {
        return TileSelected.X == SwapWithTile.X && (TileSelected.Y - 1 == SwapWithTile.Y || TileSelected.Y + 1 == SwapWithTile.Y) || TileSelected.Y == SwapWithTile.Y && (TileSelected.X - 1 == SwapWithTile.X || TileSelected.X + 1 == SwapWithTile.X);
    }

    /// <summary>
    /// Resets the tiles that player selected
    /// </summary>
    public void ResetSelection()
    {
        _inverseSwap = false;
        Interactable = true;
        TileSelected = null;
        SwapWithTile = null;

        //If we're running on simulator simulate the next move
        if (_isSimulatorRunning)
            PickTiles();
    }

    /// <summary>
    /// Performs a swap of 2 viable tiles
    /// </summary>
    public void Swap()
    {
        //If tiles are currently dropping disable swap. Needed for simulator
        if (_waitForDrop)
            return;

        Interactable = false;
        //If no math is found after initial swap, inverse the swap
        if(_inverseSwap)
        {
            Tile tmp = TileSelected;
            TileSelected = SwapWithTile;
            SwapWithTile = tmp;
        }

        //Get the anchor position of both tiles for movement animation
        _tileSelectedRectTransform = TileSelected.gameObject.GetComponent<RectTransform>();
        _swapWithTileRectTransform = SwapWithTile.gameObject.GetComponent<RectTransform>();
        _selectedTileAnchoredPos = _tileSelectedRectTransform.anchoredPosition;
        _swapWithTileAnchoredPos = _swapWithTileRectTransform.anchoredPosition;
        _moveTiles = true;

        //Swap the colors on the game field matrix
        int colorTmp = GameField[TileSelected.X, TileSelected.Y];
        GameField[TileSelected.X, TileSelected.Y] = SwapWithTile.TileColor;
        GameField[SwapWithTile.X, SwapWithTile.Y] = colorTmp;
        TileSelected.SwapWithTile(SwapWithTile);
    }

    /// <summary>
    /// Check for a vertical match in a given search side
    /// </summary>
    /// <param name="startTile"></param>
    /// <param name="searchSide"></param>
    /// <returns></returns>
    public List<Tile> MatchVertical(Tile startTile, VerticalSideSearch searchSide)
    {
        List<Tile> toReturn = new List<Tile>();

        if(searchSide == VerticalSideSearch.Up || searchSide == VerticalSideSearch.Both)
        {
            for(int y = startTile.Y; y >= 0; y--)
            {
                if (GameField[startTile.X, y] == startTile.TileColor)
                    toReturn.UniqueAdd(GameTiles.Find(o => o.X == startTile.X && o.Y == y));
                else
                    break;
            }
        }

        if(searchSide == VerticalSideSearch.Down || searchSide == VerticalSideSearch.Both)
        {
            for (int y = startTile.Y; y < Height; y++)
            {
                if (GameField[startTile.X, y] == startTile.TileColor)
                    toReturn.UniqueAdd(GameTiles.Find(o => o.X == startTile.X && o.Y == y));
                else
                    break;
            }
        }

        return (toReturn.Count >= 3) ? toReturn : new List<Tile>();
    }

    /// <summary>
    /// Check for a horizontal match in a given search side
    /// </summary>
    /// <param name="startTile"></param>
    /// <param name="searchSide"></param>
    /// <returns></returns>
    public List<Tile> MatchHorizontal(Tile startTile, HorizontalSideSearch searchSide)
    {
        List<Tile> toReturn = new List<Tile>();

        if(searchSide == HorizontalSideSearch.Left || searchSide == HorizontalSideSearch.Both)
        {
            for(int x = startTile.X; x >= 0; x--)
            {
                if (GameField[x, startTile.Y] == startTile.TileColor)
                    toReturn.UniqueAdd(GameTiles.Find(o => o.X == x && o.Y == startTile.Y));
                else
                    break;
            }
        }

        if (searchSide == HorizontalSideSearch.Right || searchSide == HorizontalSideSearch.Both)
        {
            for(int x = startTile.X; x < Width; x++)
            {
                if (GameField[x, startTile.Y] == startTile.TileColor)
                    toReturn.UniqueAdd(GameTiles.Find(o => o.X == x && o.Y == startTile.Y));
                else
                    break;
            }
        }

        return (toReturn.Count >= 3) ? toReturn : new List<Tile>();
    }

    /// <summary>
    /// Notifies the game master that the swap has finished.
    /// </summary>
    public void SwapFinished()
    {
        List<Tile> matchedTiles = new List<Tile>();

        //Determine from each side the initial tile swapped with the targeted tile. Used for optimization
        int checkSideHorizontal = TileSelected.X - SwapWithTile.X;
        int checkSideVertical = TileSelected.Y - SwapWithTile.Y;

        //If it swapped from right-to-left we can split the search time for both swapped tiles
        if (checkSideHorizontal < 0)
        {
            matchedTiles.AddRange(MatchHorizontal(TileSelected, HorizontalSideSearch.Left));
            matchedTiles.AddRange(MatchHorizontal(SwapWithTile, HorizontalSideSearch.Right));
            matchedTiles.AddRange(MatchVertical(TileSelected, VerticalSideSearch.Both));
            matchedTiles.AddRange(MatchVertical(SwapWithTile, VerticalSideSearch.Both));
        }
        //Swapped from right-to-left
        else if (checkSideHorizontal > 0)
        {
            matchedTiles.AddRange(MatchHorizontal(TileSelected, HorizontalSideSearch.Right));
            matchedTiles.AddRange(MatchHorizontal(SwapWithTile, HorizontalSideSearch.Left));
            matchedTiles.AddRange(MatchVertical(TileSelected, VerticalSideSearch.Both));
            matchedTiles.AddRange(MatchVertical(SwapWithTile, VerticalSideSearch.Both));
        }
        //Vertical swap
        else
        {
            //Top-to-bottom swap
            if (checkSideVertical > 0)
            {
                matchedTiles.AddRange(MatchVertical(TileSelected, VerticalSideSearch.Down));
                matchedTiles.AddRange(MatchVertical(SwapWithTile, VerticalSideSearch.Up));
            }
            //Bottom-to-top swap
            else
            {
                matchedTiles.AddRange(MatchVertical(TileSelected, VerticalSideSearch.Up));
                matchedTiles.AddRange(MatchVertical(SwapWithTile, VerticalSideSearch.Down));
            }

            matchedTiles.AddRange(MatchHorizontal(TileSelected, HorizontalSideSearch.Both));
            matchedTiles.AddRange(MatchHorizontal(SwapWithTile, HorizontalSideSearch.Both));
        }

        ProcessMatchedTiles(matchedTiles);
    }

    /// <summary>
    /// Notifies the game master that the inverse swap has finished.
    /// </summary>
    public void InverseSwapFinished()
    {
        _inverseSwap = false;
        ResetSelection();
    }

    /// <summary>
    /// Processes the matched tiles after a swap has occured
    /// </summary>
    /// <param name="matchedTiles"></param>
    private void ProcessMatchedTiles(List<Tile> matchedTiles)
    {
        //Track the columns that need to be filled after the match
        List<int> columnsToFill = new List<int>();
        //Ignore the duplicate match tiles in certain edge conditions
        List<Tile> uniqueTiles = new List<Tile>();
        matchedTiles.ForEach(x => uniqueTiles.UniqueAdd(x));

        //If we have a match after swap
        if (matchedTiles.Count > 0)
        {
            foreach (Tile tile in uniqueTiles)
            {
                if (tile != null)
                {
                    columnsToFill.UniqueAdd(tile.X);
                    GameField[tile.X, tile.Y] = -1;
                    tile.gameObject.SetActive(false);
                    tile.TileColor = 0;
                    //Add the game object to the pool queue. Note the game object does not de-parent
                    _tilePool.Enqueue(tile.gameObject);
                    GameTiles.Remove(tile);
                }
            }

            GenerateNewTiles(columnsToFill);
        }
        //If we don't have a match after swap call inverse swap
        else
        {
            _inverseSwap = true;
            Swap();
            return;
        }
    }

    /// <summary>
    /// Picks a color for newly generated tiles so it doesn't recycle with a automatic match 3
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public int PickColorForNewColumn(Tile tile)
    {
        int colorPick = Random.Range(1, ColorVariations + 1);

        //if one is stuck in a loop, randomize it and correct on the next tile itteration. The next tile will make sure it doesn't match
        int attempt = 0;
        while (CheckMatchLeftSetup(tile.X, tile.Y, colorPick) || CheckMatchTopSetup(tile.X, tile.Y, colorPick) || CheckMatchRightSetup(tile.X,tile.Y,colorPick) || CheckMatchBottomSetup(tile.X,tile.Y,colorPick))
        {          
            attempt++;
            colorPick = Random.Range(1, ColorVariations + 1);

            if(attempt >= 20)
                break;            
        }

        return colorPick;
    }

    /// <summary>
    /// Generates the missing tiles for a given column.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="missingTiles"></param>
    /// <param name="lastAssignableY"></param>
    /// <returns></returns>
    private List<Tile> GenerateTilesForColumn(int column, int missingTiles, int lastAssignableY)
    {
        List<Tile> toReturn = new List<Tile>();

        for(int i = 0; i < missingTiles; i++)
        {

            int offsetTest = 0;

            for(int y = lastAssignableY-i; y >= 0; y--)
            {
                if (GameField[column, y] == 0)
                    offsetTest++;
            }

            //Get the object from the pool
            GameObject newTile = _tilePool.Dequeue();
            Tile tile = newTile.GetComponent<Tile>();
            tile.gameObject.SetActive(true);
            tile.RectTransformTile.anchoredPosition = new Vector2(column*50f, (i+1) * 50f);
            tile.Init(column, lastAssignableY - i - offsetTest, false);
            tile.IsEmpty = false;
            tile.AssignColor(PickColorForNewColumn(tile));
            GameField[column, lastAssignableY - i - offsetTest] = tile.TileColor;
            toReturn.Add(tile);
        }

        return toReturn;
    }

    /// <summary>
    /// Check the whole game field matrix to try and find a match using horizontal and vertical match patterns
    /// </summary>
    /// <returns></returns>
    public bool CheckIfMatchIsAvialable()
    {
        bool toReturn = false;
        //Search horizontal for 2 match
        for(int x = 0; x < Width; x++)
        {
            int prevColor = -1;
            for(int y = 0; y < Height; y++)
            {
                int color = GameField[x, y];
                if(color == prevColor)
                {
                    //For every 2nd consecutive color pick in a column, try finding a potential match in a pattern
                    Vector2 topLeft = new Vector2(x - 1, y - 2);
                    Vector2 topRight = new Vector2(x + 1, y - 2);
                    Vector2 bottomRight = new Vector2(x + 1, y + 1);
                    Vector2 bottomLeft = new Vector2(x - 1, y + 1);

                    //Check if the tiles are playable for the pattern
                    Vector2 top = new Vector2(x, y - 2);
                    Vector2 bottom = new Vector2(x, y + 1);
                   

                    //Check if the positions are on the grid and if the color matches. If one match is found, return straight away
                    if(top.IsOnGrid(Width,Height) && GameField[(int)top.x,(int)top.y] > 0 && topLeft.IsOnGrid(Width,Height) && GameField[(int)topLeft.x,(int)topLeft.y] == color)
                    {
                        return true;
                    }

                    if (top.IsOnGrid(Width, Height) && GameField[(int)top.x, (int)top.y] > 0 && topRight.IsOnGrid(Width, Height) && GameField[(int)topRight.x, (int)topRight.y] == color)
                    {
                        return true;
                    }

                    if (bottom.IsOnGrid(Width, Height) && GameField[(int)bottom.x, (int)bottom.y] > 0 && bottomRight.IsOnGrid(Width, Height) && GameField[(int)bottomRight.x, (int)bottomRight.y] == color)
                    {
                        return true;
                    }

                    if (bottom.IsOnGrid(Width, Height) && GameField[(int)bottom.x, (int)bottom.y] > 0 && bottomLeft.IsOnGrid(Width, Height) && GameField[(int)bottomLeft.x, (int)bottomLeft.y] == color)
                    {
                        return true;
                    }

                }

                prevColor = color;
            }
        }

        for (int y = 0; y < Height; y++)
        {
            int prevColor = -1;
            for (int x = 0; x < Width; x++)
            {
                int color = GameField[x, y];
                if (color == prevColor)
                {
                    //For every 2nd consecutive color pick in a row, try finding a potential match in a pattern
                    Vector2 topLeft = new Vector2(x - 2, y - 1);
                    Vector2 topRight = new Vector2(x + 1, y - 1);
                    Vector2 bottomRight = new Vector2(x + 1, y + 1);
                    Vector2 bottomLeft = new Vector2(x - 2, y + 1);

                    //Check if the tiles are playable for the pattern
                    Vector2 right = new Vector2(x+1, y);
                    Vector2 left = new Vector2(x-1, y);

                    if (right.IsOnGrid(Width,Height) && GameField[(int)right.x,(int)right.y] > 0 && topLeft.IsOnGrid(Width, Height) && GameField[(int)topLeft.x, (int)topLeft.y] == color)
                    {
                        toReturn = true;
                        break;
                    }

                    if (right.IsOnGrid(Width, Height) && GameField[(int)right.x, (int)right.y] > 0 &&  topRight.IsOnGrid(Width, Height) && GameField[(int)topRight.x, (int)topRight.y] == color)
                    {
                        toReturn = true;
                        break;
                    }

                    if (left.IsOnGrid(Width, Height) && GameField[(int)left.x, (int)left.y] > 0 && bottomRight.IsOnGrid(Width, Height) && GameField[(int)bottomRight.x, (int)bottomRight.y] == color)
                    {
                        toReturn = true;
                        break;
                    }

                    if (left.IsOnGrid(Width, Height) && GameField[(int)left.x, (int)left.y] > 0 && bottomLeft.IsOnGrid(Width, Height) && GameField[(int)bottomLeft.x, (int)bottomLeft.y] == color)
                    {
                        toReturn = true;
                        break;
                    }

                }

                prevColor = color;
            }

            if (toReturn)
                break;
        }

        return toReturn;
    }

    /// <summary>
    /// Starts generating new tiles for given columns
    /// </summary>
    /// <param name="columnsToFill"></param>
    public void GenerateNewTiles(List<int> columnsToFill)
    {
        List<Tile> totalTilesToDrop = new List<Tile>();

        foreach (int column in columnsToFill)
        {
            //List of tiles that need to drop to their respective position
            List<Tile> tilesToDrop = new List<Tile>();
            int missingTiles = 0;
            int lastAvailableFreeTile = 0;
            for(int y = 0; y < Height; y++)
            {
                //If the tile is missing a color increase the counter and track the tile position
                if (GameField[column, y] == -1)
                {
                    missingTiles++;
                    lastAvailableFreeTile = y;
                }
            }

            //For the last free tile in a column try to get all filled tiles above
            for(int y = lastAvailableFreeTile-1; y >= 0; y--)
            {
                if(GameField[column,y] > 0)
                    tilesToDrop.Add(GameTiles.Find(o => o.X == column && o.Y == y));
            }

            int yOffset = 0;
            //Assign a Y position for every tile that needs to drop BEFORE new tiles are generated
            foreach(Tile tile in tilesToDrop)
            {
                tile.Y = lastAvailableFreeTile - yOffset;
                GameField[tile.X, tile.Y] = tile.TileColor;
                yOffset++;
            }

            //Generate new tiles for the column and assign their positions in the column
            List<Tile> generatedTiles = GenerateTilesForColumn(column, missingTiles, lastAvailableFreeTile - tilesToDrop.Count);
            tilesToDrop.AddRange(generatedTiles);
            //Add them back to the global tile list
            GameTiles.AddRange(generatedTiles);
            tilesToDrop.ForEach(o => totalTilesToDrop.UniqueAdd(o));
        }

        //Validate the drop list to avoid duplicates in certain edge match cases and have them drop
        totalTilesToDrop.ForEach(x => x.DropTheTile());
        SetTileDropListener(totalTilesToDrop.Count);
    }

    /// <summary>
    /// Notifies game master that a specific tile has dropped in it's position
    /// </summary>
    /// <param name="tile"></param>
    public void TileDropped(Tile tile)
    {       
        //Add it to a global list of dropped tiles to check for potential matches
        _droppedTiles.Add(tile);
        _tilesDropped++;
    }

    /// <summary>
    /// Enable the listener in the update loop to listen for tile drops and wait untill they have all dropped
    /// </summary>
    /// <param name="tilesToDrop"></param>
    private void SetTileDropListener(int tilesToDrop)
    {
        _waitForDrop = true;
        _tilesDropped = 0;
        _tilesDropping = tilesToDrop;
    }

    /// <summary>
    /// Notifies the game master that all the dropping tiles have finished the drop animation/movement
    /// </summary>
    public void DropFinished()
    {
        _waitForDrop = false;

        //Check for new matches, if not enable interaction
        List<Tile> matchedTiles = new List<Tile>();
        List<Tile> evaluatedTiles = new List<Tile>();

        foreach(Tile tile in _droppedTiles)
        {
            matchedTiles.AddRange(MatchHorizontal(tile, HorizontalSideSearch.Both));
            matchedTiles.AddRange(MatchVertical(tile, VerticalSideSearch.Both));
        }

        matchedTiles.ForEach(x => evaluatedTiles.UniqueAdd(x));

        if(evaluatedTiles.Count >= 3)
            ProcessMatchedTiles(evaluatedTiles);

        _droppedTiles = new List<Tile>();

        //If the play field becomes unplayable randomize it
        while (!CheckIfMatchIsAvialable())
            InitializeGameField();

        ResetSelection();       
    }

    private void Update()
    {
        //If the tiles are swapping this block controls the tile movement to their respective anchor positions
        if(_moveTiles)
        {
            float step = GameSpeed * Time.deltaTime;
            _tileSelectedRectTransform.anchoredPosition = Vector2.MoveTowards(_tileSelectedRectTransform.anchoredPosition, _swapWithTileAnchoredPos, step);
            _swapWithTileRectTransform.anchoredPosition = Vector2.MoveTowards(_swapWithTileRectTransform.anchoredPosition, _selectedTileAnchoredPos, step);

            if(_tileSelectedRectTransform.anchoredPosition == _swapWithTileAnchoredPos && _swapWithTileRectTransform.anchoredPosition == _selectedTileAnchoredPos)
            {
                _moveTiles = false;
                if (!_inverseSwap)
                    SwapFinished();
                else
                    InverseSwapFinished();           
            }
        }

        //Tile drop listener. Checks if all the dropping tiles have notified game master. Can be implemented differently now that i think of it
        if(_waitForDrop)
        {
            if(_tilesDropped >= _tilesDropping)
            {
                DropFinished();
            }
        }
    }
    #endregion
    #region Simulator

    /// <summary>
    /// Starts the simulator.
    /// </summary>
    public void StartSimulator()
    {
        Debug.Log("Simulator started. Simulating real player moves");
        _isSimulatorRunning = true;
        PickTiles();
    }

    /// <summary>
    /// Picks random tiles for the simulator.
    /// </summary>
    public void PickTiles()
    {
        //Total iterations the simulator does are set in the editor
        if(_iterationsMade >= Iterations)
        {
            Debug.Log("Simulator done");
            return;
        }

        _iterationsMade++;

        //Pick a tile that's not at the edges to allow 4 random directions
        int randomX = Random.Range(1, Width - 1);
        int randomY = Random.Range(1, Height - 1);
        while(GameField[randomX,randomY] <= 0)
        {
            randomX = Random.Range(1, Width - 1);
            randomY = Random.Range(1, Height - 1);
        }
        Tile randomStartTile = GameTiles.Find(o => o.X == randomX && o.Y == randomY);
        Tile switchWithTile = null;
        int[] directions = new int[] { -1, 1 };
        int directionDecider = Random.Range(0, 2);
        int offset = directions[Random.Range(0, 2)];

        if (directionDecider == 0)
        {
            switchWithTile = GameTiles.Find(o => o.X == randomStartTile.X + offset && o.Y == randomStartTile.Y);
        }
        else
        {
            switchWithTile = GameTiles.Find(o => o.Y == randomStartTile.Y + offset && o.X == randomStartTile.X);
        }

        if (!switchWithTile.IsBlocking)
            SimMove(randomStartTile, switchWithTile);
        else
            PickTiles();
    }

    /// <summary>
    /// Performs a swap between 2 tiles picked by the simulator
    /// </summary>
    /// <param name="one"></param>
    /// <param name="two"></param>
    public void SimMove(Tile one, Tile two)
    {
        TileSelected = one;
        SwapWithTile = two;
        Swap();
    }

    #endregion
}
