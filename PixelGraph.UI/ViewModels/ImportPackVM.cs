using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewModels
{
    internal class ImportPackVM : ViewModelBase
    {
        public bool AsGlobal {get; set;}
        public bool CopyUntracked {get; set;}
        public string SourcePath {get; set;}
        public string SourceFile {get; set;}
        public string SourceFormat {get; set;}
        //public ResourcePackOutputProperties SourceEncoding {get; set;}
        public ResourcePackInputProperties PackInput {get; set;}


        public ImportPackVM()
        {
            SourceFormat = TextureEncoding.Format_Default;

            //SourceEncoding = new ResourcePackOutputProperties {
            //    Format = TextureEncoding.Format_Default,
            //};

            CopyUntracked = true;
            AsGlobal = false;
        }
    }

    internal class ImportPackDesignVM : ImportPackVM
    {
        public ImportPackDesignVM()
        {
            SourceFile = "C:\\SomePath\\File.zip";
        }
    }
}
