using Microsoft.Extensions.DependencyInjection;
using MinecraftMappings.Minecraft;
using Nito.Disposables.Internals;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.IO;
using PixelGraph.UI.ViewData;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.ViewModels;

internal class NewMaterialViewModel : ModelBase
{
    protected GameObjectTypes _gameObjectType = GameObjectTypes.Block;
    protected string? _gameNamespace = "minecraft";
    protected string? _gameObjectName;
    protected string? _location;

    public ObservableCollection<string> GameNamespaces {get;} = new(new [] {"minecraft"});
    public ObservableCollection<GameObjectOption> GameObjectNames {get;} = [];

    public GameObjectTypes GameObjectType {
        get => _gameObjectType;
        set {
            _gameObjectType = value;
            OnPropertyChanged();

            UpdateBlockList();
            UpdateLocation();
        }
    }

    public string? GameNamespace {
        get => _gameNamespace;
        set {
            _gameNamespace = value;
            OnPropertyChanged();

            UpdateLocation();
        }
    }

    public string? GameObjectName {
        get => _gameObjectName;
        set {
            _gameObjectName = value;
            OnPropertyChanged();

            UpdateLocation();
        }
    }

    public string? Location {
        get => _location;
        private set {
            _location = value;
            OnPropertyChanged();
        }
    }


    public void Initialize(IServiceProvider provider)
    {
        var locator = provider.GetRequiredService<MinecraftResourceLocator>();

        foreach (var @namespace in locator.FindAllNamespaces().WhereNotNull()) {
            if (string.Equals(@namespace, "minecraft", StringComparison.InvariantCultureIgnoreCase)) continue;
            GameNamespaces.Add(@namespace);
        }
    }

    public void UpdateBlockList()
    {
        GameObjectNames.Clear();

        switch (GameObjectType) {
            case GameObjectTypes.Block:
            case GameObjectTypes.Optifine_CTM:
                foreach (var block in Minecraft.Java.AllBlockTextures) {
                    var latest = block.GetLatestVersion() ?? throw new ApplicationException($"Unable to get latest game version for block '{block.Name}'!");

                    GameObjectNames.Add(new GameObjectOption {
                        Id = latest.Id,
                    });
                }
                break;
            case GameObjectTypes.Item:
            case GameObjectTypes.Optifine_CIT:
                foreach (var item in Minecraft.Java.AllItems) {
                    var latest = item.GetLatestVersion() ?? throw new ApplicationException($"Unable to get latest game version for item '{item.Name}'!");

                    GameObjectNames.Add(new GameObjectOption {
                        Id = latest.Id,
                    });
                }
                break;
            case GameObjectTypes.Entity:
                foreach (var entity in Minecraft.Java.AllEntityTextures) {
                    var latest = entity.GetLatestVersion() ?? throw new ApplicationException($"Unable to get latest game version for entity '{entity.Name}'!");

                    var e = new GameObjectOption {
                        Id = latest.Id,
                    };

                    if (latest.Path != null)
                        e.Path = $"{latest.Path}/{latest.Id}";

                    GameObjectNames.Add(e);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UpdateLocation()
    {
        Location = GetLocation();
    }

    private string? GetLocation()
    {
        var pathExt = GetPathForType(GameObjectType);
        if (pathExt == null) return null;
            
        var path = $"assets/{GameNamespace}/{pathExt}";
        if (GameObjectName == null) return path;

        return $"{path}/{GameObjectName}";
    }

    private static string? GetPathForType(GameObjectTypes objType)
    {
        return objType switch {
            GameObjectTypes.Block => "textures/block",
            GameObjectTypes.Item => "textures/item",
            GameObjectTypes.Entity => "textures/entity",
            GameObjectTypes.Optifine_CTM => "optifine/ctm",
            GameObjectTypes.Optifine_CIT => "optifine/cit",
            _ => null,
        };
    }
}

internal class NewMaterialDesignerViewModel : NewMaterialViewModel
{
    public NewMaterialDesignerViewModel()
    {
        _gameObjectName = "bricks";
    }
}

public class GameObjectOption
{
    private string? _path;

    public string? Id {get; init;}

    public string? Path {
        get => _path ?? Id;
        set => _path = value;
    }
}