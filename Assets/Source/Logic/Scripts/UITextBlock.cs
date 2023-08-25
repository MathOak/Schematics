using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static System.Net.Mime.MediaTypeNames;

public class UITextBlock : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmPro;
    [SerializeField] RectTransform pivot;
    [SerializeField] Collider boxCollider;
    private UITextBlock overlapParent = null;

    public void SetupText(VisualElement visualElement, float yPosition, bool setPosition) 
    {
        SetupText(
            visualElement.SchematicItem.element.ElementName,
            yPosition,
            setPosition);
    }

    public void SetupText(string text, float yPosition, bool setPosition)
    {
        tmPro.text = text;
        if (setPosition)
        {
            print(yPosition + " posicao");
            pivot.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, yPosition, 0);
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
            RectTransform rect = pivot.GetComponent<RectTransform>();

            //for (int i = 0; i < 100; i++)
            //{
            //    if (boxCollider.bounds.Intersects(otherBlock.boxCollider.bounds))
            //    {
            //        rect.anchoredPosition = new Vector3(0, rect.anchoredPosition.y, 0);

            //    }
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
