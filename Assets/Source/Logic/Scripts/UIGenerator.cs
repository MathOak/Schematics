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
    [SerializeField] private Canvas _uiCanvas;
    [Space]
    [SerializeField] UITextBlockLeft txtBlockLeftSimple;
    [SerializeField] UITextBlockLeft txtBlockRightSimple;
    [SerializeField] UITextBlockRight txtBlockRightPrefab;
    [Space]
    [SerializeField] RectTransform canvasLeft;
    [SerializeField] RectTransform canvasRight;
    [SerializeField] RectTransform canvasRightSimple;

    List<UITextBlockLeft> leftBlocks = new List<UITextBlockLeft>();
    List<UITextBlockRight> rightBlocks = new List<UITextBlockRight>();

    public async UniTask DrawUIText(Schematic schematic)
    {
        if (schematic._hideAllText)
        {
            return;
        }

        var rTransform = _uiCanvas.GetComponent<RectTransform>();
        rTransform.sizeDelta = new Vector2(Constants.DRAW_LIMITS_HORIZONTAL.RealToVirtualScale() * 600, schematic.GetLastDepth().RealToVirtualScale());

        await DrawSchematicText(schematic);
    }

    public async UniTask DrawSchematicText(Schematic schematic) 
    {
        ClearTexts();
        await WriteLeftText(schematic);

        if (schematic._isDiagram)
        {
            var rightItems = schematic.GetAllParts()
                .Where(part => part.WriteText && (!part._mainGroup.IsNullOrWhitespace() && part._mainGroup != "default")).ToList();
            await WriteRightText(rightItems);
        }
    }

    public async UniTask WriteLeftText(Schematic schematic)
    {
        var schematicItems = schematic.GetAllParts().Where(part => part.WriteText).ToList();
        leftBlocks = new List<UITextBlockLeft>();
        schematicItems = RemoveItemCopies(schematicItems);

        var indexedItems = schematicItems.Select((item, index) => new { Item = item, Index = index }).ToList();
        indexedItems.Sort((a, b) =>
        {
            if (b.Item.element._headItem && a.Item.element._headItem)
                return b.Item.element._headItem.CompareTo(a.Item.element._headItem);
            if (b.Item.element._headItem)
                return 1;
            if (a.Item.element._headItem)
                return -1;
            return a.Index.CompareTo(b.Index);
        });

        schematicItems = indexedItems.Select(x => x.Item).ToList();
        bool writeOnLeft = false;
        int startIndex = schematic._isDiagram ? 0 : 1;
        int interval = schematic._isDiagram ? 1 : 2;
        
        canvasRight.gameObject.SetActive(schematic._isDiagram);

        List<SchematicItem> surfaceSchematicItem = schematicItems.FindAll((item) => SurfaceElementsUtil.IsFromSurface(item.element.Key));

        surfaceSchematicItem.Sort((a, b) =>
        {
            return SurfaceElementsUtil.GetGreater(a.element.Key, b.element.Key);
        });

        schematicItems.RemoveAll((item) => SurfaceElementsUtil.IsFromSurface(item.element.Key));

        schematicItems.InsertRange(0, surfaceSchematicItem);

        foreach (SchematicItem sItem in schematicItems)
        {
            UITextBlockLeft textBlock = null;

            if (!schematic._isDiagram)
            {
                if (writeOnLeft)
                    textBlock = WriteLeftBlock(sItem);
                else
                    textBlock = WriteLeftAtRightBlock(sItem);

                writeOnLeft = !writeOnLeft;
            }
            else 
            {
                textBlock = WriteLeftBlock(sItem);
            }

            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();

            if (leftBlocks.Count > startIndex && (textBlock.IsOverlappingWith(leftBlocks[leftBlocks.Count - interval]) || textBlock.pivot.position.y > leftBlocks[leftBlocks.Count - interval].pivot.position.y))
            {
                float adjustment = leftBlocks[leftBlocks.Count - interval].pivot.rect.height;
                textBlock.pivot.anchoredPosition = new Vector2(textBlock.pivot.anchoredPosition.x, leftBlocks[leftBlocks.Count - interval].pivot.anchoredPosition.y - adjustment);
            }

            leftBlocks.Add(textBlock);
            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();
        }


        LineDrawer.instance.ClearLines();
        foreach (UITextBlockLeft block in leftBlocks)
        {
            bool isLeft = block.pivot.position.x < 0;

            Vector2 originOffset = block.schematicItem.element.originOffset;
            float xOrigin = block.LinePivot.transform.position.x + originOffset.x;
            float yOrigin = block.LinePivot.transform.position.y + originOffset.y;

            Vector2 targetOffset = block.schematicItem.element.targetOffset;
            float xTarget = targetOffset.x;
            float yTarget = block.schematicItem.element._drawRectLine ? 0 : targetOffset.y;

            xTarget += block.schematicItem.element._columItem ? 0 : targetOffset.x;
            // IN CASE OF LEFT ALIGNED ITEM X POSTION SHOULD BE A NEGATIVE NUMBER
            xTarget = isLeft ? xTarget * -1 : xTarget;

            yTarget += block.schematicItem.element._drawRectLine ? 
                block.LinePivot.transform.position.y : 
                -block.schematicItem.GetMidPoint().RealToVirtualScale();

            LineDrawer.instance.CreateLine(xOrigin, yOrigin, xTarget, yTarget);
        }
    }

    public static List<SchematicItem> RemoveItemCopies(List<SchematicItem> schematicItems)
    {        
        HashSet<string> uniqueItems = new HashSet<string>();
        List<SchematicItem> itemsToRemove = new List<SchematicItem>();

        foreach (var item in schematicItems)
        {
            string uniqueKey = item.ToString();
            if (uniqueItems.Contains(uniqueKey))
            {
                itemsToRemove.Add(item);
            }
            else
            {
                uniqueItems.Add(uniqueKey);
            }
        }

        foreach (var block in itemsToRemove)
        {
            schematicItems.Remove(block);
        }

        return schematicItems;
    }

    public UITextBlockLeft WriteLeftBlock(SchematicItem sItem)
    {
        UITextBlockLeft txtBlock = Instantiate(txtBlockLeftSimple, canvasLeft);
        txtBlock.gameObject.name = $"LTXT - {sItem.ToString()}";
        txtBlock.WriteElementOnLeft(sItem);
        return txtBlock;
    }

    public UITextBlockLeft WriteLeftAtRightBlock(SchematicItem sItem)
    {
        UITextBlockLeft txtBlock = Instantiate(txtBlockRightSimple, canvasRightSimple);
        txtBlock.gameObject.name = $"RSIMPLE - {sItem.ToString()}";
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
