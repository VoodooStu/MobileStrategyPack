using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Conatains all custom classes including save and load functions for serializable classes
/// </summary>
public static partial class UsefulExtensions
{
    /// <summary>
    /// Simplifies loading a saved class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultReturn"></param>
    /// <returns></returns>
    public static T GetClass<T>(string key, T defaultReturn)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            return defaultReturn;
        }
        string saveString = PlayerPrefs.GetString(key);
        if (string.IsNullOrEmpty(saveString))
        {
            return defaultReturn;
        }
        T obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(saveString);
        return obj;

    }
    /// <summary>
    /// Simplifies saving a class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultReturn"></param>
    /// <returns></returns>
    public static void SaveClass<T>(string key, T classToSave)
    {
        var saveString = Newtonsoft.Json.JsonConvert.SerializeObject(classToSave);
        PlayerPrefs.SetString(key, saveString);
        PlayerPrefs.Save();
    }

}

/// <summary>
/// Wrapper class used to save Vector 3 types
/// </summary>
[Serializable]
public class SerializableVector3
{
    public float X;
    public float Y;
    public float Z;
    public Vector3 Get()
    {
        return new Vector3(X, Y, Z);
    }
    public SerializableVector3(Vector3 vec)
    {
        X = vec.x;
        Y = vec.y;
        Z = vec.z;
    }
}


public class SavedDateTime
{
    private string _saveString;
    private DateTime _currentValue = DateTime.MinValue;
    private DateTime _defaultValue;
    private bool _saveOnChange = false;
    private Action _onChanged = null;

    

    public SavedDateTime(string saveString, DateTime defaultValue, bool saveOnChange, Action onChange)
    {
        _saveString = saveString;
        _defaultValue = defaultValue;
        _onChanged = onChange;
        _saveOnChange = saveOnChange;
        _currentValue = DateTime.MinValue;
        if (!PlayerPrefs.HasKey(_saveString))
        {
            Value = _defaultValue;
        }
    }

    public void Save()
    {
        PlayerPrefs.SetString(_saveString, _currentValue.ToFileTimeUtc().ToString());
    }

    public DateTime Value
    {
        get
        {
            if (_currentValue == DateTime.MinValue)
            {
                string savedValue = PlayerPrefs.GetString(_saveString,_defaultValue.ToFileTimeUtc().ToString() );
                long val;
                if (long.TryParse(savedValue,out val))
                {
                    _currentValue = DateTime.FromFileTimeUtc(val);
                }
                else
                {
                    _currentValue = _defaultValue;
                }
               
            }
            return _currentValue;
        }
        set
        {
            _currentValue = value;
            if (_saveOnChange)
                Save();
            _onChanged?.Invoke();
        }
    }

    public override string ToString()
    {
        return Value.ToString("o");
    }

    public static implicit operator DateTime(SavedDateTime v)
    {
        return v.Value;
    }
}

/// <summary>
/// Allows for simple creation of persistent ints
/// </summary>
public class SavedInt
{
    private string _saveString;
    private int _currentValue = int.MinValue;
    private int _defaultValue;
    private bool _saveOnChange = false;
    /// <summary>
    /// Callback to track changes to the value
    /// </summary>
    Action _onChanged = null;
    /// <summary>
    /// Saved Bool creator
    /// </summary>
    /// <param name="saveString"></param>
    /// Key used by player prefs
    /// <param name="defaultValue"></param>
    /// Default value set on create
    /// <param name="saveOnChange"></param>
    /// Option to set the value using player prefs whenever the value is changed. If set to false use the Save function to record the value
    /// <param name="onChange"></param>
    /// Callback that is sent when the value is changed
    public SavedInt(string saveString, int defaultValue, bool saveOnChange, Action onChange)
    {
        _saveString = saveString;
        _defaultValue = defaultValue;
        _onChanged = onChange;
        _saveOnChange = saveOnChange;
    }

    public void Save()
    {
        PlayerPrefs.SetInt(_saveString, _currentValue);
    }
    /// <summary>
    /// Public value used to retrive the saved data
    /// </summary>
    public int Value
    {
        get
        {
            if (_currentValue == int.MinValue)
            {
                Value = PlayerPrefs.GetInt(_saveString, _defaultValue);
            }
            return _currentValue;
        }
        set
        {
            _currentValue = value;
            if (_saveOnChange)
                Save();
            _onChanged?.Invoke();
        }
    }
    public override string ToString()
    {
        return Value.ToString();
    }


    public static implicit operator int(SavedInt v)
    {
        return v.Value;
    }

}
/// <summary>
/// Allows for simple creation of persistent booleans
/// </summary>
public class SavedBool
{
    private string _saveString;
    private bool _currentValue = false;
    private bool _defaultValue;
    private bool _saveOnChange = false;
    /// <summary>
    /// Callback to track changes to the value
    /// </summary>
    Action _onChanged = null;
    /// <summary>
    /// Saved Bool creator
    /// </summary>
    /// <param name="saveString"></param>
    /// Key used by player prefs
    /// <param name="defaultValue"></param>
    /// Default value set on create
    /// <param name="saveOnChange"></param>
    /// Option to set the value using player prefs whenever the value is changed. If set to false use the Save function to record the value
    /// <param name="onChange"></param>
    /// Callback that is sent when the value is changed
    public SavedBool(string saveString, bool defaultValue, bool saveOnChange, Action onChange)
    {
        _saveString = saveString;
        _defaultValue = defaultValue;
        _onChanged = onChange;
        _saveOnChange = saveOnChange;
    }

    public void Save()
    {
        PlayerPrefs.SetInt(_saveString, _currentValue ? 1 : 0);
    }
    bool Initialised = false;

    /// <summary>
    /// Public value used to retrive the saved data
    /// </summary>
    public bool Value
    {
        get
        {
            if (!Initialised)
            {
                Initialised = true;
                Value = PlayerPrefs.GetInt(_saveString, _defaultValue ? 1 : 0) == 1;
            }
            return _currentValue;
        }
        set
        {
            _currentValue = value;
            if (_saveOnChange)
                Save();
            _onChanged?.Invoke();
        }
    }
    public override string ToString()
    {
        return Value.ToString();
    }


    public static implicit operator bool(SavedBool v)
    {
        return v.Value;
    }

}
/// <summary>
/// Allows for simple creation of persistent strings
/// </summary>
public class SavedString
{
    private string _saveString;
    private string _currentValue;
    private string _defaultValue;
    private bool _saveOnChange = false;
    /// <summary>
    /// Callback to track changes to the value
    /// </summary>
    Action _onChanged = null;
    /// <summary>
    /// Saved Bool creator
    /// </summary>
    /// <param name="saveString"></param>
    /// Key used by player prefs
    /// <param name="defaultValue"></param>
    /// Default value set on create
    /// <param name="saveOnChange"></param>
    /// Option to set the value using player prefs whenever the value is changed. If set to false use the Save function to record the value
    /// <param name="onChange"></param>
    /// Callback that is sent when the value is changed
    public SavedString(string saveString, string defaultValue, bool saveOnChange, Action onChange)
    {
        _saveString = saveString;
        _defaultValue = defaultValue;
        _onChanged = onChange;
        _saveOnChange = saveOnChange;
    }

    public void Save()
    {
        PlayerPrefs.SetString(_saveString, _currentValue);
    }
    bool Initialised = false;
    /// <summary>
    /// Public value used to retrive the saved data
    /// </summary>
    public string Value
    {
        get
        {
            if (!Initialised)
            {
                Initialised = true;
                Value = PlayerPrefs.GetString(_saveString, _defaultValue);
            }
            return _currentValue;
        }
        set
        {
            _currentValue = value;
            if (_saveOnChange)
                Save();
            _onChanged?.Invoke();
        }
    }
    public override string ToString()
    {
        return Value.ToString();
    }

}
/// <summary>
/// Allows for simple creation of persistent floats
/// </summary>
public class SavedFloat
{
    private string _saveString;
    private float _currentValue = float.MinValue;
    private float _defaultValue;
    private bool _saveOnChange = false;
    /// <summary>
    /// Callback to track changes to the value
    /// </summary>
    Action _onChanged = null;
    /// <summary>
    /// Saved Bool creator
    /// </summary>
    /// <param name="saveString"></param>
    /// Key used by player prefs
    /// <param name="defaultValue"></param>
    /// Default value set on create
    /// <param name="saveOnChange"></param>
    /// Option to set the value using player prefs whenever the value is changed. If set to false use the Save function to record the value
    /// <param name="onChange"></param>
    /// Callback that is sent when the value is changed
    public SavedFloat(string saveString, float defaultValue, bool saveOnChange, Action onChange)
    {
        _saveString = saveString;
        _defaultValue = defaultValue;
        _onChanged = onChange;
        _saveOnChange = saveOnChange;
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(_saveString, _currentValue);
    }
    /// <summary>
    /// Public value used to retrive the saved data
    /// </summary>
    public float Value
    {
        get
        {
            if (_currentValue == float.MinValue)
            {
                Value = PlayerPrefs.GetFloat(_saveString, _defaultValue);
            }
            return _currentValue;
        }
        set
        {
            _currentValue = value;
            if (_saveOnChange)
                Save();
            _onChanged?.Invoke();
        }
    }
    public override string ToString()
    {
        return Value.ToString();
    }
    public static implicit operator float(SavedFloat v)
    {
        return v.Value;
    }
    

}

