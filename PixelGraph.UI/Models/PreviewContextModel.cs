using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using System;
using System.Windows.Media;

namespace PixelGraph.UI.Models
{
    public class PreviewContextModel : ModelBase
    {
        private IEffectsManager _effectsManager;
        private Material _modelMaterial;
        private MeshGeometry3D _model;
        private PerspectiveCamera _camera, _sunCamera;
        private TextureModel _skyTexture;
        //private BillboardText3D _renderLoadingText;
        private ImageSource _layerImage;
        //private Vector3 _sunDirection;
        private string _selectedTag;
        private bool _enableRender;
        private bool _enableEnvironment;
        //private bool _hasContent;
        private bool _isLoading;

        public event EventHandler EnableRenderChanged;
        public event EventHandler RenderSceneChanged;
        public event EventHandler SelectedTagChanged;

        public bool HasSelectedTag => _selectedTag != null;

        public bool IsLoading {
            get => _isLoading;
            set {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        //public bool HasContent {
        //    get => _hasContent;
        //    set {
        //        _hasContent = value;
        //        OnPropertyChanged();
        //    }
        //}

        public bool EnableRender {
            get => _enableRender;
            set {
                if (value == _enableRender) return;
                _enableRender = value;
                OnPropertyChanged();
                OnEnableRenderChanged();
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

        public ImageSource LayerImage {
            get => _layerImage;
            set {
                _layerImage = value;
                OnPropertyChanged();
            }
        }

        public IEffectsManager EffectsManager {
            get => _effectsManager;
            set {
                _effectsManager = value;
                OnPropertyChanged();
            }
        }

        public PerspectiveCamera Camera {
            get => _camera;
            set {
                _camera = value;
                OnPropertyChanged();
            }
        }

        public PerspectiveCamera SunCamera {
            get => _sunCamera;
            set {
                _sunCamera = value;
                OnPropertyChanged();
            }
        }

        public TextureModel SkyTexture {
            get => _skyTexture;
            set {
                _skyTexture = value;
                OnPropertyChanged();
            }
        }

        public MeshGeometry3D Model {
            get => _model;
            set {
                _model = value;
                OnPropertyChanged();
            }
        }

        public Material ModelMaterial {
            get => _modelMaterial;
            set {
                _modelMaterial = value;
                OnPropertyChanged();
            }
        }

        //public BillboardText3D RenderLoadingText {
        //    get => _renderLoadingText;
        //    set {
        //        _renderLoadingText = value;
        //        OnPropertyChanged();
        //    }
        //}

        public bool EnableEnvironment {
            get => _enableEnvironment;
            set {
                _enableEnvironment = value;
                OnPropertyChanged();
                OnRenderSceneChanged();
            }
        }

        //public Vector3 SunDirection {
        //    get => _sunDirection;
        //    set {
        //        _sunDirection = value;
        //        OnPropertyChanged();
        //    }
        //}


        public PreviewContextModel()
        {
            _selectedTag = TextureTags.Albedo;

            //_sunDirection = new Vector3(-1, -4, 2);
            //_sunDirection.Normalize();
        }

        private void OnEnableRenderChanged()
        {
            EnableRenderChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedTagChanged()
        {
            SelectedTagChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnRenderSceneChanged()
        {
            RenderSceneChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
