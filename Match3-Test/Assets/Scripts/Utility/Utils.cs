using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Standard utility class for the game
/// </summary>
public class Utils {
    #region Utility Functions
    /// <summary>
    /// Assigns a color to a provided int value. Did it like this for demo purposes
    /// </summary>
    /// <param name="tileColor"></param>
    /// <returns></returns>
    public static Color IntToColor(int tileColor)
    {
        switch(tileColor)
        {
            case 1:
                return Color.red;
            case 2:
                return Color.blue;
            case 3:
                return Color.green;
            case 4:
                return Color.yellow;
            case 5:
                return Color.white;
            case 6:
                return Color.cyan;
            default:
                return Color.black;
        }
    }
    #endregion
}
#region Enums
/// <summary>
/// Options for horizontal search
/// </summary>
public enum HorizontalSideSearch
{
    Left,
    Right,
    Both
}

/// <summary>
/// Options for vertical search
/// </summary>
public enum VerticalSideSearch
{
    Up,
    Down,
    Both
}
#endregion
