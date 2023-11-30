using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

public class VisualElement : MonoBehaviour
{
    [Header("Data")]
    [SerializeField][ReadOnly] private Rect _drawArea;
    private SchematicItem _sItem;
    public SchematicItem SchematicItem => _sItem;

    public static List<VisualElement> visualElements = new List<VisualElement>();

    private static Transform _visualParent;
    public static Transform visualParent 
    { get 
        {
            if (_visualParent == null)
                _visualParent = new GameObject("Schematic Render").transform;

            return _visualParent;
        }
    }
    public Rect DrawArea => _drawArea;

    public SpriteRenderer renderBG = null;
    public SpriteRenderer renderArt = null;

    public bool colorChanged = false;
    private void Awake()
    {
        visualElements.Add(this);
    }

    private void OnDestroy()
    {
        visualElements.Remove(this);
    }

    public async UniTask<SpriteRenderer> CreateRender(string renderName, int sortInLayer = 0) 
    {
        var opHandler = Addressables.InstantiateAsync("Renderer");
        await UniTask.WaitUntil(() => opHandler.IsDone);

        SpriteRenderer sRender = opHandler.Result.GetComponent<SpriteRenderer>();
        sRender.transform.SetParent(transform, false);
        sRender.gameObject.name = $"Renderer {renderName}";
        sRender.sortingOrder = sortInLayer;

        return sRender;
    }

    public static void ClearElements() 
    {
        if (visualElements == null || visualElements.Count == 0) 
        {
            return;
        }

        foreach (var visualElement in visualElements)
        {
            Addressables.ReleaseInstance(visualElement.gameObject);
        }
    }

    public static async UniTask<VisualElement> CreateNew(SchematicItem element, Rect drawArea) 
    {
        var opHandler = Addressables.InstantiateAsync("VisualElement");
        await UniTask.WaitUntil(() => opHandler.IsDone);

        VisualElement visualElement = opHandler.Result.GetComponent<VisualElement>();
        visualElement.gameObject.transform.SetParent(visualParent);
        visualElement.gameObject.name = element.ToString();
        visualElement.transform.localPosition = new Vector3(drawArea.x, drawArea.y, 0);
        visualElement._drawArea = drawArea;
        visualElement._sItem = element;

        return visualElement;
    }

    public async UniTask GenerateDrawing(int additionalSort = 0)
    {
        SpriteRenderer render = null;
        SpriteRenderer backGroundRenderer = null;
        BaseElement element = _sItem.element;

        if (element.Key == "fish")
        {
            Debug.Log("??w");
        }

        if (element.useBgColor)
        {
            render = await CreateRender("FillColor", element.sortInLayer + additionalSort);

            render.color = element.defaultColor;
            render.transform.localScale = DrawArea.size * element.aditionalBgScale;

            CheckAndSetBackground(render, element);

            SetPivotPosition(render.transform, element.pivot, DrawArea.size, element.aditionalBgScale);;
        }

        if (element.useInsideArt && element.art != null)
        {
            render = await CreateRender("Art", element.sortInLayer + additionalSort + 1);

            render.sprite = element.art;
            render.color = element.artColor;
            render.maskInteraction = element.maskInteraction;
            render.drawMode = element.drawMode;

            if (element.drawMode == SpriteDrawMode.Simple)
                render.transform.localScale = DrawArea.size * element.aditionalArtScale;
            else
            {
                render.size = new Vector2(DrawArea.size.x, Mathf.Clamp(DrawArea.size.y, element.minimalVirtualHeight, Mathf.Infinity));
                
                //TODO: THIS PART SMELLS REALLY BAD. PLEASE FIX IT THE RIGHT WAY.
                if (element.Key == "perforation")
                {
                    render.size = new Vector2(2, 0.2f);
                }
                if (element.Key == "downhole_safety_valve")
                {
                    render.size *= element.aditionalArtScale;

                    float percent = 300f / 100f;
                    float totalYsize = render.size.y + percent * render.size.y;
                    render.size = new Vector2(render.size.x, totalYsize);
                }
                else
                {
                    // THIS IS NECESSARY IN THIS CLASS
                    render.size *= element.aditionalArtScale;
                }
                /////

                render.transform.localScale = Vector3.one;
            }

            renderArt = render;

            CheckAndSetBackground(render, element);

            SetPivotPosition(render.transform, element.pivot, DrawArea.size, element.aditionalArtScale);
        }
    }

    void CheckAndSetBackground(SpriteRenderer parentRender, BaseElement element)
    {
        if (!element.useWhiteBackground)
            return;

        if (element.Key == "fish")
        {
            Debug.Log("??w");
        }
        
        SpriteRenderer backGroundRenderer = parentRender.GetComponentInChildren<SpriteRenderer>();

        backGroundRenderer.enabled = true;
        backGroundRenderer.drawMode = parentRender.drawMode;
        backGroundRenderer.size = parentRender.size;
    }

    private void SetPivotPosition(Transform transform, Vector2 pivot, Vector2 drawSize, Vector2 scale)
    {
        Vector3 pivotPosition = new Vector3
        (
            Mathf.Lerp(-drawSize.x / 2, drawSize.x / 2, pivot.x),
            Mathf.Lerp(-drawSize.y / 2, drawSize.y / 2, pivot.y)
        );

        transform.localPosition = pivotPosition * scale;
    }

    public void SetToGrayscale() 
    {
        if (renderBG != null)
            ConvertToGrayscale(renderBG);

        if (renderArt != null)
            ConvertToGrayscale(renderArt);
    }

    void ConvertToGrayscale(SpriteRenderer render)
    {
        Color originalColor = render.color;
        float grayValue = originalColor.r * 0.299f + originalColor.g * 0.587f + originalColor.b * 0.114f;
        render.color = new Color(grayValue, grayValue, grayValue, originalColor.a);
    }


    public static List<string> revestimento = new List<string>() { "#FF6C00", "#E971FB", "#FF4CA700" };
    public static List<string> terreno = new List<string>() { "#FEFF00", "#FF0500", "#98007E" };

    public static void ChangeItemsColor()
    {
        if (SchematicGenerator.lastGeneration._isDiagram == false) 
        {
            foreach (var visualElement in visualElements)
            {
                visualElement.SetToGrayscale();
            }

            return;
        }

        Color color;

        List<Color32> revestimentoClone = new List<Color32>();
        int revestimentIndex = 0;
        for (int i = 0; i < revestimento.Count; i++)
        {
            ColorUtility.TryParseHtmlString(revestimento[i], out color);
            revestimentoClone.Add(color);
            color = Color.white;
        }

        List<Color32> terrenoClone = new List<Color32>();
        int terrenoIndex = 0;
        for (int i = 0; i < terreno.Count; i++)
        {
            ColorUtility.TryParseHtmlString(terreno[i], out color);
            terrenoClone.Add(color);
            color = Color.white;
        }

        for (int i = 0; i < visualElements.Count; i++)
        {
            VisualElement visualElement = visualElements[i];
            SchematicItem item = visualElement.SchematicItem;

            if (visualElement.SchematicItem._mainGroup == "default")
            {
                for (int j = 0; j < visualElements.Count; j++)
                {
                    VisualElement schematicVisualElement = visualElements[j];
                    SchematicItem schematicItem = schematicVisualElement.SchematicItem;
                    if (item == schematicItem)
                        continue;

                    if (schematicItem.element.Key == item.element.Key)
                    {
                        if (schematicVisualElement.colorChanged || visualElement.colorChanged)
                            continue;

                        if (schematicItem.element.Key == "sealant")
                        {
                            schematicVisualElement.renderBG.color = revestimentoClone[revestimentIndex];
                            revestimentIndex++;
                        }
                        if (schematicItem.element.Key == "Terreno")
                        {
                            schematicVisualElement.renderBG.color = terrenoClone[terrenoIndex];
                            terrenoIndex++;
                        }
                        schematicVisualElement.colorChanged = true;
                    }
                }
            }
        }
    }

    public static void SetGroupColor(string group, Color color)
    {
        var elements = visualElements.FindAll(x => x.SchematicItem._mainGroup == group);
        for (int i = elements.Count - 1; i >= 0; i--)
        {
            var element = elements[i];

            if (!element._sItem.element.ignoreCSBColor)
            {
                if (element.renderArt != null)
                    element.renderArt.color = color;
                else if (element.renderBG != null)
                    element.renderBG.color = color;

                element.transform.SetAsFirstSibling();
            }
        }
    }

}