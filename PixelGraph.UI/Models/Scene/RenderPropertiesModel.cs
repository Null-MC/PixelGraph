﻿using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Textures.Block;
using MinecraftMappings.Internal.Textures.Entity;
using MinecraftMappings.Minecraft;
using PixelGraph.Common.Material;
using PixelGraph.Rendering;
using PixelGraph.Rendering.CubeMaps;
using PixelGraph.Rendering.LUTs;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.IO.Models;
using SharpDX;
using System.Windows.Media.Media3D;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace PixelGraph.UI.Models.Scene;

public class RenderPropertiesModel : ModelBase
{
    private IEffectsManager? _effectsManager;
    private ILutMapSource? _dielectricBrdfLutMap;
    private ICubeMapSource? _irradianceCube;
    private ObservableElement3DCollection? _meshParts = [];
    private PerspectiveCamera? _camera = new() {
        UpDirection = Vector3.UnitY.ToVector3D(),
    };

    private FXAALevel _fxaa;
    private bool _enableSwapChain;
    private bool _enableTiling;
    private RenderPreviewModes _renderMode;
    private float _parallaxDepth;
    private int _parallaxSamples;
    private bool _enableBloom;
    private int _waterMode;
    private OrthographicCamera _sunCamera = new() {
        UpDirection = Vector3.UnitY.ToVector3D(),
        NearPlaneDistance = 1f,
        FarPlaneDistance = 48f,
        Width = 24,
    };
    private PerspectiveCamera _lightCamera = new() { 
        Position = new Point3D(8f, 8f, 8f),
        LookDirection = new Vector3D(-1f, -1f, -1f),
        UpDirection = new Vector3D(0f, 1f, 0f),
        FarPlaneDistance = 32f,
        NearPlaneDistance = 1f,
        FieldOfView = 45f,
    };
    private int _environmentMapSize;
    private int _irradianceMapSize;
    private string? _meshBlendMode;
    private string? _meshTintColor;
    private float _subSurfaceBlur;
    private bool _showIrradiance;

    public event EventHandler? RenderModeChanged;
    public event EventHandler? RenderModelChanged;

    internal ICubeMapSource? DynamicSkyCubeSource {get; set;}
    internal ICubeMapSource? ErpCubeSource {get; set;}

    public int EnvironmentMapSize {
        get => _environmentMapSize;
        set {
            if (_environmentMapSize == value) return;
            _environmentMapSize = value;
            OnPropertyChanged();
        }
    }

    public int IrradianceMapSize {
        get => _irradianceMapSize;
        set {
            if (_irradianceMapSize == value) return;
            _irradianceMapSize = value;
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

    public int ParallaxSamples {
        get => _parallaxSamples;
        set {
            _parallaxSamples = value;
            OnPropertyChanged();
        }
    }

    public bool EnableBloom {
        get => _enableBloom;
        set {
            _enableBloom = value;
            OnPropertyChanged();
        }
    }

    public int WaterMode {
        get => _waterMode;
        set {
            _waterMode = value;
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

    public PerspectiveCamera LightCamera {
        get => _lightCamera;
        set {
            _lightCamera = value;
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

    public IEffectsManager? EffectsManager {
        get => _effectsManager;
        set {
            _effectsManager = value;
            OnPropertyChanged();
        }
    }

    public FXAALevel FXAA {
        get => _fxaa;
        set {
            _fxaa = value;
            OnPropertyChanged();
        }
    }

    public bool EnableSwapChain {
        get => _enableSwapChain;
        set {
            _enableSwapChain = value;
            OnPropertyChanged();
        }
    }

    public bool EnableTiling {
        get => _enableTiling;
        set {
            _enableTiling = value;
            OnPropertyChanged();
            OnRenderModelChanged();
        }
    }

    public ILutMapSource? DielectricBrdfLutMap {
        get => _dielectricBrdfLutMap;
        set {
            _dielectricBrdfLutMap = value;
            OnPropertyChanged();
        }
    }

    public ICubeMapSource? IrradianceCube {
        get => _irradianceCube;
        set {
            _irradianceCube = value;
            OnPropertyChanged();
        }
    }

    public MaterialProperties? MissingMaterial {
        get;
    } = new() {
        Color = new MaterialColorProperties {
            Value = "#f800f8",
            Texture = "<missing>",
        }
    };

    public PerspectiveCamera? Camera {
        get => _camera;
        set {
            _camera = value;
            OnPropertyChanged();
        }
    }

    public ObservableElement3DCollection? MeshParts {
        get => _meshParts;
        set {
            _meshParts = value;
            OnPropertyChanged();
        }
    }

    public string? MeshBlendMode {
        get => _meshBlendMode;
        private set {
            _meshBlendMode = value;
            OnPropertyChanged();
        }
    }

    public string? MeshTintColor {
        get => _meshTintColor;
        private set {
            _meshTintColor = value;
            OnPropertyChanged();
        }
    }

    public float SubSurfaceBlur {
        get => _subSurfaceBlur;
        set {
            _subSurfaceBlur = value;
            OnPropertyChanged();
        }
    }

    public bool ShowIrradiance {
        get => _showIrradiance;
        set {
            if (_showIrradiance == value) return;
            _showIrradiance = value;
            OnPropertyChanged();
        }
    }


    public void ApplyMaterial(MaterialProperties? material)
    {
        var blend = material?.BlendMode;
        if (material is { Name: not null } && blend == null) {
            if (material.LocalPath != null && MinecraftPath.IsEntityPath(material.LocalPath)) {
                var textureData = Minecraft.Java.FindEntityTexturesById<JavaEntityTexture, JavaEntityTextureVersion>(material.Name).FirstOrDefault();
                blend = textureData != null
                    ? BlendModes.ToString(textureData.BlendMode)
                    : BlendModes.CutoutText;
            }
            else if (material.LocalPath != null && MinecraftPath.IsItemPath(material.LocalPath)) {
                blend = BlendModes.CutoutText;
            }
            else {
                var textureData = Minecraft.Java.FindBlockTexturesById<JavaBlockTexture, JavaBlockTextureVersion>(material.Name).FirstOrDefault();
                blend = textureData != null
                    ? BlendModes.ToString(textureData.BlendMode)
                    : BlendModes.OpaqueText;
            }
        }

        MeshBlendMode = blend ?? BlendModes.OpaqueText;
        MeshTintColor = material?.TintColor;
    }

    private void OnRenderModeChanged()
    {
        RenderModeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnRenderModelChanged()
    {
        RenderModelChanged?.Invoke(this, EventArgs.Empty);
    }
}