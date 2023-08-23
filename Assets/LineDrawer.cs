using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    public static LineDrawer instance;

    private void Awake()
    {
        instance = this;
    }

    public void CreateLine(UITextBlock textBlock) 
    {
        LineRenderer line = Instantiate(lineRenderer);
        float xPos = textBlock.visualElement.SchematicItem.element._columItem ? 0 : -0.95f;
        line.SetPosition(0, new Vector3(textBlock.LinePivot.transform.position.x, textBlock.LinePivot.transform.position.y, 0));
        float yPos = textBlock.visualElement.SchematicItem.element._drawRectLine ? textBlock.LinePivot.transform.position.y : -textBlock.visualElement.SchematicItem.GetMidPoint().RealToVirtualScale();
        line.SetPosition(1, new Vector3(xPos, yPos, 0));
    }
}
