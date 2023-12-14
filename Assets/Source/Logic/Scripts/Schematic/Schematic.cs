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

        int n = result.Count;

        for (int i = 1; i < n; i++)
        {
            SchematicItem current = result[i];
            float currentMidPoint = current.GetMidPoint();
            int j = i - 1;

            while (j >= 0 && result[j].GetMidPoint() > currentMidPoint)
            {
                result[j + 1] = result[j];
                j--;
            }

            result[j + 1] = current;
        }
        return result;
    }

    public List<SchematicItem> GetAllPartsByDepth()
    {
        List<SchematicItem> result = new List<SchematicItem>();

        result.AddRange(column);
        result.AddRange(parts);

        int n = result.Count;

        for (int i = 1; i < n; i++)
        {
            SchematicItem current = result[i];
            float currentDepth = current.__depth;
            int j = i - 1;

            while (j >= 0 && result[j].__depth > currentDepth)
            {
                result[j + 1] = result[j];
                j--;
            }

            result[j + 1] = current;
        }
        return result;
    }

    public float GetLastDepth(float offset = 0)
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

        return result + offset;
    }

    public float GetColumDepth()
    {
        float result = 0;

        var fluids = column.FindAll(s => s.element.Key == "fluid_column");
        var fluidC = fluids.Find(fluid => fluid._depth > result);

        foreach (var fluid in fluids)
        {
            if (fluid._depth > fluidC._depth) 
            {
                fluidC = fluid;
            }
        }

        if (fluidC != null)
        {
            result = fluidC._depth;
        }
        else 
        {
            foreach (var part in column)
            {
                if (part._depth > result)
                    result = part._depth;
            }
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
                if (ElementDatabase.Elements.ContainsKey(jsonPart.element))
                {
                    result.AddItem(jsonPart.ConvertToObject());
                }
                else 
                {
                    Logger.Warning($"The key {jsonPart.element} doesn't exist in the project!");
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