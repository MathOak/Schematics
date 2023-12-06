using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
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
    public SortingLayer layer;
    public int orderInLayer = 0;
    Vector2 offset = Vector2.zero;
}

[System.Serializable]
public enum ImageType
{
    SolidColor,
    Sprite
}

[System.Serializable]
public enum ElementType
{
    Column,
    Head
}
