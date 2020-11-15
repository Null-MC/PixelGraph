using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using System;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.ViewModels
{
    internal class ProfileVM : ViewModelBase
    {
        private string _loadedPackFilename;
        private ProfileItem _selected;
        private PackProperties _loaded;

        public event EventHandler SelectionChanged;
        public event EventHandler DataChanged;

        public ObservableCollection<ProfileItem> Profiles {get;}
        public PackEncodingVM Encoding {get;}
        public bool HasSelection => _selected != null;

        public ProfileItem Selected {
            get => _selected;
            set {
                _selected = value;
                OnPropertyChanged();

                OnSelectionChanged();
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        public PackProperties Loaded {
            get => _loaded;
            set {
                _loaded = value;
                OnPropertyChanged();

                Encoding.Pack = value;
                OnPropertyChanged(nameof(GameEdition));
                OnPropertyChanged(nameof(PackFormat));
                OnPropertyChanged(nameof(PackDescription));
                OnPropertyChanged(nameof(PackTags));
            }
        }

        public string LoadedFilename {
            get => _loadedPackFilename;
            set {
                _loadedPackFilename = value;
                OnPropertyChanged();
            }
        }

        public string GameEdition {
            get => _loaded?.PackEdition;
            set {
                if (_loaded == null) return;
                _loaded.PackEdition = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int PackFormat {
            get => _loaded?.PackFormat ?? 0;
            set {
                if (_loaded == null) return;
                _loaded.PackFormat = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string PackDescription {
            get => _loaded?.PackDescription;
            set {
                if (_loaded == null) return;
                _loaded.PackDescription = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string PackTags {
            get => _loaded?.PackTags;
            set {
                if (_loaded == null) return;
                _loaded.PackTags = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }


        public ProfileVM()
        {
            Profiles = new ObservableCollection<ProfileItem>();

            Encoding = new PackEncodingVM();
            Encoding.Changed += (o, e) => OnDataChanged();
        }

        private void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class ProfileDesignVM : ProfileVM
    {
        public ProfileDesignVM()
        {
            Profiles.Add(Selected = new ProfileItem {
                Name = "Test Profile #1",
                Filename = "Test_Profile_#1.pack.properties",
            });
            Profiles.Add(new ProfileItem {
                Name = "Test Profile #2",
                Filename = "Test_Profile_#2.pack.properties",
            });

            Loaded = new PackProperties {
                Properties = {
                    ["input.format"] = EncodingProperties.Raw,
                    ["output.format"] = EncodingProperties.Lab13,
                }
            };

            Loaded = new PackProperties {
                InputFormat = EncodingProperties.Raw,
                OutputFormat = EncodingProperties.Lab13,
            };
        }
    }
}
