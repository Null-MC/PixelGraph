using Microsoft.Extensions.Logging;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;

namespace PixelGraph.UI.ViewModels
{
    internal class ImportPackVM : ViewModelBase
    {
        private ImportTreeNode _rootNode;
        private string _rootDirectory;
        private string _importSource;
        private volatile bool _isReady;
        private volatile bool _isActive;
        private volatile bool _showLog;

        public bool IsArchive {get; set;}
        public bool AsGlobal {get; set;}
        public bool CopyUntracked {get; set;}
        public string SourceFormat {get; set;}
        //public ResourcePackOutputProperties SourceEncoding {get; set;}
        public ResourcePackInputProperties PackInput {get; set;}
        public LogListVM LogList {get;}

        public bool IsReady {
            get => _isReady;
            set {
                _isReady = value;
                OnPropertyChanged();
            }
        }

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

        public bool IsActive {
            get => _isActive;
            set {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool ShowLog {
            get => _showLog;
            set {
                _showLog = value;
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

            ShowLog = true;
        }
    }
}
