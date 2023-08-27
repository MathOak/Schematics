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
    [SerializeField] public float _drillDeph;
    [SerializeField] public bool _isDiagram = false;
    [SerializeField] public bool _hideAllText = false;
    [Space]
    [SerializeField] public List<SchematicItem> colum;
    [SerializeField] public List<Revestiment> coating;
    [SerializeField] public List<SchematicItem> others;
    [Space]
    [SerializeField] public List<DrillComment> comments;

    [Header("Outside")]
    [SerializeField] public TerrainFormation terrainFormation;

    public void RestartSchematic()
    {
        _drillDeph = 0;
        colum = new List<SchematicItem>();
        coating = new List<Revestiment>();
        others = new List<SchematicItem>();
    }

    public void AddItem(SchematicItem item) 
    {
        if (item.element._columItem == true)
            colum.Add(item);
        else if (item.element.Key == "sapata")
        {
            var revestiment = new Revestiment();
            coating.Add(revestiment);

            revestiment._sapata = item;
            revestiment._cimentacao = new List<SchematicItem>();
            revestiment._cimentacao.Add(new SchematicItem());
            revestiment._cimentacao[0].element = SchematicGenerator.elements["cimento"];
            revestiment._cimentacao[0]._origin = item._origin;
            revestiment._cimentacao[0]._depth = item._depth;
        }
        else
            others.Add(item);
    }

    public List<SchematicItem> GetAllParts()
    {
        List<SchematicItem> result = new List<SchematicItem>();

        result.AddRange(colum);
        result.AddRange(others);

        result = result.OrderBy(part => part.GetMidPoint()).ToList();
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
        public SchematicItem.JsonObject[] terrains;
        public DrillComment[] comments;

        public JsonObject(Schematic schematic)
        {
            this.drillDeph = schematic._drillDeph;
            this.isDiagram = schematic._isDiagram;
            this.hideAllText = schematic._hideAllText;

            List<SchematicItem> wellParts = new List<SchematicItem>(schematic.others);
            foreach (var columItem in schematic.colum)
            {
                wellParts.Add(columItem);
            }

            foreach (var revestiment in schematic.coating)
            {
                wellParts.Add(revestiment._sapata);
            }

            wellParts.Sort((partA, partB) => partA._depth < partB._depth ? -1 : 1);
            this.parts = wellParts.ConvertAll(part => part.ToJsonObject()).ToArray();

            if (schematic.terrainFormation != null)
            {
                this.terrains = schematic.terrainFormation.Sections.ConvertAll(section => section.ToJsonObject()).ToArray();
            }
            else
            {
                this.terrains = new SchematicItem.JsonObject[0];
            }

            this.comments = schematic.comments.ToArray();
        }

        public Schematic ToObject()
        {
            var result = new Schematic();
            result.RestartSchematic();
            result._drillDeph = this.drillDeph;
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

            result.terrainFormation = new TerrainFormation();
            foreach (var jsonTerrain in this.terrains)
            {
                result.terrainFormation.AddSection(jsonTerrain.ConvertToObject());
            }

            result.comments = new List<DrillComment>(this.comments);
            return result;
        }
    }
}

[System.Serializable]
public class Revestiment 
{
    public SchematicItem _sapata;
    [Space]
    public List<SchematicItem> _cimentacao;

    public async UniTask Draw(int additionalSort = 0) 
    {
        foreach (var concrete in _cimentacao)
        {
            await concrete.Draw(additionalSort);
        }

        await _sapata.Draw(additionalSort);
    }
}