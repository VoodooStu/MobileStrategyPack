using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Contains helper classes for all functions that help with UnityEngine.Random
/// </summary>
public static partial class UsefulExtensions
{
    /// <summary>
    /// Simplified randomisation
    /// </summary>
    /// <param name="chance"></param>
    /// <returns></returns>
    public static bool Choose(this float chance)
    {

        return UnityEngine.Random.Range(0, 100f) < chance;
    }
    /// <summary>
    /// Simplified usage of the Random function.
    /// </summary>
    /// <param name="range"></param>
    /// <param name="baseRange"></param>
    /// <returns></returns>
    public static float Range(this float range, float baseRange)
    {

        return UnityEngine.Random.Range(baseRange, range);


    }
    /// <summary>
    /// Allows for a random range to be created using a single number.
    /// </summary>
    /// <param name="range"></param>
    /// <param name="baseRange"></param>
    /// <returns></returns>
    public static float Range(this float range)
    {
        if (range > 0)
        {
            return range.Range(0);

        }
        else
        {
            return 0f.Range(range);

        }

    }
}
