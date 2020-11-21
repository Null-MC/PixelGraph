using PixelGraph.Common.Material;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.ViewModels
{
    internal class TextureTreeNode
    {
        public string Name {get; set;}
        public ObservableCollection<TextureTreeNode> Nodes {get; set;}


        public TextureTreeNode()
        {
            Nodes = new ObservableCollection<TextureTreeNode>();
        }
    }

    internal class TextureTreeDirectory : TextureTreeNode
    {
        //
    }

    internal class TextureTreeTexture : TextureTreeNode
    {
        public string MaterialFilename {get; set;}
        public MaterialProperties Material {get; set;}
        //public ImageSource AlbedoSource {get; set;}
    }
}
