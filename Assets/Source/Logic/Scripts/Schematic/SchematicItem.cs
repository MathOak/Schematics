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
    [Required] public BaseElement element;
    public string _virtualName;
    public string _description;
    public bool _hideText = false;
    [Space]
    public float _origin;
    public float _deph;
    [Space]
    public string _mainGroup = "default";
    public string _subGroup = "";

    public float _lenght => Mathf.Abs(_origin) + _deph;

    [Space]
    [ShowIf("dontFill")] public float _widthOffset;
    public bool dontFill = true;
    public bool hideElement = false;

    public virtual async UniTask Draw(int additionalSort = 0) => await Draw(_origin, _deph, additionalSort);
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
        return _origin + ((_deph - _origin) / 2);
    }

    public override string ToString()
    {
        string elementName = _virtualName.IsNullOrWhitespace() ? element.ToString() : _virtualName;

        if (_origin + _deph == 0 || (_origin < 0 && _deph < 0)) 
        {
            return elementName;
        }
        if (_origin <= 0)
        {
            return $"{elementName} até {_deph.ToString("F2")}m";
        }
        else
        {
            return $"{elementName}: {_origin.ToString("F2")} - {_deph.ToString("F2")}m";
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
            this.@base = schematicItem._deph;
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
            result._deph = this.@base;
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