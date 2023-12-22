using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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

        StackCasings(schematic);

        Debug.Log("END");
    }

    public void StackCasings(Schematic schematic)
    {
        List<SchematicItem> casings = schematic.GetAllParts().Where(part => part.element.Key.StartsWith("casing")).ToList();

        List<CasingItem> casingItems = new List<CasingItem>();

        foreach(var c in casings) 
        {
            casingItems.Add(new CasingItem(c));
        }

        // SORT BY SIZE
        casingItems.Sort((x, y) =>
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


        for (int i = 1; i < casingItems.Count; i++)
        {
            CasingItem targetCasing = casingItems[i];

            for (int j = i - 1; j > 0; j--)
            {
                CasingItem checkedCasing = casingItems[j];

                //IF IT NOT OVERLAPS THERE ISNT NEED TO JUMP LAYERS
                if (!checkedCasing.Overlaps(targetCasing))
                {
                    continue;
                }

                // BUT IF IT OVERLPAS...
                targetCasing.layer = checkedCasing.layer + 1;
            }
        }

        for (int i = 0; i < casingItems.Count; i++)
        {
            CasingItem targetItem = casingItems[i];

            GameObject targetPart = null;
            targetPart = GameObject.Find(targetItem.SchematicItem.ToString());

            if (targetPart == null)
            {
                continue;
            }

            Transform renderer = targetPart.transform.GetChild(0);
            Vector3 localScale = renderer.transform.localScale;
            renderer.transform.localScale = new Vector3(localScale.x + localScale.x * (0.13f * targetItem.layer), localScale.y, localScale.z);
        }

        //for (int i = 0; i < casings.Count; i++)
        //{
        //    SchematicItem targetItem = casings[i];

        //    for (int j = 0; j < casings.Count; j++)
        //    {
        //        if (i == j)
        //        {
        //            continue;
        //        }

        //        SchematicItem item = casings[j];

        //        float targetOrigin = targetItem.__origin;

        //        float origin = item.__origin;
        //        float depth = item.__depth;

        //        GameObject targetPart = null;
        //        if (targetOrigin >= origin && targetOrigin <= depth)
        //        {
        //            targetPart = GameObject.Find(targetItem.ToString());
        //        }

        //        if (targetPart != null)
        //        {
        //            Transform renderer = targetPart.transform.GetChild(0);
        //            Vector3 localScale = renderer.transform.localScale;
        //            renderer.transform.localScale = new Vector3(localScale.x + localScale.x * 0.13f, localScale.y, localScale.z);
        //        }
        //    }
        //}
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

class CasingItem
{
    SchematicItem schematicItem = null;
    public SchematicItem SchematicItem { get { return schematicItem; } }

    public int layer = 0;

    public float Height
    {
        get
        {
            if (schematicItem == null)
            {
                return 0;
            }

            return schematicItem.__depth - schematicItem.__origin;
        }
    }

    public CasingItem (SchematicItem schematicItem)
    {
        this.schematicItem = schematicItem;
    }

    public bool Overlaps(CasingItem casing)
    {
        if (schematicItem == null)
        {
            return false;
        }

        if (casing.schematicItem.__origin > schematicItem.__origin && casing.schematicItem.__origin < schematicItem.__depth)
        {
            return true;
        }

        if (casing.schematicItem.__depth > schematicItem.__origin && casing.schematicItem.__depth < schematicItem.__depth)
        {
            return true;
        }

        if (schematicItem.__origin > casing.schematicItem.__origin && schematicItem.depthOffset < casing.schematicItem.__depth)
        {
            return true;
        }

        return false;
    }
}