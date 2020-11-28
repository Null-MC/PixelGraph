namespace PixelGraph.UI.ViewModels
{
    internal class TreeSearchFilter : ViewModelBase
    {
        private string _text;
        private bool _showAllFiles;

        public string Text {
            get => _text;
            set {
                _text = value;
                OnPropertyChanged();
            }
        }

        public bool ShowAllFiles {
            get => _showAllFiles;
            set {
                _showAllFiles = value;
                OnPropertyChanged();
            }
        }
    }
}
