using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChristmasTreeGenerator : MonoBehaviour
{
    public List<BaseElement> treeElements;
    public List<BaseElement> headElements;
    public List<BaseElement> suspensorElements;

    public const float HEAD_SIZE = 3.25f;

    private async UniTask DrawHead(Schematic schematic)
    {
        var allParts = schematic.GetAllParts();

        foreach (var suspensor in suspensorElements)
        {

        }

        foreach (var head in headElements)
        {

        }

        foreach (var tree in treeElements)
        {

        }
        await Task.Delay(10);
    }

    private async UniTask DrawTree(SchematicItem tree, bool haveUnderParts) 
    {
        await tree.element.StartDraw(tree, new Rect(Vector2.zero, Vector2.one * HEAD_SIZE));
    }
}
