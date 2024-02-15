using MinecraftMappings.Minecraft.Java;

namespace PixelGraph.UI.ViewData;

internal class GameVersionValues : List<GameVersionValue>
{
    public GameVersionValues()
    {
        foreach (var version in JavaVersions.AllParsed)
            Add(new GameVersionValue {
                Text = version.ToString(),
                Version = version,
            });
    }
}

public class GameVersionValue
{
    public string? Text {get; set;}
    public Version? Version {get; set;}
    //public bool IsLatest {get; set;}
}