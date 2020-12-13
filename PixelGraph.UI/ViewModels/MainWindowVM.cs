using MaterialDesignThemes.Wpf;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowVM : ViewModelBase, ISearchParameters
    {
        #region Properties

        private readonly object busyLock;
        private ResourcePackInputProperties _packInput;
        private string _rootDirectory;
        private string _searchText;
        private bool _showAllFiles;
        private string _selectedTag;
        private ContentTreeNode _selectedNode;
        private TextureSource _selectedSource;
        private MaterialProperties _loadedMaterial;
        private ImageSource _loadedTexture;
        private ProfileItem _selectedProfile;
        private string _loadedMaterialFilename;
        private bool _publishArchive, _publishClean;
        private volatile bool _isBusy;

        public ObservableCollection<ProfileItem> Profiles {get;}
        public ObservableCollection<TextureSource> Textures {get;}
        public bool IsUpdatingSources {get; set;}

        public bool HasRootDirectory => _rootDirectory != null;
        public bool HasTreeSelection => _selectedNode is ContentTreeFile;
        public bool HasTreeTextureSelection => _selectedNode is ContentTreeFile _file && _file.Type == ContentNodeType.Texture;
        //public bool IsMaterialSelected => _selectedNode is TextureTreeFile _file && _file.Type == NodeType.Material;
        public bool HasLoadedMaterial => _loadedMaterial != null;
        public bool HasLoadedTexture => _loadedTexture != null;
        public bool HasPreviewImage => HasLoadedMaterial || HasLoadedTexture;
        public bool HasProfileSelection => _selectedProfile != null;

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasRootDirectory));
            }
        }

        private ContentTreeNode _treeRoot;
        private ObservableCollection<string> _recentDirectories;

        public ObservableCollection<string> RecentDirectories {
            get => _recentDirectories;
            set {
                _recentDirectories = value;
                OnPropertyChanged();
            }
        }

        public ContentTreeNode TreeRoot {
            get => _treeRoot;
            set {
                _treeRoot = value;
                OnPropertyChanged();
            }
        }

        public ResourcePackInputProperties PackInput {
            get => _packInput;
            set {
                _packInput = value;
                OnPropertyChanged();
            }
        }

        public string SearchText {
            get => _searchText;
            set {
                _searchText = value;
                OnPropertyChanged();

                TreeRoot.UpdateVisibility(this);
            }
        }

        public bool ShowAllFiles {
            get => _showAllFiles;
            set {
                _showAllFiles = value;
                OnPropertyChanged();

                TreeRoot.UpdateVisibility(this);
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

        public ContentTreeNode SelectedNode {
            get => _selectedNode;
            set {
                _selectedNode = value;
                OnPropertyChanged();

                SelectedSource = null;
                OnPropertyChanged(nameof(HasTreeSelection));
                OnPropertyChanged(nameof(HasTreeTextureSelection));
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

        public ProfileItem SelectedProfile {
            get => _selectedProfile;
            set {
                _selectedProfile = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasProfileSelection));
            }
        }

        public string LoadedMaterialFilename {
            get => _loadedMaterialFilename;
            set {
                _loadedMaterialFilename = value;
                OnPropertyChanged();
            }
        }

        public ImageSource LoadedTexture {
            get => _loadedTexture;
            set {
                _loadedTexture = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasLoadedTexture));
                OnPropertyChanged(nameof(HasPreviewImage));
            }
        }

        public MaterialProperties LoadedMaterial {
            get => _loadedMaterial;
            set {
                _loadedMaterial = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasLoadedMaterial));
                OnPropertyChanged(nameof(HasPreviewImage));
            }
        }

        public bool PublishArchive {
            get => _publishArchive;
            set {
                _publishArchive = value;
                OnPropertyChanged();
            }
        }

        public bool PublishClean {
            get => _publishClean;
            set {
                _publishClean = value;
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
            _treeRoot = new ContentTreeNode(null);
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

        public void SelectFirstTexture()
        {
            SelectedSource = Textures.FirstOrDefault(x => TextureTags.Is(x.Tag, _selectedTag));
        }
    }

    internal class MainWindowDesignVM : MainWindowVM
    {
        public MainWindowDesignVM()
        {
            AddRecentItems();

            AddTreeItems();

            RootDirectory = "x:\\dev\\test-rp";
            IsBusy = true;


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

        private void AddRecentItems()
        {
            for (var i = 0; i < 8; i++)
                RecentDirectories.Add($"C:\\Some\\Fake\\Path-{i + 1}");
        }

        private void AddTreeItems()
        {
            SearchText = "as";
            ShowAllFiles = true;

            TreeRoot.Nodes.Add(new ContentTreeDirectory(null) {
                Name = "assets",
                Nodes = {
                    new ContentTreeDirectory(null) {
                        Name = "minecraft",
                        Nodes = {
                            new ContentTreeFile(null) {
                                Name = "Dirt",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconKind.Image,
                            },
                            new ContentTreeFile(null) {
                                Name = "Grass",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconKind.Image,
                            },
                            new ContentTreeFile(null) {
                                Name = "Glass",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconKind.Image,
                            },
                            new ContentTreeFile(null) {
                                Name = "Stone",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconKind.Image,
                            },
                        },
                    },
                },
            });
        }
    }
}
