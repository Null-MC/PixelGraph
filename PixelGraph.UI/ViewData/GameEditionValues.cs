using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class GameEditionValues : List<GameEditionValues.Item>
    {
        public GameEditionValues()
        {
            Add(new Item {Text = "Java", Value = "java"});
            Add(new Item {Text = "Bedrock", Value = "bedrock"});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
