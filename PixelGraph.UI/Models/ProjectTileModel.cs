using PixelGraph.UI.Internal;
using System.Windows.Media;

namespace PixelGraph.UI.Models
{
    public class ProjectTileModel : ModelBase
    {
        public string Filename {get; set;}
        public ImageSource Icon {get; set;}
        public string Name {get; set;}
        public string Description {get; set;}
    }
}
