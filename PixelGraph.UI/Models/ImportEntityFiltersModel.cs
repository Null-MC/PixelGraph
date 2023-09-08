using MinecraftMappings.Internal.Models.Entity;
using MinecraftMappings.Minecraft.Java;
using PixelGraph.UI.Internal;
using System;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.Models;

public class ImportEntityFiltersModel : ModelBase
{
    protected string _gameVersion;
    protected JavaEntityModelVersion _gameEntity;

    public event EventHandler GameVersionChanged;
    //public event EventHandler GameEntityChanged;

    public ObservableCollection<GameEntityNameOption> GameEntityList {get;}

    public string GameVersion {
        get => _gameVersion;
        set {
            _gameVersion = value;
            OnPropertyChanged();
            OnGameVersionChanged();
        }
    }

    public JavaEntityModelVersion GameEntity {
        get => _gameEntity;
        set {
            _gameEntity = value;
            OnPropertyChanged();
            //OnGameEntityChanged();
        }
    }


    public ImportEntityFiltersModel()
    {
        GameEntityList = new ObservableCollection<GameEntityNameOption>();

        GameVersion = JavaVersions.Latest.ToString();
    }

    private void OnGameVersionChanged()
    {
        GameVersionChanged?.Invoke(this, EventArgs.Empty);
    }

    //private void OnGameEntityChanged()
    //{
    //    GameEntityChanged?.Invoke(this, EventArgs.Empty);
    //}
}

public class GameEntityNameOption
{
    public string Name {get; set;}
    public JavaEntityModelVersion Data {get; set;}
}