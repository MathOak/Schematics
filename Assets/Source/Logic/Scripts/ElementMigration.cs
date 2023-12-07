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

    [Space(Spacing.SPACING_SMALL)]

    public string elementEngName = "Generic Part ENG";

    [Space(Spacing.SPACING_SMALL)]

    public string key = "generic_element";

    [Space(Spacing.SPACING_SMALL)]

    [EnumToggleButtons]
    public ElementType elementType;

    [Space(Spacing.SPACING_MEDIUM)]

    public List<ElementImage> images;
}

public class Spacing
{
    public const int SPACING_SMALL = 10;
    public const int SPACING_MEDIUM = 25;
    public const int SPACING_BIG = 35;
}

[System.Serializable]
public class ElementImage
{
    [Space(Spacing.SPACING_SMALL)]

    [PreviewField]
    public Sprite image;

    [Space(Spacing.SPACING_MEDIUM)]

    public SpriteDrawMode drawMode = SpriteDrawMode.Simple;

    [Space(Spacing.SPACING_SMALL)]

    public SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.None;

    [Space(Spacing.SPACING_SMALL)]

    public SortingLayerPicker layer;

    [Space(Spacing.SPACING_SMALL)]

    public int orderInLayer = 0;

    [Space(Spacing.SPACING_SMALL)]

    [BoxGroup("ImageSizingSetup")]
    [EnumToggleButtons]
    public ImageSizeType imageSizeType;

    [Space(Spacing.SPACING_SMALL)]

    [BoxGroup("ImageSizingSetup")]
    [ShowIf("imageSizeType", ImageSizeType.FixedSize)]
    public Vector2 size;

    [Space(Spacing.SPACING_SMALL)]

    [BoxGroup("ImageSizingSetup")]
    [ShowIf("imageSizeType", ImageSizeType.Clamped)]
    public Vector2 minimumSize;

    [Space(Spacing.SPACING_SMALL)]

    [BoxGroup("ImageSizingSetup")]
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
