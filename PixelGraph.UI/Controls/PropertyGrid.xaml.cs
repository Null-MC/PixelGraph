using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace PixelGraph.UI.Controls
{
    //[ContentProperty("Items")]
    public partial class PropertyGrid
    {
        //public IEnumerable<BasePropertyRow> Items {
        //    get => (IEnumerable<BasePropertyRow>)GetValue(ItemsProperty);
        //    set => SetValue(ItemsProperty, value);
        //}


        public PropertyGrid()
        {
            InitializeComponent();
        }

        //public static readonly DependencyProperty ItemsProperty = DependencyProperty
        //    .Register("Items", typeof(IEnumerable<BasePropertyRow>), typeof(PropertyGrid));
    }

    //internal class PropertyGridDesignerData : PropertyCollection
    //{
    //    public PropertyGridDesignerData()
    //    {
    //        Add(new BoolPropertyRow {
    //            Label = "Bool Property",
    //            Value = true,
    //        });

    //        Add(new TextPropertyRow {
    //            Label = "Text Property",
    //            Value = "Hello World!",
    //        });
    //    }
    //}
}
