using PixelGraph.Common.ResourcePack;
using System;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.ViewModels
{
    internal class ProfilesVM : ViewModelBase
    {
        protected ObservableCollection<ProfileItem> _profiles;
        private ResourcePackProfileProperties _loadedProfile;
        private ProfileItem _selectedProfileItem;
        private string _selectedTextureTag;
        private string _rootDirectory;

        public event EventHandler DataChanged;

        public bool HasSelectedProfile => _selectedProfileItem != null;
        public bool HasLoadedProfile => _loadedProfile != null;

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();

                //OnPropertyChanged(nameof(HasRootDirectory));
                //OnPropertyChanged(nameof(CurrentContext));
            }
        }

        public ObservableCollection<ProfileItem> Profiles {
            get => _profiles;
            set {
                _profiles = value;
                OnPropertyChanged();
            }
        }

        public ResourcePackProfileProperties LoadedProfile {
            get => _loadedProfile;
            set {
                _loadedProfile = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasLoadedProfile));
                OnPropertyChanged(nameof(GameEdition));
                OnPropertyChanged(nameof(PackFormat));
                OnPropertyChanged(nameof(PackDescription));
                OnPropertyChanged(nameof(PackTags));
                OnPropertyChanged(nameof(OutputFormat));
                // TODO: ...
            }
        }

        public ProfileItem SelectedProfileItem {
            get => _selectedProfileItem;
            set {
                _selectedProfileItem = value;
                OnPropertyChanged();

                //UpdateOutput();
                //UpdateDefaultProperties();
                OnPropertyChanged(nameof(HasSelectedProfile));
                //OnPropertyChanged(nameof(CurrentContext));
            }
        }

        public string SelectedTextureTag {
            get => _selectedTextureTag;
            set {
                _selectedTextureTag = value;
                OnPropertyChanged();

                //UpdateInput();
                //UpdateOutput();
                //UpdateDefaultProperties();
            }
        }

        public string GameEdition {
            get => _loadedProfile?.Edition;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Edition = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int? PackFormat {
            get => _loadedProfile?.Format;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Format = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string PackDescription {
            get => _loadedProfile?.Description;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Description = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string PackTags {
            get => _loadedProfile?.Tags;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Tags = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string OutputFormat {
            get => _loadedProfile?.Output?.Format;
            set {
                if (_loadedProfile == null) return;
                _loadedProfile.Output ??= new ResourcePackOutputProperties();
                _loadedProfile.Output.Format = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }


        public ProfilesVM()
        {
            _profiles = new ObservableCollection<ProfileItem>();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class ProfilesDesignVM : ProfilesVM
    {
        public ProfilesDesignVM()
        {
            _profiles.Add(new ProfileItem {Name = "Profile A"});
            _profiles.Add(new ProfileItem {Name = "Profile B"});
            _profiles.Add(new ProfileItem {Name = "Profile C"});

            SelectedProfileItem = _profiles[0];

            LoadedProfile = new ResourcePackProfileProperties {
                Edition = "Java",
                Description = "Designer Data",
                Format = 99,
                ImageEncoding = "tga",
                Output = {
                    Sampler = "point",
                },
            };
        }
    }
}
