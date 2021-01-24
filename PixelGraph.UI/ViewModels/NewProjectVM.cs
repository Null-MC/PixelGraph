using PixelGraph.Common.Encoding;

namespace PixelGraph.UI.ViewModels
{
    internal class NewProjectVM : ViewModelBase
    {
        private NewProjectStates _state;
        private string _contentFormat;
        private string _location;
        private bool _createMinecraftFolders;
        private bool _createRealmsFolders;
        private bool _createOptifineFolders;
        private bool _enablePackImport;
        private bool _createDefaultProfile;
        private bool _importFromDirectory;
        private bool _importFromArchive;

        public string ContentFormat {
            get => _contentFormat;
            set {
                if (_contentFormat == value) return;
                _contentFormat = value;
                OnPropertyChanged();
            }
        }

        public string Location {
            get => _location;
            set {
                if (_location == value) return;
                _location = value;
                OnPropertyChanged();
            }
        }

        public bool CreateMinecraftFolders {
            get => _createMinecraftFolders;
            set {
                if (_createMinecraftFolders == value) return;
                _createMinecraftFolders = value;
                OnPropertyChanged();
            }
        }

        public bool CreateRealmsFolders {
            get => _createRealmsFolders;
            set {
                if (_createRealmsFolders == value) return;
                _createRealmsFolders = value;
                OnPropertyChanged();
            }
        }

        public bool CreateOptifineFolders {
            get => _createOptifineFolders;
            set {
                if (_createOptifineFolders == value) return;
                _createOptifineFolders = value;
                OnPropertyChanged();
            }
        }

        public bool CreateDefaultProfile {
            get => _createDefaultProfile;
            set {
                if (_createDefaultProfile == value) return;
                _createDefaultProfile = value;
                OnPropertyChanged();
            }
        }

        public bool EnablePackImport {
            get => _enablePackImport;
            set {
                if (_enablePackImport == value) return;
                _enablePackImport = value;
                OnPropertyChanged();
            }
        }

        public bool ImportFromDirectory {
            get => _importFromDirectory;
            set {
                if (_importFromDirectory == value) return;
                _importFromDirectory = value;
                OnPropertyChanged();
            }
        }

        public bool ImportFromArchive {
            get => _importFromArchive;
            set {
                if (_importFromArchive == value) return;
                _importFromArchive = value;
                OnPropertyChanged();
            }
        }

        public bool IsFormatPage => _state == NewProjectStates.Format;
        public bool IsLocationPage => _state == NewProjectStates.Location;
        public bool IsReviewPage => _state == NewProjectStates.Review;


        public NewProjectVM()
        {
            _state = NewProjectStates.Location;
            _contentFormat = TextureEncoding.Format_Raw;
            _createMinecraftFolders = true;
            _createRealmsFolders = true;
            _createOptifineFolders = true;
            _createDefaultProfile = true;
            _enablePackImport = false;
        }

        public void SetState(NewProjectStates state)
        {
            if (state == _state) return;

            _state = state;
            OnPropertyChanged(nameof(IsFormatPage));
            OnPropertyChanged(nameof(IsLocationPage));
            OnPropertyChanged(nameof(IsReviewPage));
        }
    }

    internal class NewProjectDesignVM : NewProjectVM
    {
        public NewProjectDesignVM()
        {
            ContentFormat = TextureEncoding.Format_Raw;
            Location = "C:\\Somewhere\\over\\the\\rainbow";
            SetState(NewProjectStates.Location);
        }
    }

    internal enum NewProjectStates
    {
        Location,
        Format,
        Review,
    }
}
