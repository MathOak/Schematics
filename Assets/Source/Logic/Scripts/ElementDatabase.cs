using AlmostEngine.Screenshot;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Schema;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "database_", menuName = "Database")]
public class ElementDatabase : ScriptableObject
{
    [SerializeField] private List<BaseElement> allElements;
    private Dictionary<string, BaseElement> _elements;
    private static ElementDatabase Instance;
    public static Dictionary<string, BaseElement> Elements => Instance._elements;

    public void Initialize()
    {
        _elements = new Dictionary<string, BaseElement>();

        foreach (var element in allElements)
        {
            if (!element.Key.IsNullOrWhitespace())
                _elements.Add(element.Key, element);
        }

        Instance = this;
    }

    public void AddElement(BaseElement element)
    {
        if (!element.Key.IsNullOrWhitespace())
            _elements.Add(element.Key, element);
    }

    public BaseElement GetElement(string key)
    {
        return _elements.ContainsKey(key) ? _elements[key] : null;
    }

    public void LogPartsKeys()
    {
        string partsList = "[Current Parts]";

        foreach (var part in allElements)
        {
            partsList += $"\nKEY: | {part.Key} | PART: {part.ElementName}";
        }

        Logger.Info(partsList);
    }

 #if UNITY_EDITOR
    [Button]
    public void UpdatePartsDatabase()
    {
        List<BaseElement> elements = new List<BaseElement>();

        var guids = AssetDatabase.FindAssets("t:BaseElement");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            elements.Add(AssetDatabase.LoadAssetAtPath<BaseElement>(path));
        }

        CheckIfAnyElementHaveTheSameKey();
        allElements = elements;
    }

    public bool CheckIfAnyElementHaveTheSameKey()
    {
        var allKeys = new List<string>();

        foreach (var element in allElements)
        {
            if (allKeys.Contains(element.Key))
            {
                Logger.Error($"Element {element.name} have the same key as another element!");
                return true;
            }

            allKeys.Add(element.Key);
        }

        return false;
    }
#endif
}