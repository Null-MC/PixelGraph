using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;

namespace PixelGraph.UI.Models
{
    internal class PublishOutputModel : ModelBase
    {
        private volatile bool _isLoading;
        private volatile bool _isActive;
        private bool _closeOnComplete;

        public ResourcePackInputProperties Input {get; set;}
        public ResourcePackProfileProperties Profile {get; set;}
        public string RootDirectory {get; set;}
        public string Destination {get; set;}
        public bool Archive {get; set;}
        public bool Clean {get; set;}
        
        public bool IsLoading {
            get => _isLoading;
            set {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive {
            get => _isActive;
            set {
                if (_isActive == value) return;
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool CloseOnComplete {
            get => _closeOnComplete;
            set {
                if (_closeOnComplete == value) return;
                _closeOnComplete = value;
                OnPropertyChanged();
            }
        }
    }

    internal class PublishOutputDesignerModel : PublishOutputModel
    {
        public PublishOutputDesignerModel()
        {
            IsActive = true;
            //OnAppendLog(LogLevel.Debug, "Hello World!");
            //AppendLog(LogLevel.Warning, "Something is wrong...");
            //AppendLog(LogLevel.Error, "DANGER Will Robinson");
        }
    }
}
