using System.Collections.Generic;

namespace PixelGraph.UI.ViewData;

internal class AppThemeBaseValues : List<AppThemeBaseValues.Item>
{
    public AppThemeBaseValues()
    {
        Add(new Item {Text = "Dark", Value = "dark"});
        Add(new Item {Text = "Light", Value = "light"});
    }

    public class Item
    {
        public string Text {get; set;}
        public string Value {get; set;}
    }
}

internal class AppThemeAccentValues : List<AppThemeAccentValues.Item>
{
    public AppThemeAccentValues()
    {
        Add(new Item {Text = "Red", Value = "red"});
        Add(new Item {Text = "Green", Value = "green"});
        Add(new Item {Text = "Blue", Value = "blue"});
        Add(new Item {Text = "Purple", Value = "purple"});
        Add(new Item {Text = "Orange", Value = "orange"});
        Add(new Item {Text = "Lime", Value = "lime"});
        Add(new Item {Text = "Emerald", Value = "emerald"});
        Add(new Item {Text = "Teal", Value = "teal"});
        Add(new Item {Text = "Cyan", Value = "cyan"});
        Add(new Item {Text = "Cobalt", Value = "cobalt"});
        Add(new Item {Text = "Indigo", Value = "indigo"});
        Add(new Item {Text = "Violet", Value = "violet"});
        Add(new Item {Text = "Pink", Value = "pink"});
        Add(new Item {Text = "Magenta", Value = "magenta"});
        Add(new Item {Text = "Crimson", Value = "crimson"});
        Add(new Item {Text = "Amber", Value = "amber"});
        Add(new Item {Text = "Yellow", Value = "yellow"});
        Add(new Item {Text = "Brown", Value = "brown"});
        Add(new Item {Text = "Olive", Value = "olive"});
        Add(new Item {Text = "Steel", Value = "steel"});
        Add(new Item {Text = "Mauve", Value = "mauve"});
        Add(new Item {Text = "Taupe", Value = "taupe"});
        Add(new Item {Text = "Sienna", Value = "sienna"});
    }

    public class Item
    {
        public string Text {get; set;}
        public string Value {get; set;}
    }
}