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
            revestiment._cimentacao[0]._deph = item._deph;
        }
        else
            others.Add(item);
    }

    public List<SchematicItem> GetAllParts() 
    {
        List<SchematicItem> result = new List<SchematicItem>(others);

        foreach (var columnItem in colum)
        {
            result.Add(columnItem);
        }

        foreach (var otherItem in others)
        {
            result.Add(otherItem);
        }

        result.OrderBy(item => item._deph);
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
        public SchematicItem.JsonObject[] parts;
        public SchematicItem.JsonObject[] terrains;
        public DrillComment[] comments;

        public JsonObject(Schematic schematic)
        {
            this.drillDeph = schematic._drillDeph;
            this.isDiagram = schematic._isDiagram;

            List<SchematicItem> wellParts = new List<SchematicItem>(schematic.others);
            foreach (var columItem in schematic.colum)
            {
                wellParts.Add(columItem);
            }

            foreach (var revestiment in schematic.coating)
            {
                wellParts.Add(revestiment._sapata);
            }

            wellParts.Sort((partA, partB) => partA._deph < partB._deph ? -1 : 1);
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

            foreach (var jsonPart in this.parts)
            {
                result.AddItem(jsonPart.ConvertToObject());
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