using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIGenerator : MonoBehaviour
{
    [SerializeField] UITextBlock txtBlockLeftPrefab;
    [SerializeField] UITextBlock txtBlockRightPrefab;
    [Space]
    [SerializeField] RectTransform canvasLeft;
    [SerializeField] RectTransform canvasRight;
    [SerializeField] Image whiteBg;

    public class CSBGroup
    {
        public string name;
        public List<string> schematicNames = new List<string>();

        public CSBGroup(string name, string schematicName)
        {
            this.name = name;
            this.schematicNames.Add(schematicName);
        }

        public string GetListNames()
        {
            StringBuilder builder = new StringBuilder();
            foreach(string schematicName in schematicNames)
            {
                builder.AppendLine(schematicName);
            }

            return builder.ToString();
        }
    }

    public async UniTask DrawSchematicText(List<VisualElement> allElements) 
    {
        List<UITextBlock> txtBlocks = new List<UITextBlock>();


        List<CSBGroup> mainGroupsNames = new List<CSBGroup>();

        foreach (var element in allElements)
        {
            if (element.SchematicItem.element._writePartOnDoc)
            {
                txtBlocks.Add(DrawText(element.SchematicItem.element.ElementName, element.SchematicItem._origin, txtBlockLeftPrefab, canvasLeft, true));
                
                
                if (element.SchematicItem._mainGroup != "default")
                {
                    Debug.Log(element.SchematicItem._mainGroup);

                    CSBGroup cSBGroup = new CSBGroup(element.SchematicItem._mainGroup, element.SchematicItem.element.ElementName);

                    bool hasGroup = false;
                    for (int i = 0; i < mainGroupsNames.Count; i++)
                    {
                        if (mainGroupsNames[i].name == cSBGroup.name)
                        {
                            hasGroup = true;
                            mainGroupsNames[i].schematicNames.Add(element.SchematicItem.element.ElementName);
                        }

                    }
                    if(!hasGroup)
                    {
                        mainGroupsNames.Add(cSBGroup);
                    }
                }
            }
        }

        StringBuilder builder = new StringBuilder();

        foreach (CSBGroup group in mainGroupsNames)
        {
            builder.AppendLine(group.name);
            builder.AppendLine();
            builder.AppendLine(group.GetListNames());
        }

        var rightText = DrawText(builder.ToString(), 0, txtBlockRightPrefab, canvasRight, false);


        await UniTask.WaitForFixedUpdate();
        await UniTask.WaitForFixedUpdate();

        for (int i = 0; i < txtBlocks.Count - 1; i++)
        {
            txtBlocks[i].OverlapText(txtBlocks[i + 1]);
        }

        await UniTask.WaitForFixedUpdate();
        await UniTask.WaitForFixedUpdate();
    }

    public UITextBlock DrawText(VisualElement element, UITextBlock txtPrefab, RectTransform parent, bool setPosition) 
    {
        UITextBlock txtBlock = Instantiate(txtPrefab, parent);
        txtBlock.gameObject.name = $"TXT - {element.SchematicItem.ToString()}";
        txtBlock.SetupText(element,txtBlock.transform.childCount, setPosition);
        return txtBlock;
    }

    public UITextBlock DrawText(string text, float yDistance, UITextBlock txtPrefab, RectTransform parent, bool setPosition)
    {
        UITextBlock txtBlock = Instantiate(txtPrefab, parent);
        txtBlock.gameObject.name = $"TXT - >  {text}";
        txtBlock.SetupText(text, (yDistance * (-1))/200, setPosition); ;
        return txtBlock;
    }

    public void SetBGHeight(float height) 
    {
        whiteBg.transform.localScale += new Vector3(0, height, 0);
    }
}
