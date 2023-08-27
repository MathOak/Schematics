using TMPro;
using UnityEngine;

public class UITextBlockRight : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmPro;

    public void WriteGroupBlock(ItemMainGroup mainGroup) 
    {
        string text = $"**{mainGroup._name}**";
        text += $"\n{mainGroup.GetListNames()}";
        tmPro.text = text;
    }
}