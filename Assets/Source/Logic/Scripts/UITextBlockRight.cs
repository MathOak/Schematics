using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITextBlockRight : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmPro;
    [SerializeField] TextMeshProUGUI grouptmPro;
    [SerializeField] Image backgroundImage;
    List<string> colorList = new List<string>() { "#ff081f", "#006fbc" };

    public void WriteGroupBlock(ItemMainGroup mainGroup, int index)
    {
        Color newColor;

        string colorHtml = colorList[index % colorList.Count];
        ColorUtility.TryParseHtmlString(colorHtml, out newColor);

        if (index >= colorList.Count)
        {
            int multiplier = index / colorList.Count;
            Color.RGBToHSV(newColor, out float h, out float s, out float v);
            newColor = Color.HSVToRGB(h + (0.1f) * multiplier, s, v);
        }

        backgroundImage.color = newColor;

        grouptmPro.text = $"{mainGroup._name}";
        grouptmPro.color = newColor;

        string text = $"\n{mainGroup.GetListNames()}";
        tmPro.text = text;

        VisualElement.SetGroupColor(mainGroup._name, newColor);
    }
}