using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public partial class UIGenerator : MonoBehaviour
{
    [SerializeField] UITextBlockLeft txtBlockLeftPrefab;
    [SerializeField] UITextBlockRight txtBlockRightPrefab;
    [Space]
    [SerializeField] RectTransform canvasLeft;
    [SerializeField] RectTransform canvasRight;

    public async UniTask DrawSchematicText(Schematic schematic) 
    {
        var leftElements = schematic.GetAllParts().Where(part => part.WriteText).ToList();
        await WriteLeftText(leftElements);

        var rightItems = schematic.GetAllParts()
            .Where(part => part.WriteText && (!part._mainGroup.IsNullOrWhitespace() && part._mainGroup != "default")).ToList();
        await WriteRightText(rightItems);
    }


    private async UniTask WriteLeftText(List<SchematicItem> schematicItems)
    {
        List<UITextBlockLeft> blocks = new List<UITextBlockLeft>();

        foreach (var sItem in schematicItems)
        {
            var textBlock = WriteLeftBlock(sItem);

            if (blocks.Count > 0 && (textBlock.IsOverlappingWith(blocks[blocks.Count - 1]) || textBlock.pivot.position.y > blocks[blocks.Count - 1].pivot.position.y))
            {
                float adjustment = blocks[blocks.Count - 1].pivot.rect.height;
                textBlock.pivot.anchoredPosition = new Vector2(textBlock.pivot.anchoredPosition.x, blocks[blocks.Count - 1].pivot.anchoredPosition.y - adjustment);
            }

            blocks.Add(textBlock);
            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();
        }

        foreach (var block in blocks)
        {
            LineDrawer.instance.CreateLine(block);
        }
    }

    private UITextBlockLeft WriteLeftBlock(SchematicItem sItem)
    {
        UITextBlockLeft txtBlock = Instantiate(txtBlockLeftPrefab, canvasLeft);
        txtBlock.gameObject.name = $"LTXT - {sItem.ToString()}";
        txtBlock.WriteElementOnLeft(sItem);
        return txtBlock;
    }

    private async UniTask WriteRightText(List<SchematicItem> schematicItems) 
    {
        List<ItemMainGroup> mainGroups = new List<ItemMainGroup>();

        while (schematicItems.Count > 0)
        {
            mainGroups.Add(AggroupItems(schematicItems[0]._mainGroup, schematicItems));
        }

        mainGroups.Sort((x, y) => x._name.CompareTo(y._name));

        foreach (var mainGroup in mainGroups)
        {
            WriteMainGroupBlock(mainGroup);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRight);

        await UniTask.WaitForFixedUpdate();
        await UniTask.WaitForFixedUpdate();
    }

    private ItemMainGroup AggroupItems(string groupName, in List<SchematicItem> ungroupedItems) 
    {
        ItemMainGroup mainGroup = new ItemMainGroup(groupName, ungroupedItems.Where(item => item._mainGroup == groupName).ToList());
        ungroupedItems.RemoveAll(item => item._mainGroup == groupName);
        return mainGroup;
    }

    private UITextBlockRight WriteMainGroupBlock(ItemMainGroup mainGroup)
    {
        UITextBlockRight txtBlock = Instantiate(txtBlockRightPrefab, canvasRight.transform);
        txtBlock.gameObject.name = $"GTXT - {mainGroup._name}";
        txtBlock.WriteGroupBlock(mainGroup); ;
        return txtBlock;
    }
}