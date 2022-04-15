using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.UI.Internal;
using System;
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
        private string _erpFilename;
        private float _erpExposure;
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

        public string ErpFilename {
            get => _erpFilename;
            set {
                if (_erpFilename == value) return;
                _erpFilename = value;
                OnPropertyChanged();
            }
        }

        public float ErpExposure {
            get => _erpExposure;
            set {
                if (_erpExposure.NearEqual(value)) return;
                _erpExposure = value;
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

            //_lightTransform1 = new TranslateTransform3D(10, 14, 8);
            //_lightTransform2 = new TranslateTransform3D(-12, -12, -10);
            _erpExposure = 1f;
        }

#if !RELEASENORENDER

        public void GetSunAngle(out Vector3 sunAngle, out float strength)
        {
            const float sun_overlap = 0.0f;
            const float sun_power = 0.9f;

            var time = GetLinearTimeOfDay();
            MinecraftTime.GetSunAngle(_sunAzimuth, _sunTilt, time, out sunAngle);
            strength = MinecraftTime.GetSunStrength(in sunAngle, sun_overlap, sun_power);
        }

        public float GetLinearTimeOfDay()
        {
            var t = _timeOfDay / 24_000f;
            MathEx.Wrap(ref t, 0f, 1f);
            return t;
        }

        public void SetTimeOfDay(float value)
        {
            TimeOfDay = (int)(value * 24_000f);
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
