using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    public class PomTypeValues : List<PomTypeValues.Item>
    {
        public static readonly Item Smooth = new("Smooth", true, false);
        public static readonly Item Sharp = new("Sharp", false, true);
        public static readonly Item Normal = new("Normal", false, false);


        public PomTypeValues() : base(new[] {Smooth, Normal, Sharp}) {}

        public class Item
        {
            public string Name {get; set;}
            public bool EnableLinearSampling {get; set;}
            public bool EnableSlopeNormals {get; set;}


            public Item(string name, bool enableLinearSampling, bool enableSlopeNormals)
            {
                Name = name;
                EnableLinearSampling = enableLinearSampling;
                EnableSlopeNormals = enableSlopeNormals;
            }
        }
    }
}
