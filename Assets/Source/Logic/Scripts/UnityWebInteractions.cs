
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

public class UnityWebInteractions
{
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
    [DllImport("__Internal")]
    private static extern void InternalGeneratorBootListener();

    [DllImport("__Internal")]
    private static extern void InternalBase64Generated(string base64);

    [DllImport("__Internal")]
    private static extern void InternalGeneratorQuitListener();

    [DllImport("__Internal")]
    public static extern void InternalUnityLogger(string message);

    [DllImport("__Internal")]
    public static extern void InternalUnityErrorLogger(string errorMessage);
#endif

    // Wrapper methods
    public static void GeneratorBootListener()
    {
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
        InternalGeneratorBootListener();
#else
        SchematicDebugManager.GenerateDebugSchematic();
#endif
    }

    public static void SendGeneratedTexture(Texture2D finalTexture)
    {
        var jpgBytes = finalTexture.EncodeToJPG();
        var base64 = Convert.ToBase64String(jpgBytes);
        Logger.Info($"Schematic Rendered! Returning Base64...");

#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
        InternalBase64Generated(base64);
#else
        if (Application.isEditor)
        {
            Debug.Log(base64);
        }

        string filePath = Path.Combine(Application.streamingAssetsPath, "generator_result.jpg");
        File.WriteAllBytes(filePath, jpgBytes);
#endif
    }

    public static void QuitApplication()
    {
        Logger.Info("Closing Web Module...");
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
        InternalGeneratorQuitListener();
#elif UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public static void LogInfo(string message)
    {
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
        InternalUnityLogger(message);
#endif
    }

    public static void LogError(string errorMessage)
    {
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
        InternalUnityErrorLogger(errorMessage);
#endif
        QuitApplication();
    }

}
