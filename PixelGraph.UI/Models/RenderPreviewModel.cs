using System;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.Scene;

namespace PixelGraph.UI.Models
{
    public class RenderPreviewModel : ModelBase
    {
        //private IEffectsManager _effectsManager;
        //private ICubeMapSource _environmentCube;
        //private ICubeMapSource _irradianceCube;
        //private ObservableElement3DCollection _meshParts;
        //private PerspectiveCamera _camera;
        //private Stream _brdfLutMap;
        //private MaterialProperties _missingMaterial;
        //private bool _enableAtmosphere;
        //private bool _enableLights;
        //private RenderPreviewModes _renderMode;
        private ScenePropertiesModel _sceneProperties;
        private RenderPropertiesModel _renderProperties;
        private bool _isLoaded;

        //public event EventHandler RenderModeChanged;
        public event EventHandler SceneChanged;
        //public event EventHandler RenderModelChanged;
        
        public ScenePropertiesModel SceneProperties {
            get => _sceneProperties;
            set {
                if (_sceneProperties != null)
                    _sceneProperties.SceneChanged -= OnSceneChanged;

                _sceneProperties = value;
                OnPropertyChanged();

                if (value != null)
                    value.SceneChanged += OnSceneChanged;
            }
        }

        public RenderPropertiesModel RenderProperties {
            get => _renderProperties;
            set {
                _renderProperties = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoaded {
            get => _isLoaded;
            set {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }


        //public RenderPreviewModel()
        //{
        //    SceneProperties = new ScenePropertiesModel();
        //    RenderProperties = new RenderPropertiesModel();
        //}

        //private void OnRenderModeChanged()
        //{
        //    RenderModeChanged?.Invoke(this, EventArgs.Empty);
        //}

        //private void OnRenderModelChanged()
        //{
        //    RenderModelChanged?.Invoke(this, EventArgs.Empty);
        //}

        private void OnSceneChanged(object sender, EventArgs e)
        {
            OnSceneChanged();
        }

        protected virtual void OnSceneChanged()
        {
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
