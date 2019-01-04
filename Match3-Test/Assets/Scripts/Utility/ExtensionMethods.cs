using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides extension methods
/// </summary>
public static class ExtensionMethods {
    #region Extension Methods

    /// <summary>
    /// Adds an item to the list only if it doesn't exist in the list beforehand
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="toAdd"></param>
    public static void UniqueAdd<T>(this List<T> list, T toAdd)
    {
        if (!list.Contains(toAdd))
            list.Add(toAdd);
    }
    /// <summary>
    /// Checks if the given x,y position is on the grid with specified width and height
    /// </summary>
    /// <param name="gridPos"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static bool IsOnGrid(this Vector2 gridPos, int width, int height)
    {
        return (gridPos.x < 0 || gridPos.y < 0 || gridPos.x > width - 1 || gridPos.y > height - 1) ? false : true;
    }

    #endregion
}
