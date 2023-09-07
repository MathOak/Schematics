//#define CODING_WEB_MODULE
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
    private enum testMode 
    {
        Inspector,
        JsonString,
        WebApi
    }

    [Header("Generator Setup")]
    [SerializeField] private Camera _schematicCamera;
    [SerializeField] private Camera _printCamera;
    [SerializeField] private Canvas _uiCanvas;
    [SerializeField] private UIGenerator uiGenerator;
    [SerializeField] private TerrainGenerator terrainGenerator;
    [Space]
    [Header("Header")]
    [SerializeField] private ChristmasTreeGenerator cristmasGenerator;

    [Header("Colum")]
    [SerializeField] private BaseElement columSuspensor;
    [SerializeField] private BaseElement columFill;
    [SerializeField] private float partsSize = 0.7f;
    [SerializeField] private float partsSpace = 0.2f;

    [Header("Hole")]
    [SerializeField] private BaseElement emptyElement;
    [SerializeField] private BaseElement emptyPointsElement;
    
    [Header("Database")]
    [SerializeField] private List<BaseElement> allElements;
    [Space]
    [Header("Debug")]
#if UNITY_EDITOR
    [SerializeField] private bool writeApiKeyNames;
#endif
    [SerializeField] private testMode getSchematicFrom;
    private bool debugFromInspectorList => getSchematicFrom == testMode.Inspector;
    private bool debugWebApi => getSchematicFrom == testMode.WebApi;
    private bool debugJsonString => getSchematicFrom == testMode.JsonString;

    [ShowIf("debugFromInspectorList")][SerializeField] private Schematic debugSchematic;
    [ShowIf("debugJsonString")][SerializeField][TextArea] private string jsonSchematicText;


    public static Dictionary<string, BaseElement> elements;

    public const float DRAW_LIMITS_HORIZONTAL = 4f;
    public const float DRAW_WELL_SIZE = 2f;

    public static bool throwErrorOnMissingElement = false;

    public static Schematic lastGeneration { get; private set; }


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
    private void Start()
    {
        elements = new Dictionary<string, BaseElement>();


        foreach (var element in allElements)
        {
            if(!element.Key.IsNullOrWhitespace())
                elements.Add(element.Key, element);
        }


#if UNITY_EDITOR || UNITY_WEBGL == false
        UnityStart().Forget();
#endif

#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
    InternalGeneratorBootListener();
#endif
    }

#if UNITY_EDITOR
    private async UniTask UnityStart() 
    {
        switch (getSchematicFrom)
        {
            case testMode.Inspector:
                foreach (var item in debugSchematic.GetAllParts())
                {
                    if(item.useElementKey)
                        item.element = elements[item.elementKey];
                }

                await AsyncGenerator(debugSchematic);
                RenderSchematic();
                break;
            case testMode.JsonString:
                GenerateSchematicFromString();
                break;
            case testMode.WebApi:
                jsonSchematicText = await GetJsonFromAPI();
                GenerateSchematicFromString();
                break;
            default:
                break;
        }
    }
#endif

    private async UniTask GenerateFromJson(string jsonString)
    {
        try
        {
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
            InternalUnityLogger("Starting Unity Generation Process");
#endif

            JsonApiHandler jsonSchematic = JsonUtility.FromJson<JsonApiHandler>(jsonString);
            VisualElement.ClearElements();
            Loading.instance.AddMaxProgress(jsonSchematic.data.ToObject().parts.Count);

            await AsyncGenerator(jsonSchematic.data.ToObject());
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
            InternalUnityLogger("Schematic Generated!");
#endif

            RenderSchematic();
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
            QuitApplication();
#endif
        }
        catch (Exception e) 
        {
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
            InternalUnityErrorLogger(e.Message);
            QuitApplication();
#else
            Debug.LogException(e);
            QuitApplication();
#endif
        }
    }

    private void QuitApplication() 
    {
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
        InternalUnityLogger("Closing Web Module...");
        InternalGeneratorQuitListener();
#elif UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void GenerateSchematicFromString() 
    {
        GenerateFromJson(jsonSchematicText).Forget();
    }

#if UNITY_EDITOR
    [Button]
    public void GenerateSchematic() 
    {
        if (Application.isPlaying && debugSchematic != null)
        {
            VisualElement.ClearElements();
            UnityStart().Forget();
        }
    }
#endif

    private async UniTask AsyncGenerator(Schematic schematic) 
    {
#if UNITY_EDITOR
        if (writeApiKeyNames)
            ShowApiKeyNames(schematic);
#endif

        await DrawList(schematic.parts.Where(part => !part.element._headItem).ToList());

        //schematic.coating.Sort((coatA, coatB) => coatA._sapata._depth > coatB._sapata._depth ? -1 : 1);

        //for (int i = 0; i < schematic.coating.Count; i++)
        {
            //schematic.coating[i]._sapata._widthOffset = i * 140f;
            //schematic.coating[i]._cimentacao[0]._widthOffset = i * 140f;
            //await schematic.coating[i].Draw(additionalSort: i);
        }

        await terrainGenerator.GenerateTerrain(schematic);
        await DrawVoidSpaces(schematic);
        await DrawColum(schematic);
        await DrawUIText(schematic);


        lastGeneration = schematic;
        VisualElement.ChangeItemsColor();
    }

#if UNITY_EDITOR
    private void ShowApiKeyNames(Schematic schematic) 
    {
       schematic.GetAllParts().ForEach(part => part._virtualName = part.element.Key);
    }
#endif

    private async UniTask DrawVoidSpaces(Schematic schematic) 
    {
        await cristmasGenerator.DrawHead(schematic);

        SchematicItem voidSchematic = new SchematicItem();
        voidSchematic.element = emptyPointsElement;
        voidSchematic._depth = schematic._drillDepth;

        await voidSchematic.Draw();
        voidSchematic.element = emptyElement;
        await voidSchematic.Draw();
    }

    private async UniTask DrawColum(Schematic schematic)
    {
        if (schematic.column == null || schematic.column.Count == 0)
            return;

        SchematicItem columBase = new SchematicItem();
        if (schematic.GetAllParts().Any(part => part.element.Key == "tubing_hanger"))
        {
            columBase.element = columSuspensor;
            columBase._origin = -1 * (1.1f.VirtualToRealScale());
            columBase._depth = -1 * (0.6f.VirtualToRealScale());
            await columBase.Draw();
        }

        columBase.element = columFill;
        columBase._origin = -1 * (1.1f.VirtualToRealScale());
        columBase._depth = schematic.GetColumDepth();

        await columBase.Draw();
        await DrawColumParts(schematic);
    }

    private async UniTask DrawColumParts(Schematic schematic) 
    {
        foreach (var item in schematic.column)
        {
            await item.Draw();
        }
    }

    private async UniTask DrawList(List<SchematicItem> items) 
    {
        items = UIGenerator.RemoveItemCopies(items);

        foreach (var item in items)
        {
            await item.Draw();
            Loading.instance.AddCounter();

        }
    }

    private async UniTask DrawUIText(Schematic schematic) 
    {
        if (schematic._hideAllText) 
        {
            return;
        }

        await uiGenerator.DrawSchematicText(schematic);
        var rTransform = _uiCanvas.GetComponent<RectTransform>();
        rTransform.sizeDelta = new Vector2(DRAW_LIMITS_HORIZONTAL.RealToVirtualScale() * 600, schematic.GetLastDepth().RealToVirtualScale());
    }

    public void RenderSchematic()
    {
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
        InternalUnityLogger("Rendering Schematic to image...");
#endif
        int MAX_WIDTH = 2550;
        int MAX_HEIGHT = 3300;

        // Calculando lastDeph e drawSize
        float lastDeph = SchematicGenerator.lastGeneration.GetLastDepth();
        float drawSize = (lastDeph.RealToVirtualScale() / 2) + (ChristmasTreeGenerator.HeadSize / 2);

        // Configurando _schematicCamera
        _schematicCamera.orthographicSize = drawSize;
        _schematicCamera.transform.position = new Vector3(0, -drawSize + (ChristmasTreeGenerator.HeadSize), -10);

        // Calculando a proporção da imagem baseada na drawSize
        float cameraAspect = _schematicCamera.aspect;
        float imageWidth = MAX_HEIGHT * cameraAspect;
        float imageHeight = MAX_HEIGHT;

        if (imageWidth > MAX_WIDTH)
        {
            imageWidth = MAX_WIDTH;
            imageHeight = MAX_WIDTH / cameraAspect;
        }

        // Configurando a RenderTexture para _schematicCamera
        RenderTexture renderTexture = new RenderTexture((int)imageWidth, (int)imageHeight, 24);
        _schematicCamera.targetTexture = renderTexture;

        // Configurando _printCamera
        _printCamera.orthographicSize = _schematicCamera.orthographicSize;
        _printCamera.aspect = cameraAspect;
        _printCamera.transform.position = new Vector3(0, _schematicCamera.transform.position.y, -10);

        // Capturando a imagem da _printCamera
        var tempTex = SimpleScreenshotCapture.CaptureCameraToTexture((int)imageWidth, (int)imageHeight, _printCamera);

        // Criando uma nova Texture2D com as dimensões desejadas
        Texture2D finalTex = new Texture2D(MAX_WIDTH, MAX_HEIGHT);

        // Preenchendo a textura com branco
        Color[] fillColorArray = finalTex.GetPixels();

        for (int i = 0; i < fillColorArray.Length; i++)
        {
            fillColorArray[i] = Color.white;
        }

        finalTex.SetPixels(fillColorArray);
        finalTex.Apply();

        // Calculando posições para centralizar a RenderTexture temporária na Texture2D final
        int startX = (MAX_WIDTH - (int)imageWidth) / 2;
        int startY = (MAX_HEIGHT - (int)imageHeight) / 2;

        // Copiando pixels
        finalTex.SetPixels(startX, startY, (int)imageWidth, (int)imageHeight, tempTex.GetPixels());
        finalTex.Apply();

#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
    var jpgBytes = finalTex.EncodeToJPG();

    InternalUnityLogger($"Schematic Rendered! Returning Base64...");

    if (Application.isEditor)
    {
        Debug.Log(jpgBytes);
    }

    InternalBase64Generated(Convert.ToBase64String(jpgBytes));
#else
        byte[] pngBytes = finalTex.EncodeToPNG();
        string filePath = Path.Combine(Application.streamingAssetsPath, "generator_result.png");
        File.WriteAllBytes(filePath, pngBytes);

        this.filePath = filePath;
#endif
    }

    #region
    string filePath = string.Empty;

    public Vector2Int size = new Vector2Int();
    Vector2Int oldSize = new Vector2Int();

    [Button]
    void ResizeTexture()
    {
        Texture2D texture = LoadPNG(filePath, size);

        float proportion = oldSize.y / size.y;

        Vector2Int newSize = new Vector2Int(oldSize.x / (int)proportion, oldSize.y / (int)proportion); 

        TextureScaler.Scale(texture, newSize.x, newSize.y);

        byte[] bytes = texture.EncodeToJPG();
        File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, "generator_result1.png"), bytes);
    }
    
    public Texture2D LoadPNG(string filePath, Vector2Int size)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(size.x, size.y);
            tex.LoadImage(fileData);

            oldSize = new Vector2Int(tex.width, tex.height);
        }
        return tex;
    }
    #endregion


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
#endif

    public bool CheckIfAnyElementHaveTheSameKey()
    {
        var allKeys = new List<string>();

        foreach (var element in allElements)
        {
            if (allKeys.Contains(element.Key))
            {
                Debug.LogError($"Element {element.name} have the same key as another element!");
                return true;
            }

            allKeys.Add(element.Key);
        }

        return false;
    }

    public void LogPartsKeys() 
    {
        string partsList = "[Current Parts]";

        foreach (var part in allElements)
        {
            partsList += $"\nKEY: | {part.Key} | PART: {part.ElementName}";
        }

        Debug.Log(partsList);
    }

    struct JsonApiHandler
    {
        public bool elementDataError;
        public Schematic.JsonObject data;
    }

#if UNITY_EDITOR
    private async UniTask<string> GetJsonFromAPI()
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
