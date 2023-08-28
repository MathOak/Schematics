using AlmostEngine.Screenshot;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SchematicRender : MonoBehaviour
{
    [SerializeField] private Camera _printCamera;

    [Button]
    public void RenderSchematic()
    {
        float lastDepth = SchematicGenerator.lastGeneration.terrains[SchematicGenerator.lastGeneration.terrains.Count - 1].depth;
        float drawSize = (lastDepth.RealToVirtualScale() / 2) + (SchematicGenerator.HEAD_SIZE / 2);

        _printCamera.orthographicSize = drawSize;
        _printCamera.transform.position = new Vector3(0, -drawSize + (SchematicGenerator.HEAD_SIZE), -10);

        Rect cameraViewRect = new Rect(_printCamera.rect);
        float widthValue = Mathf.InverseLerp(0, _printCamera.orthographicSize, SchematicGenerator.DRAW_LIMITS_HORIZONTAL / 2);
        cameraViewRect.size = new Vector2(widthValue, cameraViewRect.height);

        //_printCamera.rect = cameraViewRect;
        _printCamera.aspect = widthValue;

        int printWidth = 512;
        int printHeight = (Mathf.CeilToInt(drawSize / SchematicGenerator.DRAW_LIMITS_HORIZONTAL * 2)) * printWidth;

        SimpleScreenshotCapture.CaptureCameraToFile("D:/Projetos/_Resources/Schematicos/Testes/schematic_generator_result.png", printWidth, printHeight, _printCamera);
    }
}