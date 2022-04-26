using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewData;
using System;
using System.IO;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Media = System.Windows.Media;

#if !RELEASENORENDER
using PixelGraph.Common.Extensions;
using PixelGraph.Rendering;
using SharpDX;
#endif

namespace PixelGraph.UI.Models.Scene
{
    public class ScenePropertiesModel : ModelBase
    {
        public event EventHandler EnvironmentChanged;
        public event EventHandler DynamicSkyChanged;

        private readonly Rotation3DAnimation spinAnimation;
        private Media.Color _ambientColor;
        private Media.Color _lightColor;
        private int _wetness;
        private bool _enableAtmosphere;
        private int _timeOfDay;
        private int _sunTilt;
        private int _sunAzimuth;
        private Vector3 _sunDirection;
        private float _sunStrength;
        private bool _enableLights;
        private TextureModel _equirectangularMap;
        private Transform3D _meshTransform;
        private string _erpFilename, _erpName;
        private float _erpIntensity;
        private bool _spinMesh;
        private PomTypeValues.Item _pomType;
        //private bool _enableSlopeNormals;
        //private bool _enableLinearSampling;
        //private Transform3D _lightTransform1;
        //private Transform3D _lightTransform2;

        public Vector3D SunLightDirection => -_sunDirection.ToVector3D();
        public Media.Color SunLightColor => new Color4(_sunStrength, _sunStrength, _sunStrength, _sunStrength).ToColor();
        public bool HasEquirectangularMap => _equirectangularMap != null;
        public bool HasEnvironmentMap => _enableAtmosphere || HasEquirectangularMap;

        public Media.Color AmbientColor {
            get => _ambientColor;
            set {
                _ambientColor = value;
                OnPropertyChanged();
            }
        }

        public Media.Color LightColor {
            get => _lightColor;
            set {
                _lightColor = value;
                OnPropertyChanged();
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

        public bool SpinMesh {
            get => _spinMesh;
            set {
                if (_spinMesh == value) return;
                _spinMesh = value;
                OnPropertyChanged();
                UpdateSpinAnimation();
            }
        }

        public PomTypeValues.Item PomType {
            get => _pomType;
            set {
                if (_pomType == value) return;
                _pomType = value;
                OnPropertyChanged();

                //EnableLinearSampling = _pomType?.EnableLinearSampling ?? false;
                //EnableSlopeNormals = _pomType?.EnableSlopeNormals ?? false;
                OnEnvironmentChanged();
            }
        }

        //public bool EnableSlopeNormals {
        //    get => _enableSlopeNormals;
        //    set {
        //        if (_enableSlopeNormals == value) return;
        //        _enableSlopeNormals = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public bool EnableLinearSampling {
        //    get => _enableLinearSampling;
        //    set {
        //        if (_enableLinearSampling == value) return;
        //        _enableLinearSampling = value;
        //        OnPropertyChanged();
        //    }
        //}

        public float WetnessLinear {
            get => _wetness * 0.01f;
            set {
                _wetness = (int)(value * 100f + 0.5f);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Wetness));
            }
        }

        public bool EnableAtmosphere {
            get => _enableAtmosphere;
            set {
                _enableAtmosphere = value;
                OnPropertyChanged();
                OnEnvironmentChanged();
                OnPropertyChanged(nameof(HasEnvironmentMap));
            }
        }

        public int TimeOfDay {
            get => _timeOfDay;
            set {
                _timeOfDay = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeOfDayLinear));
                OnDynamicSkyChanged();
            }
        }

        public float TimeOfDayLinear {
            get => GetLinearTimeOfDay();
            set {
                SetTimeOfDay(value);
                OnPropertyChanged();
            }
        }

        public int SunTilt {
            get => _sunTilt;
            set {
                _sunTilt = value;
                OnPropertyChanged();
                OnDynamicSkyChanged();
            }
        }

        public int SunAzimuth {
            get => _sunAzimuth;
            set {
                _sunAzimuth = value;
                OnPropertyChanged();
                OnDynamicSkyChanged();
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

        public bool EnableLights {
            get => _enableLights;
            set {
                _enableLights = value;
                OnPropertyChanged();
                OnDynamicSkyChanged();
            }
        }

        public string ErpName {
            get => _erpName;
            private set {
                if (_erpName == value) return;
                _erpName = value;
                OnPropertyChanged();
            }
        }

        public string ErpFilename {
            get => _erpFilename;
            set {
                if (_erpFilename == value) return;
                _erpFilename = value;
                OnPropertyChanged();

                ErpName = Path.GetFileName(value);
            }
        }

        public float ErpIntensity {
            get => _erpIntensity;
            set {
                if (_erpIntensity.NearEqual(value)) return;
                _erpIntensity = value;
                OnPropertyChanged();
                OnDynamicSkyChanged();
            }
        }

        //public Transform3D LightTransform1 {
        //    get => _lightTransform1;
        //    set {
        //        _lightTransform1 = value;
        //        OnPropertyChanged();
        //        OnSceneChanged();
        //    }
        //}

        //public Transform3D LightTransform2 {
        //    get => _lightTransform2;
        //    set {
        //        _lightTransform2 = value;
        //        OnPropertyChanged();
        //        OnSceneChanged();
        //    }
        //}

        public Transform3D MeshTransform {
            get => _meshTransform;
            set {
                _meshTransform = value;
                OnPropertyChanged();
            }
        }

        public TextureModel EquirectangularMap {
            get => _equirectangularMap;
            set {
                if (_equirectangularMap == value) return;
                _equirectangularMap = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasEquirectangularMap));
                OnPropertyChanged(nameof(HasEnvironmentMap));
                OnEnvironmentChanged();
            }
        }


        public ScenePropertiesModel()
        {
            _enableAtmosphere = true;
            _ambientColor = Media.Color.FromRgb(60, 60, 60);
            _lightColor = Media.Color.FromRgb(60, 255, 60);
            _timeOfDay = 6_000;
            _erpIntensity = 0f;

            //_lightTransform1 = new TranslateTransform3D(10, 14, 8);
            //_lightTransform2 = new TranslateTransform3D(-12, -12, -10);

            _pomType = PomTypeValues.Normal;

            _meshTransform = new RotateTransform3D {
                CenterX = 0f,
                CenterY = 0f,
                CenterZ = 0f,
            };

            spinAnimation = new Rotation3DAnimation {
                RepeatBehavior = RepeatBehavior.Forever,
                By = new AxisAngleRotation3D(new Vector3D(0f, 1f, 0f), 240),
                Duration = TimeSpan.FromSeconds(14),
                IsCumulative = true,
            };

            if (spinAnimation.CanFreeze) spinAnimation.Freeze();
        }

#if !RELEASENORENDER

        private void GetSunAngle(out Vector3 sunAngle, out float strength)
        {
            const float sun_overlap = 0.0f;
            const float sun_power = 0.9f;

            var time = GetLinearTimeOfDay();
            MinecraftTime.GetSunAngle(_sunAzimuth, _sunTilt, time, out sunAngle);
            strength = MinecraftTime.GetSunStrength(in sunAngle, sun_overlap, sun_power);
        }

        private float GetLinearTimeOfDay()
        {
            var t = _timeOfDay / 24_000f;
            MathEx.Wrap(ref t, 0f, 1f);
            return t;
        }

        private void SetTimeOfDay(float value)
        {
            TimeOfDay = (int)(value * 24_000f);
        }

        private void UpdateSpinAnimation()
        {
            if (_spinMesh)
                _meshTransform.BeginAnimation(RotateTransform3D.RotationProperty, spinAnimation, HandoffBehavior.SnapshotAndReplace);
            else
                _meshTransform.BeginAnimation(RotateTransform3D.RotationProperty, null);
        }

#endif

        protected virtual void OnEnvironmentChanged()
        {
            EnvironmentChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDynamicSkyChanged()
        {
            GetSunAngle(out var sunDirection, out var sunStrength);
            SunDirection = sunDirection;
            SunStrength = sunStrength;

            DynamicSkyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
