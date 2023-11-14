using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "element_simple_", menuName = "Schematics/Elements/Simple")]
public class BaseElement : ScriptableObject
{
    [Header("Info")]
    [SerializeField] private string elementName = "Generic Part";
    [SerializeField] private string elementEngName = "Generic Part ENG";
    [SerializeField] private string _key;
    [Space]
    public bool useDrawSettings = false;
    [ShowIf("useDrawSettings")] public ElementDrawSettings drawSettings;
    [Space]
    [Header("Position")]
    [HideIf("useDrawSettings")] public bool _headItem = false;
    [HideIf("_headItem")][HideIf("useDrawSettings")] public bool _columItem;

    [Header("Write Settings")]
    [HideIf("useDrawSettings")] public bool _writePartOnDoc = true;
    [HideIf("useDrawSettings")] public bool _drawRectLine = false;
    [HideIf("useDrawSettings")] public bool _fixedWritePosition = false;
    [ShowIf("_fixedWritePosition")][HideIf("useDrawSettings")] public float _fixedPosition = 0;
    [ShowIf("_fixedWritePosition")][HideIf("useDrawSettings")] public bool addHasOffset = false;

    [Header("Drawing")]
    [HideIf("useDrawSettings")][SerializeField] public Vector2 pivot = new Vector2(0.5f, 0f);
    [Space]
    [HideIf("useDrawSettings")][SerializeField] public bool useBgColor = true;
    [ShowIf("useBgColor")][HideIf("useDrawSettings")][SerializeField] public Color defaultColor = Color.white;
    [ShowIf("useBgColor")][HideIf("useDrawSettings")][SerializeField] public Vector2 aditionalBgScale = Vector2.one;
    [Space]
    [HideIf("useDrawSettings")][SerializeField] public bool useInsideArt = false;
    [ShowIf("useInsideArt")][HideIf("useDrawSettings")][SerializeField] public Sprite art;
    [ShowIf("useInsideArt")][HideIf("useDrawSettings")][SerializeField] public Color artColor = Color.black;
    [ShowIf("useInsideArt")][HideIf("useDrawSettings")][SerializeField] public SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    [ShowIf("useInsideArt")][HideIf("useDrawSettings")][SerializeField] public SpriteDrawMode drawMode = SpriteDrawMode.Simple;
    [Space]
    [ShowIf("useInsideArt")][HideIf("useDrawSettings")][SerializeField] public Vector2 aditionalArtScale = Vector2.one;
    [ShowIf("useInsideArt")][HideIf("useDrawSettings")][SerializeField] public float minimalVirtualHeight = 0;
    [SerializeField] public bool ignoreResize = false;
    [SerializeField] public bool ignoreCSBColor = false;
    [Space]
    [HideIf("useDrawSettings")][SerializeField] public int sortInLayer;

    public string ElementName => elementName;
    public string ElementEngName => elementEngName;
    public string Key => _key;
    public Color DefaultColor { get => defaultColor; set => defaultColor = value; }
    

    public BaseElement(string key, string elementName, string elementEngName)
    {
        _key = key;
        this.elementName = elementName;
        this.elementEngName = elementEngName;
    }

    public async UniTask<VisualElement> StartDraw(SchematicItem sElement, Rect drawArea, int additionalSort = 0) 
    {
        GetValuesFromDrawSettings();
        VisualElement visualElement = await VisualElement.CreateNew(sElement, drawArea);
        await visualElement.GenerateDrawing(additionalSort);
        return visualElement;
    }

    public override string ToString()
    {
        return elementName;
    }

    public async UniTask<SchematicItem> DrawHasGeneric(float origin, float depth) 
    {
        var result = new SchematicItem();
        result.element = this;
        result.__origin = origin;
        result.__depth = depth;

        await result.Draw();
        return result;
    }

    public void CopyDrawSettings(BaseElement otherElement) 
    {
        _writePartOnDoc = otherElement._writePartOnDoc;
        _columItem = otherElement._columItem;
        pivot = otherElement.pivot;
        useBgColor = otherElement.useBgColor;
        defaultColor = otherElement.defaultColor;
        aditionalBgScale = otherElement.aditionalBgScale;
        useInsideArt = otherElement.useInsideArt;
        art = otherElement.art;
        artColor = otherElement.artColor;
        maskInteraction = otherElement.maskInteraction;
        drawMode = otherElement.drawMode;
        aditionalArtScale = otherElement.aditionalArtScale;
        minimalVirtualHeight = otherElement.minimalVirtualHeight;
        sortInLayer = otherElement.sortInLayer;
    }

    public void GetValuesFromDrawSettings()
    {
        if (useDrawSettings && drawSettings != null)
        {
            pivot = drawSettings.pivot;
            useBgColor = drawSettings.useBgColor;
            defaultColor = drawSettings.defaultColor;
            aditionalBgScale = drawSettings.additionalBgScale;
            useInsideArt = drawSettings.useInsideArt;
            art = drawSettings.art;
            artColor = drawSettings.artColor;
            maskInteraction = drawSettings.maskInteraction;
            drawMode = drawSettings.drawMode;
            aditionalArtScale = drawSettings.additionalArtScale;
            minimalVirtualHeight = drawSettings.minimalVirtualHeight;
            sortInLayer = drawSettings.sortInLayer;
            _headItem = drawSettings._headItem;
            _columItem = drawSettings._columItem;
            _writePartOnDoc = drawSettings._writePartOnDoc;
            _drawRectLine = drawSettings._drawRectLine;
            _fixedWritePosition = drawSettings._fixedWritePosition;
            _fixedPosition = drawSettings._fixedPosition;
            addHasOffset = drawSettings.addHasOffset;
            ignoreResize = drawSettings.ignoreResize;
            ignoreCSBColor = drawSettings.ignoreCSBColor;            
        }
        else 
        {
            Logger.Warning($"Draw Settings of {this.name} is not updated!");
        }
    }
}