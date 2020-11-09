using PixelGraph.Common;

namespace PixelGraph.UI.ViewModels
{
    internal class PackPropertiesVM : ViewModelBase
    {
        private readonly PackProperties pack;


        public PackPropertiesVM(PackProperties pack = null)
        {
            this.pack = pack ?? new PackProperties();
        }
    }
}
