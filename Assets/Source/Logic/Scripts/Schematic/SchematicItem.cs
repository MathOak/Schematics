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
    public float __origin;
    public float __depth;

    public float originOffset = 0;
    public float depthOffset = 0;
    
    public float _origin => __origin + originOffset;
    public float _depth => __depth + depthOffset;
    [Space]
    [FoldoutGroup("Grouping", expanded: false)] public string _mainGroup = "default";
    [FoldoutGroup("Grouping", expanded: false)] public string _subGroup = "";

    public float _length => Mathf.Abs(_origin) + _depth;

    [Space]
    [HideInInspector] public bool dontFill = true;
    [HideInInspector] public float _widthOffset;
    [HideInInspector] public bool hideElement = false;

    public enum truncateMethod { none, warp, breakLine}

    public bool WriteText => !_hideText && element._writePartOnDoc;

    public virtual async UniTask Draw(int additionalSort = 0) => await Draw(_origin, _depth, additionalSort);
    public virtual async UniTask Draw(float origin, float deph, int additionalSort = 0)
    {
        if (hideElement)
            return;

        Rect drawArea = new Rect();

        float width = !dontFill ? Constants.DRAW_LIMITS_HORIZONTAL : Constants.DRAW_WELL_SIZE + ExtensionMethods.RealToVirtualScale(_widthOffset);

        drawArea.size = new Vector2(width, ExtensionMethods.RealToVirtualScale(deph - origin));
        drawArea.position = new Vector2(0, -1 * ExtensionMethods.RealToVirtualScale(origin));

        await element.StartDraw(this, drawArea, additionalSort);
    }

    public float GetMidPoint() 
    {
        return _origin + ((_depth - _origin) / 2);
    }

    public float GetBotPoint()
    {
        float result = _depth;

        if (element.minimalVirtualHeight > 0 && ExtensionMethods.RealToVirtualScale(_depth - _origin) < element.minimalVirtualHeight) 
        {
            result = GetMidPoint() + ExtensionMethods.VirtualToRealScale(element.minimalVirtualHeight / 2);
        }

        return result;
    }

    public float GetTopPoint()
    {
        float result = _origin;

        if (element.minimalVirtualHeight > 0 && ExtensionMethods.RealToVirtualScale(_depth - _origin) < element.minimalVirtualHeight)
        {
            result = GetMidPoint() - ExtensionMethods.VirtualToRealScale(element.minimalVirtualHeight / 2);
        }

        return result;
    }

    public override string ToString()
    {
        if (SurfaceElementsUtil.IsFromSurface(element.Key))
        {
            return GetElementName();
        }

        return GetElementName() + GetElementPositions();
    }

    public string GetElementName(truncateMethod truncate = truncateMethod.none)
    {
        var result = _virtualName.IsNullOrWhitespace() ? element.ToString() : _virtualName;

        if (truncate == truncateMethod.none)
            return result;
        else if (truncate == truncateMethod.warp)
            return LimitAndAppend(result, Constants.DEFAULT_CHARS_LIMIT - GetElementPositions().Length);
        else
        {
            result = Regex.Replace(result, Constants.DEFAULT_CHARS_LIMIT.ToString(), "$1\n");

            return result;
        }
    }

    public string GetElementPositions() 
    {
        if (__origin + __depth == 0 || (__origin < 0 && __depth < 0))
        {
            return "";
        }
        if (__origin <= 0)
        {
            return $" até {__depth.ToString("F2")}m";
        }
        else
        {
            return $" {__origin.ToString("F2")}-{__depth.ToString("F2")}m";
        }
    }

    private string LimitAndAppend(string input, int maxLength)
    {
        if (input == null)
            return null;

        if (input.Length <= maxLength)
            return input;

        return input.Substring(0, maxLength - 3) + "... ";
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
            this.topo = schematicItem.__origin;
            this.@base = schematicItem.__depth;
            this.mainGroup = schematicItem._mainGroup;
            this.subGroup = schematicItem._subGroup;
        }

        public SchematicItem ConvertToObject() 
        {
            var result = new SchematicItem();
            result._virtualName = name;
            result._description = description;
            result._hideText = hideText;
            result.element = ElementDatabase.Elements[this.element];
            result._mainGroup = this.mainGroup;
            result._subGroup = this.subGroup;
            result.__origin = this.topo;
            result.__depth = this.@base;

            return result;
        }
    }
}

public interface SchematicDrawable 
{
    public UniTask Draw(int additionalSort = 0);
}