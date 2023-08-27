using System.Collections.Generic;
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
            builder.AppendLine(item.ToString());
        }

        return builder.ToString();
    }
}
