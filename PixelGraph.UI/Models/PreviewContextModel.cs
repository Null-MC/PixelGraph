using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Preview.Sky;
using SharpDX;
using System;
using System.Windows.Media;
using PixelGraph.Common.Extensions;
using Color = System.Windows.Media.Color;

namespace PixelGraph.UI.Models
{
    public class PreviewContextModel : ModelBase
    {
        private IEffectsManager _effectsManager;
        private IEnvironmentCube _environmentCube;
        private Material _modelMaterial;
        private MeshGeometry3D _model;
        private PerspectiveCamera _camera;
        private OrthographicCamera _sunCamera;
        private ImageSource _layerImage;
        private string _selectedTag;
        private bool _enableRender;
        private bool _enableEnvironment;
        private RenderPreviewModes _renderMode;
        private float _parallaxDepth;
        private int _parallaxSamplesMin;
        private int _parallaxSamplesMax;
        private Vector3 _sunDirection;
        private float _sunStrength;
        private Color _sunColor;
        private int _timeOfDay;
        private float _wetness;
        private bool _isLoading;

        public event EventHandler EnableRenderChanged;
        public event EventHandler RenderModeChanged;
        public event EventHandler RenderSceneChanged;
        public event EventHandler SelectedTagChanged;

        public bool HasSelectedTag => _selectedTag != null;
        public float TimeOfDayLinear => GetLinearTimeOfDay();

        public float ParallaxDepth {
            get => _parallaxDepth;
            set {
                _parallaxDepth = value;
                OnPropertyChanged();
            }
        }

        public int ParallaxSamplesMin {
            get => _parallaxSamplesMin;
            set {
                _parallaxSamplesMin = value;
                OnPropertyChanged();
            }
        }

        public int ParallaxSamplesMax {
            get => _parallaxSamplesMax;
            set {
                _parallaxSamplesMax = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading {
            get => _isLoading;
            set {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

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

        public int TimeOfDay {
            get => _timeOfDay;
            set {
                _timeOfDay = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeOfDayLinear));
            }
        }

        //private IMinecraftScene _minecraftScene;
        //public IMinecraftScene MinecraftScene {
        //    get => _minecraftScene;
        //    set {
        //        _minecraftScene = value;
        //        OnPropertyChanged();
        //    }
        //}

        public Vector3 SunDirection {
            get => _sunDirection;
            set {
                _sunDirection = value;
                OnPropertyChanged();
            }
        }

        public float SunStrength {
            get => _sunStrength;
            set {
                _sunStrength = value;
                OnPropertyChanged();
            }
        }

        public Color SunColor {
            get => _sunColor;
            set {
                _sunColor = value;
                OnPropertyChanged();
            }
        }

        public float Wetness {
            get => _wetness;
            set {
                _wetness = value;
                OnPropertyChanged();
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

        public IEnvironmentCube EnvironmentCube {
            get => _environmentCube;
            set {
                _environmentCube = value;
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

        public OrthographicCamera SunCamera {
            get => _sunCamera;
            set {
                _sunCamera = value;
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

        public RenderPreviewModes RenderMode {
            get => _renderMode;
            set {
                _renderMode = value;
                OnPropertyChanged();
                OnRenderModeChanged();
                //OnRenderSceneChanged();
            }
        }

        public bool EnableEnvironment {
            get => _enableEnvironment;
            set {
                _enableEnvironment = value;
                OnPropertyChanged();
                OnRenderSceneChanged();
            }
        }


        public PreviewContextModel()
        {
            _selectedTag = TextureTags.Albedo;

            TimeOfDay = 6_000;
        }

        private float GetLinearTimeOfDay()
        {
            var t = _timeOfDay;
            MathEx.Wrap(ref t, 0, 24_000);
            return t / 24_000f;
        }

        private void OnEnableRenderChanged()
        {
            EnableRenderChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnRenderModeChanged()
        {
            RenderModeChanged?.Invoke(this, EventArgs.Empty);
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
