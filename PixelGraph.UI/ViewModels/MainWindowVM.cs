using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowVM : ViewModelBase
    {
        private string _loadedPackFilename;
        private string _rootDirectory;
        private ProfileItem _selectedProfile;
        private PackProperties _loadedPack;

        public event EventHandler ProfileChanged;
        public event EventHandler DataChanged;

        public ObservableCollection<ProfileItem> Profiles {get;}
        public PackEncodingVM Encoding {get;}
        public TextureVM Texture {get;}

        public bool HasRootDirectory => _rootDirectory != null;
        public Visibility PackEditVisibility => GetVisibility(_selectedProfile != null);

        public string CurrentContext => _rootDirectory == null ? null
            : PathEx.Join(_rootDirectory, SelectedProfile?.Name ?? "*");

        public bool HasSelectedProfile => _selectedProfile != null;

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasRootDirectory));
                OnPropertyChanged(nameof(CurrentContext));
            }
        }

        public ProfileItem SelectedProfile {
            get => _selectedProfile;
            set {
                _selectedProfile = value;
                OnPropertyChanged();

                OnProfileChanged();
                OnPropertyChanged(nameof(CurrentContext));
                OnPropertyChanged(nameof(HasSelectedProfile));
            }
        }

        public PackProperties LoadedPack {
            get => _loadedPack;
            set {
                _loadedPack = value;
                OnPropertyChanged();

                Encoding.Pack = value;
                OnPropertyChanged(nameof(PackEditVisibility));
                OnPropertyChanged(nameof(GameEdition));
                OnPropertyChanged(nameof(PackFormat));
                OnPropertyChanged(nameof(PackDescription));
                OnPropertyChanged(nameof(PackTags));
            }
        }

        public string LoadedPackFilename {
            get => _loadedPackFilename;
            set {
                _loadedPackFilename = value;
                OnPropertyChanged();
            }
        }

        public string GameEdition {
            get => LoadedPack?.PackEdition;
            set {
                if (LoadedPack == null) return;
                LoadedPack.PackEdition = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int PackFormat {
            get => LoadedPack?.PackFormat ?? 0;
            set {
                if (LoadedPack == null) return;
                LoadedPack.PackFormat = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string PackDescription {
            get => LoadedPack?.PackDescription;
            set {
                if (LoadedPack == null) return;
                LoadedPack.PackDescription = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string PackTags {
            get => LoadedPack?.PackTags;
            set {
                if (LoadedPack == null) return;
                LoadedPack.PackTags = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }


        public MainWindowVM()
        {
            Profiles = new ObservableCollection<ProfileItem>();

            Encoding = new PackEncodingVM();
            Encoding.Changed += (o, e) => OnDataChanged();

            Texture = new TextureVM();
        }

        private void OnProfileChanged()
        {
            ProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class MainWindowDVM : MainWindowVM
    {
        public MainWindowDVM()
        {
            RootDirectory = "x:\\dev\\test-rp";

            Profiles.Add(SelectedProfile = new ProfileItem {
                Name = "Test Profile #1",
                Filename = "Test_Profile_#1.pack.properties",
            });
            Profiles.Add(new ProfileItem {
                Name = "Test Profile #2",
                Filename = "Test_Profile_#2.pack.properties",
            });

            LoadedPack = new PackProperties {
                Properties = {
                    ["input.format"] = EncodingProperties.Raw,
                    ["output.format"] = EncodingProperties.Lab13,
                }
            };

            Texture.Search = "as";
            Texture.TreeRoot.Nodes.Add(new TextureTreeDirectory {
                Name = "assets",
                Nodes = {
                    new TextureTreeDirectory {
                        Name = "minecraft",
                        Nodes = {
                            new TextureTreeTexture {
                                Name = "Dirt",
                            },
                            new TextureTreeTexture {
                                Name = "Grass",
                            },
                            new TextureTreeTexture {
                                Name = "Glass",
                            },
                            new TextureTreeTexture {
                                Name = "Stone",
                            },
                        },
                    },
                },
            });

            Texture.Textures.Add(new TextureSource {
                Tag = TextureTags.Albedo,
                Name = "albedo.png",
                Image = null,
            });

            Texture.Textures.Add(new TextureSource {
                Tag = TextureTags.Normal,
                Name = "normal.png",
                Image = null,
            });
        }
    }
}
