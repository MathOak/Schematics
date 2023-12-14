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

    public void CreateLine(UITextBlockLeft block) 
    {
        bool isLeft = block.pivot.position.x < 0;
        LineRenderer line = Instantiate(lineRenderer, lineParent);
        float xPos = block.schematicItem.element._columItem ? 0 : (isLeft ? -0.95f : 0.95f) ;
        line.SetPosition(0, new Vector3(block.LinePivot.transform.position.x, block.LinePivot.transform.position.y, 0));
        float yPos = block.schematicItem.element._drawRectLine ?
            block.LinePivot.transform.position.y : ExtensionMethods.RealToVirtualScale(-block.schematicItem.GetMidPoint());
        line.SetPosition(1, new Vector3(xPos, yPos, 0));
        lines.Add(line);
    }

    public void CreateLine(float xOrigin, float yOrigin, float xTarget, float yTarget)
    {
        LineRenderer line = Instantiate(lineRenderer, lineParent);
        line.SetPosition(0, new Vector3(xOrigin, yOrigin, 0));        
        line.SetPosition(1, new Vector3(xTarget, yTarget, 0));
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
