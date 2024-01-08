using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

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

        StackCasings(schematic, "casing");
        StackCasings(schematic, "sapata", 0.16f);
    }

    public void StackCasings(Schematic schematic, string keyStart, float startOffset = 0)
    {
        List<SchematicItem> casings = schematic.GetAllParts().Where(part => part.element.Key.StartsWith(keyStart)).ToList();

        // SORT BY SIZE
        casings.Sort((x, y) =>
        {
            float xSize = x.Height;
            float ySize = y.Height;

            if (x.Height > y.Height)
            {
                return -1;
            }

            else if (y.Height > x.Height)
            {
                return 1;
            }

            return 0;
        });


        for (int i = 0; i < casings.Count - 1; i++)
        {
            SchematicItem checkedCasing = casings[i];
            SchematicItem targetCasing = casings[i + 1];


            //IF IT NOT OVERLAPS THERE ISNT NEED TO JUMP LAYERS
            if (!checkedCasing.Overlaps(targetCasing))
            {
                continue;
            }

            // BUT IF IT OVERLPAS...
            targetCasing.layer = checkedCasing.layer + 1;
        }

        for (int i = 0; i < casings.Count; i++)
        {
            SchematicItem targetItem = casings[i];

            GameObject targetPart = null;
            targetPart = GameObject.Find(targetItem.ToString());

            if (targetPart == null)
            {
                continue;
            }

            Transform renderer = targetPart.transform.GetChild(0);
            Vector3 localScale = renderer.transform.localScale;
            renderer.transform.localScale = new Vector3(localScale.x + localScale.x * (0.13f * targetItem.layer), localScale.y, localScale.z);
        }
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
                            else if (currentItem.element.minimalVirtualHeight > 0 || nextItem.element.minimalVirtualHeight > 0)
                            {
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