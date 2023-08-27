#define CODING_WEB_MODULE
using AlmostEngine.Screenshot;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Schema;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

public class SchematicGenerator : SerializedMonoBehaviour
{
    [SerializeField] private Camera _schematicCamera;
    [SerializeField] private Camera _printCamera;
    [SerializeField] private Canvas _uiCanvas;
    [SerializeField] private Schematic wellSchematic;
    [SerializeField] private UIGenerator uiGenerator;
    [Space]
    [SerializeField] private bool getJsonFromWeb;
    [HideIf("getJsonFromWeb")][SerializeField][TextArea] private string jsonSchematicText;
    [Space]
    [Header("Head")]
    [SerializeField] private BaseElement cristmasHead;

    [Header("Colum")]
    [SerializeField] private BaseElement columSuspensor;
    [SerializeField] private BaseElement columFill;
    [SerializeField] private float partsSize = 0.7f;
    [SerializeField] private float partsSpace = 0.2f;

    [Header("Hole")]
    [SerializeField] private BaseElement emptyElement;
    [SerializeField] private BaseElement emptyPointsElement;
    [Space]
    [SerializeField] private List<BaseElement> allElements;

    public static Dictionary<string, BaseElement> elements;

    public const float HEAD_SIZE = 3.25f;
    public const float DRAW_LIMITS_HORIZONTAL = 4f;
    public const float DRAW_WELL_SIZE = 2f;

    public static bool throwErrorOnMissingElement = false;

    public static Action onStartLoading;
    public static Action onEndLoading;
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

        onStartLoading?.Invoke();
        UnityStart().Forget();
#endif

#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
    InternalGeneratorBootListener();
#endif
    }

#if UNITY_EDITOR
    private async UniTask UnityStart() 
    {
        if (getJsonFromWeb)
        {
            jsonSchematicText = await GetJsonFromAPI();
        }

        GenerateSchematicFromString();
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
            onEndLoading?.Invoke();

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

    [Button]
    public void ConvertToJson() 
    {
        jsonSchematicText = wellSchematic.ToJsonFormat();
    }

    [Button]
    public void GenerateSchematicFromString() 
    {
        GenerateFromJson(jsonSchematicText).Forget();
    }

    [Button]
    public void GenerateSchematic() 
    {
        if (Application.isPlaying && wellSchematic != null)
        {
            VisualElement.ClearElements();
            AsyncGenerator(wellSchematic).Forget();
        }
    }

    private async UniTask AsyncGenerator(Schematic schematic) 
    {
        if (schematic.terrainFormation == null) 
        {
            schematic.terrainFormation = await Addressables.LoadAssetAsync<TerrainFormation>("DefaultFormation");
        }

        await schematic.terrainFormation.DrawTerrain(schematic._drillDeph);
        await DrawList(schematic.others);

        schematic.coating.Sort((coatA, coatB) => coatA._sapata._depth > coatB._sapata._depth ? -1 : 1);

        for (int i = 0; i < schematic.coating.Count; i++)
        {
            schematic.coating[i]._sapata._widthOffset = i * 140f;
            schematic.coating[i]._cimentacao[0]._widthOffset = i * 140f;
            await schematic.coating[i].Draw(additionalSort: i);
        }

        await DrawColum(schematic);
        await DrawVoidSpaces(schematic);
        await DrawUIText(schematic);

        lastGeneration = schematic;
    }

    private async UniTask DrawVoidSpaces(Schematic Schematic) 
    {
        await DrawHead();

        SchematicItem voidSchematic = new SchematicItem();
        voidSchematic.element = emptyPointsElement;
        voidSchematic._depth = Schematic._drillDeph;

        await voidSchematic.Draw();
        voidSchematic.element = emptyElement;
        await voidSchematic.Draw();
    }

    private async UniTask DrawHead() 
    {
        SchematicItem head = new SchematicItem();
        head.element = cristmasHead;
        await head.element.StartDraw(head, new Rect(Vector2.zero, Vector2.one * HEAD_SIZE));
    }

    private async UniTask DrawColum(Schematic schematic)
    {
        if (schematic.colum == null || schematic.colum.Count == 0)
            return;

        SchematicItem columBase = new SchematicItem();
        columBase.element = columSuspensor;
        columBase._origin = -1 * (1.1f.VirtualToRealScale());
        columBase._depth = -1 * (0.6f.VirtualToRealScale());
        await columBase.Draw();

        columBase.element = columFill;
        columBase._origin = -1 * (1.1f.VirtualToRealScale());
        columBase._depth = schematic.colum[schematic.colum.Count - 1]._depth;
        await columBase.Draw();

        await DrawColumParts(schematic);
    }

    private async UniTask DrawColumParts(Schematic schematic) 
    {
        var lastPart = schematic.colum[schematic.colum.Count - 1];
        float lastAbstractDeph = lastPart._depth;

        for (int i = schematic.colum.Count - 1; i >= 0; i--)
        {
            float abstractOrigin = lastAbstractDeph - partsSize.VirtualToRealScale();
            await schematic.colum[i].Draw(abstractOrigin, lastAbstractDeph);

            lastAbstractDeph = abstractOrigin;

            if (i - 1 > 0 && schematic.colum[i - 1]._depth < schematic.colum[i]._origin) 
            {
                lastAbstractDeph -= partsSpace.VirtualToRealScale();
            }
        }
    }

    private async UniTask DrawList(List<SchematicItem> items) 
    {
        foreach (var item in items)
        {
            await item.Draw();
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
        rTransform.sizeDelta = new Vector2(DRAW_LIMITS_HORIZONTAL.RealToVirtualScale() * 600, schematic._drillDeph.RealToVirtualScale());
    }

    private List<SchematicItem> GetSchematicItems(Schematic schematic) 
    {
        var result = schematic.GetAllParts();
        SchematicItem head = cristmasHead.CreateVirtualItem();
        SchematicItem suspensor = columSuspensor.CreateVirtualItem();
        SchematicItem production = columSuspensor.CreateVirtualItem();
        result.Insert(0, production);
        result.Insert(0, suspensor);
        result.Insert(0, head);
        return result;
    }

    [Button]
    public void RenderSchematic()
    {
#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
        InternalUnityLogger("Rendering Schematic to image...");
#endif
        float lastDeph = SchematicGenerator.lastGeneration.terrainFormation.Sections[SchematicGenerator.lastGeneration.terrainFormation.Sections.Count - 1]._depth;
        float drawSize = (lastDeph.RealToVirtualScale() / 2) + (SchematicGenerator.HEAD_SIZE / 2);

        _schematicCamera.orthographicSize = drawSize;
        _schematicCamera.transform.position = new Vector3(0, -drawSize + (SchematicGenerator.HEAD_SIZE), -10);

        Rect cameraViewRect = new Rect(_printCamera.rect);
        float widthValue = Mathf.InverseLerp(0, _printCamera.orthographicSize, SchematicGenerator.DRAW_LIMITS_HORIZONTAL / 2);
        cameraViewRect.size = new Vector2(widthValue, cameraViewRect.height);

        //_printCamera.rect = cameraViewRect;
       _schematicCamera.aspect = widthValue;

        int baseScale = 258;
        int printWidth = (baseScale * 5);
        int printHeight = (Mathf.CeilToInt(drawSize / SchematicGenerator.DRAW_LIMITS_HORIZONTAL * 2)) * baseScale;


        //PrintCamera
        float printCameraWidth = widthValue * 5;
        _printCamera.orthographicSize = _schematicCamera.orthographicSize;
        _printCamera.aspect = printCameraWidth;
        _printCamera.transform.position = new Vector3(0, _schematicCamera.transform.position.y, -10);

        _printCamera.transform.position = new Vector3(_printCamera.transform.position.x, _schematicCamera.transform.position.y, _printCamera.transform.position.z);

#if CODING_WEB_MODULE || (!UNITY_EDITOR && UNITY_WEBGL)
        var tex = SimpleScreenshotCapture.CaptureCameraToTexture(printWidth, printHeight, _printCamera);
        var jpgBytes = tex.EncodeToJPG();

        InternalUnityLogger($"Schematic Rendered! Returning Base64...");

        if (Application.isEditor)
        {
            Debug.Log(jpgBytes);
        }

        InternalBase64Generated(Convert.ToBase64String(jpgBytes));
#else
        string filePath = Path.Combine(Application.streamingAssetsPath, "generator_result.png");
        SimpleScreenshotCapture.CaptureCameraToFile(filePath, printWidth, printHeight, _printCamera);


        this.filePath = filePath;
        size = new Vector2Int(printWidth, printHeight);

        VisualElement.ChangeItemsColor();
        onEndLoading?.Invoke();
#endif
    }

    #region Resize Image
    string filePath = "";
    Vector2Int size = new Vector2Int();
    [Button]
    void ResizeTexture()
    {
        Texture2D texture = LoadPNG(filePath, size);

        //size = new Vector2Int(250, 10000);

        TextureScaler.Scale(texture, size.x/5, size.y/5);

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
        }
        return tex;
    }
    #endregion


#if UNITY_EDITOR
    [Button]
    public void GetAllBaseElementInProject()
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

    [Button]
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
#endif
}
