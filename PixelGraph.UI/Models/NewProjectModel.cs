using PixelGraph.UI.Internal;

namespace PixelGraph.UI.Models
{
    internal class NewProjectModel : ModelBase
    {
        private NewProjectStates _state;
        private string _packName;
        private string _projectFilename;
        private bool _createMinecraftFolders;
        private bool _createRealmsFolders;
        private bool _createOptifineFolders;
        private bool _enablePackImport;
        //private bool _createDefaultProfile;
        private bool _importFromDirectory;
        private bool _importFromArchive;

        public string PackName {
            get => _packName;
            set {
                if (_packName == value) return;
                _packName = value;
                OnPropertyChanged();
            }
        }

        public string ProjectFilename {
            get => _projectFilename;
            set {
                if (_projectFilename == value) return;
                _projectFilename = value;
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

        //public bool CreateDefaultProfile {
        //    get => _createDefaultProfile;
        //    set {
        //        if (_createDefaultProfile == value) return;
        //        _createDefaultProfile = value;
        //        OnPropertyChanged();
        //    }
        //}

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

        public bool IsLocationPage => _state == NewProjectStates.Location;
        public bool IsReviewPage => _state == NewProjectStates.Review;


        public NewProjectModel()
        {
            _state = NewProjectStates.Location;
            _packName = "My New RP";
            _createMinecraftFolders = true;
            _createRealmsFolders = false;
            _createOptifineFolders = false;
            //_createDefaultProfile = true;
            _enablePackImport = false;
        }

        public void SetState(NewProjectStates state)
        {
            if (state == _state) return;

            _state = state;
            OnPropertyChanged(nameof(IsLocationPage));
            OnPropertyChanged(nameof(IsReviewPage));
        }
    }

    internal class NewProjectDesignVM : NewProjectModel
    {
        public NewProjectDesignVM()
        {
            PackName = "Sample RP";
            ProjectFilename = "C:\\Somewhere\\over\\the\\rainbow\\isShit.yml";
            SetState(NewProjectStates.Location);
        }
    }

    internal enum NewProjectStates
    {
        Location,
        Review,
    }
}
