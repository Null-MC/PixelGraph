using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class GameObjectTypeValues : List<GameObjectTypeValues.Item>
    {
        public GameObjectTypeValues()
        {
            Add(new Item("Block", GameObjectTypes.Block));
            Add(new Item("Entity", GameObjectTypes.Entity));
            Add(new Item("Item", GameObjectTypes.Item));
            Add(new Item("Optifine CTM", GameObjectTypes.Optifine_CTM));
            Add(new Item("Optifine CIT", GameObjectTypes.Optifine_CIT));
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
        Item,
        Optifine_CTM,
        Optifine_CIT,
    }
}
