﻿using MahApps.Metro.IconPacks;
using Microsoft.Xaml.Behaviors.Core;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.Tabs;
using PixelGraph.UI.ViewData;
using PixelGraph.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

#if NORENDER
using PixelGraph.UI.Models.MockScene;
#else
using PixelGraph.UI.Models.Scene;
#endif

namespace PixelGraph.UI.Models
{
    internal class MainWindowModel : ModelBase, ISearchParameters
    {
        #region Properties

        private readonly object busyLock;
        private volatile bool _isBusy, _isInitializing;
        private volatile bool _isImageEditorOpen;
        private ObservableCollection<string> _recentDirectories;
        private ObservableCollection<ResourcePackProfileProperties> _profileList;
        private List<LocationDisplayModel> _publishLocations;
        private ResourcePackProfileProperties _selectedProfile;
        private string _projectFilename;
        private string _searchText;
        private bool _showAllFiles;
        private ContentTreeNode _selectedNode;
        private LocationDisplayModel _selectedLocation;
        private ContentTreeNode _treeRoot;
        private ITabModel _tabListSelection;
        private ITabModel _previewTab;
        private bool _isPreviewTabSelected;
        private ViewModes _viewMode;
        private EditModes _editMode;
        private string _selectedTag;

        //public event EventHandler SelectedProfileChanged;
        public event EventHandler SelectedTabChanged;
        public event EventHandler SelectedTagChanged;
        public event EventHandler<TabClosedEventArgs> TabClosed;
        public event EventHandler ViewModeChanged;

#if NORENDER
        public MockScenePropertiesModel SceneProperties {get;}
        public MockRenderPropertiesModel RenderProperties {get;}
#else
        public ScenePropertiesModel SceneProperties {get;}
        public RenderPropertiesModel RenderProperties {get;}
#endif

        //public MaterialConnectionsModel ConnectionsModel {get; set;}

        public ObservableCollection<ITabModel> TabList {get;}
        public ICommand TabCloseButtonCommand {get;}
        public bool SupportsRender {get;}

        public bool IsInitializing => _isInitializing;
        public bool IsProjectLoaded => _projectFilename != null;

        public bool HasProfileSelected => _selectedProfile != null;

        public bool HasTreeSelection => _selectedNode is ContentTreeFile;
        public bool HasTreeMaterialSelection => _selectedNode is ContentTreeMaterialDirectory;
        public bool HasTreeTextureSelection => _selectedNode is ContentTreeFile {Type: ContentNodeType.Texture};
        public bool HasSelectedTag => _selectedTag != null;

        public MaterialProperties SelectedTabMaterial => (SelectedTab as MaterialTabModel)?.MaterialRegistration?.Value;
        public ITabModel SelectedTab => _isPreviewTabSelected ? _previewTab : _tabListSelection;
        public bool HasSelectedMaterial => SelectedTab is MaterialTabModel;
        public bool HasSelectedTab => IsPreviewTabSelected || TabListSelection != null;
        public bool HasPreviewTab => PreviewTab != null;

        public ObservableCollection<ResourcePackProfileProperties> ProfileList {
            get => _profileList;
            set {
                if (_profileList == value) return;
                _profileList = value;
                OnPropertyChanged();
            }
        }

        public ResourcePackProfileProperties SelectedProfile {
            get => _selectedProfile;
            set {
                if (value == _selectedProfile) return;
                _selectedProfile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasProfileSelected));
                //OnSelectedProfileChanged();
            }
        }

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

        public string SelectedTag {
            get => _selectedTag;
            set {
                if (value == _selectedTag) return;

                _selectedTag = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasSelectedTag));
                OnSelectedTagChanged();
            }
        }

        public bool IsViewModeLayer {
            get => _viewMode == ViewModes.Layer;
            set {
                if (!value) return;
                _viewMode = ViewModes.Layer;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsViewModeRender));
                OnViewModeChanged();
            }
        }

        public bool IsViewModeRender {
            get => _viewMode == ViewModes.Render;
            set {
                if (!value) return;
                _viewMode = ViewModes.Render;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsViewModeLayer));
                OnViewModeChanged();
            }
        }

        public bool IsEditModeMaterial {
            get => _editMode == EditModes.Material;
            set {
                if (!value) return;
                _editMode = EditModes.Material;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditModeFilters));
                OnPropertyChanged(nameof(IsEditModeConnections));
                OnPropertyChanged(nameof(IsEditModeScene));
                //OnEditModeChanged();
            }
        }

        public bool IsEditModeFilters {
            get => _editMode == EditModes.Filters;
            set {
                if (!value) return;
                _editMode = EditModes.Filters;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditModeMaterial));
                OnPropertyChanged(nameof(IsEditModeConnections));
                OnPropertyChanged(nameof(IsEditModeScene));
                //OnEditModeChanged();
            }
        }

        public bool IsEditModeConnections {
            get => _editMode == EditModes.Connections;
            set {
                if (!value) return;
                _editMode = EditModes.Connections;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditModeMaterial));
                OnPropertyChanged(nameof(IsEditModeFilters));
                OnPropertyChanged(nameof(IsEditModeScene));
                //OnEditModeChanged();
            }
        }

        public bool IsEditModeScene {
            get => _editMode == EditModes.Scene;
            set {
                if (!value) return;
                _editMode = EditModes.Scene;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditModeMaterial));
                OnPropertyChanged(nameof(IsEditModeFilters));
                OnPropertyChanged(nameof(IsEditModeConnections));
                //OnEditModeChanged();
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
        
        public string ProjectFilename {
            get => _projectFilename;
            set {
                _projectFilename = value;
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

        public List<LocationDisplayModel> PublishLocations {
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

        //public ResourcePackInputProperties PackInput {
        //    get => _packInput;
        //    set {
        //        _packInput = value;
        //        OnPropertyChanged();
        //    }
        //}

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

        public LocationDisplayModel SelectedLocation {
            get => _selectedLocation;
            set {
                _selectedLocation = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy {
            get => _isBusy || _isInitializing;
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
            _publishLocations = new List<LocationDisplayModel>();
            _treeRoot = new ContentTreeNode(null);
            busyLock = new object();

            //Profile = new ProfileContextModel();

            TabList = new ObservableCollection<ITabModel>();
            TabCloseButtonCommand = new ActionCommand(OnTabCloseButtonClicked);

            _isInitializing = true;
            _selectedTag = TextureTags.Color;

#if !NORENDER
            SupportsRender = true;
            SceneProperties = new ScenePropertiesModel();
            RenderProperties = new RenderPropertiesModel();
#endif
        }

        public void EndInit()
        {
            _isInitializing = false;
            OnPropertyChanged(nameof(IsInitializing));
            OnPropertyChanged(nameof(IsBusy));
        }

        public bool TryStartBusy()
        {
            lock (busyLock) {
                if (_isBusy) return false;
                _isBusy = true;
            }

            OnPropertyChanged(nameof(IsBusy));
            return true;
        }

        public void EndBusy()
        {
            lock (busyLock) {
                _isBusy = false;
            }

            OnPropertyChanged(nameof(IsBusy));
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

        //private void OnSelectedProfileChanged()
        //{
        //    SelectedProfileChanged?.Invoke(this, EventArgs.Empty);
        //}

        private void OnSelectedTabChanged()
        {
            SelectedTabChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedTagChanged()
        {
            SelectedTagChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnTabClosed(Guid tabId)
        {
            var e = new TabClosedEventArgs(tabId);
            TabClosed?.Invoke(this, e);
        }

        private void OnViewModeChanged()
        {
            ViewModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class MainDesignerModel : MainWindowModel
    {
        public MainDesignerModel()
        {
            AddRecentItems();

            AddTreeItems();

            ProjectFilename = "x:\\dev\\test-rp\\project.yml";
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
