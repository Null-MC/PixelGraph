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
        protected string _location;

        public event EventHandler GameObjectTypeChanged;
        public event EventHandler GameObjectNameChanged;

        public ObservableCollection<GameObjectOption> GameObjectNames {get;}

        public GameObjectTypes GameObjectType {
            get => _gameObjectType;
            set {
                _gameObjectType = value;
                OnPropertyChanged();
                OnGameObjectTypeChanged();

                //UpdateBlockList();
                //UpdateLocation();
            }
        }

        public string GameObjectName {
            get => _gameObjectName;
            set {
                _gameObjectName = value;
                OnPropertyChanged();
                OnGameObjectNameChanged();

                //UpdateLocation();
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

            GameObjectType = GameObjectTypes.Block;
        }

        private void OnGameObjectTypeChanged()
        {
            GameObjectTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnGameObjectNameChanged()
        {
            GameObjectNameChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class NewMaterialDesignVM : NewMaterialModel
    {
        public NewMaterialDesignVM()
        {
            _gameObjectType = GameObjectTypes.Block;
            _gameObjectName = "bricks";
            //UpdateLocation();
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
