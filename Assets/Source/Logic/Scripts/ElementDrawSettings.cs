using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "draw_", menuName = "Schematics/Draw Setting")]
public class ElementDrawSettings : ScriptableObject
{
    [Header("Position")]
    public bool _headItem = false;
    [HideIf("_headItem")] public bool _columItem;

    [Header("Write Settings")]
    public bool _writePartOnDoc = true;
    public bool _drawRectLine = false;
    public bool _fixedWritePosition = false;
    [ShowIf("_fixedWritePosition")] public float _fixedPosition = 0;
    [ShowIf("_fixedWritePosition")] public bool addHasOffset;
    
    [Tooltip("Add offset from indicator line start")]
    //[ShowIf("_fixedWritePosition")]
    public Vector2 originOffset = Vector2.zero;

    [Tooltip("Add offset from indicator line end")]
    //[ShowIf("_fixedWritePosition")] 
    public Vector2 targetOffset = Vector2.zero;

    [Header("Drawing")]
    public Vector2 pivot = new Vector2(0.5f, 0f);
    [Space]
    public bool useBgColor = true;
    [ShowIf("useBgColor")] public Color defaultColor = Color.white;
    [ShowIf("useBgColor")] public Vector2 additionalBgScale = Vector2.one;
    [Space]
    public bool useInsideArt = false;
    [ShowIf("useInsideArt")] public Sprite art;
    [ShowIf("useInsideArt")] public Sprite additionArt;
    [ShowIf("useInsideArt")] public Color artColor = Color.black;
    [ShowIf("useInsideArt")] public SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    [ShowIf("useInsideArt")] public SpriteDrawMode drawMode = SpriteDrawMode.Simple;
    [Space]
    [ShowIf("useInsideArt")] public Vector2 additionalArtScale = Vector2.one;
    [ShowIf("useInsideArt")] public float minimalVirtualHeight = 0;
    public bool ignoreResize = false;
    [SerializeField] public bool ignoreCSBColor = false;
    [Space]
    public int sortInLayer;

#if UNITY_EDITOR
    [Button]
    private void CopyDataFrom(BaseElement element) 
    {
        _headItem = element._headItem;
        _columItem = element._columItem;
        _writePartOnDoc = element._writePartOnDoc;
        _drawRectLine = element._drawRectLine;
        _fixedWritePosition = element._fixedWritePosition;
        _fixedPosition = element._fixedPosition;
        originOffset = element.originOffset;
        targetOffset = element.targetOffset;
        pivot = element.pivot;
        useBgColor = element.useBgColor;
        defaultColor = element.defaultColor;
        additionalBgScale = element.aditionalBgScale;
        useInsideArt = element.useInsideArt;
        art = element.art;
        additionArt = element.additionalArt;
        artColor = element.artColor;
        maskInteraction = element.maskInteraction;
        drawMode = element.drawMode;
        additionalArtScale = element.aditionalArtScale;
        minimalVirtualHeight = element.minimalVirtualHeight;
        sortInLayer = element.sortInLayer;
        addHasOffset = element.addHasOffset;
        ignoreResize = element.ignoreResize;
        ignoreCSBColor = element.ignoreCSBColor;
    }
#endif
}
