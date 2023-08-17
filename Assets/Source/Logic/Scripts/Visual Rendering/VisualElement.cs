using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class VisualElement : MonoBehaviour
{
    [Header("Data")]
    [SerializeField][ReadOnly] private Rect _drawArea;

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

        return visualElement;
    }
}