using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.UI.Image;

[System.Serializable]
public class SchematicItem : SchematicDrawable
{
#if UNITY_EDITOR
    public bool useElementKey = true;
    [ShowIf("useElementKey")] public string elementKey = "casing";
    [HideIf("useElementKey")]
#endif
    public BaseElement element;
    [HideInInspector] public string _virtualName;
    [HideInInspector] public string _description;
    private bool _hideText = false;
    [Space]
    public float _origin;
    public float _depth;
    [Space]
    [FoldoutGroup("Grouping", expanded: false)] public string _mainGroup = "default";
    [FoldoutGroup("Grouping", expanded: false)] public string _subGroup = "";

    public float _length => Mathf.Abs(_origin) + _depth;

    [Space]
    [HideInInspector] public bool dontFill = true;
    [HideInInspector] public float _widthOffset;
    [HideInInspector] public bool hideElement = false;

    public bool WriteText => !_hideText && element._writePartOnDoc;

    public virtual async UniTask Draw(int additionalSort = 0) => await Draw(_origin, _depth, additionalSort);
    public virtual async UniTask Draw(float origin, float deph, int additionalSort = 0)
    {
        if (hideElement)
            return;

        Rect drawArea = new Rect();

        float width = !dontFill ? SchematicGenerator.DRAW_LIMITS_HORIZONTAL : SchematicGenerator.DRAW_WELL_SIZE + _widthOffset.RealToVirtualScale();

        drawArea.size = new Vector2(width, (deph - origin).RealToVirtualScale());
        drawArea.position = new Vector2(0, -1 * (origin.RealToVirtualScale()));

        await element.StartDraw(this, drawArea, additionalSort);
    }

    public float GetMidPoint() 
    {
        return _origin + ((_depth - _origin) / 2);
    }

    public override string ToString()
    {
        string elementName = _virtualName.IsNullOrWhitespace() ? element.ToString() : _virtualName;

        if (_origin + _depth == 0 || (_origin < 0 && _depth < 0)) 
        {
            return elementName;
        }
        if (_origin <= 0)
        {
            return $"{elementName} até {_depth.ToString("F2")}m";
        }
        else
        {
            return $"{elementName}: {_origin.ToString("F2")} - {_depth.ToString("F2")}m";
        }
    }

    public JsonObject ToJsonObject() 
    {
        return new JsonObject(this);
    }

    [System.Serializable]
    public struct JsonObject
    {
        public string name;
        public string description;
        public bool hideText;
        public string element;
        public float topo;
        public float @base;
        public string mainGroup;
        public string subGroup;

        public JsonObject(SchematicItem schematicItem)
        {
            this.name = schematicItem._virtualName;
            this.description = schematicItem._description;
            this.hideText = schematicItem._hideText;
            this.element = schematicItem.element.Key;
            this.topo = schematicItem._origin;
            this.@base = schematicItem._depth;
            this.mainGroup = schematicItem._mainGroup;
            this.subGroup = schematicItem._subGroup;
        }

        public SchematicItem ConvertToObject() 
        {
            var result = new SchematicItem();
            result._virtualName = name;
            result._description = description;
            result._hideText = hideText;
            result.element = SchematicGenerator.elements[this.element];
            result._mainGroup = this.mainGroup;
            result._origin = this.topo;
            result._depth = this.@base;
            result._mainGroup = mainGroup;
            result._subGroup = subGroup;
            return result;
        }
    }
}

public interface SchematicDrawable 
{
    public UniTask Draw(int additionalSort = 0);
}