using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class GameObjectTypeValues : List<GameObjectTypeValues.Item>
    {
        public GameObjectTypeValues()
        {
            Add(new Item("Block", GameObjectTypes.Block));
            Add(new Item("Entity", GameObjectTypes.Entity));
            Add(new Item("Optifine CTM", GameObjectTypes.OptifineCtm));
        }

        public class Item
        {
            public string Text {get;}
            public GameObjectTypes Value {get;}

            public Item(string text, GameObjectTypes value)
            {
                Text = text;
                Value = value;
            }
        }
    }

    public enum GameObjectTypes
    {
        Block,
        Entity,
        OptifineCtm,
    }
}
