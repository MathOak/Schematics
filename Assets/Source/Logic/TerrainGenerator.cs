using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private Color defaultTerrainColor;

    public async UniTask GenerateTerrain(Schematic schematic) 
    {
        if (schematic._isDiagram == false && schematic.terrains == null || schematic.terrains.Count == 0) 
        {
            return;
        }

        BaseElement genericTerrain = await Addressables.LoadAssetAsync<BaseElement>("GenericTerrain");
        Color mainColor = defaultTerrainColor;

        foreach (var terrain in schematic.terrains)
        {
            SchematicItem terrainItem = new SchematicItem();
            terrainItem._virtualName = terrain.terrainName;
            terrainItem._description = terrain.terrainDesc;
            terrainItem.element = genericTerrain;
            terrainItem.__origin = terrain.origin;
            terrainItem.__depth = terrain.depth;
            terrainItem._mainGroup = terrain.mainGroup;

            if (!terrain.colorHex.IsNullOrWhitespace() && ColorUtility.TryParseHtmlString(terrain.colorHex, out Color color))
            {
                terrainItem.element.DefaultColor = color;
            }
            else 
            {
                terrainItem.element.DefaultColor = mainColor;
                Color.RGBToHSV(mainColor, out float h, out float s, out float v);
                mainColor = Color.HSVToRGB(h + 0.1f, s, v);
            }

            await terrainItem.Draw();
        }
    }
}
