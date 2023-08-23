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
    private UITextBlock overlapParent = null;

    public void SetupText(VisualElement visualElement, bool setPosition) 
    {
        SetupText(
            visualElement.SchematicItem.ToString(),
            visualElement.transform.localPosition.y,
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
        Debug.Log($"Testing {gameObject} with {otherBlock.gameObject}");
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
