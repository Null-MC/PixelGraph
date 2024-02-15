using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixelGraph.Common.Bedrock;

internal class BedrockTextureSet
{
    [JsonProperty("format_version")]
    public string? FormatVersion {get; set;}
        
    [JsonProperty("minecraft:texture_set")]
    public JObject? TextureSet {get; set;}
}
