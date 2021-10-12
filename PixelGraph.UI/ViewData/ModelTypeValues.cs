using PixelGraph.Common.Models;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class ModelTypeValues : List<ModelTypeValues.Item>
    {
        public ModelTypeValues()
        {
            Add(new Item {Text = "Bell", Value = ModelType.Bell});
            Add(new Item {Text = "Boat", Value = ModelType.Boat});
            Add(new Item {Text = "Cow", Value = ModelType.Cow});
            Add(new Item {Text = "Cube", Value = ModelType.Cube});
            Add(new Item {Text = "Plane", Value = ModelType.Plane});
            Add(new Item {Text = "Zombie", Value = ModelType.Zombie});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
