using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    public class IrradianceSizeList : List<IrradianceSizeList.Item>
    {
        public IrradianceSizeList()
        {
            Add(new Item("Low (32x)", 32));
            Add(new Item("Medium (64x)", 64));
            Add(new Item("High (128x)", 128));
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
