using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITextBlock : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmPro;
    [SerializeField] RectTransform pivot;
    [SerializeField] Collider boxCollider;
    [SerializeField] Transform linePivot;
    public Transform LinePivot => linePivot;
    private UITextBlock overlapParent = null;

    public VisualElement visualElement;

    public void SetupElementString(VisualElement visualElement, bool setPosition) 
    {
        this.visualElement = visualElement;
        float writeYpos = visualElement.SchematicItem.element._fixedWritePosition ?
            -visualElement.SchematicItem.element._fixedPosition :
            -visualElement.SchematicItem.GetMidPoint().RealToVirtualScale();

        SetupText(
            visualElement.SchematicItem.ToString(),
            writeYpos,
            setPosition);
    }

    public void SetupText(string text, float yPosition, bool setPosition)
    {
        tmPro.text = text;
        if (setPosition)
        {
            pivot.anchoredPosition = new Vector3(0, yPosition, 0);
        }
    }

    public void AddLine(string textLine) 
    {
        tmPro.text += (tmPro.text.IsNullOrWhitespace()) ? $"{textLine}" : $"\n{textLine}";
    }

    public void OverlapText(UITextBlock otherBlock) 
    {
        DebugBondingBox();
        otherBlock.DebugBondingBox();

        if (otherBlock.overlapParent == null && boxCollider.bounds.Intersects(otherBlock.boxCollider.bounds) == true) 
        {
            UITextBlock parent = overlapParent == null ? this : overlapParent;

            otherBlock.overlapParent = parent;
            otherBlock.tmPro.transform.SetParent(parent.transform);

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.pivot);
        }
    }

    [Button]
    public void DebugBondingBox()
    {
        Debug.Log(boxCollider.bounds);
    }
}
