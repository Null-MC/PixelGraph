using PixelGraph.Common;
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
        public string TextureFilename {get; set;}
        public PbrProperties Texture {get; set;}
        //public ImageSource AlbedoSource {get; set;}
    }
}
