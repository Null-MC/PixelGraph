using HelixToolkit.Wpf.SharpDX;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.Scene;
using SharpDX;

namespace PixelGraph.UI.Models;

public class RenderPreviewModel : ModelBase
{
    private ScenePropertiesModel _sceneProperties;
    private RenderPropertiesModel _renderProperties;
    private bool _isLoaded;

    public ScenePropertiesModel SceneProperties {
        get => _sceneProperties;
        set {
            _sceneProperties = value;
            OnPropertyChanged();
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


    public void UpdateSunPosition()
    {
        RenderProperties.SunCamera.Position = (new Vector3(0f, 2f, 0f) + SceneProperties.SunDirection * 16f).ToPoint3D();
        RenderProperties.SunCamera.LookDirection = -SceneProperties.SunDirection.ToVector3D();
    }
}