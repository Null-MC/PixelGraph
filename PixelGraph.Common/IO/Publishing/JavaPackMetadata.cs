using Newtonsoft.Json;

namespace PixelGraph.Common.IO.Publishing;

internal class JavaPackMetadata
{
    [JsonProperty("pack_format")]
    public int PackFormat {get; set;}

    [JsonProperty("description")]
    public string Description {get; set;}


    public JavaPackMetadata()
    {
        PackFormat = 5;
    }
}