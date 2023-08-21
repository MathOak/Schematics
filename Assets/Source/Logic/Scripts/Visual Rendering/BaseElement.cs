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
    public bool _writePartOnDoc = true;
    [SerializeField] private string _key;
    private Sprite elementIcon;
    public bool _columItem;

    [Header("Drawing")]
    [SerializeField] Vector2 pivot = new Vector2(0.5f, 0f);
    [Space]
    [SerializeField] private bool useBgColor = true;
    [ShowIf("useBgColor")][SerializeField] private Color defaultColor = Color.white;
    [ShowIf("useBgColor")][SerializeField] Vector2 aditionalBgScale = Vector2.one;
    [Space]
    [SerializeField] private bool useInsideArt = false;
    [SerializeField][ShowIf("useInsideArt")] private Sprite art;
    [SerializeField][ShowIf("useInsideArt")] private Color artColor = Color.black;
    [SerializeField][ShowIf("useInsideArt")] SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    [SerializeField][ShowIf("useInsideArt")] SpriteDrawMode drawMode = SpriteDrawMode.Simple;
    [Space]
    [SerializeField][ShowIf("useInsideArt")] Vector2 aditionalArtScale = Vector2.one;
    [SerializeField][ShowIf("useInsideArt")] float minimalVirtualHeight = 0;
    [Space]
    [SerializeField] private int sortInLayer;

    public string ElementName => elementName;
    public string ElementEngName => elementEngName;
    public string Key => _key;
    public Sprite Icon => elementIcon;
    public Color DefaultColor => defaultColor;

    public BaseElement(string key, string elementName, string elementEngName)
    {
        _key = key;
        this.elementName = elementName;
        this.elementEngName = elementEngName;
    }

    public async UniTask<VisualElement> StartDraw(SchematicItem sElement, Rect drawArea, int additionalSort = 0) 
    {
        VisualElement visualElement = await VisualElement.CreateNew(sElement, drawArea);
        await GenerateDrawing(visualElement, additionalSort);
        return visualElement;
    }

    public SchematicItem CreateVirtualItem(float origin = 0, float depth = 0) 
    {
        var result = new SchematicItem();
        result.element = this;
        result._origin = origin;
        result._deph = depth;
        return result;
    }

    protected async UniTask GenerateDrawing(VisualElement visualElement, int additionalSort = 0)
    {
        SpriteRenderer render = null;

        if (useBgColor)
        {
            render = await visualElement.CreateRender("FillColor", sortInLayer + additionalSort);
            render.color = defaultColor;
            render.transform.localScale = visualElement.DrawArea.size * aditionalBgScale;

            SetPivotPosition(render.transform, visualElement.DrawArea.size, aditionalBgScale);
        }

        if (useInsideArt && art != null)
        {
            render = await visualElement.CreateRender("Art", sortInLayer + additionalSort + 1);

            render.sprite = art;
            render.color = artColor;
            render.maskInteraction = maskInteraction;
            render.drawMode = drawMode;

            if (drawMode == SpriteDrawMode.Simple)
                render.transform.localScale = visualElement.DrawArea.size * aditionalArtScale;
            else 
            {
                render.size = new Vector2(visualElement.DrawArea.size.x, Mathf.Clamp(visualElement.DrawArea.size.y, minimalVirtualHeight, Mathf.Infinity));
                render.size *= aditionalArtScale;
                render.transform.localScale = Vector3.one;
            }

            SetPivotPosition(render.transform, visualElement.DrawArea.size, aditionalArtScale);
        }
    }

    private void SetPivotPosition(Transform transform, Vector2 drawSize, Vector2 scale) 
    {
        Vector3 pivotPosition = new Vector3
        (
            Mathf.Lerp(-drawSize.x / 2, drawSize.x / 2, pivot.x),
            Mathf.Lerp(-drawSize.y / 2, drawSize.y / 2, pivot.y)
        );

        transform.localPosition = pivotPosition * scale;
    }

    public override string ToString()
    {
        return elementName;
    }

    public void CopyDrawSettings(BaseElement otherElement) 
    {
        _writePartOnDoc = otherElement._writePartOnDoc;
        elementIcon = otherElement.elementIcon;
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
}