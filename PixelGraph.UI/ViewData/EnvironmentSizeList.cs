using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    public class EnvironmentSizeList : List<EnvironmentSizeList.Item>
    {
        public EnvironmentSizeList()
        {
            Add(new Item("Low (x256)", 256));
            Add(new Item("Medium (x512)", 512));
            Add(new Item("High (x1024)", 1024));
        }

        public class Item
        {
            public string Name {get;}
            public int Value {get;}

            public Item(string name, int value)
            {
                Name = name;
                Value = value;
            }
        }
    }
}
