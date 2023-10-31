using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform lineParent;

    public static LineDrawer instance;

    private List<LineRenderer> lines = new List<LineRenderer>();

    private void Awake()
    {
        instance = this;
    }

    public void CreateLine(UITextBlockLeft textBlock) 
    {
        bool isLeft = textBlock.pivot.position.x < 0;
        LineRenderer line = Instantiate(lineRenderer, lineParent);
        float xPos = textBlock.schematicItem.element._columItem ? 0 : (isLeft ? -0.95f : 0.95f) ;
        line.SetPosition(0, new Vector3(textBlock.LinePivot.transform.position.x, textBlock.LinePivot.transform.position.y, 0));
        float yPos = textBlock.schematicItem.element._drawRectLine ? 
            textBlock.LinePivot.transform.position.y : 
            -textBlock.schematicItem.GetMidPoint().RealToVirtualScale();
        line.SetPosition(1, new Vector3(xPos, yPos, 0));
        lines.Add(line);
    }

    public void ClearLines()
    {
        foreach (var line in lines)
        {
            Destroy(line.gameObject);
        }
        lines = new List<LineRenderer>();
    }
}
