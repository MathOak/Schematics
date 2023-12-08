using AlmostEngine.Screenshot;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Schema;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

public class SchematicRenderer : MonoBehaviour
{
    [SerializeField] private Camera _schematicCamera;
    [SerializeField] private Camera _printCamera;

    private static SchematicRenderer _instance;

    private Vector2Int size = new Vector2Int();
    private Vector2Int oldSize = new Vector2Int();

    private void Awake()
    {
        _instance = this;
    }

    public static void RenderSchematic() => _instance.InternalRenderSchematic();

    private void InternalRenderSchematic()
    {
        Logger.Info("Rendering Schematic to image...");

        float lastDeph = SchematicGenerator.lastGeneration.GetLastDepth(2);
        lastDeph = lastDeph < 2500 ? 2500 : lastDeph;

        float drawSize = (lastDeph.RealToVirtualScale() / 2) + (ChristmasTreeGenerator.HeadSize / 2) + 1.4f;

        _schematicCamera.orthographicSize = drawSize;
        _schematicCamera.transform.position = new Vector3(0, -drawSize + (ChristmasTreeGenerator.HeadSize), -10);

        float cameraAspect = _schematicCamera.aspect;
        float imageWidth = Constants.MAX_HEIGHT * cameraAspect;
        float imageHeight = Constants.MAX_HEIGHT;

        if (imageWidth > Constants.MAX_WIDTH)
        {
            imageWidth = Constants.MAX_WIDTH;
            imageHeight = Constants.MAX_WIDTH / cameraAspect;
        }

        RenderTexture renderTexture = new RenderTexture((int)imageWidth, (int)imageHeight, 32);
        _schematicCamera.targetTexture = renderTexture;

        _printCamera.orthographicSize = _schematicCamera.orthographicSize;
        _printCamera.aspect = cameraAspect;
        _printCamera.transform.position = new Vector3(0, _schematicCamera.transform.position.y, -10);

        var tempTex = SimpleScreenshotCapture.CaptureCameraToTexture((int)imageWidth, (int)imageHeight, _printCamera);

        Texture2D finalTex = new Texture2D(Constants.MAX_WIDTH, Constants.MAX_HEIGHT);
        Color[] fillColorArray = finalTex.GetPixels();

        for (int i = 0; i < fillColorArray.Length; i++)
        {
            fillColorArray[i] = Color.white;
        }

        finalTex.SetPixels(fillColorArray);
        finalTex.Apply();

        int startX = (Constants.MAX_WIDTH - (int)imageWidth) / 2;
        int startY = (Constants.MAX_HEIGHT - (int)imageHeight) / 2;

        finalTex.SetPixels(startX, startY, (int)imageWidth, (int)imageHeight, tempTex.GetPixels());
        finalTex.Apply();

        UnityWebInteractions.SendGeneratedTexture(finalTex);
    }

    [Button]
    private void ResizeTexture()
    {
        Texture2D texture = LoadPNG(Path.Combine(Application.streamingAssetsPath, "generator_result.jpg"), size);

        float proportion = oldSize.y / size.y;

        Vector2Int newSize = new Vector2Int(oldSize.x / (int)proportion, oldSize.y / (int)proportion);

        TextureScaler.Scale(texture, newSize.x, newSize.y);

        byte[] bytes = texture.EncodeToJPG();
        File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, "generator_result1.png"), bytes);
    }

    private Texture2D LoadPNG(string filePath, Vector2Int size)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(size.x, size.y);
            tex.LoadImage(fileData);

            oldSize = new Vector2Int(tex.width, tex.height);
        }
        return tex;
    }
}
