using PixelGraph.Common;
using PixelGraph.UI.Internal;
using System.Windows;

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowVM : ViewModelBase
    {
        private PackPropertiesEditor _profile;
        public PackPropertiesEditor Profile {
            get => _profile;
            set {
                _profile = value;
                OnPropertyChanged();
            }
        }

        private string _packFilename;
        public string PackFilename {
            get => _packFilename;
            set {
                _packFilename = value;
                OnPropertyChanged();
            }
        }

        private bool _isEditingProfile = true;
        public bool IsEditingProfile {
            get => _isEditingProfile;
            set {
                _isEditingProfile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProfileVisibility));
            }
        }

        private bool _isEditingContent;
        public bool IsEditingContent {
            get => _isEditingContent;
            set {
                _isEditingContent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ContentVisibility));
            }
        }

        public virtual Visibility EditVisibility => GetVisibility(_profile != null);
        public virtual Visibility ProfileVisibility => GetVisibility(_profile != null && _isEditingProfile);
        public virtual Visibility ContentVisibility => GetVisibility(_profile != null && _isEditingContent);

        public void Initialize(PackProperties pack)
        {
            _profile = new PackPropertiesEditor(pack);
            _isEditingProfile = true;
            _isEditingContent = false;

            OnPropertyChanged(nameof(Profile));
            OnPropertyChanged(nameof(EditVisibility));
            OnPropertyChanged(nameof(ProfileVisibility));
            OnPropertyChanged(nameof(ContentVisibility));
        }
    }

    internal class MainWindowDVM : MainWindowVM
    {
        public override Visibility EditVisibility => Visibility.Visible;
        public override Visibility ProfileVisibility => Visibility.Visible;
        public override Visibility ContentVisibility => Visibility.Visible;

        public MainWindowDVM()
        {
            Profile = new PackPropertiesEditor();
        }
    }
}
