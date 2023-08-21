using Sirenix.OdinInspector;
using UnityEngine;

public class NorsokCreator : MonoBehaviour
{
    public TextAsset csvFile;

    [Button]
    private void ReadCSVDataAndCreateScriptableObjects()
    {
        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] fields = line.Split(',');

            if (fields.Length >= 2)
            {
                string elementName = fields[0];
                string key = fields[1];
                int norsokKey = int.Parse(fields[2]);
                string elementEngName = fields[3];

                CreateScriptableObject(norsokKey, key, elementName, elementEngName);
            }
        }

        UnityEditor.AssetDatabase.SaveAssets();
    }

    private void CreateScriptableObject(int norsokId, string key, string name, string engName)
    {
        BaseElement newBaseElement = new BaseElement(key, name, engName);
        UnityEditor.AssetDatabase.CreateAsset(newBaseElement, $"Assets/Source/Logic/Elements/Well/Norsok Table/element_nt_{norsokId}_{key}.asset");
    }
}

