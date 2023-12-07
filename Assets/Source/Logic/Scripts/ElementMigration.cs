using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "part_name", menuName = "Part/Create Part")]
public class ElementMigration : ScriptableObject
{
    [Header("Element Ids")]
    public string elementName = "Generic Part";

    public string elementEngName = "Generic Part ENG";
    
    public string key;

    [EnumToggleButtons]
    public ElementType elementType;

    public List<ElementImage> images;
}

[System.Serializable]
public class ElementImage
{
    [PreviewField]
    public Sprite sprite;

    public SpriteDrawMode drawMode = SpriteDrawMode.Simple;
    public SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.None;
    public SortingLayerTest layer;
    public int orderInLayer = 0;
    Vector2 offset = Vector2.zero;

    [EnumToggleButtons]
    public ImageSizeType imageSizeType;

    [ShowIf("imageSizeType", ImageSizeType.FixedSize)]
    public Vector2 size;

    [ShowIf("imageSizeType", ImageSizeType.Clamped)]
    public Vector2 minimumSize;

    [ShowIf("imageSizeType", ImageSizeType.Clamped)]
    public Vector2 maximumSize;
}

[System.Serializable]
public enum ImageType
{
    SolidColor,
    Sprite
}

[System.Serializable]

public enum ImageSizeType
{
    AutoResize,
    NoResize,
    FixedSize,
    Clamped
}

[System.Serializable]
public enum ElementType
{
    Column,
    Head
}

[Serializable]
public struct SortingLayerPicker
{
    public int Layer;
}

[CustomPropertyDrawer(typeof(SortingLayerPicker))]
public class SortingLayerTest : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var valueProp = property.FindPropertyRelative("Layer");

        var layers = SortingLayer.layers.Select(x => x.name).ToArray();
        valueProp.intValue = EditorGUI.Popup(position, label.text, valueProp.intValue, layers);
    }
}
