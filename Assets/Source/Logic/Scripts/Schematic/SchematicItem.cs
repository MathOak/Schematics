using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.UI.Image;

[System.Serializable]
public class SchematicItem : SchematicDrawable
{
    [Required] public BaseElement element;
    [Space]
    public float _origin;
    public float _deph;
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

    public override string ToString()
    {
        if (_origin + _deph == 0 || (_origin < 0 && _deph < 0)) 
        {
            return element.ToString();
        }
        if (_origin <= 0)
        {
            return $"{element} até {_deph.ToString("F2")}m";
        }
        else
        {
            return $"{element}: {_origin.ToString("F2")} - {_deph.ToString("F2")}m";
        }
    }

    public JsonObject ToJsonObject() 
    {
        return new JsonObject(this);
    }

    [System.Serializable]
    public struct JsonObject
    {
        public string part;
        public float origin;
        public float deph;

        public JsonObject(SchematicItem schematicItem)
        {
            this.part = schematicItem.element.Key;
            this.origin = schematicItem._origin;
            this.deph = schematicItem._deph;
        }

        public SchematicItem ConvertToObject() 
        {
            var result = new SchematicItem();
            result.element = SchematicGenerator.elements[this.part];
            result._origin = this.origin;
            result._deph = this.deph;
            return result;
        }
    }
}

public interface SchematicDrawable 
{
    public UniTask Draw(int additionalSort = 0);
}