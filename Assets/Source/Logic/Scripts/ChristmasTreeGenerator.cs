using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading;

public class ChristmasTreeGenerator : MonoBehaviour
{
    [SerializeField] float headSize = 3.25f;
    public static ChristmasTreeGenerator Instance { get; private set; }
    public static float HeadSize => Instance.headSize;

    private void Awake()
    {
        Instance = this;
    }

    public async UniTask DrawHead(Schematic schematic)
    {
        var allParts = schematic.GetAllParts().Where(part => part.element._headItem);

        foreach (var part in allParts)
        {
            await DrawTree(part);
        }
    }

    private async UniTask DrawTree(SchematicItem tree) 
    {
        await tree.element.StartDraw(tree, new Rect(Vector2.zero, Vector2.one * headSize));
    }
}
