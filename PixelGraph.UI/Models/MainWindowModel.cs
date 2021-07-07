using MahApps.Metro.IconPacks;
using Microsoft.Xaml.Behaviors.Core;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.Tabs;
using PixelGraph.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PixelGraph.UI.Models
{
    public class MainWindowModel : ModelBase, ISearchParameters
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
        //private ITabModel _selectedTab;
        private ITabModel _tabListSelection;
        private ITabModel _previewTab;
        private bool _isPreviewTabSelected;

        public event EventHandler SelectedTabChanged;
        public event EventHandler<TabClosedEventArgs> TabClosed;

        //public MaterialContextModel Material {get;}
        public ProfileContextModel Profile {get;}
        public PreviewContextModel Preview {get;}
        //public ObservableCollection<ITabModel> PreviewTabList {get;}
        public ObservableCollection<ITabModel> TabList {get;}
        public ICommand TabCloseButtonCommand {get;}

        public bool IsInitializing => _isInitializing;
        public bool IsProjectLoaded => _rootDirectory != null;

        public bool HasTreeSelection => _selectedNode is ContentTreeFile;
        public bool HasTreeMaterialSelection => _selectedNode is ContentTreeMaterialDirectory;
        public bool HasTreeTextureSelection => _selectedNode is ContentTreeFile {Type: ContentNodeType.Texture};

        public MaterialProperties SelectedTabMaterial => (SelectedTab as MaterialTabModel)?.Material;
        public ITabModel SelectedTab => _isPreviewTabSelected ? _previewTab : _tabListSelection;
        public bool HasSelectedMaterial => SelectedTab is MaterialTabModel;
        public bool HasSelectedTab => IsPreviewTabSelected || TabListSelection != null;
        public bool HasPreviewTab => PreviewTab != null;

        public ITabModel PreviewTab {
            get => _previewTab;
            set {
                if (_previewTab == value) return;
                _previewTab = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPreviewTab));

                if (_isPreviewTabSelected) {
                    OnPropertyChanged(nameof(SelectedTab));
                    OnPropertyChanged(nameof(SelectedTabMaterial));
                    OnPropertyChanged(nameof(HasSelectedMaterial));
                    OnSelectedTabChanged();
                }
            }
        }

        public bool IsPreviewTabSelected {
            get => _isPreviewTabSelected;
            set {
                if (_isPreviewTabSelected == value) return;
                _isPreviewTabSelected = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedTab));

                if (_isPreviewTabSelected) {
                    OnPropertyChanged(nameof(SelectedTab));
                    OnPropertyChanged(nameof(SelectedTabMaterial));
                    OnPropertyChanged(nameof(HasSelectedMaterial));
                }
            }
        }
        
        public ITabModel TabListSelection {
            get => _tabListSelection;
            set {
                if (_tabListSelection == value) return;
                _tabListSelection = value;

                if (value != null && _isPreviewTabSelected)
                    _isPreviewTabSelected = false;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedMaterial));
                OnPropertyChanged(nameof(IsPreviewTabSelected));
                OnPropertyChanged(nameof(HasSelectedTab));

                if (!_isPreviewTabSelected) {
                    OnPropertyChanged(nameof(SelectedTab));
                    OnPropertyChanged(nameof(SelectedTabMaterial));
                    OnPropertyChanged(nameof(HasSelectedMaterial));
                }

                OnSelectedTabChanged();
            }
        }
        
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


        public MainWindowModel()
        {
            RecentDirectories = new ObservableCollection<string>();
            _publishLocations = new List<LocationModel>();
            _treeRoot = new ContentTreeNode(null);
            busyLock = new object();

            Profile = new ProfileContextModel();
            Preview = new PreviewContextModel();

            TabList = new ObservableCollection<ITabModel>();
            TabCloseButtonCommand = new ActionCommand(OnTabCloseButtonClicked);

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
        }

        private void OnTabCloseButtonClicked(object parameter)
        {
            if (parameter is Guid tabId) OnTabClosed(tabId);
        }

        private void OnSelectedTabChanged()
        {
            SelectedTabChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnTabClosed(Guid tabId)
        {
            var e = new TabClosedEventArgs(tabId);
            TabClosed?.Invoke(this, e);
        }
    }

    internal class MainDesignerModel : MainWindowModel
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

            PreviewTab = new MaterialTabModel {
                DisplayName = "Bricks",
                IsPreview = true,
            };
        }

        private void AddRecentItems()
        {
            for (var i = 0; i < 8; i++)
                RecentDirectories.Add($"C:\\Some\\Fake\\Path-{i + 1}");
        }

        private void AddTreeItems()
        {
            //Material.Loaded = new MaterialProperties();
            IsPreviewTabSelected = true;
            PreviewTab = new MaterialTabModel {
                DisplayName = "Test Material",
            };

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

    public class TabClosedEventArgs : EventArgs
    {
        public Guid TabId {get;}


        public TabClosedEventArgs(Guid tabId)
        {
            TabId = tabId;
        }
    }
}
