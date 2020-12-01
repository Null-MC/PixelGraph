using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixelGraph.UI.ViewModels
{
    internal class ImportTreeNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name {get; set;}
        public ObservableCollection<ImportTreeNode> Nodes {get; set;}

        private bool _included;
        public bool Included {
            get => _included;
            set {
                _included = value;
                OnPropertyChanged();
            }
        }


        public ImportTreeNode()
        {
            Nodes = new ObservableCollection<ImportTreeNode>();
            _included = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class ImportTreeDirectory : ImportTreeNode
    {
        public string Path {get; set;}
    }

    internal class ImportTreeFile : ImportTreeNode
    {
        public string Filename {get; set;}
    }
}
