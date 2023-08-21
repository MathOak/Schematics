using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "tF_", menuName = "Schematics/Terrain Formation")]
public class TerrainFormation : ScriptableObject 
{
    [SerializeField] private List<SchematicItem> _sections;
    public List<SchematicItem> Sections => _sections;

    public async UniTask DrawTerrain(float wellSize) 
    {
        if (_sections == null || _sections.Count == 0)
        {
            return;
        }

        for (int i = 0; i < _sections.Count; i++)
        {
            _sections[i]._deph = wellSize + 200;
            await _sections[i].Draw();
        }
    }

    public void AddSection(SchematicItem section)
    {
        if (_sections == null)
        {
            _sections = new List<SchematicItem>();
        }

        _sections.Add(section);
    }
}