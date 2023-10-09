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

public static class Logger
{
    private enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public static void Info(string message) => Log(message, LogLevel.Info);
    public static void Warning(string message) => Log(message, LogLevel.Warning);
    public static void Error(string message) => Log(message, LogLevel.Error);
    public static void Error(Exception e) 
    {
        if (Application.isEditor)
        {
            Debug.LogException(e);
        }
        else 
        {
            Log(e.ToString(), LogLevel.Error);
        }
    }

    private static void Log(string message, LogLevel level = LogLevel.Info)
    {
        var log = "";
        switch (level)
        {
            case LogLevel.Warning:
                log = $"[WARNING] {message}";
                Debug.LogWarning(log);
                break;

            case LogLevel.Error:
                log = $"[ERROR] {message}";
                Debug.LogError(log);
                UnityWebInteractions.LogError(log);
                break;

            case LogLevel.Info:
            default:
                log = $"[INFO] {message}";
                Debug.Log(log);
                UnityWebInteractions.LogInfo(log);
                break;
        }
    }
}
