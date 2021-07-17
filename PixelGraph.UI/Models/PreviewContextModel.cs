using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Preview.CubeMaps;
using SharpDX;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using Color = System.Windows.Media.Color;
using Material = HelixToolkit.Wpf.SharpDX.Material;
using MeshGeometry3D = HelixToolkit.SharpDX.Core.MeshGeometry3D;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace PixelGraph.UI.Models
{
    public class PreviewContextModel : ModelBase
    {
        private IEffectsManager _effectsManager;
        private ICubeMapSource _environmentCube;
        private ICubeMapSource _irradianceCube;
        private Material _modelMaterial;
        private MeshGeometry3D _model;
        private PerspectiveCamera _camera;
        private ImageSource _layerImage;
        private Stream _brdfLutMap;
        private string _selectedTag;
        private bool _enableRender;
        private bool _enableEnvironment;
        private bool _enableLights;
        private RenderPreviewModes _renderMode;
        private float _parallaxDepth;
        private int _parallaxSamplesMin;
        private int _parallaxSamplesMax;
        private bool _enableLinearSampling;
        private Camera _sunCamera;
        private Vector3 _sunDirection;
        private float _sunStrength;
        private int _timeOfDay;
        private int _wetness;
        private string _frameRateText;

        public event EventHandler EnableRenderChanged;
        public event EventHandler RenderModeChanged;
        public event EventHandler RenderSceneChanged;
        public event EventHandler SelectedTagChanged;

        public bool HasSelectedTag => _selectedTag != null;
        public Vector3D SunLightDirection => -_sunDirection.ToVector3D();
        public Color SunLightColor => new Color4(_sunStrength, _sunStrength, _sunStrength, _sunStrength).ToColor();

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

        public bool EnableLinearSampling {
            get => _enableLinearSampling;
            set {
                _enableLinearSampling = value;
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

        public float TimeOfDayLinear {
            get => GetLinearTimeOfDay();
            set {
                SetTimeOfDay(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeOfDay));
            }
        }

        public Camera SunCamera {
            get => _sunCamera;
            set {
                _sunCamera = value;
                OnPropertyChanged();
            }
        }

        public Vector3 SunDirection {
            get => _sunDirection;
            set {
                _sunDirection = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SunLightDirection));
            }
        }

        public float SunStrength {
            get => _sunStrength;
            set {
                _sunStrength = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SunLightColor));
            }
        }

        public int Wetness {
            get => _wetness;
            set {
                _wetness = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WetnessLinear));
            }
        }

        public float WetnessLinear {
            get => _wetness * 0.01f;
            set {
                _wetness = (int)(value * 100f + 0.5f);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Wetness));
            }
        }

        public ImageSource LayerImage {
            get => _layerImage;
            set {
                _layerImage = value;
                OnPropertyChanged();
            }
        }

        public RenderPreviewModes RenderMode {
            get => _renderMode;
            set {
                _renderMode = value;
                OnPropertyChanged();
                OnRenderModeChanged();
            }
        }

        public IEffectsManager EffectsManager {
            get => _effectsManager;
            set {
                _effectsManager = value;
                OnPropertyChanged();
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

        public bool EnableLights {
            get => _enableLights;
            set {
                _enableLights = value;
                OnPropertyChanged();
                OnRenderSceneChanged();
            }
        }

        public ICubeMapSource EnvironmentCube {
            get => _environmentCube;
            set {
                _environmentCube = value;
                OnPropertyChanged();
            }
        }

        public ICubeMapSource IrradianceCube {
            get => _irradianceCube;
            set {
                _irradianceCube = value;
                OnPropertyChanged();
            }
        }

        public Stream BrdfLutMap {
            get => _brdfLutMap;
            set {
                _brdfLutMap = value;
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

        public string FrameRateText {
            get => _frameRateText;
            set {
                _frameRateText = value;
                OnPropertyChanged();
            }
        }


        public PreviewContextModel()
        {
            _selectedTag = TextureTags.Color;
            _enableEnvironment = true;
            _timeOfDay = 6_000;
        }

        private float GetLinearTimeOfDay()
        {
            var t = _timeOfDay;
            MathEx.Wrap(ref t, 0, 24_000);
            return t / 24_000f;
        }

        private void SetTimeOfDay(float value)
        {
            TimeOfDay = (int)(value * 24_000f);
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
