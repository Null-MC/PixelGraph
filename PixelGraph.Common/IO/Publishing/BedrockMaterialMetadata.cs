using Newtonsoft.Json;

namespace PixelGraph.Common.IO.Publishing;

internal class BedrockMaterialMetadata
{
    [JsonProperty("format_version")]
    public string FormatVersion {get; set;}

    [JsonProperty("minecraft:texture_set")]
    public BedrockMaterialTextureSetMetadata TextureSet {get; set;}


    public BedrockMaterialMetadata()
    {
        TextureSet = new BedrockMaterialTextureSetMetadata();
    }
}

internal class BedrockMaterialTextureSetMetadata
{
    [JsonProperty("color")]
    public string Color {get; set;}

    [JsonProperty("normal", NullValueHandling=NullValueHandling.Ignore)]
    public string Normal {get; set;}

    [JsonProperty("heightmap", NullValueHandling=NullValueHandling.Ignore)]
    public string Height {get; set;}

    [JsonProperty("metalness_emissive_roughness")]
    public string MER {get; set;}
}