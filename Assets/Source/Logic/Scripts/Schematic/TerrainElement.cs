using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class TerrainElement
{
    public string terrainName = "Terreno Genérico";
    [HideInInspector] public string terrainDesc;
    public float origin = 0;
    public float depth = 100;
    [Space]
    public string mainGroup = "default";
    [HideInInspector] public string subGroup;
    [HideInInspector] public string colorHex = "";
    [HideInInspector] public string patternKey = "";

    public TerrainElement(string name, string desc, float origin, float depth, string mainGroup, string subGroup, string colorHex, string patternKey)
    {
        this.terrainName = name;
        this.terrainDesc = desc;
        this.origin = origin;
        this.depth = depth;
        this.mainGroup = mainGroup;
        this.subGroup = subGroup;
        this.colorHex = colorHex;
        this.patternKey = patternKey;
    }

    public JsonObject ToJsonObject()
    {
        return new JsonObject(this);
    }

    [System.Serializable]
    public struct JsonObject
    {
        public string name;
        public string description;
        public float topo;
        public float @base;
        public string subGroup;
        public string mainGroup;
        public string colorHex;
        public string patternKey;

        public JsonObject(TerrainElement terrain)
        {
            this.name = terrain.terrainName;
            this.description = terrain.terrainDesc;
            this.topo = terrain.origin;
            this.@base = terrain.depth;
            this.subGroup = terrain.mainGroup;
            this.mainGroup = terrain.mainGroup;
            this.colorHex = terrain.colorHex;
            this.patternKey = terrain.patternKey;
        }

        public TerrainElement ConvertToObject()
        {
            return new TerrainElement(name, description, topo, @base, mainGroup, subGroup, colorHex, patternKey);
        }
    }
}