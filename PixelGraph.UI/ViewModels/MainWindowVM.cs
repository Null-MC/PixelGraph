using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using System;

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowVM : ViewModelBase
    {
        private readonly object busyLock;
        public ProfileVM Profile {get;}
        public TextureVM Texture {get;}
        private string _rootDirectory;
        private volatile bool _isBusy;

        public bool HasRootDirectory => _rootDirectory != null;

        public string CurrentContext => _rootDirectory == null ? null
            : PathEx.Join(_rootDirectory, Profile.Selected?.Name ?? "*");

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasRootDirectory));
                OnPropertyChanged(nameof(CurrentContext));
            }
        }

        public bool IsBusy {
            get => _isBusy;
            set {
                _isBusy = value;
                OnPropertyChanged();
            }
        }


        public MainWindowVM(ProfileVM profile = null, TextureVM texture = null)
        {
            Profile = profile ?? new ProfileVM();
            Profile.SelectionChanged += Profile_OnSelectionChanged;

            Texture = texture ?? new TextureVM();

            busyLock = new object();
        }

        public bool TryStartBusy()
        {
            lock (busyLock) {
                if (IsBusy) return false;
                IsBusy = true;
                return true;
            }
        }

        public void EndBusy()
        {
            lock (busyLock) {
                IsBusy = false;
            }
        }

        private void Profile_OnSelectionChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentContext));
        }
    }

    internal class MainWindowDesignVM : MainWindowVM
    {
        public MainWindowDesignVM() : base(new ProfileDesignVM())
        {
            RootDirectory = "x:\\dev\\test-rp";
            IsBusy = true;

            Texture.TreeSearch = "as";
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
