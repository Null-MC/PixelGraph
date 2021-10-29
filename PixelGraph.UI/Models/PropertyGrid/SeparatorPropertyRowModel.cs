using PixelGraph.UI.Internal;

namespace PixelGraph.UI.Models.PropertyGrid
{
    public interface ISeparatorPropertyRow : IPropertyRow {}

    public class SeparatorPropertyRowModel : ModelBase, ISeparatorPropertyRow
    {
        public object ActualValue => null;

        public bool Enabled {
            get => true;
            set {}
        }
    }
}
