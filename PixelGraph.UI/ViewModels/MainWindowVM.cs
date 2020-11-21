using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowVM : ViewModelBase
    {
        //public event EventHandler PackInputChanged;
        //public event EventHandler PackProfileChanged;
        //public event EventHandler MaterialChanged;

        #region Properties

        private readonly object busyLock;
        private ResourcePackInputProperties _packInput;
        //private ProfileItem _selectedProfileItem;
        //private TextureEncoding _inputEncoding;
        //private TextureOutputEncoding _outputEncoding;
        //private ResourcePackProfileProperties _loadedProfile;
        //private string _selectedTextureTag;
        private string _rootDirectory;
        private string _treeSearch;
        private TextureTreeNode _selectedNode;
        private TextureSource _selectedSource;
        private MaterialProperties _loadedMaterial;
        private string _loadedMaterialFilename;
        private volatile bool _isBusy;

        //private MaterialVM _materialVM;

        public ObservableCollection<ProfileItem> Profiles {get;}
        public ObservableCollection<TextureSource> Textures {get;}
        public TextureTreeNode TreeRoot {get;}
        //public ProfileVM Profile {get;}

        public bool HasRootDirectory => _rootDirectory != null;
        //public bool HasSelectedProfile => _selectedProfileItem != null;
        //public bool HasLoadedProfile => _loadedProfile != null;
        public bool HasTreeSelection => _selectedNode is TextureTreeTexture;

        //public string CurrentContext => _rootDirectory == null ? null
        //    : PathEx.Join(_rootDirectory, _selectedProfileItem?.Name ?? "*");

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasRootDirectory));
                //OnPropertyChanged(nameof(CurrentContext));
            }
        }

        public ResourcePackInputProperties PackInput {
            get => _packInput;
            set {
                _packInput = value;
                OnPropertyChanged();

                //UpdateInput();
                //UpdateDefaultProperties();
            }
        }

        //public ResourcePackProfileProperties LoadedProfile {
        //    get => _loadedProfile;
        //    set {
        //        _loadedProfile = value;
        //        OnPropertyChanged();

        //        OnPropertyChanged(nameof(HasLoadedProfile));
        //    }
        //}

        //public string SelectedTextureTag {
        //    get => _selectedTextureTag;
        //    set {
        //        _selectedTextureTag = value;
        //        OnPropertyChanged();

        //        //UpdateInput();
        //        //UpdateOutput();
        //        //UpdateDefaultProperties();
        //    }
        //}

        //public TextureEncoding InputEncoding {
        //    get => _inputEncoding;
        //    set {
        //        _inputEncoding = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public TextureOutputEncoding OutputEncoding {
        //    get => _outputEncoding;
        //    set {
        //        _outputEncoding = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public MaterialVM Material {
        //    get => _materialVM;
        //    set {
        //        _materialVM = value;
        //        OnPropertyChanged();
        //    }
        //}

        public string TreeSearch {
            get => _treeSearch;
            set {
                _treeSearch = value;
                OnPropertyChanged();
            }
        }

        public TextureTreeNode SelectedNode {
            get => _selectedNode;
            set {
                _selectedNode = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasTreeSelection));
            }
        }

        public TextureSource SelectedSource {
            get => _selectedSource;
            set {
                _selectedSource = value;
                OnPropertyChanged();
            }
        }

        public string LoadedMaterialFilename {
            get => _loadedMaterialFilename;
            set {
                _loadedMaterialFilename = value;
                OnPropertyChanged();
            }
        }

        public MaterialProperties LoadedMaterial {
            get => _loadedMaterial;
            set {
                _loadedMaterial = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy {
            get => _isBusy;
            set {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        #endregion


        public MainWindowVM()
        {
            //Profile = profile ?? new ProfileVM();
            //Profile.SelectionChanged += Profile_OnSelectionChanged;

            //LoadedMaterial = material;

            Profiles = new ObservableCollection<ProfileItem>();
            Textures = new ObservableCollection<TextureSource>();
            TreeRoot = new TextureTreeNode();
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

        internal TextureTreeNode GetTreeNode(string path)
        {
            var parts = path.Split('/', '\\');
            var parent = TreeRoot;

            foreach (var part in parts) {
                var node = parent.Nodes.FirstOrDefault(x => string.Equals(x.Name, part, StringComparison.InvariantCultureIgnoreCase));

                if (node == null) {
                    node = new TextureTreeDirectory {Name = part};
                    parent.Nodes.Add(node);
                }

                parent = node;
            }

            return parent;
        }

        public void SelectFirstTexture()
        {
            var albedo = Textures.FirstOrDefault(x => TextureTags.Is(x.Tag, TextureTags.Albedo));
            SelectedSource = albedo ?? Textures.FirstOrDefault();
        }

        //private void Profile_OnSelectionChanged(object sender, EventArgs e)
        //{
        //    OnPropertyChanged(nameof(CurrentContext));
        //}

        //private void OnLoadedPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    DataChanged?.Invoke(this, EventArgs.Empty);
        //}
    }

    internal class MainWindowDesignVM : MainWindowVM
    {
        public MainWindowDesignVM()
        {
            RootDirectory = "x:\\dev\\test-rp";
            IsBusy = true;

            TreeSearch = "as";
            TreeRoot.Nodes.Add(new TextureTreeDirectory {
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

            Textures.Add(new TextureSource {
                Tag = TextureTags.Albedo,
                Name = "albedo.png",
                Image = null,
            });

            Textures.Add(new TextureSource {
                Tag = TextureTags.Normal,
                Name = "normal.png",
                Image = null,
            });
        }
    }
}
