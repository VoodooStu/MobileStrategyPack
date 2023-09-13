using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static partial class UsefulExtensions
{
    /// <summary>
    /// Extents the TimeSpan class to print out a reader friendly version
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public static string ToTimerString(this System.TimeSpan span, bool ignoreSeconds = false)
    {
        if (span.TotalDays > 1)
        {
            return (int)span.TotalDays + "d " + span.Hours.ToString("00") + "h";
        }
        else if (span.TotalHours > 1 || ignoreSeconds)
        {
            return (int)span.TotalHours + "h " + span.Minutes.ToString("00") + "min";
        }
        else if (span.TotalMinutes > 1)
        {
            return span.Minutes + "min " + span.Seconds.ToString("00") + "s";
        }
        else
        {
            return span.Seconds.ToString() + "s";
        }
    }

    /// <summary>
    /// Rounds a number to the best
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int Round(this int value)
    {
        if (value < 10)
        {
            return value;
        }
        else if (value < 100)
        {
            return value.Round(10);
        }
        else if (value < 1000)
        {
            return value.Round(100);
        }
        else if (value < 10000)
        {
            return value.Round(1000);
        }
        else
        {
            return value.Round(10000);
        }
    }

    /// <summary>
    /// Rounds a number given a specified factor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static int Round(this int value, int factor)
    {
        if (value < factor)
        {
            return value;
        }
        return (value / factor) * factor;
    }

    public static string ToReadableString(this int value)
    {
        if (value < 1000)
        {
            return value.ToString();
        }
        else if (value < 1000000)
        {
            return (value / 1000).ToString() + "K";
        }
        else
        {
            return (value / 1000000).ToString() + "M";
        }
        
    }

    public static string ToReadableString(this float value)
    {
        if (value < 1000f)
        {
            return value.ToString("n1");
        }
        else if (value < 1000000f)
        {
            return (value / 1000f).ToString("n1") + "K";
        }
        else
        {
            return (value / 1000000f).ToString("n1") + "M";
        }
    }


}
