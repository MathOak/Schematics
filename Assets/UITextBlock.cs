using Sirenix.OdinInspector;
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

    public void SetupText(VisualElement visualElement)
    {
        tmPro.text = visualElement.SchematicItem.ToString();
        pivot.anchoredPosition = new Vector3(0, visualElement.transform.localPosition.y, 0);
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
