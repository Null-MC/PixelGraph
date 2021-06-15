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
        private volatile bool _isImageEditorOpen;
        private ObservableCollection<string> _recentDirectories;
        private List<LocationModel> _publishLocations;
        private ResourcePackInputProperties _packInput;
        private string _rootDirectory;
        private string _searchText;
        private bool _showAllFiles;
        private ContentTreeNode _selectedNode;
        private LocationModel _selectedLocation;
        private ContentTreeNode _treeRoot;

        public MaterialContextModel Material {get;}
        public ProfileContextModel Profile {get;}
        public PreviewContextModel Preview {get;}

        public bool IsInitializing => _isInitializing;
        public bool IsProjectLoaded => _rootDirectory != null;

        public bool HasTreeSelection => _selectedNode is ContentTreeFile;
        public bool HasTreeMaterialSelection => _selectedNode is ContentTreeMaterialDirectory;
        public bool HasTreeTextureSelection => _selectedNode is ContentTreeFile {Type: ContentNodeType.Texture};
        public bool HasTreeMaterialOrTextureSelection => HasTreeMaterialSelection || HasTreeTextureSelection;

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsProjectLoaded));
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

        public ContentTreeNode SelectedNode {
            get => _selectedNode;
            set {
                _selectedNode = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasTreeSelection));
                NotifyTreeSelectionChanged();
            }
        }

        public LocationModel SelectedLocation {
            get => _selectedLocation;
            set {
                _selectedLocation = value;
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
            _publishLocations = new List<LocationModel>();
            _treeRoot = new ContentTreeNode(null);
            busyLock = new object();

            Material = new MaterialContextModel();
            Profile = new ProfileContextModel();
            Preview = new PreviewContextModel();

            _isInitializing = true;
        }

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
