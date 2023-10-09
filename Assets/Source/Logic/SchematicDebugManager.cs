using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class SchematicDebugManager : MonoBehaviour
{
#if UNITY_EDITOR
    private enum TestMode
    {
        Inspector,
        JsonString,
        WebApi
    }

    private enum ConvertMode 
    {
        none,
        diagram,
        schematic
    }

    [Space]
    [SerializeField] private TestMode getSchematicFrom;
    [SerializeField] private ConvertMode convertResultTo;

    [ShowIf("IsDebugFromInspectorList")]
    [SerializeField] private Schematic debugSchematic;

    [ShowIf("IsDebugJsonString")]
    [SerializeField]
    [TextArea]
    private string jsonSchematicText;

    [SerializeField] private bool writeApiKeyNames;

    private static SchematicDebugManager _instance;
    SchematicGenerator _schematicGenerator;

    private bool IsDebugFromInspectorList => getSchematicFrom == TestMode.Inspector;
    private bool IsDebugWebApi => getSchematicFrom == TestMode.WebApi;
    private bool IsDebugJsonString => getSchematicFrom == TestMode.JsonString;

    private void Awake()
    {
        _instance = this;
        _schematicGenerator = FindObjectOfType<SchematicGenerator>();
    }

    [Button]
    public void GenerateSchematic()
    {
        if (Application.isPlaying && debugSchematic != null)
        {
            UnityStart().Forget();
        }
    }

    public static void GenerateDebugSchematic()
    {
        if (_instance != null)
        {
            _instance.GenerateSchematic();
        }
    }

    private async UniTask UnityStart()
    {
        switch (getSchematicFrom)
        {
            case TestMode.Inspector:
                AssignElementsToSchematic();
                await _schematicGenerator.AsyncGenerator(debugSchematic);
                SchematicRenderer.RenderSchematic();
                break;

            case TestMode.JsonString:
                await GenerateFromJson();
                break;

            case TestMode.WebApi:
                jsonSchematicText = await GetJsonFromAPIAsync();
                await GenerateFromJson();
                break;

            default:
                break;
        }
    }

    private async UniTask GenerateFromJson()
    {
        var schematic = _schematicGenerator.ConvertJsonToSchematic(jsonSchematicText);

        if (convertResultTo != ConvertMode.none) 
        {
            schematic._isDiagram = (convertResultTo == ConvertMode.diagram);
        }

        if (writeApiKeyNames) 
        {
            ShowApiKeyNames(schematic);
        }

        await _schematicGenerator.AsyncGenerator(schematic);
    }

    private void AssignElementsToSchematic()
    {
        foreach (var item in debugSchematic.GetAllParts())
        {
            if (item.useElementKey)
                item.element = ElementDatabase.Elements[item.elementKey];
        }
    }

    private void ShowApiKeyNames(Schematic schematic)
    {
        schematic.GetAllParts().ForEach(part => part._virtualName = part.element.Key);
    }

    private async UniTask<string> GetJsonFromAPIAsync()
    {
        string apiUrl = "https://api.dbragas.com:4010/schematics/schematic/12df826a-6e51-4abf-a2f6-3baf620d2f11";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            // Envia a requisição e aguarda a resposta
            await webRequest.SendWebRequest();

            // Verifica se ocorreu algum erro na requisição
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro na requisição: " + webRequest.error);
            }
            else
            {
                string apiString = webRequest.downloadHandler.text;
                Debug.Log("String da API: " + apiString);
                return apiString;
            }
        }

        return "";
    }

    [System.Serializable]
    private class DebugSchematic
    {
        public float drillDepth = 500;

        [System.Serializable]
        public class DebugItem
        {
            public bool useElementKey = true;
            [ShowIf("useElementKey")] public string elementKey = "casing";
            [HideIf("useElementKey")] public BaseElement element;
            public float depth = 0;
            public float origin = 50;

            [FoldoutGroup("Grouping", expanded: false)] public string mainGroup = "default";
            [FoldoutGroup("Grouping", expanded: false)] public string subGroup;
        }
    }

#endif
}
