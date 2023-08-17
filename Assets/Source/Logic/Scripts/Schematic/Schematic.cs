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
    [Space]
    [SerializeField] public List<SchematicItem> colum;
    [SerializeField] public List<Revestiment> coating;
    [SerializeField] public List<SchematicItem> others;

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
            var revertiment = new Revestiment();
            coating.Add(revertiment);

            revertiment._sapata = item;
            revertiment._cimentacao = new List<SchematicItem>();
            revertiment._cimentacao.Add(new SchematicItem());
            revertiment._cimentacao[0].element = SchematicGenerator.elements["cimento"];
            revertiment._cimentacao[0]._origin = item._origin;
            revertiment._cimentacao[0]._deph = item._deph;
        }
        else
            others.Add(item);
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
    public struct JsonObject
    {
        public float drillDeph;
        public SchematicItem.JsonObject[] parts;

        public JsonObject(Schematic schematicItem)
        {
            this.drillDeph = schematicItem._drillDeph;

            List<SchematicItem> wellParts = new List<SchematicItem>(schematicItem.others);
            foreach (var columItem in schematicItem.colum)
            {
                wellParts.Add(columItem);
            }

            foreach (var revestiment in schematicItem.coating)
            {
                wellParts.Add(revestiment._sapata);
            }

            wellParts.Sort((partA, partB) => partA._deph < partB._deph ? -1 : 1);

            this.parts = wellParts.ConvertAll(part => part.ToJsonObject()).ToArray();
        }

        public Schematic ToObject()
        {
            var result = new Schematic();
            result.RestartSchematic();
            result._drillDeph = this.drillDeph;

            foreach (var jsonPart in this.parts)
            {
                result.AddItem(jsonPart.ConvertToObject());
            }

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