using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor component for extending GameMaster inspector window
/// </summary>
[CustomEditor(typeof(GameMaster))]
public class GameMasterInspector : Editor {

    #region Serializable Properties
    SerializedProperty width;
    SerializedProperty height;
    SerializedProperty colorVariations;
    SerializedProperty swapSpeed;
    SerializedProperty tilePrefab;
    SerializedProperty totalIterations;
    SerializedProperty simSpeed;
    #endregion
    #region Editor
    void OnEnable()
    {
        width = serializedObject.FindProperty("Width");
        height = serializedObject.FindProperty("Height");
        colorVariations = serializedObject.FindProperty("ColorVariations");
        swapSpeed = serializedObject.FindProperty("SwapSpeed");
        tilePrefab = serializedObject.FindProperty("TilePrefab");
        totalIterations = serializedObject.FindProperty("Iterations");
        simSpeed = serializedObject.FindProperty("SimulationSpeed");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(width);
        EditorGUILayout.PropertyField(height);
        EditorGUILayout.PropertyField(colorVariations);
        EditorGUILayout.PropertyField(swapSpeed);
        EditorGUILayout.PropertyField(tilePrefab);      
        if (GUILayout.Button("Randomize field"))
        {
            ((GameMaster)target).InitializeGameField();
        }
        EditorGUILayout.PropertyField(totalIterations);
        EditorGUILayout.PropertyField(simSpeed);
        if(GUILayout.Button("Start simulation"))
        {
            ((GameMaster)target).StartSimulator();
        }
        serializedObject.ApplyModifiedProperties();
    }
    #endregion
}
