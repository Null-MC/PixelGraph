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
        #region Properties

        private readonly object busyLock;
        private ResourcePackInputProperties _packInput;
        private string _rootDirectory;
        private string _treeSearch;
        private string _selectedTag;
        private TextureTreeNode _selectedNode;
        private TextureSource _selectedSource;
        private MaterialProperties _loadedMaterial;
        private string _loadedMaterialFilename;
        private volatile bool _isBusy;

        public ObservableCollection<string> RecentDirectories {get; set;}
        public ObservableCollection<ProfileItem> Profiles {get;}
        public ObservableCollection<TextureSource> Textures {get;}
        public TextureTreeNode TreeRoot {get;}
        public bool IsUpdatingSources {get; set;}

        public bool HasRootDirectory => _rootDirectory != null;
        public bool HasTreeSelection => _selectedNode is TextureTreeTexture;

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasRootDirectory));
            }
        }

        public ResourcePackInputProperties PackInput {
            get => _packInput;
            set {
                _packInput = value;
                OnPropertyChanged();
            }
        }

        public string TreeSearch {
            get => _treeSearch;
            set {
                _treeSearch = value;
                OnPropertyChanged();
            }
        }

        public string SelectedTag {
            get => _selectedTag;
            set {
                _selectedTag = value;
                OnPropertyChanged();

                if (value != null && !TextureTags.Is(value, _selectedSource?.Tag)) {
                    SelectFirstTexture();
                }
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
                if (IsUpdatingSources) return;

                _selectedSource = value;
                OnPropertyChanged();

                if (value?.Tag != null && !TextureTags.Is(_selectedTag, value.Tag))
                    SelectedTag = value.Tag;
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
            RecentDirectories = new ObservableCollection<string>();
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
            //var tag = _selectedTag ?? TextureTags.Albedo;
            SelectedSource = Textures.FirstOrDefault(x => TextureTags.Is(x.Tag, _selectedTag));
            //SelectedSource = texture; // ?? Textures.FirstOrDefault();
        }
    }

    internal class MainWindowDesignVM : MainWindowVM
    {
        public MainWindowDesignVM()
        {
            //AddRecentItems();

            AddTreeItems();

            RootDirectory = "x:\\dev\\test-rp";
            IsBusy = true;


            Textures.Add(new TextureSource
            {
                Tag = TextureTags.Albedo,
                Name = "albedo.png",
                Image = null,
            });

            Textures.Add(new TextureSource
            {
                Tag = TextureTags.Normal,
                Name = "normal.png",
                Image = null,
            });
        }

        private void AddRecentItems()
        {
            for (var i = 0; i < 8; i++)
                RecentDirectories.Add($"C:\\Some\\Fake\\Path-{i + 1}");
        }

        private void AddTreeItems()
        {
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
        }
    }
}
