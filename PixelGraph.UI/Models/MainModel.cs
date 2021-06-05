using MahApps.Metro.IconPacks;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.Models
{
    public class MainModel : ModelBase, ISearchParameters
    {
        #region Properties

        private readonly object busyLock;
        private volatile bool _isBusy, _isInitializing;
        //private volatile bool _isPreviewLoading;
        private volatile bool _isImageEditorOpen;
        private ObservableCollection<string> _recentDirectories;
        private List<LocationModel> _publishLocations;
        private ResourcePackInputProperties _packInput;
        private string _rootDirectory;
        private string _searchText;
        private bool _showAllFiles;
        //private string _selectedTag;
        private ContentTreeNode _selectedNode;
        //private MaterialProperties _loadedMaterial;
        //private ImageSource _loadedTexture;
        //private ProfileItem _selectedProfile;
        private LocationModel _selectedLocation;
        //private bool _publishArchive, _publishClean;
        private ContentTreeNode _treeRoot;
        //private PreviewContextModel _preview;

        //public event EventHandler SelectedTagChanged;
        //public event EventHandler SelectedProfileChanged;

        public MaterialContextModel Material {get;}
        public ProfileContextModel Profile {get;}
        public PreviewContextModel Preview {get;}

        //public ObservableCollection<ProfileItem> PublishProfiles {get;}
        //public Preview3DVM Preview3D {get;}

        public bool IsInitializing => _isInitializing;
        public bool IsProjectLoaded => _rootDirectory != null;
        //public bool IsProjectUnloaded => _rootDirectory == null;

        public bool HasTreeSelection => _selectedNode is ContentTreeFile;
        public bool HasTreeMaterialSelection => _selectedNode is ContentTreeMaterialDirectory;
        public bool HasTreeTextureSelection => _selectedNode is ContentTreeFile {Type: ContentNodeType.Texture};
        public bool HasTreeMaterialOrTextureSelection => HasTreeMaterialSelection || HasTreeTextureSelection;

        //public bool HasLoadedMaterial => _loadedMaterial != null;
        //public bool HasLoadedTexture => _loadedTexture != null;
        //public bool HasPreviewImage => HasLoadedTexture || (HasLoadedMaterial && _selectedTag != null);
        //public bool HasTagSelection => !string.IsNullOrEmpty(_selectedTag);
        //public bool HasProfileSelection => _selectedProfile != null;

        //public PreviewContextModel Preview {
        //    get => _preview;
        //    set {
        //        _preview = value;
        //        OnPropertyChanged();
        //    }
        //}

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsProjectLoaded));
                //OnPropertyChanged(nameof(IsProjectUnloaded));
            }
        }
        
        public ObservableCollection<string> RecentDirectories {
            get => _recentDirectories;
            set {
                _recentDirectories = value;
                OnPropertyChanged();
            }
        }

        public List<LocationModel> PublishLocations {
            get => _publishLocations;
            set {
                _publishLocations = value;
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

        //public string SelectedTag {
        //    get => _selectedTag;
        //    set {
        //        _selectedTag = value;
        //        OnPropertyChanged();

        //        OnSelectedTagChanged();
        //        OnPropertyChanged(nameof(HasTagSelection));
        //        //OnPropertyChanged(nameof(HasPreviewImage));
        //    }
        //}

        public ContentTreeNode SelectedNode {
            get => _selectedNode;
            set {
                _selectedNode = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasTreeSelection));
                NotifyTreeSelectionChanged();
            }
        }

        //public ProfileItem SelectedProfile {
        //    get => _selectedProfile;
        //    set {
        //        _selectedProfile = value;
        //        OnPropertyChanged();

        //        OnSelectedProfileChanged();
        //        OnPropertyChanged(nameof(HasProfileSelection));
        //    }
        //}

        public LocationModel SelectedLocation {
            get => _selectedLocation;
            set {
                _selectedLocation = value;
                OnPropertyChanged();
            }
        }

        //public string LoadedMaterialFilename {
        //    get => _loadedMaterialFilename;
        //    set {
        //        _loadedMaterialFilename = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public ImageSource LoadedTexture {
        //    get => _loadedTexture;
        //    set {
        //        _loadedTexture = value;
        //        OnPropertyChanged();

        //        OnPropertyChanged(nameof(HasLoadedTexture));
        //        OnPropertyChanged(nameof(HasPreviewImage));
        //    }
        //}

        //public MaterialProperties LoadedMaterial {
        //    get => _loadedMaterial;
        //    set {
        //        _loadedMaterial = value;
        //        OnPropertyChanged();

        //        OnPropertyChanged(nameof(HasLoadedMaterial));
        //        //OnPropertyChanged(nameof(HasPreviewImage));
        //    }
        //}

        //public bool PublishArchive {
        //    get => _publishArchive;
        //    set {
        //        _publishArchive = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public bool PublishClean {
        //    get => _publishClean;
        //    set {
        //        _publishClean = value;
        //        OnPropertyChanged();
        //    }
        //}

        public bool IsBusy {
            get => _isBusy;
            set {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        //public bool IsPreviewLoading {
        //    get => _isPreviewLoading;
        //    set {
        //        _isPreviewLoading = value;
        //        OnPropertyChanged();
        //    }
        //}

        public bool IsImageEditorOpen {
            get => _isImageEditorOpen;
            set {
                _isImageEditorOpen = value;
                OnPropertyChanged();
            }
        }

        #endregion


        public MainModel()
        {
            RecentDirectories = new ObservableCollection<string>();
            //PublishProfiles = new ObservableCollection<ProfileItem>();
            _publishLocations = new List<LocationModel>();
            _treeRoot = new ContentTreeNode(null);
            busyLock = new object();

            Material = new MaterialContextModel();
            Profile = new ProfileContextModel();
            Preview = new PreviewContextModel();

            _isInitializing = true;
            //_selectedTag = TextureTags.Albedo;

            //Preview3D = new Preview3DVM();
        }

        //public void Dispose()
        //{
        //    Preview3D?.Dispose();
        //}

        public void EndInit()
        {
            _isInitializing = false;
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

        private void NotifyTreeSelectionChanged()
        {
            OnPropertyChanged(nameof(HasTreeMaterialSelection));
            OnPropertyChanged(nameof(HasTreeTextureSelection));
            OnPropertyChanged(nameof(HasTreeMaterialOrTextureSelection));
        }

        //public void CloseProject()
        //{
        //    SelectedNode = null;
        //    //LoadedTexture = null;
        //    LoadedMaterial = null;
        //    PackInput = null;
        //    TreeRoot = null;
        //    RootDirectory = null;

        //    PublishProfiles.Clear();
        //}

        //public void SetSelectedLocation(LocationViewModel location)
        //{
        //    _selectedLocation = location;
        //    OnPropertyChanged(nameof(SelectedLocation));
        //}

        //private void OnSelectedTagChanged()
        //{
        //    SelectedTagChanged?.Invoke(this, EventArgs.Empty);
        //}

        //private void OnSelectedProfileChanged()
        //{
        //    SelectedProfileChanged?.Invoke(this, EventArgs.Empty);
        //}
    }

    internal class MainDesignerModel : MainModel
    {
        public MainDesignerModel()
        {
            AddRecentItems();

            AddTreeItems();

            RootDirectory = "x:\\dev\\test-rp";
            //IsBusy = true;
            SearchText = "as";
            ShowAllFiles = true;
            //IsPreviewLoading = true;

            //SelectedNode = new ContentTreeFile(null) {
            //    Type = ContentNodeType.Material,
            //};

            //Materials.Loaded = new MaterialProperties();
        }

        private void AddRecentItems()
        {
            for (var i = 0; i < 8; i++)
                RecentDirectories.Add($"C:\\Some\\Fake\\Path-{i + 1}");
        }

        private void AddTreeItems()
        {
            Material.Loaded = new MaterialProperties();
            //LoadedTexture = new BitmapImage();

            TreeRoot.Nodes.Add(new ContentTreeDirectory(null) {
                Name = "assets",
                Nodes = {
                    new ContentTreeDirectory(null) {
                        Name = "minecraft",
                        Nodes = {
                            new ContentTreeFile(null) {
                                Name = "Dirt",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconFontAwesomeKind.ImageSolid,
                            },
                            new ContentTreeFile(null) {
                                Name = "Grass",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconFontAwesomeKind.ImageSolid,
                            },
                            new ContentTreeFile(null) {
                                Name = "Glass",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconFontAwesomeKind.ImageSolid,
                            },
                            new ContentTreeFile(null) {
                                Name = "Stone",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconFontAwesomeKind.ImageSolid,
                            },
                        },
                    },
                },
            });
        }
    }
}
