using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class GameNamespaceValues : List<GameNamespaceValues.Item>
    {
        public GameNamespaceValues()
        {
            Add(new Item("Minecraft", "minecraft"));
        }

        public class Item
        {
            public string Text {get;}
            public string Value {get;}

            public Item(string text, string value)
            {
                Text = text;
                Value = value;
            }
        }
    }
}
