using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    public static LineDrawer instance;

    private List<LineRenderer> lines = new List<LineRenderer>();

    private void Awake()
    {
        instance = this;
    }

    public void CreateLine(UITextBlockLeft textBlock) 
    {
        LineRenderer line = Instantiate(lineRenderer);
        float xPos = textBlock.schematicItem.element._columItem ? 0 : -0.95f;
        line.SetPosition(0, new Vector3(textBlock.LinePivot.transform.position.x, textBlock.LinePivot.transform.position.y, 0));
        float yPos = textBlock.schematicItem.element._drawRectLine ? textBlock.LinePivot.transform.position.y : -textBlock.schematicItem.GetMidPoint().RealToVirtualScale();
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
    public void CreateHeadLine(UITextBlockLeft textBlock)
    {
        LineRenderer line = Instantiate(lineRenderer);
        float xPos = textBlock.schematicItem.element._columItem ? 0 : -0.95f;
        line.SetPosition(0, new Vector3(textBlock.LinePivot.transform.position.x, textBlock.LinePivot.transform.position.y, 0));
        float yPos = textBlock.schematicItem.element._drawRectLine ? textBlock.LinePivot.transform.position.y : -textBlock.schematicItem.GetMidPoint().RealToVirtualScale();
        line.SetPosition(1, new Vector3(xPos, yPos + 1.5f, 0));
        lines.Add(line);
    }
}
