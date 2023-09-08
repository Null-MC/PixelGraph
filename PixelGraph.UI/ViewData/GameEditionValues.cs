using PixelGraph.Common;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData;

internal class GameEditionValues : List<GameEditionValues.Item>
{
    public GameEditionValues()
    {
        Add(new Item {Text = "Java", Value = GameEditions.Java});
        Add(new Item {Text = "Bedrock", Value = GameEditions.Bedrock});
    }

    public class Item
    {
        public string Text {get; set;}
        public GameEditions Value {get; set;}
    }
}