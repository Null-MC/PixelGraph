using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewData;
using System;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.Models
{
    public class NewMaterialModel : ModelBase
    {
        protected GameObjectTypes _gameObjectType;
        protected string _gameObjectName;
        protected string _gameNamespace;
        protected string _location;

        public event EventHandler GameObjectTypeChanged;
        public event EventHandler GameObjectLocationChanged;

        public ObservableCollection<GameObjectOption> GameObjectNames {get;}

        public GameObjectTypes GameObjectType {
            get => _gameObjectType;
            set {
                _gameObjectType = value;
                OnPropertyChanged();
                OnGameObjectTypeChanged();
                OnGameObjectLocationChanged();
            }
        }

        public string GameNamespace {
            get => _gameNamespace;
            set {
                _gameNamespace = value;
                OnPropertyChanged();
                OnGameObjectLocationChanged();
            }
        }

        public string GameObjectName {
            get => _gameObjectName;
            set {
                _gameObjectName = value;
                OnPropertyChanged();
                OnGameObjectLocationChanged();
            }
        }

        public string Location {
            get => _location;
            set {
                _location = value;
                OnPropertyChanged();
            }
        }


        public NewMaterialModel()
        {
            GameObjectNames = new ObservableCollection<GameObjectOption>();

            _gameObjectType = GameObjectTypes.Block;
            _gameNamespace = "minecraft";
        }

        private void OnGameObjectTypeChanged()
        {
            GameObjectTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnGameObjectLocationChanged()
        {
            GameObjectLocationChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class NewMaterialDesignVM : NewMaterialModel
    {
        public NewMaterialDesignVM()
        {
            _gameObjectName = "bricks";
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
