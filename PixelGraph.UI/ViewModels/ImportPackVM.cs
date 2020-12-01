using Microsoft.Extensions.Logging;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewModels
{
    internal class ImportPackVM : ViewModelBase
    {
        private ImportTreeNode _rootNode;
        private string _rootDirectory;
        private string _importSource;

        public bool IsArchive {get; set;}
        public bool AsGlobal {get; set;}
        public bool CopyUntracked {get; set;}
        public string SourceFormat {get; set;}
        //public ResourcePackOutputProperties SourceEncoding {get; set;}
        public ResourcePackInputProperties PackInput {get; set;}
        public LogListVM LogList {get;}

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();
            }
        }

        public string ImportSource {
            get => _importSource;
            set {
                _importSource = value;
                OnPropertyChanged();
            }
        }

        public ImportTreeNode RootNode {
            get => _rootNode;
            set {
                _rootNode = value;
                OnPropertyChanged();
            }
        }


        public ImportPackVM()
        {
            LogList = new LogListVM();

            SourceFormat = TextureEncoding.Format_Default;
            CopyUntracked = true;
            AsGlobal = false;
        }
    }

    internal class ImportPackDesignVM : ImportPackVM
    {
        public ImportPackDesignVM()
        {
            ImportSource = "C:\\SomePath\\File.zip";
            IsArchive = true;

            LogList.Append(LogLevel.Information, "Hello World!");
        }
    }
}
