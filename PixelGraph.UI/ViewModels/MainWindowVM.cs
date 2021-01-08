using MaterialDesignThemes.Wpf;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowVM : ViewModelBase, ISearchParameters
    {
        #region Properties

        private readonly object busyLock;
        private volatile bool _isBusy, _isPreviewLoading;
        private ObservableCollection<string> _recentDirectories;
        private ResourcePackInputProperties _packInput;
        private string _rootDirectory;
        private string _searchText;
        private bool _showAllFiles;
        private string _selectedTag;
        private ContentTreeNode _selectedNode;
        private MaterialProperties _loadedMaterial;
        private ImageSource _loadedTexture;
        private ProfileItem _selectedProfile;
        private string _loadedMaterialFilename;
        private bool _publishArchive, _publishClean;
        private ContentTreeNode _treeRoot;

        public event EventHandler SelectedTagChanged;

        public ObservableCollection<ProfileItem> Profiles {get;}

        public bool HasRootDirectory => _rootDirectory != null;
        public bool HasTreeSelection => _selectedNode is ContentTreeFile;
        public bool HasTreeTextureSelection => _selectedNode is ContentTreeFile _file && _file.Type == ContentNodeType.Texture;
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

                OnSelectedTagChanged();
            }
        }

        public ContentTreeNode SelectedNode {
            get => _selectedNode;
            set {
                _selectedNode = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasTreeSelection));
                OnPropertyChanged(nameof(HasTreeTextureSelection));
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

        public bool IsPreviewLoading {
            get => _isPreviewLoading;
            set {
                _isPreviewLoading = value;
                OnPropertyChanged();
            }
        }

        #endregion


        public MainWindowVM()
        {
            RecentDirectories = new ObservableCollection<string>();
            Profiles = new ObservableCollection<ProfileItem>();
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

        public void CloseProject()
        {
            SelectedNode = null;
            LoadedTexture = null;
            LoadedMaterial = null;
            PackInput = null;
            TreeRoot = null;
            RootDirectory = null;

            Profiles.Clear();
        }

        private void OnSelectedTagChanged()
        {
            SelectedTagChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class MainWindowDesignVM : MainWindowVM
    {
        public MainWindowDesignVM()
        {
            AddRecentItems();

            AddTreeItems();

            //RootDirectory = "x:\\dev\\test-rp";
            //IsBusy = true;
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
            IsPreviewLoading = true;

            LoadedMaterial = new MaterialProperties();
            LoadedTexture = new BitmapImage();

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
