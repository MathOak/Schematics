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

    List<UITextBlockLeft> leftBlocks = new List<UITextBlockLeft>();
    List<UITextBlockRight> rightBlocks = new List<UITextBlockRight>();

    public async UniTask DrawSchematicText(Schematic schematic) 
    {
        ClearTexts();

        var leftElements = schematic.GetAllParts().Where(part => part.WriteText).ToList();
        await WriteLeftText(leftElements);

        if (schematic._isDiagram)
        {
            var rightItems = schematic.GetAllParts()
                .Where(part => part.WriteText && (!part._mainGroup.IsNullOrWhitespace() && part._mainGroup != "default")).ToList();
            await WriteRightText(rightItems);
        }
    }


    public async UniTask WriteLeftText(List<SchematicItem> schematicItems)
    {
        leftBlocks = new List<UITextBlockLeft>();

        foreach (var sItem in schematicItems)
        {
            var textBlock = WriteLeftBlock(sItem);

            if (leftBlocks.Count > 0 && (textBlock.IsOverlappingWith(leftBlocks[leftBlocks.Count - 1]) || textBlock.pivot.position.y > leftBlocks[leftBlocks.Count - 1].pivot.position.y))
            {
                float adjustment = leftBlocks[leftBlocks.Count - 1].pivot.rect.height;
                textBlock.pivot.anchoredPosition = new Vector2(textBlock.pivot.anchoredPosition.x, leftBlocks[leftBlocks.Count - 1].pivot.anchoredPosition.y - adjustment);
            }

            leftBlocks.Add(textBlock);
            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();
        }


        LineDrawer.instance.ClearLines();
        foreach (var block in leftBlocks)
        {
            LineDrawer.instance.CreateLine(block);
        }
    }

    public UITextBlockLeft WriteLeftBlock(SchematicItem sItem)
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

        int index = 0;
        foreach (var mainGroup in mainGroups)
        {
            WriteMainGroupBlock(mainGroup, index);
            index++;
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

    private UITextBlockRight WriteMainGroupBlock(ItemMainGroup mainGroup,int index)
    {
        UITextBlockRight txtBlock = Instantiate(txtBlockRightPrefab, canvasRight.transform);
        txtBlock.gameObject.name = $"GTXT - {mainGroup._name}";
        txtBlock.WriteGroupBlock(mainGroup, index);
        rightBlocks.Add(txtBlock);
        return txtBlock;
    }

    private void ClearTexts() 
    {
        foreach (var leftBlock in leftBlocks)
        {
            Destroy(leftBlock.gameObject);
        }

        foreach (var rightBlock in rightBlocks)
        {
            Destroy(rightBlock.gameObject);
        }

        leftBlocks.Clear();
        rightBlocks.Clear();
    }
}