using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static System.Net.Mime.MediaTypeNames;

public class UITextBlockLeft : MonoBehaviour
{
    public TextMeshProUGUI tmPro;
    public RectTransform pivot;
    public Transform LinePivot;
    public SchematicItem schematicItem;

    public void WriteElementOnLeft(SchematicItem sItem) 
    {
        this.schematicItem = sItem;
        tmPro.text = sItem.GetElementName(SchematicItem.truncateMethod.warp) + sItem.GetElementPositions();

        float yPos = sItem.element._fixedWritePosition ? sItem.element._fixedPosition : sItem.GetMidPoint().RealToVirtualScale();
        
        pivot.anchoredPosition = new Vector3(0, -yPos, 0);
    }

    public bool IsOverlappingWith(UITextBlockLeft otherBlock) 
    {
        return GetWorldRect(this.pivot).Overlaps(GetWorldRect(otherBlock.pivot));
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector2 size = Vector2.Scale(rt.rect.size, rt.lossyScale);
        float rectWidth = size.x;
        float rectHeight = size.y;
        Vector2 position = rt.position;
        return new Rect(position.x - rectWidth * 0.5f, position.y - rectHeight * 0.5f, rectWidth, rectHeight);
    }
}
