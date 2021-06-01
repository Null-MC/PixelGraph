using MinecraftMappings.Minecraft;
using PixelGraph.UI.ViewData;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.ViewModels
{
    public class NewMaterialVM : ViewModelBase
    {
        protected GameObjectTypes _gameObjectType;
        protected string _gameObjectName;
        protected string _location;

        public ObservableCollection<GameObjectOption> GameObjectNames {get;}
        public string Location => _location;

        public GameObjectTypes GameObjectType {
            get => _gameObjectType;
            set {
                _gameObjectType = value;
                OnPropertyChanged();

                UpdateBlockList();
                UpdateLocation();
            }
        }

        public string GameObjectName {
            get => _gameObjectName;
            set {
                _gameObjectName = value;
                OnPropertyChanged();

                UpdateLocation();
            }
        }


        public NewMaterialVM()
        {
            GameObjectNames = new ObservableCollection<GameObjectOption>();

            GameObjectType = GameObjectTypes.Block;
            UpdateBlockList();
            UpdateLocation();
        }

        private void UpdateBlockList()
        {
            GameObjectNames.Clear();

            switch (GameObjectType) {
                case GameObjectTypes.Block:
                case GameObjectTypes.OptifineCtm:
                    foreach (var block in Minecraft.Java.AllBlocks) {
                        var latest = block.GetLatestVersion();

                        GameObjectNames.Add(new GameObjectOption {
                            Id = latest.Id,
                        });
                    }
                    break;
                case GameObjectTypes.Entity:
                    foreach (var entity in Minecraft.Java.AllEntities) {
                        var latest = entity.GetLatestVersion();

                        var e = new GameObjectOption {
                            Id = latest.Id,
                        };

                        if (latest.Path != null)
                            e.Path = $"{latest.Path}/{latest.Id}";

                        GameObjectNames.Add(e);
                    }
                    break;
            }
        }

        protected void UpdateLocation()
        {
            _location = GetLocation();
            OnPropertyChanged(nameof(Location));
        }

        private string GetLocation()
        {
            var path = GetPathForType();
            if (path == null) return null;

            if (_gameObjectName == null) return path;
            return $"{path}/{_gameObjectName}";
        }

        private string GetPathForType()
        {
            return _gameObjectType switch {
                GameObjectTypes.Block => "assets/minecraft/textures/block",
                GameObjectTypes.Entity => "assets/minecraft/textures/entity",
                GameObjectTypes.OptifineCtm => "assets/minecraft/optifine/ctm",
                _ => null,
            };
        }
    }

    public class NewMaterialDesignVM : NewMaterialVM
    {
        public NewMaterialDesignVM()
        {
            _gameObjectType = GameObjectTypes.Block;
            _gameObjectName = "bricks";
            UpdateLocation();
        }
    }

    public class GameObjectOption
    {
        private string _path;

        public string Id {get; set;}

        public string Path {
            get => _path ?? Id;
            set => _path = value;
        }
    }
}
