using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

public static partial class UsefulExtensions
{
   

    /// <summary>
    /// Allows for the batch activation/deactivation of GameObjects
    /// </summary>
    /// <param name="list"></param>
    /// <param name="isActive"></param>
    public static void SetActive(this List<GameObject> list, bool isActive)
    {
        foreach (var item in list)
        {
            if (item != null)
            {
                item.SetActive(isActive);
            }
        }
    }

    public static void SetActive(this List<GameObject> list, bool isActive, float delay)
    {
        foreach (var item in list)
        {
            if (item != null)
            {
                item.SetActive(isActive,delay);
            }
        }
    }

   

   
    /// <summary>
    /// Recursivly sets the tags of all children objects
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="tag"></param>
    public static void SetTagsRecursive(this GameObject parent, string tag)
    {
        for(int i = 0;i< parent.transform.childCount; i++)
        {
            SetTagsRecursive(parent.transform.GetChild(i).gameObject, tag);
        }
        parent.tag = tag;
    }

    /// <summary>
    /// Recursivly sets the tags of all children objects
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="tag"></param>
    public static void SetLayersRecursive(this GameObject parent, int mask)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            SetLayersRecursive(parent.transform.GetChild(i).gameObject, mask);
        }
        parent.layer = mask;
    }

   

  
   
    /// <summary>
    /// Disables the gameobject of a timer
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="timer"></param>
    public async static void DisableAfterSeconds(this GameObject obj,float timer)
    {
        await WaitSeconds(timer);
        obj.SetActive(false);
    }



  
    public async static void SetActive(this GameObject gameObejct, bool isActive, float delay)
    {
        if (gameObejct)
        {
            await WaitSeconds(delay);
            gameObejct.SetActive(isActive);
        }
    }

   

    /// <summary>
    /// Simple shortcut to delete persistent data with game that use it.
    /// </summary>
#if UNITY_EDITOR
    [MenuItem("Tools/Useful/Clean Persistent Files")]
    public static void CleanPersistentFiles()
    {
        Directory.Delete(Application.persistentDataPath);
        Directory.CreateDirectory(Application.persistentDataPath);
    }
    [MenuItem("Tools/Useful/Load First Scene")]
    public static void LoadFirstScene()
    {

        EditorSceneManager.OpenScene(EditorSceneManager.GetSceneByBuildIndex(0).path);
    }

#endif

}

public class Logger{
    public static void Log(string message)
    {
#if UNITY_EDITOR || DEBUG
        Debug.Log(message);
#endif
    }

    public static void Log(string message,Color color)
    {
#if UNITY_EDITOR || DEBUG
        string colorTag = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string messageWithTag = $"{colorTag}{message}</color>";
        Debug.LogFormat(messageWithTag);
#endif
    }
    public static void Log(string tag,string message, Color color)
    {
#if UNITY_EDITOR || DEBUG
        message = "[" + tag + "]: " + message;
        string colorTag = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        string messageWithTag = $"{colorTag}{message}</color>";
        Debug.LogFormat(messageWithTag);
#endif
    }
    public static void Log(string tag,string message)
    {
#if UNITY_EDITOR || DEBUG
        message = "[" + tag + "]: " + message;
        Debug.LogFormat(message);
#endif
    }
}

