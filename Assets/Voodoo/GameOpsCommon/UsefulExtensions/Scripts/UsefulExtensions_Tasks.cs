using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// Contains all helper extensions for Unity Tasks
/// </summary>
public static partial class UsefulExtensions
{
    /// <summary>
    /// Allows the use of the Task class with seconds like a Coroutine.
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static async Task WaitSeconds(float seconds)
    {
        await Task.Delay((int)(seconds * 1000f));
    }
}
