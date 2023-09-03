using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITextBlockRight : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmPro;
    [SerializeField] TextMeshProUGUI grouptmPro;
    [SerializeField] Image backgroundImage;
    [SerializeField] Image backgroundImage2;
    List<Color> colorList = new List<Color>() { Color.blue, Color.red, Color.green};

    public void WriteGroupBlock(ItemMainGroup mainGroup, int index)
    {
        Color mainColor = colorList[index % colorList.Count];

        if (index >= colorList.Count)
        {
            int multiplier = index / colorList.Count;
            Color.RGBToHSV(mainColor, out float h, out float s, out float v);
            mainColor = Color.HSVToRGB(h + (0.1f) * multiplier, s, v);
        }

        grouptmPro.text = $"{mainGroup._name}";

        backgroundImage.color = mainColor;
        backgroundImage2.color = mainColor;
        string text = $"\n{mainGroup.GetListNames()}";
        tmPro.text = text;

        VisualElement.SetGroupColor(mainGroup._name, mainColor);
    }
}