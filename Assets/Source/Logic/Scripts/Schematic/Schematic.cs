using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Schematic
{
    [Header("Inside")]
    [SerializeField] public float _drillDepth;
    [SerializeField] public bool _isDiagram = false;
    [SerializeField] public bool _hideAllText = false;
    [Space]
    [SerializeField] public List<SchematicItem> parts;
    [SerializeField] public List<SchematicItem> column;
    [SerializeField] public List<TerrainElement> terrains;
    [Space]
    [SerializeField] public List<DrillComment> comments;

    public void RestartSchematic()
    {
        _drillDepth = 0;
        column = new List<SchematicItem>();
        parts = new List<SchematicItem>();
        terrains = new List<TerrainElement>();
    }

    public void AddItem(SchematicItem item) 
    {
        if (item.element._columItem == true)
            column.Add(item);
        else
            parts.Add(item);
    }

    public List<SchematicItem> GetAllParts()
    {
        List<SchematicItem> result = new List<SchematicItem>();

        result.AddRange(column);
        result.AddRange(parts);

        result = result.OrderBy(part => part.GetMidPoint()).ToList();
        return result;
    }

    public float GetLastDepth()
    {
        float result = _drillDepth;
        foreach (var part in GetAllParts())
        {
            if (part._depth > result)
                result = part._depth;
        }

        foreach (var terrain in terrains)
        {
            if (terrain.depth > result)
                result = terrain.depth;
        }

        return result;
    }

    public string ToJsonFormat ()
    {
        return JsonUtility.ToJson(this.ToJsonObject());
    }

    public JsonObject ToJsonObject()
    {
        return new JsonObject(this);
    }

    [System.Serializable]
    public class DrillComment 
    {
        public string comment;
        public float @base;
    }

    [System.Serializable]
    public struct JsonObject
    {
        public float drillDeph;
        public bool isDiagram;
        public bool hideAllText;
        public SchematicItem.JsonObject[] parts;
        public TerrainElement.JsonObject[] terrains;
        public DrillComment[] comments;

        public JsonObject(Schematic schematic)
        {
            this.drillDeph = schematic._drillDepth;
            this.isDiagram = schematic._isDiagram;
            this.hideAllText = schematic._hideAllText;

            List<SchematicItem> wellParts = new List<SchematicItem>(schematic.parts);
            foreach (var columnItem in schematic.column)
            {
                wellParts.Add(columnItem);
            }

            wellParts.Sort((partA, partB) => partA._depth < partB._depth ? -1 : 1);
            this.parts = wellParts.ConvertAll(part => part.ToJsonObject()).ToArray();

            if (schematic.terrains != null)
            {
                this.terrains = schematic.terrains.ConvertAll(terrain => terrain.ToJsonObject()).ToArray();
            }
            else
            {
                this.terrains = new TerrainElement.JsonObject[0];
            }

            this.comments = schematic.comments.ToArray();
        }

        public Schematic ToObject()
        {
            var result = new Schematic();
            result.RestartSchematic();
            result._drillDepth = this.drillDeph;
            result._isDiagram = this.isDiagram;
            result._hideAllText = this.hideAllText;

            foreach (var jsonPart in this.parts)
            {
                if (SchematicGenerator.elements.ContainsKey(jsonPart.element))
                {
                    result.AddItem(jsonPart.ConvertToObject());
                }
                else 
                {
#if !UNITY_EDITOR && UNITY_WEBGL
                    SchematicGenerator.InternalUnityErrorLogger($"The key {jsonPart.element} is not present on the project!");
#endif
                }
            }

            foreach (TerrainElement.JsonObject jsonTerrain in this.terrains)
            {
                result.terrains.Add(jsonTerrain.ConvertToObject());
            }

            result.comments = new List<DrillComment>(this.comments);
            return result;
        }
    }
}