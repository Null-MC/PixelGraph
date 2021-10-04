using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Preview;
using SharpDX;
using System;
using System.IO;
using PixelGraph.UI.Helix.CubeMaps;
using Media = System.Windows.Media;

namespace PixelGraph.UI.Models
{
    public class RenderPreviewModel : ModelBase
    {
        private IEffectsManager _effectsManager;
        private ICubeMapSource _environmentCube;
        private ICubeMapSource _irradianceCube;
        private Material _modelMaterial;
        private MeshGeometry3D _cubeModel;
        private PerspectiveCamera _camera;
        //private ImageSource _layerImage;
        private Stream _brdfLutMap;
        //private string _selectedTag;
        private bool _enableEnvironment;
        private bool _enableLights;
        private RenderPreviewModes _renderMode;
        private float _parallaxDepth;
        private int _parallaxSamplesMin;
        private int _parallaxSamplesMax;
        private bool _enableLinearSampling;
        private bool _enableSlopeNormals;
        private bool _enablePuddles;
        private Camera _sunCamera;
        private Vector3 _sunDirection;
        private float _sunStrength;
        private int _timeOfDay;
        private int _wetness;
        //private string _frameRateText;
        private bool _isLoaded;

        public event EventHandler RenderModeChanged;
        public event EventHandler RenderSceneChanged;
        //public event EventHandler SelectedTagChanged;

        //public bool HasSelectedTag => _selectedTag != null;
        public Media.Media3D.Vector3D SunLightDirection => -_sunDirection.ToVector3D();
        public Media.Color SunLightColor => new Color4(_sunStrength, _sunStrength, _sunStrength, _sunStrength).ToColor();

        public bool IsLoaded {
            get => _isLoaded;
            set {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }

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

        public bool EnableSlopeNormals {
            get => _enableSlopeNormals;
            set {
                _enableSlopeNormals = value;
                OnPropertyChanged();
            }
        }

        public bool EnablePuddles {
            get => _enablePuddles;
            set {
                _enablePuddles = value;
                OnPropertyChanged();
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

        public MeshGeometry3D CubeModel {
            get => _cubeModel;
            set {
                _cubeModel = value;
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


        public RenderPreviewModel()
        {
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

        private void OnRenderModeChanged()
        {
            RenderModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnRenderSceneChanged()
        {
            RenderSceneChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
