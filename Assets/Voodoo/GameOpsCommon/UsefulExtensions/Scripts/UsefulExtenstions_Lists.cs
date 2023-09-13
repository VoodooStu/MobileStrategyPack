using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Conatains all functions that extend the list class
/// </summary>
public static partial class UsefulExtensions
{
    /// <summary>
    /// Returns a copy of a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="originalList"></param>
    /// <returns></returns>
    public static List<T> Copy<T>(this List<T> originalList)
    {
        List<T> newList = new List<T>();
        foreach (var obj in originalList)
        {
            newList.Add(obj);
        }
        return newList;
    }
    /// <summary>
    /// Removes any items that are null in a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<T> RemoveEmpty<T>(this List<T> list)
    {
        var modifiedList = list.Copy();
        for (int i = 0; i < modifiedList.Count; i++)
        {
            if (modifiedList[i] == null)
            {
                modifiedList.RemoveAt(i);
                i--;
            }
        }
        return modifiedList;
    }
    /// <summary>
    /// Returns a random item from a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T GetRandom<T>(this List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
    /// <summary>
    /// Retrieves an item from a list and clamps your index to ensure that exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static T GetClamped<T>(this List<T> list, int index)
    {
        return list[Mathf.Clamp(index, 0, list.Count - 1)];
    }
    /// <summary>
    /// Shuffles your list into a random order
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<T> Shuffle<T>(this List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var choice = list[i];
            list.RemoveAt(i);
            list.Insert(UnityEngine.Random.Range(0, list.Count), choice);
        }
        return list;
    }

    /// <summary>
    /// Returns a random item from a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<T> GetRandomRange<T>(this List<T> list, int count)
    {
        List<T> TempList = new List<T>();
        TempList.AddRange(list);
        List<T> ReturnList = new List<T>();
        while (TempList.Count > 0 && ReturnList.Count < count)
        {
            T item = TempList.GetRandom();
            TempList.Remove(item);
            ReturnList.Add(item);
        }

        return ReturnList;
    }

}
