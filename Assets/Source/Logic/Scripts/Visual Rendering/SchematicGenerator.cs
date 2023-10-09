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

public class SchematicGenerator : SerializedMonoBehaviour
{
    [SerializeField] private UIGenerator uiGenerator;
    [SerializeField] private SchematicDrawer drawer;
    [SerializeField] private ElementDatabase elementDatabase;

    public static Schematic lastGeneration { get; private set; }

    private void Start()
    {
        try
        {
            elementDatabase.Initialize();
            UnityWebInteractions.GeneratorBootListener();
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    public async UniTask GenerateFromJson(string jsonString) => await GenerateFromJsonAsync(jsonString);
    public async UniTask GenerateFromJsonAsync(string jsonString) => await AsyncGenerator(ConvertJsonToSchematic(jsonString));

    public Schematic ConvertJsonToSchematic(string jsonString)
    {
        try
        {
            Logger.Info("Converting Json Data to Schematic Object... ");
            JsonApiHandler jsonSchematic = JsonUtility.FromJson<JsonApiHandler>(jsonString);
            Schematic schematic = jsonSchematic.data.ToObject();
            return schematic;
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return null;
        }
    }

    public async UniTask AsyncGenerator(Schematic schematic) 
    {
        try
        {
            Logger.Info("Starting Unity Generation Process");
            VisualElement.ClearElements();
            Loading.instance.AddMaxProgress(schematic.parts.Count);

            await drawer.DrawSchematic(schematic);
            await uiGenerator.DrawUIText(schematic);

            lastGeneration = schematic;
            VisualElement.ChangeItemsColor();
            SchematicRenderer.RenderSchematic();
            Logger.Info("Schematic Generated!");

            if (!Application.isEditor)
            {
                UnityWebInteractions.QuitApplication();
            }
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    struct JsonApiHandler
    {
        public bool elementDataError;
        public Schematic.JsonObject data;
    }
}