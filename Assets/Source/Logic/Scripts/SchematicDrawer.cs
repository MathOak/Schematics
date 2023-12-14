using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SchematicDrawer : MonoBehaviour
{
    [Header("Generators")]
    [SerializeField] private TerrainGenerator terrainGenerator;
    [SerializeField] private ChristmasTreeGenerator christmasGenerator;

    [Header("Colum")]
    [SerializeField] private BaseElement columSuspensor;
    [SerializeField] private BaseElement columFill;

    [Header("Hole")]
    [SerializeField] private BaseElement emptyElement;
    [SerializeField] private BaseElement emptyPointsElement;

    public async UniTask DrawSchematic(Schematic schematic)
    {
        SetColumn(schematic);
        await DrawList(schematic);

        await terrainGenerator.GenerateTerrain(schematic);
        await christmasGenerator.DrawHead(schematic);

        await DrawVoidSpaces(schematic);
    }

    private async UniTask DrawList(Schematic schematic)
    {
        var items = schematic.GetAllPartsByDepth().Where(part => !part.element._headItem).ToList();
        items = UIGenerator.RemoveItemCopies(items);

        for (int i = 0; i < items.Count; i++)
        {
            var currentItem = items[i];            
            await currentItem.Draw();

            if ((i + 1) < items.Count) 
            {
                if (!currentItem.element.ignoreResize && !currentItem.element._headItem)
                {
                    for (int j = i + 1; j < items.Count; j++)
                    {
                        var nextItem = items[j];
                        if (!nextItem.element._headItem)
                        {
                            if (nextItem.element.ignoreResize) 
                            {
                                if (nextItem.__origin > currentItem.__depth) 
                                {
                                    nextItem.originOffset = currentItem.originOffset;
                                    nextItem.depthOffset = currentItem.depthOffset;
                                }
                                else if (nextItem.__depth > currentItem.__depth) 
                                {
                                    nextItem.depthOffset = currentItem.depthOffset;
                                }
                            }
                            else
                            if (currentItem.element.minimalVirtualHeight > 0 || nextItem.element.minimalVirtualHeight > 0)
                            {
                                if (nextItem._virtualName == "Packer")
                                {
                                    Debug.Log("STOP");
                                }

                                float diference = Mathf.Max(0, currentItem.GetBotPoint() - nextItem.GetTopPoint());
                                nextItem.originOffset += diference;
                                nextItem.depthOffset += diference;

                                foreach (var terrain in schematic.terrains)
                                {
                                    if (terrain._origin > nextItem.__depth)
                                    {
                                        terrain.offsetOrigin = nextItem.originOffset;
                                        terrain.offsetDepth = nextItem.depthOffset;
                                    }
                                    else if (terrain.depth > nextItem.__depth)
                                    {
                                        terrain.offsetDepth = nextItem.depthOffset;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Loading.instance.AddCounter();
        }
    }

    private async UniTask DrawVoidSpaces(Schematic schematic)
    {
        float bottomDepth = schematic.GetLastDepth(2);

        await emptyPointsElement.DrawHasGeneric(0, bottomDepth);
        await emptyElement.DrawHasGeneric(0, bottomDepth);
    }

    private void SetColumn(Schematic schematic)
    {
        if (schematic.column == null || schematic.column.Count == 0)
            return;

        var fill = schematic.column.Find(item => item.element.Key == "column_casing" && item.__origin == 0);

        if (fill == null)
            return;

        fill.__origin = ((ExtensionMethods.VirtualToRealScale(1.1f) * -1));
    }
}
