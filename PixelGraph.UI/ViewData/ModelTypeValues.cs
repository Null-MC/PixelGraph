using PixelGraph.Common.Models;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class ModelTypeValues : List<ModelTypeValues.Item>
    {
        public ModelTypeValues()
        {
            Add(new Item {Text = "Cube", Value = ModelType.Cube});
            Add(new Item {Text = "Cross", Value = ModelType.Cross});
            Add(new Item {Text = "Plane", Value = ModelType.Plane});
            Add(new Item {Text = "File", Value = ModelType.File});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
