using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewData;

namespace PixelGraph.UI.Models
{
    internal class PublishOutputModel : ModelBase
    {
        private volatile bool _isActive;
        private bool _closeOnComplete;

        public ProfileItem Profile {get; set;}
        public string RootDirectory {get; set;}
        public string Destination {get; set;}
        public bool Archive {get; set;}
        public bool Clean {get; set;}

        public bool IsActive {
            get => _isActive;
            set {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool CloseOnComplete {
            get => _closeOnComplete;
            set {
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
