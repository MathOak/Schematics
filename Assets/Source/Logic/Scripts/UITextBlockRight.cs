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
        while(index >= colorList.Count) 
        {
            index -= colorList.Count;
        }
        

        Color color = colorList[index];
        grouptmPro.text = $"{mainGroup._name}";

        backgroundImage.color = color;
        backgroundImage2.color = color;
        string text = $"\n{mainGroup.GetListNames()}";
        tmPro.text = text;
    }
}