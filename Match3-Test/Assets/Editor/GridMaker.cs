using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor component for creating custom grid.
/// </summary>
public class GridMaker : EditorWindow {

    #region Variables
    //Tile prefab used to build the grid
    private GameObject _tilePrefab;
    //Width of the grid
    private int _width;
    //Height of the grid
    private int _height;
    //Total colors used in the game
    private int _colors;
    //Used as a tracker in editor to check if the GUI grid has been built
    private bool _gridBuilt;
    //Used as a tracker in editor to check if the grid has been built in the scene
    private bool _sceneBuilt;
    //Used to hold information about the grid layout. Stores blocked tiles and available ones
    private static bool[,] _gridTileLayout;
    //Reference to the grid holder game object in the scene
    private GameObject _gridHolder;
    //Reference to the GameMaster component
    private GameMaster _gameMaster;
    #endregion
    #region Editor
    [MenuItem("Window/Grid Builder")]
    public static void ShowWindow()
    {
        EditorWindow getWindow = EditorWindow.GetWindow(typeof(GridMaker));
        getWindow.titleContent.text = "Grid Builder";      
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid parameters", EditorStyles.boldLabel);
        _tilePrefab = EditorGUILayout.ObjectField("Tile prefab", _tilePrefab, typeof(GameObject), false) as GameObject;
        _width = EditorGUILayout.IntField("Width", _width);
        _height = EditorGUILayout.IntField("Height", _height);
        _colors = EditorGUILayout.IntField("Color variations", _colors);

        if(GUILayout.Button("Build grid"))
        {
            _gridBuilt = true;
            _sceneBuilt = false;
            SetInitialTileLayout();
            DrawGridScene();
            Repaint();
        }

        if (GUILayout.Button("Destroy grid"))
        {
            _gridBuilt = false;
            DestroyGridScene();
            _sceneBuilt = false;
            Repaint();
        }

        if (_gridBuilt)
        {

            GUILayout.Label("(Optional) Draw grid layout", EditorStyles.boldLabel);
            DrawGridEditor();        
        }
    }

    /// <summary>
    /// Sets the initial tile layout to all available
    /// </summary>
    private void SetInitialTileLayout()
    {
        //1 is available tile to populate, 0 is blocked
        _gridTileLayout = new bool[_width, _height];
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                _gridTileLayout[x, y] = true;
    }

    /// <summary>
    /// Draws the grid in the editor.
    /// </summary>
    private void DrawGridEditor()
    {
        using (var horizontalScope = new GUILayout.HorizontalScope())
        {
            for (int x = 0; x < _width; x++)
            {
                GUILayout.BeginVertical();
                for (int y = 0; y < _height; y++)
                {
                    Color buttonColor = (!_gridTileLayout[x,y]) ? Color.red : Color.white;
                    GUI.backgroundColor = buttonColor;
                    
                    if(GUILayout.Button(""))
                    {
                        _gridTileLayout[x, y] = !_gridTileLayout[x,y];
                        Repaint();
                    }

                }
                GUILayout.EndVertical();
            }
        }

        if(GUILayout.Button("Rebuild grid"))
        {
            DestroyGridScene();
            DrawGridScene();
        }
    }

    /// <summary>
    /// Draws the grid in the scene
    /// </summary>
    private void DrawGridScene()
    {
        _gameMaster = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        _gridHolder = GameObject.Find("GridHolder");
        _gameMaster.GameTiles = new List<Tile>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GameObject go = PrefabUtility.InstantiatePrefab(_tilePrefab) as GameObject;
                 
                go.name = "Tile" + x + y;
                go.transform.parent = _gridHolder.transform;
                go.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x*50f, -y*50f, 0f);

                if (!_gridTileLayout[x, y])
                    go.SetActive(false);

                go.GetComponent<Tile>().Init(x, y, !_gridTileLayout[x,y]);
                _gameMaster.GameTiles.Add(go.GetComponent<Tile>());
            }
        }

        _gameMaster.Init(_width,_height,_colors);
    }

    /// <summary>
    /// Destroys the grid in the scene.
    /// </summary>
    private void DestroyGridScene()
    {
        for (int i = _gridHolder.transform.childCount; i > 0; --i)
            DestroyImmediate(_gridHolder.transform.GetChild(0).gameObject);

        GameObject.Find("GameMaster").GetComponent<GameMaster>().DeInit();
    }
    #endregion

}
