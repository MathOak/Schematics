using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class ItemMainGroup
{
    public string _name;
    public List<SchematicItem> items;

    public ItemMainGroup(string mainGroupName, List<SchematicItem> items)
    {
        this._name = mainGroupName;
        this.items = items;
    }

    public string GetListNames()
    {
        StringBuilder builder = new StringBuilder();
        foreach (var item in items)
        {
            builder.AppendLine($"-<indent=3%>{item.GetElementName(SchematicItem.truncateMethod.breakLine) + item.GetElementPositions()}</indent>");
        }

        return builder.ToString();
    }

    string AutoWordWrap(string textToAppend)
    {
        if (textToAppend.Length <= 25)
        {
            return textToAppend;
        }

        int numLetters = 0;

        string[] words = textToAppend.Split(' ');

        StringBuilder stringBuilder = new StringBuilder();

        foreach (string word in words)
        {
            if (numLetters + word.Length > 25)
            {
                stringBuilder.Append("\n" + word + " ");
                numLetters = 0;
                continue;
            }

            stringBuilder.Append(word + " ");
            numLetters += word.Length + 1;
        }

        return stringBuilder.ToString();
    }
}
